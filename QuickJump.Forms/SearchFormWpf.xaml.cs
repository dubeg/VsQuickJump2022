using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
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
using QuickJump2022.QuickJump.Tools;
using QuickJump2022.Tools;
using Utilities = QuickJump2022.Tools.Utilities;

namespace QuickJump2022.Forms;

public partial class SearchFormWpf : DialogWindow, INotifyPropertyChanged {
    public SearchController SearchController { get; init; }
    private ObservableCollection<ListItemViewModel> _items;
    private GeneralOptionsPage _options;
    public ObservableCollection<ListItemViewModel> Items {
        get => _items;
        set { _items = value; OnPropertyChanged(); }
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

    public static void ShowNonBlockingModal(SearchController searchController) {
        var thread = new System.Threading.Thread(() => {
            try {
                SynchronizationContext.SetSynchronizationContext(
                    new DispatcherSynchronizationContext(Dispatcher.CurrentDispatcher)
                );
                var dialog = new SearchFormWpf(searchController);
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

    public SearchFormWpf(SearchController type) {
        SearchController = type;
        _options = QuickJumpData.Instance.GeneralOptions;
        Items = new ObservableCollection<ListItemViewModel>();
        SetValue(RenderOptions.BitmapScalingModeProperty, BitmapScalingMode.NearestNeighbor);
        SetValue(RenderOptions.EdgeModeProperty, EdgeMode.Aliased);
        InitializeComponent();
        DataContext = this;
        TextOptions.SetTextFormattingMode(this, TextFormattingMode.Ideal);
        TextOptions.SetTextRenderingMode(this, TextRenderingMode.Auto);
        TextOptions.SetTextHintingMode(this, TextHintingMode.Auto);
        Deactivated += (s, e) => {
            //try { Close(); } catch { }
        };
        
    }

    private async void Window_Loaded(object sender, RoutedEventArgs e) {
        Width = _options.Width;
        var screenWidth = SystemParameters.PrimaryScreenWidth;
        var screenHeight = SystemParameters.PrimaryScreenHeight;
        Left = (screenWidth - Width) / 2 + _options.OffsetLeft;
        Top = (screenHeight - Height) / 2 + _options.OffsetTop;
        try {
            await SearchController.LoadDataThreadSafeAsync();
            RefreshList();
            if (Items.Count > 0) {
                lstItems.SelectedIndex = 0;
            }
            await Dispatcher.BeginInvoke(
                new Action(() => {
                    txtSearch.Focus();
                    Keyboard.Focus(txtSearch);
                }), 
                DispatcherPriority.Loaded
            );
        }
        catch (Exception ex) {
            System.Windows.MessageBox.Show(ex.ToString());
        }
    }

    private void RefreshList() {
        try {
            var searchText = txtSearch.Text;
            Items.Clear();
            var results = SearchController.Search(searchText);
            foreach (var item in results) {
                Items.Add(
                    new ListItemViewModel(item, _options)
                );
            }
            lblCountValue.Text = Items.Count.ToString();
            var itemHeight = _options.ItemFont.Height + 6;
            Height = Utilities.Clamp(Items.Count * itemHeight + 56, 100, _options.MaxHeight);
        }
        catch (Exception ex) {
            System.Windows.MessageBox.Show(ex.ToString());
        }
    }

    private void txtSearch_TextChanged(object sender, TextChangedEventArgs e) {
        RefreshList();
        if (Items.Count > 0) {
            lstItems.SelectedIndex = 0;
        }
    }

    private void txtSearch_PreviewKeyDown(object sender, KeyEventArgs e) {
        // Note: We don't handle Left/Right arrows, Ctrl+Shift+Left/Right, etc.
        // so they work normally for text navigation and selection
        if (e.Key == Key.Escape) {
            Close();
            e.Handled = true;
        }
        else if (e.Key == Key.Return) {
            _ = GoToItemAsync(); // Fire and forget for UI responsiveness
            Close();
            e.Handled = true;
        }
        else if (e.Key == Key.A && Keyboard.Modifiers == ModifierKeys.Control) {
            // Handle Ctrl+A to select all text
            txtSearch.SelectAll();
            e.Handled = true;
        }
        else if (e.Key == Key.Up) {
            if (lstItems.SelectedIndex > 0) {
                lstItems.SelectedIndex--;
            }
            _ = GoToItemAsync(); // Fire and forget
            e.Handled = true;
        }
        else if (e.Key == Key.Down) {
            if (lstItems.SelectedIndex < Items.Count - 1) {
                lstItems.SelectedIndex++;
            }
            _ = GoToItemAsync(); // Fire and forget
            e.Handled = true;
        }
        if (e.Key == Key.PageUp) {
            if (lstItems.SelectedIndex >= 10)
                lstItems.SelectedIndex -= 10;
            else
                lstItems.SelectedIndex = 0;
            _ = GoToItemAsync(); // Fire and forget
            e.Handled = true;
        }
        else if (e.Key == Key.PageDown) {
            if (lstItems.SelectedIndex < Items.Count - 10)
                lstItems.SelectedIndex += 10;
            else
                lstItems.SelectedIndex = Items.Count - 1;
            _ = GoToItemAsync(); // Fire and forget
            e.Handled = true;
        }
        else if (e.Key == Key.Back && Keyboard.Modifiers == ModifierKeys.Control) {
            txtSearch.Text = "";
            e.Handled = true;
        }
    }

    private async Task GoToItemAsync() {
        var selectedItem = lstItems.SelectedItem as ListItemViewModel;
        if (selectedItem != null) {
            // Marshal the navigation call to VS UI thread
            await ThreadHelper.JoinableTaskFactory.RunAsync(async delegate {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                var listItem = selectedItem.Item;
                if (listItem is ListItemFile file) { file.ProjectItem.GoToLine(file.Line); }
                if (listItem is ListItemSymbol symbol) { symbol.Document.GoToLine(symbol.Line); }
            });
        }
    }

    private void lstItems_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
        var item = (e.OriginalSource as FrameworkElement)?.DataContext as ListItemViewModel;
        if (item != null) {
            _ = GoToItemAsync(); // Fire and forget
            Close();
        }
    }

    private void lstItems_PreviewKeyUp(object sender, KeyEventArgs e) {
        txtSearch.Focus();
        txtSearch.SelectionStart = txtSearch.Text.Length;
    }

    private static Color ToMediaColor(System.Drawing.Color color) {
        return Color.FromArgb(color.A, color.R, color.G, color.B);
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
