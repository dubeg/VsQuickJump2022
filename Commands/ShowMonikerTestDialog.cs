using Microsoft.VisualStudio.PlatformUI;
using QuickJump2022.Forms;

namespace QuickJump2022;

[Command(PackageIds.ShowMonikerTestDialog)]
internal sealed class ShowMonikerTestDialog : BaseCommand<ShowMonikerTestDialog> {
    protected override async Task ExecuteAsync(OleMenuCmdEventArgs e) {
        await Package.JoinableTaskFactory.SwitchToMainThreadAsync();
        var dialog = new MonikerTestDialog();
        dialog.ShowModal();
    }
}
