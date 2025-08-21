using EnvDTE;
using QuickJump2022.Models;
using QuickJump2022.Forms;
using QuickJump2022.Services;

namespace QuickJump2022;

[Command(PackageIds.ShowAllSearchForm)]
internal sealed class ShowAllSearchForm : BaseCommand<ShowAllSearchForm> {
    protected override async Task ExecuteAsync(OleMenuCmdEventArgs e) {
        await Package.JoinableTaskFactory.SwitchToMainThreadAsync();
        await SearchForm.ShowModalAsync(Package as QuickJumpPackage, Enums.SearchType.All);
    }
}

