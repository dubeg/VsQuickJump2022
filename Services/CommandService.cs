using EnvDTE;
using QuickJump2022.Models;
using QuickJump2022.Tools;
using System.Collections.Generic;
using System.Linq;

namespace QuickJump2022.Services;

public class CommandService(DTE Dte) {
    private List<CommandItem> _commands = new();
    private string[] _exclusions => [
        "TeamFoundation",
        "OtherContextMenus.",
        "ClassViewContextMenus.",
        "DebuggerContextMenus.",
        "EditorContextMenus.",
        "ProjectandSolutionContextMenus.",
        "File.Tfs",
        "Image",
        "Tfs",
        "SSDT",
        "DSLTools.",
        "LiveShare.",
        "QueryDesigner.",
        "Table.",
        "TableDesigner.",
        "XsdDesigner.",
        "SQLTableDesigner.",
    ];

    public void PreloadCommands() {
        var commands = GetCommands();
        _commands = commands.Where(x => x.Name.IsNotIn(_exclusions, (a, b) => a.StartsWith(b))).ToList();
    }
    
    public List<CommandItem> GetCachedCommands() {
        if (_commands.Count == 0) PreloadCommands();
        return _commands;
    }

    public void Execute(CommandItem commandItem) {
        var cmd = Dte.Commands.Item(commandItem.Name);
        try {
            if (cmd != null && cmd.IsAvailable) {
                Dte.Commands.Raise(cmd.Guid, cmd.ID, null, null);
                Dte.StatusBar.Clear();
            }
            else {
                Dte.StatusBar.Text = $"The command '{cmd.Name}' is not available in the current context";
            }
        }
        catch (Exception) {
            Dte.StatusBar.Text = $"The command '{cmd.Name}' is not available in the current context";
        }
    }

    public List<CommandItem> GetCommands(bool availableOnly = false) {
        var results = new List<CommandItem>();
        foreach (Command command in Dte.Commands) {
            if (string.IsNullOrEmpty(command.Name)) continue;
            if (availableOnly && !command.IsAvailable) continue;
            var result = new CommandItem() { 
                Name = command.Name,
                Guid = command.Guid,
                ID = command.ID,
                Shortcuts = GetBindings(command.Bindings as object[])
            };
            results.Add(result);
        }
        return results;
    }

    private static List<string> GetBindings(IEnumerable<object> bindings) {
        IEnumerable<string> result = bindings.Select(binding => binding.ToString().IndexOf("::") >= 0
            ? binding.ToString().Substring(binding.ToString().IndexOf("::") + 2)
            : binding.ToString()).Distinct();
        return result.ToList();
    }
}
