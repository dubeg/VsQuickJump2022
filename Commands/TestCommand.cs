using System.Collections.Generic;
using System.Windows.Media.Imaging;
using EnvDTE;
using Microsoft.VisualStudio.CommandBars;
using QuickJump2022.Forms;
using QuickJump2022.Tools;

namespace QuickJump2022.Commands;

[Command(PackageIds.TestCommand)]
internal sealed class TestCommand : BaseCommand<TestCommand> {
    protected override async Task ExecuteAsync(OleMenuCmdEventArgs e) {
        await Package.JoinableTaskFactory.SwitchToMainThreadAsync();
        var form = new InputForm();
        form.ShowModal();
    }
}
