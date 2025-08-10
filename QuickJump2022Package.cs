global using System;
global using Community.VisualStudio.Toolkit;
global using Microsoft.VisualStudio.Shell;
global using Task = System.Threading.Tasks.Task;
using System.Runtime.InteropServices;
using System.Threading;
using QuickJump2022.Forms;
using QuickJump2022.Options;

namespace QuickJump2022;

[PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
[InstalledProductRegistration(Vsix.Name, Vsix.Description, Vsix.Version)]
[ProvideMenuResource("Menus.ctmenu", 1)]
[Guid(PackageGuids.QuickJump2022String)]
[ProvideOptionPage(typeof(GeneralOptionsPage), "QuickJump2022", "General", 0, 0, true)]
public sealed class QuickJump2022Package : ToolkitPackage {
    protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress) {
        await this.RegisterCommandsAsync();
        var generalOptionsPage = (GeneralOptionsPage)GetDialogPage(typeof(GeneralOptionsPage));
        await QuickJumpData.CreateAsync(this, generalOptionsPage);
    }
}