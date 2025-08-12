using Microsoft.Internal.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.Imaging.Interop;
using QuickJump2022.Models;
using QuickJump2022.Options;
using QuickJump2022.QuickJump.Tools;
using QuickJump2022.Tools;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Utilities = QuickJump2022.Tools.Utilities;
using Window = System.Windows.Window;

namespace QuickJump2022.Forms {
    /// <summary>
    /// Main search window that runs in VS UI thread with proper theming support
    /// </summary>
    public partial class MainSearchWindow : Window, INotifyPropertyChanged {
        public SearchController SearchController { get; init; }
        private ObservableCollection<ListItemViewModel> _items;
        private GeneralOptionsPage _options;
        private InputCaptureWindow _inputWindow;
        private Thread _inputWindowThread;
        private bool _inputWindowLoaded = false;

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
        public double SearchFontSize => _options.SearchFont.Size * 96.0 / 72.0;
        public string ItemFontFamily => _options.ItemFont.FontFamily.Name;
        public double ItemFontSize => _options.ItemFont.Size * 96.0 / 72.0;

        /// <summary>
        /// Shows the search dialog using the new dual-window architecture
        /// Main window runs in VS UI thread for proper theming
        /// Input capture window runs in separate STA thread for keyboard focus
        /// </summary>
        public static async Task ShowWithDualWindowAsync(SearchController searchController) {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            var mainWindow = new MainSearchWindow(searchController);
            var thisWindow = new WindowInteropHelper(mainWindow);
            thisWindow.Owner = WindowHelper.GetDialogOwnerHandle();
            mainWindow.Show();
        }

        public MainSearchWindow(SearchController searchController) {
            SearchController = searchController;
            _options = QuickJumpData.Instance.GeneralOptions;
            Items = new ObservableCollection<ListItemViewModel>();
            // SetValue(RenderOptions.BitmapScalingModeProperty, BitmapScalingMode.NearestNeighbor);
            // SetValue(RenderOptions.EdgeModeProperty, EdgeMode.Aliased);

            InitializeComponent();
            DataContext = this;

            // TextOptions.SetTextFormattingMode(this, TextFormattingMode.Ideal);
            // TextOptions.SetTextRenderingMode(this, TextRenderingMode.Auto);
            // TextOptions.SetTextHintingMode(this, TextHintingMode.Auto);

            Closed += (s, e) => {
                _inputWindow?.Dispatcher.BeginInvokeShutdown(DispatcherPriority.Normal);
            };
            LocationChanged += (s, e) => UpdateInputWindowPositionThreadSafe();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e) {
            Width = _options.Width;
            var screenWidth = SystemParameters.PrimaryScreenWidth;
            var screenHeight = SystemParameters.PrimaryScreenHeight;
            Left = (screenWidth - Width) / 2 + _options.OffsetLeft;
            Top = (screenHeight - Height) / 2 + _options.OffsetTop;

            try {
                await SearchController.LoadDataAsync();
                RefreshList("");
                if (Items.Count > 0) {
                    lstItems.SelectedIndex = 0;
                }
                CreateInputCaptureWindow();
            }
            catch (Exception ex) {
                System.Windows.MessageBox.Show(ex.ToString());
            }
        }

        private void CreateInputCaptureWindow() {
            var mainHandle = new WindowInteropHelper(this).Handle; // WindowHelper.GetDialogOwnerHandle();

            var left = Left + BorderThickness.Left;
            var top = Top + BorderThickness.Top;
            var width = txtSearchDisplay.ActualWidth;
            var height = txtSearchDisplay.ActualHeight;
            var rect = new Rect(left, top, width, height);

            _inputWindowThread = new Thread(() => {
                try {
                    SynchronizationContext.SetSynchronizationContext(
                        new DispatcherSynchronizationContext(Dispatcher.CurrentDispatcher)
                    );
                    _inputWindow = new InputCaptureWindow(this);
                    _inputWindow.UpdateRectangle(rect);
                    _inputWindow.Show();
                    var child = new WindowInteropHelper(_inputWindow);
                    child.Owner = mainHandle; 
                    Dispatcher.Run();
                }
                catch (Exception ex) {
                    ex.Log();
                    Dispatcher.CurrentDispatcher.BeginInvokeShutdown(DispatcherPriority.Background);
                }
            });
            _inputWindowThread.SetApartmentState(ApartmentState.STA);
            _inputWindowThread.Start();
        }

        private void UpdateInputWindowPositionThreadSafe() {
            if (!_inputWindowLoaded) return;
            var left = Left + BorderThickness.Left;
            var top = Top + BorderThickness.Top;
            var width = txtSearchDisplay.ActualWidth;
            var height = txtSearchDisplay.ActualHeight;
            var rect = new Rect(left, top, width, height);
            _inputWindow.Dispatcher.BeginInvoke(() => _inputWindow.UpdateRectangle(rect));
        }

        // ----------------------------------------------
        // Called from child input window
        // ----------------------------------------------
        public void OnInputWindowLoaded() {
            _inputWindowLoaded = true;
            UpdateInputWindowPositionThreadSafe();
        }

        public void OnInputTextChanged(string text) {
            txtSearchDisplay.Text = text;
            RefreshList(text);
            if (Items.Count > 0) {
                lstItems.SelectedIndex = 0;
            }
        }
        public void OnArrowUp() {
            if (lstItems.SelectedIndex > 0) {
                lstItems.SelectedIndex--;
            }
            GoToItem();
        }
        public void OnArrowDown() {
            if (lstItems.SelectedIndex < Items.Count - 1) {
                lstItems.SelectedIndex++;
            }
            GoToItem();
        }
        public void OnPageUp() { 
            if (lstItems.SelectedIndex >= 10)
                lstItems.SelectedIndex -= 10;
            else
                lstItems.SelectedIndex = 0;
            GoToItem();
        }
        public void OnPageDown() { 
            if (lstItems.SelectedIndex < Items.Count - 10)
                lstItems.SelectedIndex += 10;
            else
                lstItems.SelectedIndex = Items.Count - 1;
            GoToItem();
        }
        public void OnEscapePressed() => Close();
        public void OnEnterPressed() { GoToItem(); Close(); }
        // ----------------------------------------------

        private void RefreshList(string searchText) {
            try {
                Items.Clear();
                var results = SearchController.Search(searchText);
                foreach (var item in results) {
                    var viewModel = new ListItemViewModel(item, _options);
                    Items.Add(viewModel);
                    _ = LoadIconAsync(viewModel, item);
                }
                lblCountValue.Text = Items.Count.ToString();
                var itemHeight = _options.ItemFont.Height + 6;
                Height = Utilities.Clamp(Items.Count * itemHeight + 56, 100, _options.MaxHeight);
            }
            catch (Exception ex) {
                System.Windows.MessageBox.Show(ex.ToString());
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

        private void GoToItem() {
            var selectedItem = lstItems.SelectedItem as ListItemViewModel;
            if (selectedItem != null) {
                var listItem = selectedItem.Item;
                if (listItem is ListItemFile file) {
                    file.ProjectItem.GoToLine(file.Line);
                }
                if (listItem is ListItemSymbol symbol) {
                    symbol.Document.GoToLine(symbol.Line);
                }
            }
        }

        private void lstItems_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            var item = (e.OriginalSource as FrameworkElement)?.DataContext as ListItemViewModel;
            if (item != null) {
                GoToItem();
                Close();
            }
        }

        private static Color ToMediaColor(System.Drawing.Color color) => Color.FromArgb(color.A, color.R, color.G, color.B);
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
