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
[ProvideToolWindow(typeof(SearchToolWindow.Pane), Transient = true)]
[ProvideToolWindowVisibility(typeof(SearchToolWindow.Pane), /*UICONTEXT_SolutionExists*/"f1536ef8-92ec-443c-9ed7-fdadf150da82")]
public sealed class QuickJump2022Package : ToolkitPackage {
    protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress) {
        this.RegisterToolWindows();
        await this.RegisterCommandsAsync();
        var generalOptionsPage = (GeneralOptionsPage)GetDialogPage(typeof(GeneralOptionsPage));
        await QuickJumpData.CreateAsync(this, generalOptionsPage);
    }
}