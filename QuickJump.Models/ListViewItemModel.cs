using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using QuickJump2022.Models;
using QuickJump2022.Options;

namespace QuickJump2022.Models;

public class ListItemViewModel : INotifyPropertyChanged {
    private readonly GeneralOptionsPage _options;
    private bool _isSelected;

    public ListItemBase Item { get; }

    public double ItemHeight => _options.ItemFont.Height + 6;
    public string ItemFontFamily => _options.ItemFont.FontFamily.Name;
    public double ItemFontSize => _options.ItemFont.Size * 96.0 / 72.0;
    public bool ShowIcon => ItemHeight >= 20 && _options.ShowIcons;

    public string DisplayName => Item.Name;
    public string DescriptionText { get; }
    public string TypeSuffix { get; }

    public bool IsSelected {
        get => _isSelected;
        set {
            if (_isSelected != value) {
                _isSelected = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(NameForeground));
                OnPropertyChanged(nameof(TypeForeground));
                OnPropertyChanged(nameof(DescriptionForeground));
                OnPropertyChanged(nameof(BackgroundColor));
            }
        }
    }

    public Brush NameForeground {
        get {
            if (Item is ListItemSymbol) {
                return new SolidColorBrush(ToMediaColor(
                    IsSelected ? _options.CodeSelectedForegroundColor : _options.CodeForegroundColor));
            }
            else {
                return new SolidColorBrush(ToMediaColor(
                    IsSelected ? _options.FileSelectedForegroundColor : _options.FileForegroundColor));
            }
        }
    }

    public Brush TypeForeground {
        get {
            if (Item is ListItemSymbol) {
                return new SolidColorBrush(ToMediaColor(
                    IsSelected ? _options.CodeSelectedDescriptionForegroundColor : _options.CodeDescriptionForegroundColor));
            }
            else {
                return new SolidColorBrush(ToMediaColor(
                    IsSelected ? _options.FileSelectedDescriptionForegroundColor : _options.FileDescriptionForegroundColor));
            }
        }
    }

    public Brush DescriptionForeground {
        get {
            if (Item is ListItemSymbol) {
                return new SolidColorBrush(ToMediaColor(
                    IsSelected ? _options.CodeSelectedDescriptionForegroundColor : _options.CodeDescriptionForegroundColor));
            }
            else {
                return new SolidColorBrush(ToMediaColor(
                    IsSelected ? _options.FileSelectedDescriptionForegroundColor : _options.FileDescriptionForegroundColor));
            }
        }
    }

    public Brush BackgroundColor {
        get {
            if (Item is ListItemSymbol) {
                return new SolidColorBrush(ToMediaColor(
                    IsSelected ? _options.CodeSelectedBackgroundColor : _options.CodeBackgroundColor));
            }
            else {
                return new SolidColorBrush(ToMediaColor(
                    IsSelected ? _options.FileSelectedBackgroundColor : _options.FileBackgroundColor));
            }
        }
    }

    public Brush SeparatorColor => new SolidColorBrush(ToMediaColor(_options.ItemSeparatorColor));

    public BitmapSource IconSource {
        get {
            if (!ShowIcon)
                return null;

            // Use the new BitmapSource if available
            if (Item.IconBitmapSource != null)
                return Item.IconBitmapSource;

            // Fallback to Icon if BitmapSource not yet loaded
            if (Item.IconImage != null) {
                using (var icon = Item.IconImage) {
                    return System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(
                        icon.Handle,
                        Int32Rect.Empty,
                        BitmapSizeOptions.FromEmptyOptions());
                }
            }

            return null;
        }
    }

    public ListItemViewModel(ListItemBase item, GeneralOptionsPage options) {
        Item = item;
        _options = options;

        // Set text properties based on item type
        if (item is ListItemSymbol csharpItem) {
            TypeSuffix = !string.IsNullOrEmpty(csharpItem.Type) ? $" -> {csharpItem.Type}" : "";
            DescriptionText = $"{item.Description}:{item.Line}";
        }
        else {
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

    public void UpdateIcon(BitmapSource iconBitmapSource) {
        Item.IconBitmapSource = iconBitmapSource;
        OnPropertyChanged(nameof(IconSource));
    }
}