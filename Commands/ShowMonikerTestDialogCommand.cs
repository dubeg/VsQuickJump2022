using Microsoft.VisualStudio.PlatformUI;
using QuickJump2022.Forms;

namespace QuickJump2022;

[Command(PackageIds.ShowMonikerTestDialog)]
internal sealed class ShowMonikerTestDialogCommand : BaseCommand<ShowMonikerTestDialogCommand> {
    protected override async Task ExecuteAsync(OleMenuCmdEventArgs e) {
        await Package.JoinableTaskFactory.SwitchToMainThreadAsync();
        
        // Create and show the dialog on the main UI thread
        var dialog = new MonikerTestDialog();
        dialog.ShowModal();
    }
}
