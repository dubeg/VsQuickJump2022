using System.Drawing;
using System.Windows.Media.Imaging;

namespace QuickJump2022.Models;

public abstract class ListItemBase : IComparable {
    public int Weight;
    public BitmapSource IconBitmapSource;
    public virtual string Name => string.Empty;
    public virtual string Type => string.Empty;
    public virtual string Description => string.Empty;

    public int CompareTo(object obj) {
        var item = (ListItemBase)obj;
        if (item.Weight > Weight) return -1;
        if (item.Weight >= Weight) return 0;
        return 1;
    }

    public override string ToString() => Name;
}
