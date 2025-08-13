using System.Collections.Generic;
using System.Windows.Media.Imaging;
using EnvDTE;
using Microsoft.VisualStudio.CommandBars;
using QuickJump2022.Tools;

namespace QuickJump2022.Commands;

[Command(PackageIds.TestCommand)]
internal sealed class TestCommand : BaseCommand<TestCommand> {


    protected override async Task ExecuteAsync(OleMenuCmdEventArgs e) {

        await VS.StatusBar.ShowMessageAsync("Not implemented");

        // --------------------
        // Known commands
        // Using known commands, I could hardcode a dictionary of 
        // (icon, display name) & KnownCommand pairs.
        // --------------------
        // Eg. KnownCommands.Build_BatchBuild
        // --
        // public static class KnownCommands
        //   * public static CommandID Build_BatchBuild


        // --------------------
        //for (int i = 0; i <= commandBars.Count; i++) {
        //    try {
        //        Type curType = commandBars[i].GetType();
        //        CommandBar commBarProject = commandBars[i];
        //        if (commBarProject != null) {
        //            creatorButton = (CommandBarButton)createCommand.AddControl(
        //                            commBarProject, commBarProject.Controls.Count + 1);
        //            creatorButton.Caption = "ViewModel - " + cmdBars[i].Name;
        //        }
        //    }
        //    catch (Exception) { }
        //}

        // var cmdNameSvc = await VS.Services.GetCommandNameMappingAsync();
        // cmd.MapNameToGUIDID
        // cmd.MapGUIDIDToName
        // cmdNameSvc.EnumNames;
        // cmdNameSvc.EnumMacroNames;


        //VsMenus.guidSHLMainMenu;
        //Microsoft.VisualStudio.OLE.Interop.Constants.
        //var cmdSvc = await VS.Services.GetCommandServiceAsync();
        //var x = cmdSvc.FindCommand(new System.ComponentModel.Design.CommandID(VsMenus.guidSHLMainMenu, ));


        //cmd.MapNameToGUIDID("Debug.SetCurrentProcess", out var setCurrentProcessCmdGroup, out var setCurrentProcessCmdId);
        //var commandService = new OleMenuCommandService(ServiceProvider.GlobalProvider);
        //if (!commandService.GlobalInvoke(new CommandID(setCurrentProcessCmdGroup, (int)setCurrentProcessCmdId), Convert.ToString(targetProcessIndex + 1))) 
        //    throw new DebuggerException("Unable to set the active process in the debugger.");
        //}
    }
}
