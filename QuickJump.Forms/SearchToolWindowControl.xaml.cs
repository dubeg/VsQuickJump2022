
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.Imaging.Interop;
using Microsoft.VisualStudio.Shell.Interop;
using QuickJump2022.Models;
using QuickJump2022.Options;
using QuickJump2022.QuickJump.Tools;
using QuickJump2022.Tools;
using Utilities = QuickJump2022.Tools.Utilities;

namespace QuickJump2022.Forms;

/// <summary>
/// Interaction logic for SearchToolWindowControl.
/// </summary>
public partial class SearchToolWindowControl : UserControl, INotifyPropertyChanged {
    private SearchController _searchController;
    private ObservableCollection<ListItemViewModel> _items;
    private GeneralOptionsPage _options;
    private bool _isInitialized = false;

    public ObservableCollection<ListItemViewModel> Items {
        get => _items;
        set { _items = value; OnPropertyChanged(); }
    }

    // Binding properties for UI
    public Brush BorderColor => new SolidColorBrush(ToMediaColor(_options?.BorderColor ?? System.Drawing.Color.CornflowerBlue));
    public Brush BackgroundColor => new SolidColorBrush(Colors.Black);
    public Brush StatusBackgroundColor => new SolidColorBrush(ToMediaColor(_options?.StatusBackgroundColor ?? System.Drawing.Color.DimGray));
    public bool ShowStatusBar => _options?.ShowStatusBar ?? true;
    public string SearchFontFamily => _options?.SearchFont.FontFamily.Name ?? "Consolas";
    public double SearchFontSize => (_options?.SearchFont.Size ?? 12) * 96.0 / 72.0;
    public string ItemFontFamily => _options?.ItemFont.FontFamily.Name ?? "Consolas";
    public double ItemFontSize => (_options?.ItemFont.Size ?? 12) * 96.0 / 72.0;

    /// <summary>
    /// Initializes a new instance of the <see cref="SearchToolWindowControl"/> class.
    /// </summary>
    public SearchToolWindowControl() {
        Items = new ObservableCollection<ListItemViewModel>();
        InitializeComponent();
        DataContext = this;
    }

    /// <summary>
    /// Initialize the control with a SearchController
    /// </summary>
    public async Task InitializeAsync(SearchController searchController) {
        _searchController = searchController;
        _options = QuickJumpData.Instance.GeneralOptions;
        _isInitialized = true;

        // Force property change notifications for binding
        OnPropertyChanged(nameof(BorderColor));
        OnPropertyChanged(nameof(BackgroundColor));
        OnPropertyChanged(nameof(StatusBackgroundColor));
        OnPropertyChanged(nameof(ShowStatusBar));
        OnPropertyChanged(nameof(SearchFontFamily));
        OnPropertyChanged(nameof(SearchFontSize));
        OnPropertyChanged(nameof(ItemFontFamily));
        OnPropertyChanged(nameof(ItemFontSize));

        try {
            await _searchController.LoadDataAsync();
            RefreshList("");
            if (Items.Count > 0) {
                lstItems.SelectedIndex = 0;
            }
            lblSolutionName.Text = _searchController.SolutionName;
        }
        catch (Exception ex) {
            await VS.MessageBox.ShowErrorAsync("QuickJump Error", ex.ToString());
        }
    }

    private async void UserControl_Loaded(object sender, RoutedEventArgs e) {
        // Focus the search textbox when the control loads
        txtSearch.Focus();
        Keyboard.Focus(txtSearch);
    }

    private void txtSearch_TextChanged(object sender, TextChangedEventArgs e) {
        if (!_isInitialized) return;
        
        var searchText = txtSearch.Text.Trim();
        RefreshList(searchText);
        if (Items.Count > 0) {
            lstItems.SelectedIndex = 0;
        }
    }

    private async void txtSearch_PreviewKeyDown(object sender, KeyEventArgs e) {
        switch (e.Key) {
            case Key.Escape:
                await HideAsync();
                e.Handled = true;
                break;

            case Key.Return:
                GoToItem(commit: true);
                await HideAsync();
                e.Handled = true;
                break;

            case Key.Up:
                if (lstItems.SelectedIndex > 0) {
                    lstItems.SelectedIndex--;
                }
                GoToItem();
                e.Handled = true;
                break;

            case Key.Down:
                if (lstItems.SelectedIndex < Items.Count - 1) {
                    lstItems.SelectedIndex++;
                }
                GoToItem();
                e.Handled = true;
                break;

            case Key.PageUp:
                if (lstItems.SelectedIndex >= 10)
                    lstItems.SelectedIndex -= 10;
                else
                    lstItems.SelectedIndex = 0;
                GoToItem();
                e.Handled = true;
                break;

            case Key.PageDown:
                if (lstItems.SelectedIndex < Items.Count - 10)
                    lstItems.SelectedIndex += 10;
                else
                    lstItems.SelectedIndex = Items.Count - 1;
                GoToItem();
                e.Handled = true;
                break;

            case Key.A when Keyboard.Modifiers == ModifierKeys.Control:
                txtSearch.SelectAll();
                e.Handled = true;
                break;

            case Key.Back when Keyboard.Modifiers == ModifierKeys.Control:
                txtSearch.Clear();
                e.Handled = true;
                break;
        }
    }

    private async Task HideAsync() => await SearchToolWindow.HideAsync();

    private void RefreshList(string searchText) {
        if (_searchController == null) return;

        try {
            Items.Clear();
            var results = _searchController.Search(searchText);
            foreach (var item in results) {
                var viewModel = new ListItemViewModel(item, _options);
                Items.Add(viewModel);
                _ = LoadIconAsync(viewModel, item);
            }
            lblCountValue.Text = Items.Count.ToString();
        }
        catch (Exception ex) {
            VS.MessageBox.ShowError("QuickJump Error", ex.ToString());
        }
    }

    private async Task LoadIconAsync(ListItemViewModel viewModel, ListItemBase item) {
        var moniker = KnownMonikers.None;
        if (item is ListItemFile fileItem) {
            var extension = System.IO.Path.GetExtension(fileItem.FullPath);
            moniker = KnownMonikerService.GetFileMoniker(extension);
        }
        else if (item is ListItemSymbol symbolItem) {
            moniker = KnownMonikerService.GetCodeMoniker(symbolItem.BindType);
        }
        viewModel.UpdateIcon(moniker);
    }

    private void GoToItem(bool commit = false) {
        var selectedItem = lstItems.SelectedItem as ListItemViewModel;
        if (selectedItem != null) {
            var listItem = selectedItem.Item;
            if (listItem is ListItemFile file) {
                file.ProjectItem.GoToLine(file.Line, commit);
            }
            else if (listItem is ListItemSymbol symbol) {
                symbol.Document.GoToLine(symbol.Line, commit);
            }
        }
    }

    private void lstItems_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
        var item = (e.OriginalSource as FrameworkElement)?.DataContext as ListItemViewModel;
        if (item != null) {
            GoToItem();
        }
    }

    private static Color ToMediaColor(System.Drawing.Color color) => Color.FromArgb(color.A, color.R, color.G, color.B);
    
    public event PropertyChangedEventHandler PropertyChanged;
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}