using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.Imaging.Interop;
using QuickJump2022.Options;
using QuickJump2022.Tools;

namespace QuickJump2022.Models;

public class ListItemViewModel : INotifyPropertyChanged {

    public ListItemViewModel(ListItemBase item) {
        Item = item;
        IconMoniker =
            item is ListItemSymbol symbol ? IconMoniker = KnownMonikerUtils.GetCodeMoniker(symbol.Item.BindType)
            : item is ListItemFile file ? IconMoniker = KnownMonikerUtils.GetFileMoniker(file.FileExtension)
            : KnownMonikers.None;
    }

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
    public bool UseCustomForeground { get; set; } = false;
    public Brush NameForeground { get; set; }
    public Brush TypeForeground { get; set; }

    public event PropertyChangedEventHandler PropertyChanged;
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}