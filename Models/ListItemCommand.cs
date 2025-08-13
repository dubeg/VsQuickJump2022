using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QuickJump2022.Models;

public class ListItemCommand: ListItemBase {
    public CommandItem Item { get; private set; }

    public override string Name => Item.Name;
    public override string Description => 
        string.Join(" | ", Item.Shortcuts.Select(ReplaceKeys).Take(1)); // TODO: make it configurable.

    public static ListItemCommand FromCommandItem(CommandItem item)
        => new ListItemCommand { Item = item };

    private static string ReplaceKeys(string s) => s
        .Replace("Left Arrow", "←")
        .Replace("Right Arrow", "→")
        .Replace("Up Arrow", "↑")
        .Replace("Down Arrow", "↓")
        .Replace("Enter", "↲")
        .Replace("Backspace", "⌫")
        .Replace("Bkspce", "⌫")
        ;

}
