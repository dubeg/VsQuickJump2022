using QuickJump2022.Data;
using QuickJump2022.Forms;

namespace QuickJump2022.Commands;

[Command(PackageIds.ShowSearchToolWindow)]
internal sealed class ShowSearchToolWindow : BaseCommand<ShowSearchToolWindow> {
    /// <summary>
    /// Shows the tool window when the menu item is clicked.
    /// </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The event args.</param>
    protected override async Task ExecuteAsync(OleMenuCmdEventArgs e) {
        await Package.JoinableTaskFactory.SwitchToMainThreadAsync();
        var tool = await SearchToolWindow.ShowAsync();
    }
}
