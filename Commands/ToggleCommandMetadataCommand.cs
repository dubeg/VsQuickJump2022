using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.Shell;

namespace QuickJump2022.Commands;

[Command(PackageIds.ToggleCommandMetadata)]
internal sealed class ToggleCommandMetadataCommand : BaseCommand<ToggleCommandMetadataCommand> {
    protected override async Task ExecuteAsync(OleMenuCmdEventArgs e) {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
        QuickJumpPackage.ShowCommandMetadata = !QuickJumpPackage.ShowCommandMetadata;
        await VS.StatusBar.ShowMessageAsync($"Command metadata display: {(QuickJumpPackage.ShowCommandMetadata ? "ON" : "OFF")}");
    }
}
