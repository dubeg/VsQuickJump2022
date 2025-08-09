using EnvDTE;
using QuickJump2022.Data;
using QuickJump2022.Forms;
using QuickJump2022.Tools;
using System.IO;

namespace QuickJump2022;

[Command(PackageIds.ShowAllSearchFormDualWindow)]
internal sealed class ShowAllSearchFormDualWindow : BaseCommand<ShowAllSearchFormDualWindow> {
    protected override async Task ExecuteAsync(OleMenuCmdEventArgs e) {
        await Package.JoinableTaskFactory.SwitchToMainThreadAsync();
        var path = QuickJumpData.Instance.Dte.ActiveWindow.Document?.ProjectItem.TryGetProperty<string>("FullPath") ?? "";
        var isCSharp = false;
        if (!string.IsNullOrEmpty(path)) {
            var file = new FileInfo(path);
            isCSharp = (file.Extension ?? "") == ".cs";
        }
        var searchType = Enums.ESearchType.All;
        var searchController = new SearchController(QuickJumpData.Instance.Package, searchType);
        await MainSearchWindow.ShowWithDualWindowAsync(searchController);
    }
}
