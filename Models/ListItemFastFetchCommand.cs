using System.Linq;
using QuickJump2022.Services;

namespace QuickJump2022.Models;

public class ListItemFastFetchCommand : ListItemBase {
    public FastFetchCommandItem Item { get; private set; }

    public override string Name => Item.Name;
    public override string Description => string.Join(" | ", Item.Shortcuts.Take(1));

    public static ListItemFastFetchCommand FromFastFetchItem(FastFetchCommandItem item)
        => new ListItemFastFetchCommand { Item = item };
}



