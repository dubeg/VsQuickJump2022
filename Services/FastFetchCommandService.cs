using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Imaging.Interop;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell.Settings;
using Microsoft.VisualStudio.Utilities;
using QuickJump2022.VisualStudio.Interop;

namespace QuickJump2022.Services;

public class FastFetchCommandItem {
    public string Key => $"{CommandID.Guid}.{CommandID.ID}";
    public string Name { get; set; }
    public string Description { get; set; }
    public CommandID CommandID { get; set; }
    public int Index { get; set; }
    public List<string> Shortcuts { get; set; } = new();
    public ImageMoniker Icon { get; set; }
}

/// <summary>
/// Get commands using VS's FastFetch internal service.
/// </summary>
/// <param name="_serviceProvider"></param>
public class FastFetchCommandService(IServiceProvider _serviceProvider) {
    private const string ScopePath = "Search\\CommandScopes";
    private ScopeLocation2[] _cachedScope = null;
    private List<FastFetchCommandItem> _cachedCommands = null;

    public async Task<List<FastFetchCommandItem>> GetCommandsAsync() {
        if (_cachedCommands != null) {
            return _cachedCommands;
        }
        _cachedScope ??= GetScopeLocations();
        var fastFetch = await VS.GetServiceAsync<SVsCommandSearchPrivate, IVsFastFetchCommands>();
        var searchFlags = 
            __SearchCandidateProcessingFlags.ConsiderDynamicText |
            __SearchCandidateProcessingFlags.ExpandDynamicItemStartCommands |
            __SearchCandidateProcessingFlags.ConsiderCommandWellOnlyCommands;
        var commandEnumerator2 = fastFetch.GetCommandEnumerator((uint)searchFlags, 0, null);
        var commandEnumerator = fastFetch.GetCommandEnumerator((uint)searchFlags, (uint)_cachedScope.Length, _cachedScope);
        var commands = (CommandMetadata[])(object)new CommandMetadata[commandEnumerator.Count];
        var commands2 = (CommandMetadata[])(object)new CommandMetadata[commandEnumerator2.Count];
        commandEnumerator.GetCommands(0, commands.Length, commands);
        commandEnumerator2.GetCommands(0, commands2.Length, commands2);
        var commandInfos = new List<FastFetchCommandItem>();
        var AccessKeyRemovingConverter = new AccessKeyRemovingConverter();
        foreach (var cmd in commands.Concat(commands2)) {
            if (cmd.CommandId.CommandSet == Guid.Empty) continue;
            var (name, description) = FormatNameAndDescription(cmd.CommandPlacementText);
            name = Accelerator.StripAccelerators(name, null);
            description = Accelerator.StripAccelerators(description, null);
            commandInfos.Add(new() { 
                Name = name,
                Description = description,
                CommandID = new CommandID(cmd.CommandId.CommandSet, (int)cmd.CommandId.CommandId),
                Index = (int)cmd.DiscoveryOrder,
                Shortcuts = [cmd.CommandKeyBinding],
                Icon = cmd.Icon,
            });
        }
        _cachedCommands = commandInfos.GroupBy(x => x.Key).Select(g => g.First()).ToList();
        return _cachedCommands;
    }

    private ScopeLocation2[] GetScopeLocations() {
        var val = new ShellSettingsManager(_serviceProvider);
        var store = val.GetReadOnlySettingsStore((SettingsScope)1);
        var list = new List<ScopeLocation2>();
        if (store.CollectionExists(ScopePath)) {
            foreach (var propertyName in store.GetPropertyNames(ScopePath)) {
                var text = store.GetString(ScopePath, propertyName);
                var array = text.Split(new char[1] { ';' }, StringSplitOptions.RemoveEmptyEntries);
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
        var item = string.Empty;
        var text = string.Empty;
        var array = commandPlacementText.Split(SegmentSeparatorArray, StringSplitOptions.RemoveEmptyEntries);
        if (array.Length > 1) {
            text = array[1];
            for (var i = 2; i < array.Length; i++) {
                text = $"{text} > {array[i]}";
            }
            item = array[array.Length - 1];
        }
        return (name: item, description: text);
    }

    private static readonly char[] SegmentSeparatorArray = new char[1];
}
