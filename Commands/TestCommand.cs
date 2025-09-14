using EnvDTE;
using QuickJump2022.Forms;

namespace QuickJump2022.Commands;

[Command(PackageIds.TestCommand)]
internal sealed class TestCommand : BaseCommand<TestCommand> {
    protected override async Task ExecuteAsync(OleMenuCmdEventArgs e) {
        await Package.JoinableTaskFactory.SwitchToMainThreadAsync();
    }
}
