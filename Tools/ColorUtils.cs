using System.Windows.Media;

namespace QuickJump2022.Tools;

public static class ColorUtils {
    public static Color ToMediaColor(System.Drawing.Color color) => Color.FromArgb(color.A, color.R, color.G, color.B);
    public static Brush ToBrush(System.Drawing.Color color) => new SolidColorBrush(ToMediaColor(color));
}
