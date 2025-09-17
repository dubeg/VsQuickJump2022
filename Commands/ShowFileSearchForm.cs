using EnvDTE;
using QuickJump2022.Forms;
using QuickJump2022.Models;
using QuickJump2022.Services;
using static QuickJump2022.Models.Enums;

namespace QuickJump2022;

[Command(PackageIds.ShowFileSearchForm)]
internal sealed class ShowFileSearchForm : BaseCommand<ShowFileSearchForm> {
    private static FileSearchScope _lastScope = FileSearchScope.Solution;

    protected override async Task ExecuteAsync(OleMenuCmdEventArgs e) {
        await Package.JoinableTaskFactory.SwitchToMainThreadAsync();
        var dialog = await SearchForm.ShowModalAsync(Package as QuickJumpPackage, Enums.SearchType.Files, fileSearchScope: _lastScope);
        _lastScope = dialog.FileScope;
    }
}
