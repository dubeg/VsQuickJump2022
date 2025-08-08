using EnvDTE;
using QuickJump2022.Data;
using QuickJump2022.Forms;

namespace QuickJump2022;

[Command(PackageIds.ShowFileSearchForm)]
internal sealed class ShowFileSearchForm : BaseCommand<ShowFileSearchForm> {
    protected override async Task ExecuteAsync(OleMenuCmdEventArgs e) {
        await Package.JoinableTaskFactory.SwitchToMainThreadAsync();
        var isInSolution = !string.IsNullOrEmpty(QuickJumpData.Instance.Dte.Solution?.FullName);
        if (!isInSolution) {
            await VS.MessageBox.ShowWarningAsync(nameof(ShowFileSearchForm), "You must be in a solution to search files.");
            return;
        }
        // Show modeless so clicking outside can deactivate and close the form
        new SearchForm(Enums.ESearchType.Files).Show();
    }
}
