using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Utilities;
using QuickJump2022.Models;
using QuickJump2022.Options;
using QuickJump2022.QuickJump.Tools;
using QuickJump2022.Tools;
using Rect = System.Windows.Rect;

namespace QuickJump2022.Forms;

public partial class SearchFormWpf : DialogWindow, INotifyPropertyChanged {
    public int PageSize => 20; // TODO: make it configurable
    public SearchController SearchController { get; init; }
    private ObservableCollection<ListItemViewModel> _items;
    private GeneralOptionsPage _options;
    public ObservableCollection<ListItemViewModel> Items {
        get => _items;
        set { _items = value; OnPropertyChanged(); }
    }

    public static void ShowModal(SearchController searchController) {
        ThreadHelper.ThrowIfNotOnUIThread();
        var vsWindowRect = new Rect();
        var vsWindow = QuickJumpData.Instance.Dte.MainWindow;
        vsWindowRect = vsWindow.WindowState == EnvDTE.vsWindowState.vsWindowStateMaximize 
            ? WindowUtils.GetMaximizedWindowBounds(vsWindow.HWnd)
            : new Rect(vsWindow.Left, vsWindow.Top, vsWindow.Width, vsWindow.Height);
        var activeDocWindow = QuickJumpData.Instance.Dte.ActiveDocument?.ActiveWindow;
        if (activeDocWindow is not null) {
            vsWindowRect = new Rect(activeDocWindow.Left, activeDocWindow.Top, activeDocWindow.Width, activeDocWindow.Height);
        }
        vsWindowRect = Application.Current.MainWindow.DeviceToLogicalRect(vsWindowRect);
        var dialog = new SearchFormWpf(searchController);
        dialog.WindowStartupLocation = WindowStartupLocation.Manual;
        dialog.Top = vsWindowRect.Y + 100; // TODO: use y-offset from options
        dialog.Left = vsWindowRect.X + (vsWindowRect.Width / 2) - (dialog.Width / 2);
        dialog.ShowModal();
    }

    public SearchFormWpf(SearchController type) {
        SearchController = type;
        _options = QuickJumpData.Instance.GeneralOptions;
        Items = new ObservableCollection<ListItemViewModel>();
        InitializeComponent();
        // --
        var (fontFamily, fontSize) = FontsAndColorsHelper.GetEditorFontInfo(true);
        FontFamily = fontFamily;
        FontSize = fontSize + 2; // TODO: configure via options?
    }

    private async void Window_Loaded(object sender, RoutedEventArgs e) {
        try {
            await SearchController.LoadDataThreadSafeAsync();
            RefreshList();
            if (Items.Count > 0) {
                lstItems.SelectedIndex = 0;
                EnsureSelectedItemIsVisible();
            }
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
                // TODO:
                // Use ClassificationHelper.GetClassificationFormat( eg. "Comment")
                // to color a symbol item according to its defined color in texteditor.
                // Alternatively, use FontsAndColorsHelper.GetTextEditorInfos() -> Plain text, Comment, etc.
                var viewModel = new ListItemViewModel(item, _options);
                Items.Add(viewModel);
            }
        }
        catch (Exception ex) {
            System.Windows.MessageBox.Show(ex.ToString());
        }
    }

    private void txtSearch_TextChanged(object sender, TextChangedEventArgs e) {
        RefreshList();
        if (Items.Count > 0) {
            lstItems.SelectedIndex = 0;
            EnsureSelectedItemIsVisible();
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
            GoToItem(true); // Fire and forget for UI responsiveness
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
                EnsureSelectedItemIsVisible();
            }
            GoToItem();
            e.Handled = true;
        }
        else if (e.Key == Key.Down) {
            if (lstItems.SelectedIndex < Items.Count - 1) {
                lstItems.SelectedIndex++;
                EnsureSelectedItemIsVisible();
            }
            GoToItem();
            e.Handled = true;
        }
        if (e.Key == Key.PageUp) {
            if (lstItems.SelectedIndex >= PageSize)
                lstItems.SelectedIndex -= PageSize;
            else
                lstItems.SelectedIndex = 0;
            EnsureSelectedItemIsVisible();
            GoToItem();
            e.Handled = true;
        }
        else if (e.Key == Key.PageDown) {
            if (lstItems.SelectedIndex < Items.Count - PageSize)
                lstItems.SelectedIndex += PageSize;
            else
                lstItems.SelectedIndex = Items.Count - 1;
            EnsureSelectedItemIsVisible();
            GoToItem();
            e.Handled = true;
        }
        else if (e.Key == Key.Back && Keyboard.Modifiers == ModifierKeys.Control) {
            txtSearch.Text = "";
            e.Handled = true;
        }
    }

    private void GoToItem(bool commit = false) {
        var selectedItem = lstItems.SelectedItem as ListItemViewModel;
        if (selectedItem != null) {
            var listItem = selectedItem.Item;
            if (listItem is ListItemFile file) { file.ProjectItem.GoToLine(file.Line, commit); }
            if (listItem is ListItemSymbol symbol) { symbol.Document.GoToLine(symbol.Line); }
        }
    }

    private void EnsureSelectedItemIsVisible() {
        if (lstItems.SelectedItem != null) {
            lstItems.ScrollIntoView(lstItems.SelectedItem);
        }
    }

    private void lstItems_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
        var item = (e.OriginalSource as FrameworkElement)?.DataContext as ListItemViewModel;
        if (item != null) {
            GoToItem(true); // Fire and forget
            Close();
        }
    }

    private void lstItems_PreviewKeyUp(object sender, KeyEventArgs e) {
        txtSearch.Focus();
        txtSearch.SelectionStart = txtSearch.Text.Length;
    }

    private static Color ToMediaColor(System.Drawing.Color color) => Color.FromArgb(color.A, color.R, color.G, color.B);
    public event PropertyChangedEventHandler PropertyChanged;
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    private void DialogWindow_Deactivated(object sender, EventArgs e) {
        try { this.Close(); } catch { }
    }
}
