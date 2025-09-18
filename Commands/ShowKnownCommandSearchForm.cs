using EnvDTE;
using QuickJump2022.Forms;
using QuickJump2022.Models;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

namespace QuickJump2022;

[Command(PackageIds.ShowKnownCommandSearchForm)]
internal sealed class ShowKnownCommandSearchForm : BaseCommand<ShowKnownCommandSearchForm> {
    private static string _lastFilter = string.Empty;
    private static string _lastSelectedCommandText = string.Empty;

    protected override async Task ExecuteAsync(OleMenuCmdEventArgs e) {
        await Package.JoinableTaskFactory.SwitchToMainThreadAsync();
        var dialog = await SearchForm.ShowModalAsync(Package as QuickJumpPackage, Enums.SearchType.KnownCommands, _lastFilter, initialSelectedCommandText: _lastSelectedCommandText);
        _lastFilter = dialog.CurrentText;
        _lastSelectedCommandText = dialog.ResultText;
    }
}
