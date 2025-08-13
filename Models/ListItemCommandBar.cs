using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using QuickJump2022.Services;

namespace QuickJump2022.Models;

public class ListItemCommandBar : ListItemBase {
    public CommandBarButtonInfo Item { get; private set; }

    public override string Name => Item.Caption?.Replace("&", ""); // Remove accelerator key
    public override string Description => Item.CommandBarName;

    public static ListItemCommandBar FromCommandBarButtonInfo(CommandBarButtonInfo item)
        => new ListItemCommandBar { Item = item };
}
