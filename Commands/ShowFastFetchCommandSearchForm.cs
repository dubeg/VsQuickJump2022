using QuickJump2022.Forms;
using QuickJump2022.Models;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

namespace QuickJump2022;

[Command(PackageIds.ShowFastFetchCommandSearchForm)]
internal sealed class ShowFastFetchCommandSearchForm : BaseCommand<ShowFastFetchCommandSearchForm> {
    private static string _lastFilter = string.Empty;
    private static string _lastSelectedCommandText = string.Empty;

    protected override async Task ExecuteAsync(OleMenuCmdEventArgs e) {
        await Package.JoinableTaskFactory.SwitchToMainThreadAsync();
        var dialog = await SearchForm.ShowModalAsync(Package as QuickJumpPackage, Enums.SearchType.FastFetchCommands, _lastFilter, initialSelectedCommandText: _lastSelectedCommandText);
        _lastFilter = dialog.CurrentText;
        _lastSelectedCommandText = dialog.ResultText;
        // Quick hack: if user dismissed the dialog (ie. didn't select anything),
        // reset the filter to null so that next time all commands are shown.
        if (string.IsNullOrWhiteSpace(_lastSelectedCommandText)) {
            _lastFilter = null;
        }
    }
}



