using System.Drawing;
using System.Windows.Media.Imaging;

namespace QuickJump2022.Models;

public abstract class ListItemBase : IComparable {
    public int Weight;
    public Icon IconImage;
    public BitmapSource IconBitmapSource;
    public string Name;
    public string Type;
    public string Description;
    public int Line = 1;

    public int CompareTo(object obj) {
        var item = (ListItemBase)obj;
        if (item.Weight > Weight) return -1;
        if (item.Weight >= Weight) return 0;
        return 1;
    }

    public override string ToString() => Name;
}
