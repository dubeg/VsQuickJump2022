global using System;
global using Community.VisualStudio.Toolkit;
global using Microsoft.VisualStudio.Shell;
global using Task = System.Threading.Tasks.Task;
using System.Runtime.InteropServices;
using System.Threading;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.LanguageServices;
using QuickJump2022.Options;
using QuickJump2022.Services;

namespace QuickJump2022;

[PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
[InstalledProductRegistration(Vsix.Name, Vsix.Description, Vsix.Version)]
[ProvideMenuResource("Menus.ctmenu", 1)]
[Guid(PackageGuids.QuickJump2022String)]
[ProvideOptionPage(typeof(GeneralOptionsPage), "QuickJump2022", "General", 0, 0, true)]
public sealed class QuickJumpPackage : ToolkitPackage {
    public GeneralOptionsPage GeneralOptions;
    public DTEEvents DteEvents;
    public DTE Dte;
    public QuickJumpPackage Package { get; private set; }
    public SymbolService SymbolService { get; private set; }
    public ProjectFileService ProjectFileService { get; private set; }
    public SettingsService SettingsService { get; private set; }
    public GoToService GoToService { get; private set; }
    public ClassificationService ClassificationService { get; set; }
    public CommandService CommandService { get; private set; }
    public CommandBarService CommandBarService { get; private set; }

    protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress) {
        await this.RegisterCommandsAsync();
        // -- 
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
        var generalOptions = (GeneralOptionsPage)GetDialogPage(typeof(GeneralOptionsPage));
        var dte = await VS.GetServiceAsync<DTE, DTE2>();
        var componentModel = await VS.GetServiceAsync<SComponentModel, IComponentModel>();
        var workspace = componentModel.GetService<VisualStudioWorkspace>();
        Dte = dte;
        SymbolService = new(workspace);
        ProjectFileService = new();
        SettingsService = new(generalOptions, this);
        GoToService = new();
        GeneralOptions = generalOptions;
        ClassificationService = new(this);
        CommandService = new(Dte);
        CommandBarService = new(Dte);
        // --
        SettingsService.LoadSettings();
        ClassificationService.PreloadCommonBrushes();
        CommandService.PreloadCommands();
        // CommandBarService.PreloadCommands();
        dte.Events.DTEEvents.OnBeginShutdown += () => SettingsService.SaveSettings();
    }
}