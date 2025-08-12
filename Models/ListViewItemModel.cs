using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.Imaging.Interop;
using QuickJump2022.Options;
using QuickJump2022.Tools;

namespace QuickJump2022.Models;

public class ListItemViewModel : INotifyPropertyChanged {
    
    public ListItemViewModel(ListItemBase item, GeneralOptionsPage options) {
        Item = item;
        _options = options;
        IconMoniker =
            item is ListItemSymbol symbol ? IconMoniker = KnownMonikerUtils.GetCodeMoniker(symbol.Item.BindType)
            : item is ListItemFile file ? IconMoniker = KnownMonikerUtils.GetFileMoniker(file.FileExtension)
            : KnownMonikers.None;
        var isCode = item is ListItemSymbol;
        ShowIcon = options.ShowIcons;
        
        NameForeground = ToBrush(isCode ? _options.CodeForegroundColor : _options.FileForegroundColor);
        NameForegroundSelected = ToBrush(isCode ? _options.CodeSelectedForegroundColor : _options.FileSelectedForegroundColor);
        
        TypeForeground = ToBrush(isCode ? _options.CodeDescriptionForegroundColor : _options.FileDescriptionForegroundColor);
        TypeForegroundSelected = ToBrush(isCode ? _options.CodeSelectedDescriptionForegroundColor : _options.FileSelectedDescriptionForegroundColor);
        
        DescriptionForeground = ToBrush(isCode ? _options.CodeDescriptionForegroundColor : _options.FileDescriptionForegroundColor);
        DescriptionForegroundSelected = ToBrush(isCode ? _options.CodeSelectedDescriptionForegroundColor : _options.FileSelectedDescriptionForegroundColor);
    }

    private readonly GeneralOptionsPage _options;

    public ListItemBase Item { get; init; }
    public bool ShowIcon { get; init; }
    public string Name => Item.Name;
    public string Description => Item.Description;
    public string Type => Item.Type;
    public ImageMoniker IconMoniker { get; init; }
    
    private bool _isSelected;
    public bool IsSelected { 
        get => _isSelected;
        set {
            if (_isSelected != value) {
                _isSelected = value;
                OnPropertyChanged();
            }
        }
    }

    public Brush NameForeground { get; init; }
    public Brush NameForegroundSelected { get; init; }
    
    public Brush TypeForeground { get; init; }
    public Brush TypeForegroundSelected { get; init; }
    
    public Brush DescriptionForeground { get; init; }
    public Brush DescriptionForegroundSelected { get; init; }

    private static Color ToMediaColor(System.Drawing.Color color) => Color.FromArgb(color.A, color.R, color.G, color.B);
    private static Brush ToBrush(System.Drawing.Color color) => new SolidColorBrush(ToMediaColor(color));
    public event PropertyChangedEventHandler PropertyChanged;
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}