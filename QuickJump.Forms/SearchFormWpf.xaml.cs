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
using System.Windows.Shapes;
using System.Windows.Threading;
using EnvDTE;
using Microsoft.Internal.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Utilities;
using QuickJump2022.Data;
using QuickJump2022.Models;
using QuickJump2022.Options;
using QuickJump2022.QuickJump.Tools;
using QuickJump2022.Tools;
using Utilities = QuickJump2022.Tools.Utilities;
using Window = System.Windows.Window;
using Rect = System.Windows.Rect;

namespace QuickJump2022.Forms;

public partial class SearchFormWpf : Window, INotifyPropertyChanged {
    public int PageSize => 20; // TODO: make it configurable
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
        var vsWindowHandle = IntPtr.Zero;
        var vsWindowRect = new Rect();
        ThreadHelper.JoinableTaskFactory.Run(async delegate {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            vsWindowHandle = new WindowInteropHelper(Application.Current.MainWindow).Handle;
            var vsWindow = QuickJumpData.Instance.Dte.MainWindow;
            vsWindowRect = new Rect(vsWindow.Left, vsWindow.Top, vsWindow.Width, vsWindow.Height);
            var activeDocWindow = QuickJumpData.Instance.Dte.ActiveDocument?.ActiveWindow;
            if (activeDocWindow is not null) {

                vsWindowRect = new Rect(activeDocWindow.Left, activeDocWindow.Top, activeDocWindow.Width, activeDocWindow.Height);
                vsWindowRect = Application.Current.MainWindow.DeviceToLogicalRect(vsWindowRect);
            }
        });

        var thread = new System.Threading.Thread(() => {
            try {
                SynchronizationContext.SetSynchronizationContext(
                    new DispatcherSynchronizationContext(Dispatcher.CurrentDispatcher)
                );
                var dialog = new SearchFormWpf(searchController);
                
                // Set VS main window as owner
                if (vsWindowHandle != IntPtr.Zero) {
                    var interopHelper = new WindowInteropHelper(dialog);
                    interopHelper.Owner = vsWindowHandle;
                }
                
                dialog.Closed += (s, e) => Dispatcher.CurrentDispatcher.BeginInvokeShutdown(DispatcherPriority.Background);
                dialog.WindowStartupLocation = WindowStartupLocation.Manual;
                dialog.Top = vsWindowRect.Y + 100; // TODO: use y-offset from options
                dialog.Left = vsWindowRect.X + (vsWindowRect.Width / 2) - (dialog.Width / 2);
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
    }

    private async void Window_Loaded(object sender, RoutedEventArgs e) {
        try {
            await SearchController.LoadDataThreadSafeAsync();
            RefreshList();
            if (Items.Count > 0) {
                lstItems.SelectedIndex = 0;
                EnsureSelectedItemIsVisible();
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
                var viewModel = new ListItemViewModel(item, _options);
                Items.Add(viewModel);
                _ = LoadIconAsync(viewModel, item);
            }
        }
        catch (Exception ex) {
            System.Windows.MessageBox.Show(ex.ToString());
        }
    }

    private async Task LoadIconAsync(ListItemViewModel viewModel, ListItemBase item) {
        try {
            var monikerService = QuickJumpData.Instance.MonikerService;
            if (monikerService == null) return;

            BitmapSource iconBitmap = null;
            if (item is ListItemFile fileItem) {
                var extension = System.IO.Path.GetExtension(fileItem.FullPath);
                iconBitmap = await monikerService.GetFileIconAsync(extension);
            }
            else if (item is ListItemSymbol symbolItem) {
                iconBitmap = await monikerService.GetCodeIconAsync(symbolItem.BindType);
            }

            if (iconBitmap != null) {
                // Update the icon on the UI thread
                await Dispatcher.InvokeAsync(() => {
                    viewModel.UpdateIcon(iconBitmap);
                });
            }
        }
        catch (Exception ex) {
            // Log but don't crash on icon loading errors
            ex.Log();
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
            _ = GoToItemAsync(true); // Fire and forget for UI responsiveness
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
            _ = GoToItemAsync(); // Fire and forget
            e.Handled = true;
        }
        else if (e.Key == Key.Down) {
            if (lstItems.SelectedIndex < Items.Count - 1) {
                lstItems.SelectedIndex++;
                EnsureSelectedItemIsVisible();
            }
            _ = GoToItemAsync(); // Fire and forget
            e.Handled = true;
        }
        if (e.Key == Key.PageUp) {
            if (lstItems.SelectedIndex >= PageSize)
                lstItems.SelectedIndex -= PageSize;
            else
                lstItems.SelectedIndex = 0;
            EnsureSelectedItemIsVisible();
            _ = GoToItemAsync(); // Fire and forget
            e.Handled = true;
        }
        else if (e.Key == Key.PageDown) {
            if (lstItems.SelectedIndex < Items.Count - PageSize)
                lstItems.SelectedIndex += PageSize;
            else
                lstItems.SelectedIndex = Items.Count - 1;
            EnsureSelectedItemIsVisible();
            _ = GoToItemAsync(); // Fire and forget
            e.Handled = true;
        }
        else if (e.Key == Key.Back && Keyboard.Modifiers == ModifierKeys.Control) {
            txtSearch.Text = "";
            e.Handled = true;
        }
    }

    private async Task GoToItemAsync(bool commit = false) {
        var selectedItem = lstItems.SelectedItem as ListItemViewModel;
        if (selectedItem != null) {
            // Marshal the navigation call to VS UI thread
            await ThreadHelper.JoinableTaskFactory.RunAsync(async delegate {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                var listItem = selectedItem.Item;
                if (listItem is ListItemFile file) { file.ProjectItem.GoToLine(file.Line, commit); }
                if (listItem is ListItemSymbol symbol) { symbol.Document.GoToLine(symbol.Line); }
            });
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
            _ = GoToItemAsync(true); // Fire and forget
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
}
