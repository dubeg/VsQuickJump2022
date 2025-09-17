using EnvDTE;
using QuickJump2022.Forms;
using QuickJump2022.Models;

namespace QuickJump2022;

// Note: Using a literal ID here to avoid depending on generated constants.
// The value must match VSCommandTable.vsct IDSymbol for ShowCanonicalCommandSearchForm
[Command(0x0900)]
internal sealed class ShowCanonicalCommandSearchForm : BaseCommand<ShowCanonicalCommandSearchForm> {
    private static string _lastFilter = string.Empty;

    protected override async Task ExecuteAsync(OleMenuCmdEventArgs e) {
        await Package.JoinableTaskFactory.SwitchToMainThreadAsync();
        var dialog = await SearchForm.ShowModalAsync(Package as QuickJumpPackage, Enums.SearchType.Commands, _lastFilter, enableCommandTabCycle: false);
        _lastFilter = dialog.ResultText ?? dialog.ResultText;
    }
}


