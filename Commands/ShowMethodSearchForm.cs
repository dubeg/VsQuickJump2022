using System.IO;
using EnvDTE;
using QuickJump2022.Data;
using QuickJump2022.Forms;
using QuickJump2022.Tools;

namespace QuickJump2022;

[Command(PackageIds.ShowMethodSearchForm)]
internal sealed class ShowMethodSearchForm : BaseCommand<ShowMethodSearchForm> {
    protected override async Task ExecuteAsync(OleMenuCmdEventArgs e) {
        await Package.JoinableTaskFactory.SwitchToMainThreadAsync();
        if (QuickJumpData.Instance.Dte.ActiveWindow?.Document is null) return; // No active document.
        var path = QuickJumpData.Instance.Dte.ActiveWindow.Document.ProjectItem.TryGetProperty<string>("FullPath");
        if (string.IsNullOrEmpty(path)) {
            // Unsaved document (?)
            await VS.MessageBox.ShowWarningAsync(nameof(ShowMethodSearchForm), "The active document doesn't have a path.");
            return;
        }
        var file = new FileInfo(path);
        var isCsharp = !string.IsNullOrEmpty(file.Extension) && file.Extension.Equals(".cs", StringComparison.InvariantCultureIgnoreCase);
        if (!isCsharp) return;
        var searchType = Enums.ESearchType.Methods;
        var searchController = new SearchController(QuickJumpData.Instance.Package, searchType);
        SearchFormWpf.ShowModal(searchController);
    }
}
