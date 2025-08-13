using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickJump2022.Models;

public class ListItemCommand: ListItemBase {
    public CommandItem Item { get; private set; }

    public override string Name => Item.Name;
    public override string Description => Item.ShortcutsAsString;

    public static ListItemCommand FromCommandItem(CommandItem item)
        => new ListItemCommand { Item = item };
}
