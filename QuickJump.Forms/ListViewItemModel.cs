using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using QuickJump2022.Models;
using QuickJump2022.Options;

namespace QuickJump2022.Forms;

public class ListItemViewModel : INotifyPropertyChanged {
    private readonly GeneralOptionsPage _options;

    public ListItemBase Item { get; }

    public double ItemHeight => _options.ItemFont.Height + 6;
    public string ItemFontFamily => _options.ItemFont.FontFamily.Name;
    public double ItemFontSize => _options.ItemFont.Size * 96.0 / 72.0;
    public bool ShowIcon => ItemHeight >= 20 && _options.ShowIcons;

    public string DisplayName => Item.Name;
    public string DescriptionText { get; }
    public string TypeSuffix { get; }

    public Brush NameForeground { get; }
    public Brush TypeForeground { get; }
    public Brush DescriptionForeground { get; }
    public Brush SelectedBackgroundColor { get; }
    public Brush SeparatorColor => new SolidColorBrush(ToMediaColor(_options.ItemSeperatorColor));

    public BitmapSource IconSource {
        get {
            if (!ShowIcon || Item.IconImage == null)
                return null;

            using (var icon = Item.IconImage) {
                return System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(
                    icon.Handle,
                    Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());
            }
        }
    }

    public ListItemViewModel(ListItemBase item, GeneralOptionsPage options) {
        Item = item;
        _options = options;

        // Set colors based on item type
        if (item is ListItemCSharp csharpItem) {
            NameForeground = new SolidColorBrush(ToMediaColor(options.CodeForegroundColor));
            TypeForeground = new SolidColorBrush(ToMediaColor(options.CodeDescriptionForegroundColor));
            DescriptionForeground = new SolidColorBrush(ToMediaColor(options.CodeDescriptionForegroundColor));
            SelectedBackgroundColor = new SolidColorBrush(ToMediaColor(options.CodeSelectedBackgroundColor));

            TypeSuffix = !string.IsNullOrEmpty(csharpItem.Type) ? $" -> {csharpItem.Type}" : "";
            DescriptionText = $"{item.Description}:{item.Line}";
        }
        else {
            NameForeground = new SolidColorBrush(ToMediaColor(options.FileForegroundColor));
            TypeForeground = new SolidColorBrush(ToMediaColor(options.FileDescriptionForegroundColor));
            DescriptionForeground = new SolidColorBrush(ToMediaColor(options.FileDescriptionForegroundColor));
            SelectedBackgroundColor = new SolidColorBrush(ToMediaColor(options.FileSelectedBackgroundColor));

            TypeSuffix = "";
            DescriptionText = item.Description ?? "";
        }
    }

    private static Color ToMediaColor(System.Drawing.Color color) {
        return Color.FromArgb(color.A, color.R, color.G, color.B);
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}