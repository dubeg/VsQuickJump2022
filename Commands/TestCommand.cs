using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using EnvDTE;
using Microsoft.VisualStudio.CommandBars;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell.Interop;
using QuickJump2022.Forms;
using QuickJump2022.Tools;
using System.Linq;
using Microsoft.VisualStudio.Shell;
using System.Windows.Input;
using Microsoft.Internal.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell.Settings;
using Microsoft.VisualStudio.Settings;
using IServiceProvider = System.IServiceProvider;
using System.Globalization;

namespace QuickJump2022.Commands;

[Command(PackageIds.TestCommand)]
internal sealed class TestCommand : BaseCommand<TestCommand> {
    protected override async Task ExecuteAsync(OleMenuCmdEventArgs e) {
        await Package.JoinableTaskFactory.SwitchToMainThreadAsync();
        //var form = new InputForm();
        //form.ShowModal();

        // OK!
        // var commandWindow = await VS.GetServiceAsync<SVsCommandWindow, IVsCommandWindow>(); 

        //Microsoft.VisualStudio.VisualStudioServices.VS2019_11.
        // var componentModel = Package.GetService<SComponentModel, IComponentModel>();

        var z = await VS.GetServiceAsync<SVsGlobalSearch, IVsGlobalSearch>();
        
        // OK!
        var d = await VS.GetServiceAsync<SVsCommandSearchPrivate, IVsCommandSearchPrivate>();
        var fastFetch = await VS.GetServiceAsync<SVsCommandSearchPrivate, IVsFastFetchCommands>();
        var scopes = LoadScopeLocations();
        var commandEnumerator = fastFetch.GetCommandEnumerator(
            (int)(
                __SearchCandidateProcessingFlags.ExpandDynamicItemStartCommands
                | __SearchCandidateProcessingFlags.ConsiderDynamicText
                //| __SearchCandidateProcessingFlags.ConsiderOnlyVisibleCommands
                //| __SearchCandidateProcessingFlags.ConsiderOnlyEnabledCommands
                | __SearchCandidateProcessingFlags.ConsiderCommandWellOnlyCommands
            ),
            //(uint)(scopes?.Length ?? 0),  scopes
            0, null
        );
        var commands = (CommandMetadata[])(object)new CommandMetadata[commandEnumerator.Count];
        commandEnumerator.GetCommands(0, commands.Length, commands);
        var commandStrings = new List<(string, string)>();
        var AccessKeyRemovingConverter = new AccessKeyRemovingConverter();
        foreach (var cmd in commands) {
            (string name, string description) tuple = FormatNameAndDescription(cmd.CommandPlacementText);
            string item = tuple.name;
            string item2 = tuple.description;
            item = (string)((ValueConverter<string, string>)(object)AccessKeyRemovingConverter).Convert((object)item, typeof(string), (object)null, CultureInfo.CurrentCulture);
            item2 = (string)((ValueConverter<string, string>)(object)AccessKeyRemovingConverter).Convert((object)item2, typeof(string), (object)null, CultureInfo.CurrentCulture);
            commandStrings.Add((item, item2));
        }


        // OK!
        var cmdInfoDisplay = await VS.GetServiceAsync<SVsCommandInfoDisplayServicePrivate, IVsCommandInfoDisplayServicePrivate>();
        

        // Bad!
        //var a = await VS.GetServiceAsync<?, IVsCommandTableService>();
        //var cmdInfo = await VS.GetServiceAsync<?, IVsCommandInfoQueryService>();
        //var cmdTable = await VS.GetServiceAsync<?, IVsCommandTable>();
        //var ct = new CommandTableId() { 
        //    GuidId = KnownCommands.Edit_FormatDocument.Guid,
        //    DWordId = KnownCommands.Edit_FormatDocument.ID
        //};
        //cmdInfo.GetCommandTextData(ct, ctTata);

        // OK!
        var guidTable = await VS.GetServiceAsync<SVsGuidTable, IVsGuidTable>();
        var guidID = guidTable.GetIDFromGuid(KnownCommands.Edit_FormatDocument.Guid);
        var id = KnownCommands.Edit_FormatDocument.ID;
        var ctID = new CommandTableId() { 
            GuidId = (ushort)guidID,
            DWordId = (uint)id
        };

        

        var remotableCommandInteropService = await VS.GetServiceAsync<SVsRemotableCommandInteropService, IVsRemotableCommandInteropService2>();
        //remotableCommandInteropService.GetControlId("Edit.FormatDocument", out var ctID);
        var ctData = new CommandData[1];
        remotableCommandInteropService.GetControlData(ctID, ctData);

        var ctData2 = new FullCommandAddress[1];
        remotableCommandInteropService.GetCommandData(ctID, 1, ctData2);
    }

    public ScopeLocation2[] LoadScopeLocations() {
        //IL_0006: Unknown result type (might be due to invalid IL or missing references)
        //IL_000c: Expected O, but got Unknown
        //IL_008d: Unknown result type (might be due to invalid IL or missing references)
        //IL_00a5: Unknown result type (might be due to invalid IL or missing references)
        ShellSettingsManager val = new ShellSettingsManager((IServiceProvider)Package);
        SettingsStore readOnlySettingsStore = ((SettingsManager)val).GetReadOnlySettingsStore((SettingsScope)1);
        List<ScopeLocation2> list = new List<ScopeLocation2>();
        if (readOnlySettingsStore.CollectionExists("Search\\CommandScopes")) {
            foreach (string propertyName in readOnlySettingsStore.GetPropertyNames("Search\\CommandScopes")) {
                string text = readOnlySettingsStore.GetString("Search\\CommandScopes", propertyName);
                string[] array = text.Split(new char[1] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                if (array.Length == 2 && Guid.TryParse(array[0], out var result) && uint.TryParse(array[1], out var result2)) {
                    list.Add(new ScopeLocation2 {
                        ScopeGuid = result,
                        ScopeDWord = result2
                    });
                }
            }
        }
        return list.ToArray();
    }

    private static (string name, string description) FormatNameAndDescription(string commandPlacementText) {
        string item = string.Empty;
        string text = string.Empty;
        string[] array = commandPlacementText.Split(SegmentSeparatorArray, StringSplitOptions.RemoveEmptyEntries);
        if (array.Length > 1) {
            text = array[1];
            for (int i = 2; i < array.Length; i++) {
                text = $"{text} > {array[i]}";
            }
            item = array[array.Length - 1];
        }
        return (name: item, description: text);
    }

    private static readonly char[] SegmentSeparatorArray = new char[1];
}
