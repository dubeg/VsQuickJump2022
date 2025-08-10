using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.Shell;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using QuickJump2022.Data;
using QuickJump2022.Tools;

namespace QuickJump2022.Forms;

public class SearchToolWindow : BaseToolWindow<SearchToolWindow> {
    private static SearchToolWindowControl _control;
    private static SearchController _searchController;
    private static Enums.ESearchType _searchType = Enums.ESearchType.All;

    public override string GetTitle(int toolWindowId) => "QuickJump Search";

    public override Type PaneType => typeof(Pane);

    public override async Task<FrameworkElement> CreateAsync(int toolWindowId, CancellationToken cancellationToken) {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
        _control = new SearchToolWindowControl();
        var package = await VS.GetServiceAsync<QuickJump2022Package, QuickJump2022Package>();
        _searchController = new SearchController(package, _searchType);
        await _control.InitializeAsync(_searchController);
        return _control;
    }

    [Guid("d3b3ebd9-87d1-41cd-bf84-268d88953417")]
    internal class Pane : ToolWindowPane {
        public Pane() {
            BitmapImageMoniker = KnownMonikers.Search;
        }
    }
}
