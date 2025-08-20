using EnvDTE;
using QuickJump2022.Forms;
using QuickJump2022.Models;
using QuickJump2022.Services;

namespace QuickJump2022;

[Command(PackageIds.ShowCommandSearchForm)]
internal sealed class ShowCommandSearchForm : BaseCommand<ShowCommandSearchForm> {
    private static string _lastFilter = string.Empty;
    
    protected override async Task ExecuteAsync(OleMenuCmdEventArgs e) {
        await Package.JoinableTaskFactory.SwitchToMainThreadAsync();
        var result = await SearchForm.ShowModalAsync(Package as QuickJumpPackage, Enums.ESearchType.Commands, _lastFilter);
        _lastFilter = result ?? string.Empty;
    }
}
