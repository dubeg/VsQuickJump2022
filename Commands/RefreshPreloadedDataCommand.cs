using System.ComponentModel.Design;
using System.Windows.Input;
using EnvDTE;
using QuickJump2022.Services;

namespace QuickJump2022.Commands;

[Command(PackageIds.RefreshPreloadedData)]
internal sealed class RefreshPreloadedDataCommand : BaseCommand<RefreshPreloadedDataCommand> {
    protected override async Task ExecuteAsync(OleMenuCmdEventArgs e) {
        await Package.JoinableTaskFactory.SwitchToMainThreadAsync();
        
        var package = Package as QuickJumpPackage;
        if (package == null) {
            await VS.StatusBar.ShowMessageAsync("Unable to access QuickJumpPackage");
            return;
        }

        try {
            await VS.StatusBar.ShowMessageAsync("Refreshing preloaded data...");
            
            // Refresh CommandService preloaded commands
            if (package.CommandService != null) {
                package.CommandService.PreloadCommandsCache();
            }
            
            await VS.StatusBar.ShowMessageAsync("Preloaded data refreshed successfully");
        }
        catch (Exception ex) {
            await VS.StatusBar.ShowMessageAsync($"Error refreshing preloaded data: {ex.Message}");
            await VS.MessageBox.ShowErrorAsync("Refresh Failed", $"Failed to refresh preloaded data: {ex.Message}");
        }
    }
}
