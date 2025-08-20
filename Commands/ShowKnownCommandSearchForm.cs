using EnvDTE;
using QuickJump2022.Forms;
using QuickJump2022.Models;

namespace QuickJump2022;

[Command(PackageIds.ShowKnownCommandSearchForm)]
internal sealed class ShowKnownCommandSearchForm : BaseCommand<ShowKnownCommandSearchForm> {
    private static string _lastFilter = string.Empty;

    protected override async Task ExecuteAsync(OleMenuCmdEventArgs e) {
        await Package.JoinableTaskFactory.SwitchToMainThreadAsync();
        var result = await SearchForm.ShowModalAsync(Package as QuickJumpPackage, Enums.ESearchType.KnownCommands, _lastFilter);
        _lastFilter = result ?? string.Empty;
    }
}
