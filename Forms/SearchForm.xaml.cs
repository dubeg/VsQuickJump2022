using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using EnvDTE;
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.Imaging.Interop;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Utilities;
using QuickJump2022.Interop;
using QuickJump2022.Models;
using QuickJump2022.QuickJump.Tools;
using QuickJump2022.Services;
using QuickJump2022.TextEditor;
using QuickJump2022.Tools;
using static QuickJump2022.Models.Enums;
using Rect = System.Windows.Rect;

namespace QuickJump2022.Forms;

public partial class SearchForm : DialogWindow, INotifyPropertyChanged {
    public static readonly DependencyProperty HintFontSizeProperty = DP.Register<SearchForm, double>(nameof(HintFontSize), 0d);
    public static readonly DependencyProperty StatusLeftIconProperty = DP.Register<SearchForm, ImageMoniker>(nameof(StatusLeftIcon), default(ImageMoniker));
    public static readonly DependencyProperty StatusLeftTextProperty = DP.Register<SearchForm, string>(nameof(StatusLeftText), string.Empty);
    public static readonly DependencyProperty StatusLeftIconVisibilityProperty = DP.Register<SearchForm, bool>(nameof(StatusLeftIconVisibility), false);
    public static readonly DependencyProperty StatusRightIconProperty = DP.Register<SearchForm, ImageMoniker>(nameof(StatusRightIcon), default(ImageMoniker));
    public static readonly DependencyProperty StatusRightTextProperty = DP.Register<SearchForm, string>(nameof(StatusRightText), string.Empty);
    public static readonly DependencyProperty StatusRightIconVisibilityProperty = DP.Register<SearchForm, bool>(nameof(StatusRightIconVisibility), false);
    // -----------------
    // Status Bar (Left: Type, Right: Scope)
    // -----------------
    public ImageMoniker StatusLeftIcon { get => (ImageMoniker)GetValue(StatusLeftIconProperty); set => SetValue(StatusLeftIconProperty, value);    }
    public string StatusLeftText { get => (string)GetValue(StatusLeftTextProperty); set => SetValue(StatusLeftTextProperty, value);    }
    public bool StatusLeftIconVisibility { get => (bool)GetValue(StatusLeftIconVisibilityProperty); set => SetValue(StatusLeftIconVisibilityProperty, value);    }
    public ImageMoniker StatusRightIcon { get => (ImageMoniker)GetValue(StatusRightIconProperty); set => SetValue(StatusRightIconProperty, value);    }
    public string StatusRightText { get => (string)GetValue(StatusRightTextProperty); set => SetValue(StatusRightTextProperty, value);    }
    public bool StatusRightIconVisibility { get => (bool)GetValue(StatusRightIconVisibilityProperty); set => SetValue(StatusRightIconVisibilityProperty, value);    }
    // --
    private DismissOnClickOutsideBounds _dismissOnClickOutsideBounds;
    public int PageSize => 20; // TODO: make it configurable
    public double HintFontSize { get => (double)GetValue(HintFontSizeProperty); set => SetValue(HintFontSizeProperty, value); }
    public SearchInstance SearchInstance { get; private set; }
    public GoToService GoToService { get; init; }
    public CommandService CommandService { get; init; }
    public ClassificationService ClassificationService { get; init; }
    public string CurrentText => txtSearch.Text ?? string.Empty;
    public Action<bool, ListItemFile> DebouncedGoToFile { get; }
    private bool _useSymbolColors = false;
    private DTE _dte;
    private List<ListItemViewModel> Items = new();
    private string _initialText = string.Empty;
    private SearchType _searchType;
    private Enums.FileSearchScope _fileScope = Enums.FileSearchScope.Solution;

    public static async Task<string> ShowModalAsync(QuickJumpPackage package, SearchType searchType, string initialText = "", bool enableCommandTabCycle = false) {
        var dialog = new SearchForm(package, searchType, initialText, enableCommandTabCycle);
        await dialog.LoadDataAsync();
        dialog.ShowModal();
        return dialog._resultText ?? dialog.CurrentText;
    }

    private readonly QuickJumpPackage _package;
    private readonly bool _enableCommandTabCycle;

    protected SearchForm(QuickJumpPackage package, SearchType searchType, string initialText = "", bool enableCommandTabCycle = false) {
        var (fontFamily, fontSize) = FontsAndColorsHelper.GetEditorFontInfo(true);
        FontFamily = fontFamily;
        FontSize = fontSize < 14 ? fontSize + 1 : fontSize; // TODO: configure via options (?)
        HintFontSize = Math.Max(8, fontSize - 1);
        // --
        InitializeComponent();
        // --
        // TODO: InputTextEditor doesn't expose FontSize directly
        // --
        _package = package;
        _searchType = searchType;
        _enableCommandTabCycle = enableCommandTabCycle;
        var searchInstance = new SearchInstance(
            package.ProjectFileService,
            package.SymbolService,
            package.CommandService,
            package.KnownCommandService,
            package.FastFetchCommandService,
            searchType,
            _fileScope,
            package.GeneralOptions.FileSortType,
            package.GeneralOptions.CSharpSortType,
            package.GeneralOptions.MixedSortType
        );
        SearchInstance = searchInstance;
        GoToService = package.GoToService;
        CommandService = package.CommandService;
        ClassificationService = package.ClassificationService;
        _dte = package.Dte;
        _useSymbolColors = package.GeneralOptions.UseSymbolColors;
        // --
        WindowStartupLocation = WindowStartupLocation.Manual;
        this.Loaded += (s, e) => AdjustPosition();
        this.SizeChanged += (s, e) => AdjustPosition();
        _dismissOnClickOutsideBounds = new(this);
        // --
        Width = searchType switch {
            SearchType.Files => 800,
            SearchType.Symbols => 800,
            _ => 600
        }; // TODO: configure via options.
        UpdateStatusBar();
        // TODO: check the active document's width & don't make it larger than that.
        _initialText = initialText; // TODO: configure via options.
        if (!string.IsNullOrWhiteSpace(_initialText)) {
            txtSearch.Text = _initialText;
            txtSearch.SelectAll();
        }
        // --
        this.txtSearch.SpecialKeyPressed += (_, args) => txtSearch_HandleKey(this, args);
        this.txtSearch.TextChanged += (_, _) => RefreshList();
        // --
        this.lstItems.PreviewMouseLeftButtonUp += lstItems_PreviewMouseLeftButtonUp;
        this.lstItems.PreviewKeyUp += lstItems_PreviewKeyUp;
        Action<bool, ListItemFile> goToItem = (commit, file) => {
            if (commit) GoToService.GoToFile(file);
            else GoToService.PreviewFileAsync(file);
        };
        DebouncedGoToFile = goToItem.Debounce(TaskScheduler.FromCurrentSynchronizationContext(), 50);
    }

    private string _resultText;

    private async Task SwitchCommandSearchTypeAsync(SearchType newType) {
        if (newType != SearchType.Commands && newType != SearchType.KnownCommands && newType != SearchType.FastFetchCommands) return;
        _searchType = newType;
        UpdateStatusBar();
        SearchInstance = new SearchInstance(
            _package.ProjectFileService,
            _package.SymbolService,
            _package.CommandService,
            _package.KnownCommandService,
            _package.FastFetchCommandService,
            newType,
            _fileScope,
            _package.GeneralOptions.FileSortType,
            _package.GeneralOptions.CSharpSortType,
            _package.GeneralOptions.MixedSortType
        );
        await SearchInstance.LoadDataAsync();
        RefreshList();
        if (Items.Count > 0) {
            lstItems.SelectedIndex = 0;
            EnsureSelectedItemIsVisible();
        }
    }

    private async Task SwitchFileScopeAsync(Enums.FileSearchScope newScope) {
        if (_searchType != SearchType.Files) return;
        _fileScope = newScope;
        UpdateStatusBar();
        SearchInstance = new SearchInstance(
            _package.ProjectFileService,
            _package.SymbolService,
            _package.CommandService,
            _package.KnownCommandService,
            _package.FastFetchCommandService,
            _searchType,
            _fileScope,
            _package.GeneralOptions.FileSortType,
            _package.GeneralOptions.CSharpSortType,
            _package.GeneralOptions.MixedSortType
        );
        await SearchInstance.LoadDataAsync();
        RefreshList();
        if (Items.Count > 0) {
            lstItems.SelectedIndex = 0;
            EnsureSelectedItemIsVisible();
        }
    }

    private void SuspendAndClose() {
        txtSearch.SuspendProcessing();
        Close();
    }

    private void AdjustPosition() {
        ThreadHelper.ThrowIfNotOnUIThread();
        var vsWindowRect = new Rect();
        var vsWindow = _dte.MainWindow;
        vsWindowRect = vsWindow.WindowState == EnvDTE.vsWindowState.vsWindowStateMaximize
            ? WindowUtils.GetMaximizedWindowBounds(vsWindow.HWnd)
            : new Rect(vsWindow.Left, vsWindow.Top, vsWindow.Width, vsWindow.Height);
        var activeDocWindow = _dte.ActiveDocument?.ActiveWindow;
        if (activeDocWindow is not null) {
            vsWindowRect = new Rect(activeDocWindow.Left, activeDocWindow.Top, activeDocWindow.Width, activeDocWindow.Height);
        }
        vsWindowRect = Application.Current.MainWindow.DeviceToLogicalRect(vsWindowRect);
        this.Top = vsWindowRect.Top + 75; // TODO: make configurable in options
        this.Left = vsWindowRect.Left
            + (vsWindowRect.Width / 2.0)
            - (ActualWidth / 2.0);
    }

    public async Task LoadDataAsync() {
        await SearchInstance.LoadDataAsync();
        RefreshList();
        if (Items.Count > 0) {
            lstItems.SelectedIndex = 0;
            EnsureSelectedItemIsVisible();
        }
    }

    private void RefreshList() {
        var searchText = txtSearch.Text;
        var listItems = new List<ListItemViewModel>();
        var results = SearchInstance.Search(searchText);
        var brush = Application.Current.TryFindResource(ThemedDialogColors.ListBoxTextBrushKey) as System.Windows.Media.Brush;
        foreach (var item in results) {
            var viewModel = new ListItemViewModel(item);
            if (item is ListItemSymbol symbol && _useSymbolColors) {
                viewModel.NameForeground = ClassificationService.GetFgColorForClassification(symbol.Item.BindType);
                viewModel.NameParametersForeground = ClassificationService.GetFgColorForClassification(Enums.TokenType.ParameterName);
                viewModel.NamePunctuationMarksForeground = ClassificationService.GetFgColorForClassification(Enums.TokenType.Text);
            }
            else {
                viewModel.NameForeground = brush;
                viewModel.NameParametersForeground = brush;
                viewModel.NamePunctuationMarksForeground = brush;
            }
            listItems.Add(viewModel);
        }
        lstItems.ItemsSource = Items = listItems;
        if (Items.Count >= 1) {
            lstItems.SelectedIndex = 0;
        }
    }

    private async void txtSearch_HandleKey(object sender, VsKeyInfo e) {
        // Note: We don't handle Left/Right arrows, Ctrl+Shift+Left/Right, etc.
        // so they work normally for text navigation and selection
        if (e.Key == Key.Tab) {
            if (_searchType == SearchType.Files) {
                var reverse = e.ShiftPressed;
                var nextScope = (_fileScope, reverse) switch {
                    (Enums.FileSearchScope.Solution, false) => Enums.FileSearchScope.ActiveProject,
                    (Enums.FileSearchScope.Solution, true) => Enums.FileSearchScope.ActiveProject,
                    (Enums.FileSearchScope.ActiveProject, false) => Enums.FileSearchScope.Solution,
                    (Enums.FileSearchScope.ActiveProject, true) => Enums.FileSearchScope.Solution,
                    _ => _fileScope
                };
                await SwitchFileScopeAsync(nextScope);
            }
            // Cycle only among command search modes, and only if enabled by the invoker
            else if (_enableCommandTabCycle && (_searchType == SearchType.Commands || _searchType == SearchType.KnownCommands || _searchType == SearchType.FastFetchCommands)) {
                var reverse = e.ShiftPressed;
                var nextType = (_searchType, reverse) switch {
                    (SearchType.Commands, false) => SearchType.KnownCommands,
                    (SearchType.Commands, true) => SearchType.FastFetchCommands,
                    (SearchType.KnownCommands, false) => SearchType.FastFetchCommands,
                    (SearchType.KnownCommands, true) => SearchType.Commands,
                    (SearchType.FastFetchCommands, false) => SearchType.Commands,
                    (SearchType.FastFetchCommands, true) => SearchType.KnownCommands,
                    _ => _searchType
                };
                await SwitchCommandSearchTypeAsync(nextType);
            }
        }
        else if (e.Key == Key.Escape) {
            SuspendAndClose();
        }
        else if (e.Key == Key.Return) {
            await GoToItem(true);
        }
        else if (e.Key == Key.Up) {
            if (lstItems.SelectedIndex > 0) {
                lstItems.SelectedIndex--;
                EnsureSelectedItemIsVisible();
            }
            GoToItem();
        }
        else if (e.Key == Key.Down) {
            if (lstItems.SelectedIndex < Items.Count - 1) {
                lstItems.SelectedIndex++;
                EnsureSelectedItemIsVisible();
            }
            GoToItem();
        }
        if (e.Key == Key.PageUp) {
            if (lstItems.SelectedIndex >= PageSize)
                lstItems.SelectedIndex -= PageSize;
            else
                lstItems.SelectedIndex = 0;
            EnsureSelectedItemIsVisible();
            await GoToItem();
        }
        else if (e.Key == Key.PageDown) {
            if (lstItems.SelectedIndex < Items.Count - PageSize)
                lstItems.SelectedIndex += PageSize;
            else
                lstItems.SelectedIndex = Items.Count - 1;
            EnsureSelectedItemIsVisible();
            await GoToItem();
        }
    }

    /// <summary>
    /// Go to selected item.
    /// </summary>
    /// <param name="commit">
    /// False => Go to item without closing the dialog.
    /// True => Go to item and close the dialog.
    /// If the item is a command, it will be executed only on commit.
    /// If the item is a file or symbol, it will always be navigated to.
    /// </param>
    /// <returns></returns>
    private async Task GoToItem(bool commit = false) {
        var selectedItem = lstItems.SelectedItem as ListItemViewModel;
        if (selectedItem != null) {
            if (commit) SuspendAndClose();
            var listItem = selectedItem.Item;
            if (listItem is ListItemFile file) DebouncedGoToFile(commit, file);
            else if (listItem is ListItemSymbol symbol) Dispatcher.BeginInvoke(() => GoToService.GoToSymbolAsync(symbol));
            else if (listItem is ListItemCommand command) {
                if (commit) {
                    _resultText = command.Name;
                    // The dialog must be closed before executing a command
                    // in case the command opens another modal dialog.
                    Dispatcher.BeginInvoke(() => CommandService.Execute(command.Item));
                    return;
                }
            }
            else if (listItem is ListItemKnownCommand knownCommand) {
                if (commit) {
                    _resultText = knownCommand.Name;
                    // The dialog must be closed before executing a command bar button
                    // in case it opens another modal dialog.
                    Dispatcher.BeginInvoke(() => CommandService.Execute(knownCommand.Item.Command));
                    return;
                }
            }
            else if (listItem is ListItemFastFetchCommand fastFetch) {
                if (commit) {
                    _resultText = fastFetch.Name;
                    Dispatcher.BeginInvoke(() => CommandService.Execute(fastFetch.Item.CommandID));
                    return;
                }
            }
        }
    }



    private void EnsureSelectedItemIsVisible() {
        if (lstItems.SelectedItem != null) {
            lstItems.ScrollIntoView(lstItems.SelectedItem);
        }
    }

    private async void lstItems_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
        var item = (e.OriginalSource as FrameworkElement)?.DataContext as ListItemViewModel;
        if (item != null) {
            await GoToItem(true);
        }
    }

    private void lstItems_PreviewKeyUp(object sender, KeyEventArgs e) {
        var editor = txtSearch as InputTextEditor;
        if (editor != null) {
            //editor.Focus();
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    // --------------------------------------
    // Status Bar helpers
    // --------------------------------------
    private void UpdateStatusBar() {
        (StatusLeftText, StatusLeftIcon) = GetSearchTypeDisplay();
        (StatusRightText, StatusRightIcon) = GetScopeDisplay();
        StatusLeftIconVisibility = StatusLeftIcon.Id != KnownMonikers.None.Id;
        StatusRightIconVisibility = StatusRightIcon.Id != KnownMonikers.None.Id;
    }

    private (string text, ImageMoniker moniker) GetSearchTypeDisplay() {
        switch (_searchType) {
            case SearchType.Files: return ("Files", KnownMonikers.Document);
            case SearchType.Symbols: return ("Symbols", KnownMonikers.CodeDefinitionWindow);
            case SearchType.Commands:
            case SearchType.KnownCommands:
            case SearchType.FastFetchCommands: return ("Commands", KnownMonikers.Settings);
            case SearchType.All: return ("All", KnownMonikers.Search);
            default: return (string.Empty, KnownMonikers.None);
        }
    }

    private (string text, ImageMoniker moniker) GetScopeDisplay() {
        switch (_searchType) {
            case SearchType.Files:
                return _fileScope == Enums.FileSearchScope.ActiveProject
                    ? ("Active Project", KnownMonikers.CSProjectNode)
                    : ("Solution", KnownMonikers.Solution);
            case SearchType.Symbols: return ("Document", KnownMonikers.Document);
            case SearchType.Commands: return ("Canonical Name", KnownMonikers.None);
            case SearchType.KnownCommands: return ("Custom Name", KnownMonikers.None);
            case SearchType.FastFetchCommands: return ("Friendly Name", KnownMonikers.None);
            case SearchType.All:
            default: return (string.Empty, KnownMonikers.None);
        }
    }
}
