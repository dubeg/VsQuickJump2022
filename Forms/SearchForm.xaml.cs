using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Threading;
using EnvDTE;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;
using QuickJump2022.Interop;
using QuickJump2022.Models;
using QuickJump2022.QuickJump.Tools;
using QuickJump2022.Services;
using QuickJump2022.Text;
using QuickJump2022.TextEditor;
using QuickJump2022.Tools;
using static QuickJump2022.Models.Enums;
using Rect = System.Windows.Rect;

namespace QuickJump2022.Forms;

public partial class SearchForm : DialogWindow, INotifyPropertyChanged {
    private DismissOnClickOutsideBounds _dismissOnClickOutsideBounds;
    public int PageSize => 20; // TODO: make it configurable
    public static readonly DependencyProperty HintFontSizeProperty = DependencyProperty.Register(
        nameof(HintFontSize), typeof(double), typeof(SearchForm), new PropertyMetadata(0d)
    );
    public double HintFontSize { get => (double)GetValue(HintFontSizeProperty); set => SetValue(HintFontSizeProperty, value); }
    public SearchInstance SearchInstance { get; init; }
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
    private string _lastSearchText = string.Empty;

    public static async Task<string> ShowModalAsync(QuickJumpPackage package, SearchType searchType, string initialText = "") {
        var dialog = new SearchForm(package, searchType, initialText);
        await dialog.LoadDataAsync();
        dialog.ShowModal();
        return dialog.CurrentText;
    }

    protected SearchForm(QuickJumpPackage package, SearchType searchType, string initialText = "") {
        var (fontFamily, fontSize) = FontsAndColorsHelper.GetEditorFontInfo(true);
        FontFamily = fontFamily;
        FontSize = fontSize < 14 ? fontSize + 1 : fontSize; // TODO: configure via options (?)
        HintFontSize = Math.Max(8, fontSize - 1);
        // --
        InitializeComponent();
        // --
        // TODO: InputTextEditor doesn't expose FontSize directly
        // --
        _searchType = searchType;
        var searchInstance = new SearchInstance(
            package.ProjectFileService,
            package.SymbolService,
            package.CommandService,
            package.KnownCommandService,
            package.FastFetchCommandService,
            searchType,
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
        // TODO: check the active document's width & don't make it larger than that.
        _initialText = initialText; // TODO: configure via options.
        if (!string.IsNullOrWhiteSpace(_initialText)) {
            txtSearch.Text = _initialText;
            txtSearch.SelectAll();
        }
        // --
        // TODO: implement TextChanged in InputTextEditor
        //this.txtSearch.ArrowUp += 
        //this.txtSearch.ArrowUp += 
        //this.txtSearch.PageUp += 
        //this.txtSearch.PageTab += 
        //this.txtSearch.Tab += 
        //this.txtSearch.ShiftTab += 
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

    record KeyEventArgs2(
        KeyboardDevice KeyboardDevice,
        Key Key
    );

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
            var reverse = e.ShiftPressed;
            Close();
            var dict = new Dictionary<SearchType, (int backward, int forward)>() {
                { SearchType.Files, (backword: PackageIds.ShowFastFetchCommandSearchForm, forward: PackageIds.ShowMethodSearchForm)},
                { SearchType.Symbols, (backword: PackageIds.ShowFileSearchForm, forward: PackageIds.ShowCommandSearchForm)},
                { SearchType.Commands, (backword: PackageIds.ShowMethodSearchForm, forward: PackageIds.ShowKnownCommandSearchForm)},
                { SearchType.KnownCommands, (backword: PackageIds.ShowCommandSearchForm, forward: PackageIds.ShowFastFetchCommandSearchForm)},
                { SearchType.FastFetchCommands, (backword: PackageIds.ShowKnownCommandSearchForm, forward: PackageIds.ShowFileSearchForm)},
            };
            if (dict.TryGetValue(_searchType, out var cmds)) {
                Dispatcher.BeginInvoke(() => 
                    CommandService.Execute(new CommandID(PackageGuids.QuickJump2022, reverse ? cmds.backward : cmds.forward))
                );
            }
        }
        else if (e.Key == Key.Escape) {
            Close();
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
            if (commit) Close();
            var listItem = selectedItem.Item;
            if (listItem is ListItemFile file) DebouncedGoToFile(commit, file);
            else if (listItem is ListItemSymbol symbol) await GoToService.GoToSymbolAsync(symbol);
            else if (listItem is ListItemCommand command) {
                if (commit) {
                    // The dialog must be closed before executing a command
                    // in case the command opens another modal dialog.
                    CommandService.Execute(command.Item);
                    return;
                }
            }
            else if (listItem is ListItemKnownCommand knownCommand) {
                if (commit) {
                    // The dialog must be closed before executing a command bar button
                    // in case it opens another modal dialog.
                    CommandService.Execute(knownCommand.Item.Command);
                    return;
                }
            }
            else if (listItem is ListItemFastFetchCommand fastFetch) {
                if (commit) {
                    CommandService.Execute(fastFetch.Item.CommandID);
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

    protected override void OnClosed(EventArgs e) {
        base.OnClosed(e);
        if (txtSearch != null) {
            // TODO: 
            // txtSearch.EscapePressed -= txtSearch_EscapePressed;
            // txtSearch.EnterPressed -= txtSearch_EnterPressed;
            // txtSearch.PreviewKeyDown -= txtSearch_PreviewKeyDown;
        }
    }
}
