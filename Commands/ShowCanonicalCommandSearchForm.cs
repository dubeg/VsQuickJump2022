using EnvDTE;
using QuickJump2022.Forms;
using QuickJump2022.Models;

namespace QuickJump2022;

// Note: Using a literal ID here to avoid depending on generated constants.
// The value must match VSCommandTable.vsct IDSymbol for ShowCanonicalCommandSearchForm
[Command(0x0900)]
internal sealed class ShowCanonicalCommandSearchForm : BaseCommand<ShowCanonicalCommandSearchForm> {
    private static string _lastFilter = string.Empty;
    private static string _lastSelectedCommandText = string.Empty;

    protected override async Task ExecuteAsync(OleMenuCmdEventArgs e) {
        await Package.JoinableTaskFactory.SwitchToMainThreadAsync();
        var dialog = await SearchForm.ShowModalAsync(Package as QuickJumpPackage, Enums.SearchType.Commands, _lastFilter, initialSelectedCommandText: _lastSelectedCommandText);
        _lastFilter = dialog.CurrentText;
        _lastSelectedCommandText = dialog.ResultText;
        // Quick hack: if user dismissed the dialog (ie. didn't select anything),
        // reset the filter to null so that next time all commands are shown.
        if (string.IsNullOrWhiteSpace(_lastSelectedCommandText)) {
            _lastFilter = null;
        }
    }
}

