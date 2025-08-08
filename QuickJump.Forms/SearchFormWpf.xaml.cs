using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using EnvDTE;
using Microsoft.Internal.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using QuickJump2022.Data;
using QuickJump2022.Models;
using QuickJump2022.Options;
using QuickJump2022.Tools;
using Utilities = QuickJump2022.Tools.Utilities;

namespace QuickJump2022.Forms;

public partial class SearchFormWpf : DialogWindow, INotifyPropertyChanged {
    public List<ProjectItem> DocFileNames;
    public List<CodeItem> CodeItems;
    public Enums.ESearchType SearchType;

    private ObservableCollection<ListItemViewModel> _items;
    private GeneralOptionsPage _options;

    public ObservableCollection<ListItemViewModel> Items {
        get => _items;
        set {
            _items = value;
            OnPropertyChanged();
        }
    }

    // Binding properties for UI
    public Brush BorderColor => new SolidColorBrush(ToMediaColor(_options.BorderColor));
    public Brush BackgroundColor => new SolidColorBrush(Colors.Black);
    public Brush StatusBackgroundColor => new SolidColorBrush(ToMediaColor(_options.StatusBackgroundColor));
    public bool ShowStatusBar => _options.ShowStatusBar;
    public string SearchFontFamily => _options.SearchFont.FontFamily.Name;
    public double SearchFontSize => _options.SearchFont.Size * 96.0 / 72.0; // Convert points to WPF units
    public string ItemFontFamily => _options.ItemFont.FontFamily.Name;
    public double ItemFontSize => _options.ItemFont.Size * 96.0 / 72.0;

    public static void ShowNonBlockingModal(Enums.ESearchType type) {
        var thread = new System.Threading.Thread(() => {
            try {
                SynchronizationContext.SetSynchronizationContext(
                    new DispatcherSynchronizationContext(Dispatcher.CurrentDispatcher)
                );
                var dialog = new SearchFormWpf(type);
                dialog.Closed += (s, e) => Dispatcher.CurrentDispatcher.BeginInvokeShutdown(DispatcherPriority.Background);
                dialog.Show();
                dialog.Activate();
                Dispatcher.Run();
            }
            catch (Exception ex) {
                ex.Log();
                Dispatcher.CurrentDispatcher.BeginInvokeShutdown(DispatcherPriority.Background);
            }
        }
        );
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
    }

    public SearchFormWpf(Enums.ESearchType type) {
        // ThreadHelper.ThrowIfNotOnUIThread();
        SearchType = type;
        _options = QuickJumpData.Instance.GeneralOptions;
        Items = new ObservableCollection<ListItemViewModel>();
        
        // Set render options before InitializeComponent for better startup performance
        SetValue(RenderOptions.BitmapScalingModeProperty, BitmapScalingMode.NearestNeighbor);
        SetValue(RenderOptions.EdgeModeProperty, EdgeMode.Aliased);

        InitializeComponent();
        DataContext = this;

        // Optimize text rendering
        TextOptions.SetTextFormattingMode(this, TextFormattingMode.Ideal);
        TextOptions.SetTextRenderingMode(this, TextRenderingMode.Auto);
        TextOptions.SetTextHintingMode(this, TextHintingMode.Auto);
    }

    private async void Window_Loaded(object sender, RoutedEventArgs e) {
        Width = _options.Width;

        // Center to screen and apply offsets
        var screenWidth = SystemParameters.PrimaryScreenWidth;
        var screenHeight = SystemParameters.PrimaryScreenHeight;
        Left = (screenWidth - Width) / 2 + _options.OffsetLeft;
        Top = (screenHeight - Height) / 2 + _options.OffsetTop;

        try {
            ClearData();
            Document document = QuickJumpData.Instance.Dte.ActiveWindow.Document;

            if (SearchType == Enums.ESearchType.Files || SearchType == Enums.ESearchType.All) {
                DocFileNames = QuickJumpData.Instance.GetDocFilenames();
            }

            if (SearchType == Enums.ESearchType.Methods || SearchType == Enums.ESearchType.All) {
                // TODO: implement cross-thread call.
                CodeItems = await QuickJumpData.Instance.GetCodeItemsUsingWorkspaceAsync(document);
            }

            lblSolutionName.Text = QuickJumpData.Instance.Dte.Solution.FullName;
            RefreshList();

            if (Items.Count > 0) {
                lstItems.SelectedIndex = 0;
            }

            // Ensure the textbox gets focus
            // Using Dispatcher to ensure this happens after the window is fully rendered
            await Dispatcher.BeginInvoke(new Action(() => {
                txtSearch.Focus();
                Keyboard.Focus(txtSearch);
            }), System.Windows.Threading.DispatcherPriority.Loaded);
        }
        catch (Exception ex) {
            System.Windows.MessageBox.Show(ex.ToString());
        }
    }

    private void RefreshList() {
        ThreadHelper.ThrowIfNotOnUIThread();
        try {
            List<ListItemBase> objectList = new List<ListItemBase>(2048);
            string searchText = txtSearch.Text;

            Items.Clear();

            // Gather items
            if (SearchType == Enums.ESearchType.Files || SearchType == Enums.ESearchType.All) {
                foreach (ProjectItem doc in DocFileNames) {
                    if (Utilities.Filter(doc.Name, searchText)) {
                        objectList.Add(new ListItemFile(doc, searchText));
                    }
                }
            }

            if (SearchType == Enums.ESearchType.Methods || SearchType == Enums.ESearchType.All) {
                foreach (CodeItem item in CodeItems) {
                    if (Utilities.Filter(item.Name, searchText)) {
                        objectList.Add(new ListItemCSharp(item, searchText));
                    }
                }
            }

            // Apply fuzzy search scoring
            if (!string.IsNullOrEmpty(searchText)) {
                foreach (var item in objectList) {
                    var fuzzyScore = FuzzySearch.ScoreFuzzy(item.Name, searchText);
                    item.Weight = fuzzyScore.Score;
                }

                objectList.Sort((a, b) => {
                    if (a.Weight != b.Weight) {
                        return b.Weight.CompareTo(a.Weight);
                    }

                    var sortType = SearchType switch {
                        Enums.ESearchType.Files => _options.FileSortType,
                        Enums.ESearchType.Methods => _options.CSharpSortType,
                        _ => _options.MixedSortType,
                    };

                    return GetSortComparison(a, b, sortType);
                });
            }
            else {
                SortObjects(objectList, SearchType switch {
                    Enums.ESearchType.Files => _options.FileSortType,
                    Enums.ESearchType.Methods => _options.CSharpSortType,
                    _ => _options.MixedSortType,
                });
            }

            // Convert to ViewModels and add to collection
            foreach (var item in objectList) {
                Items.Add(new ListItemViewModel(item, _options));
            }

            lblCountValue.Text = Items.Count.ToString();

            // Adjust height based on item count
            double itemHeight = _options.ItemFont.Height + 6;
            Height = Utilities.Clamp(Items.Count * (int)itemHeight + 56, 100, _options.MaxHeight);
        }
        catch (Exception ex) {
            System.Windows.MessageBox.Show(ex.ToString());
        }
    }

    private void SortObjects(List<ListItemBase> objectList, Enums.SortType sortType) {
        switch (sortType) {
            case Enums.SortType.Alphabetical:
                objectList.Sort(Sort.Alphabetical);
                break;
            case Enums.SortType.AlphabeticalReverse:
                objectList.Sort(Sort.AlphabeticalReverse);
                break;
            case Enums.SortType.LineNumber:
                objectList.Sort(Sort.LineNumber);
                break;
            case Enums.SortType.LineNumberReverse:
                objectList.Sort(Sort.LineNumberReverse);
                break;
            case Enums.SortType.Weight:
                objectList.Sort(Sort.Weight);
                break;
            case Enums.SortType.WeightReverse:
                objectList.Sort(Sort.WeightReverse);
                break;
        }
    }

    private int GetSortComparison(ListItemBase a, ListItemBase b, Enums.SortType sortType) {
        switch (sortType) {
            case Enums.SortType.Alphabetical:
                return Sort.Alphabetical(a, b);
            case Enums.SortType.AlphabeticalReverse:
                return Sort.AlphabeticalReverse(a, b);
            case Enums.SortType.LineNumber:
                return Sort.LineNumber(a, b);
            case Enums.SortType.LineNumberReverse:
                return Sort.LineNumberReverse(a, b);
            case Enums.SortType.Weight:
                return Sort.Weight(a, b);
            case Enums.SortType.WeightReverse:
                return Sort.WeightReverse(a, b);
            case Enums.SortType.Fuzzy:
                return Sort.Fuzzy(a, b);
            case Enums.SortType.FuzzyReverse:
                return Sort.FuzzyReverse(a, b);
            default:
                return Sort.Alphabetical(a, b);
        }
    }

    private void ClearData() {
        DocFileNames = null;
        CodeItems = null;
        GC.Collect();
    }

    private void txtSearch_TextChanged(object sender, TextChangedEventArgs e) {
        ThreadHelper.ThrowIfNotOnUIThread();
        RefreshList();
        if (Items.Count > 0) {
            lstItems.SelectedIndex = 0;
        }
    }

    private void txtSearch_PreviewKeyDown(object sender, KeyEventArgs e) {
        if (e.Key == Key.Escape) {
            Close();
            e.Handled = true;
        }
        else if (e.Key == Key.Return) {
            GotoItem();
            Close();
            e.Handled = true;
        }
        else if (e.Key == Key.A && Keyboard.Modifiers == ModifierKeys.Control) {
            // Handle Ctrl+A to select all text
            txtSearch.SelectAll();
            e.Handled = true;
        }
        // Note: We don't handle Left/Right arrows, Ctrl+Shift+Left/Right, etc.
        // so they work normally for text navigation and selection
    }

    private void txtSearch_KeyDown(object sender, KeyEventArgs e) {
        if (Items.Count <= 0)
            return;

        // Only handle navigation keys for list browsing
        // Don't handle Left/Right arrows - let them work normally for text cursor movement
        if (e.Key == Key.PageUp) {
            if (lstItems.SelectedIndex >= 10)
                lstItems.SelectedIndex -= 10;
            else
                lstItems.SelectedIndex = 0;
            PreviewSelectedItem();
            e.Handled = true;
        }
        else if (e.Key == Key.PageDown) {
            if (lstItems.SelectedIndex < Items.Count - 10)
                lstItems.SelectedIndex += 10;
            else
                lstItems.SelectedIndex = Items.Count - 1;
            PreviewSelectedItem();
            e.Handled = true;
        }
        else if (e.Key == Key.Up) {
            if (lstItems.SelectedIndex > 0) {
                lstItems.SelectedIndex--;
            }
            PreviewSelectedItem();
            e.Handled = true;
        }
        else if (e.Key == Key.Down) {
            if (lstItems.SelectedIndex < Items.Count - 1) {
                lstItems.SelectedIndex++;
            }
            PreviewSelectedItem();
            e.Handled = true;
        }
        // Note: We intentionally don't handle Left/Right arrows, 
        // Ctrl+Left/Right, Ctrl+Shift+Left/Right, Home, End, etc.
        // These keys will be handled by the TextBox for normal text navigation
    }

    private void Window_PreviewKeyDown(object sender, KeyEventArgs e) {
        // Handle Ctrl+Backspace to clear search
        if (e.Key == Key.Back && Keyboard.Modifiers == ModifierKeys.Control) {
            txtSearch.Text = "";
            e.Handled = true;
        }
    }

    private void GotoItem() {
        var selectedItem = lstItems.SelectedItem as ListItemViewModel;
        selectedItem?.Item.Go();
    }

    private void PreviewSelectedItem() {
        ThreadHelper.ThrowIfNotOnUIThread();
        var selectedItem = lstItems.SelectedItem as ListItemViewModel;
        if (selectedItem?.Item is ListItemCSharp) {
            try {
                selectedItem.Item.Go();
            }
            catch {
                // Ignore preview navigation exceptions
            }
            finally {
                try {
                    txtSearch.Focus();
                }
                catch {
                    // Best-effort to restore focus
                }
            }
        }
    }

    private void lstItems_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
        var item = (e.OriginalSource as FrameworkElement)?.DataContext as ListItemViewModel;
        if (item != null) {
            GotoItem();
            Close();
        }
    }

    private void lstItems_PreviewKeyUp(object sender, KeyEventArgs e) {
        txtSearch.Focus();
        txtSearch.SelectionStart = txtSearch.Text.Length;
    }

    private void Window_Closed(object sender, EventArgs e) {
        ClearData();
    }

    private static Color ToMediaColor(System.Drawing.Color color) {
        return Color.FromArgb(color.A, color.R, color.G, color.B);
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
