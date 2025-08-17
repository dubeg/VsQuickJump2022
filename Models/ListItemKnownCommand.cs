using QuickJump2022.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static QuickJump2022.Services.KnownCommandService;

namespace QuickJump2022.Models;

public class ListItemKnownCommand: ListItemBase {
    public KnownCommandMapping Item { get; private set; }

    public override string Name => Item.DisplayName;

    public static ListItemKnownCommand FromKnownCommandMapping(KnownCommandMapping item)
        => new ListItemKnownCommand { Item = item };
}
