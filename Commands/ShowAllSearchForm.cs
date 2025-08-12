using System.IO;
using EnvDTE;
using Microsoft.VisualStudio.Shell.Interop;
using QuickJump2022.Data;
using QuickJump2022.Forms;
using QuickJump2022.Tools;

namespace QuickJump2022;

[Command(PackageIds.ShowAllSearchForm)]
internal sealed class ShowAllSearchForm : BaseCommand<ShowAllSearchForm> {
    protected override async Task ExecuteAsync(OleMenuCmdEventArgs e) {
        await Package.JoinableTaskFactory.SwitchToMainThreadAsync();
        var path = QuickJumpData.Instance.Dte.ActiveWindow.Document?.ProjectItem.TryGetProperty<string>("FullPath") ?? "";
        var isCSharp = false;
        if (!string.IsNullOrEmpty(path)) {
            var file = new FileInfo(path);
            isCSharp = (file.Extension ?? "") == ".cs";
        }
        var searchType = isCSharp ? Enums.ESearchType.All : Enums.ESearchType.Files;
        var searchController = new SearchController(QuickJumpData.Instance.Package, searchType);
        SearchFormWpf.ShowModal(searchController);
    }
}

