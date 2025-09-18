using EnvDTE;
using QuickJump2022.Forms;
using QuickJump2022.Models;
using QuickJump2022.Services;

namespace QuickJump2022;

[Command(PackageIds.ShowCommandSearchForm)]
internal sealed class ShowCommandSearchForm : BaseCommand<ShowCommandSearchForm> {
    private static string _lastFilter = string.Empty;
    private static Enums.SearchType _lastCommandScope = Enums.SearchType.Commands;
    private static string _lastSelectedCommandText = string.Empty;
    
    protected override async Task ExecuteAsync(OleMenuCmdEventArgs e) {
        await Package.JoinableTaskFactory.SwitchToMainThreadAsync();
        var dialog = await SearchForm.ShowModalAsync(Package as QuickJumpPackage, _lastCommandScope, _lastFilter, initialSelectedCommandText: _lastSelectedCommandText);
        _lastFilter = dialog.CurrentText;
        _lastCommandScope = dialog.SearchType;
        _lastSelectedCommandText = dialog.ResultText;
    }
}
