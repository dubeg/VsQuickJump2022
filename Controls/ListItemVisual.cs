using System;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.Imaging.Interop;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell.Interop;
using QuickJump2022.Models;
using QuickJump2022.Services;

namespace QuickJump2022.Controls;

public class ListItemVisual : Control {

    private int IconSize => 16;
    private const double IconLeftMargin = 5.0;
    private const double IconRightMargin = 5.0;
    private const double TextRightMargin = 5.0;
    private const double SpacingBetweenNameAndType = 4.0;
    
    private ListItemViewModel _viewModel;
    private ImageSource _cachedIconImage;
    private ImageMoniker _cachedMoniker;
    private System.Drawing.Color _backColor;
    
    public static readonly DependencyProperty HintFontSizeProperty = DependencyProperty.Register(
        name: nameof(HintFontSize),
        propertyType: typeof(double),
        ownerType: typeof(ListItemVisual),
        new FrameworkPropertyMetadata(0d, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure)
    );

    public double HintFontSize {
        get => (double)GetValue(HintFontSizeProperty);
        set => SetValue(HintFontSizeProperty, value);
    }

    public ListItemVisual() {
        DataContextChanged += OnDataContextChanged;
        UseLayoutRounding = true;
        SnapsToDevicePixels = true;
        RenderOptions.SetBitmapScalingMode(this, BitmapScalingMode.NearestNeighbor);
        RenderOptions.SetEdgeMode(this, EdgeMode.Aliased);
    }

    protected override void OnRender(DrawingContext drawingContext) {
        base.OnRender(drawingContext);
        var useLRounding = UseLayoutRounding;
        if (ActualWidth <= 0 || ActualHeight <= 0) return;
        var dpi = VisualTreeHelper.GetDpi(this);
        var pixelsPerDip = dpi.PixelsPerDip;

        var vm = _viewModel;
        if (vm == null) return;

        // Brushes
        var defaultTextBrush = (Brush)Application.Current.TryFindResource(ThemedDialogColors.ListBoxTextBrushKey);
        var disabledTextBrush = (Brush)Application.Current.TryFindResource(ThemedDialogColors.ListItemDisabledTextBrushKey);
        var nameBrush = vm.NameForeground ?? defaultTextBrush ?? Brushes.Black;
        var auxBrush = (vm.IsSelected ? defaultTextBrush : disabledTextBrush) ?? Brushes.Gray;

        // Fonts
        var nameTypeface = new Typeface(FontFamily, FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);
        var hintFontSize = HintFontSize;
        if (hintFontSize == 0) hintFontSize = FontSize;

        // Layout metrics
        var iconX = IconLeftMargin;
        var iconY = (ActualHeight - IconSize) / 2.0;
        var leftTextStartX = iconX + IconSize + IconRightMargin;
        var rightLimitX = ActualWidth - TextRightMargin;

        // Right description
        var descriptionText = vm.Description ?? string.Empty;
        var description = new FormattedText(
            descriptionText,
            CultureInfo.CurrentUICulture,
            FlowDirection.LeftToRight,
            nameTypeface,
            hintFontSize,
            auxBrush,
            pixelsPerDip
        ) { Trimming = TextTrimming.CharacterEllipsis };

        // Max width allowed for description so that center area keeps at least some space
        var maxDescWidth = Math.Max(0, rightLimitX - leftTextStartX - 20);
        description.MaxTextWidth = maxDescWidth;
        var descWidth = Math.Min(description.Width, maxDescWidth);
        var descX = rightLimitX - descWidth;
        var textBaselineY = (ActualHeight - Math.Max(IconSize, description.Height)) / 2.0;

        // Center area: Name + Type
        var typeText = vm.Type ?? string.Empty;
        var type = new FormattedText(
            typeText,
            CultureInfo.CurrentUICulture,
            FlowDirection.LeftToRight,
            nameTypeface,
            hintFontSize,
            auxBrush,
            pixelsPerDip
        ) { Trimming = TextTrimming.None };
        var availableCenterWidth = Math.Max(0, descX - leftTextStartX);
        var nameAvailableWidth = Math.Max(0, availableCenterWidth - type.Width - SpacingBetweenNameAndType);

        var nameText = vm.Name ?? string.Empty;
        var name = new FormattedText(
            nameText,
            CultureInfo.CurrentUICulture,
            FlowDirection.LeftToRight,
            nameTypeface,
            FontSize,
            nameBrush,
            pixelsPerDip
        ) { 
            Trimming = TextTrimming.CharacterEllipsis, 
            MaxTextWidth = nameAvailableWidth 
        };

        // ---------------------
        // Icon
        // ---------------------
        if (_cachedIconImage != null) {
            if (useLRounding) {
                iconX = RoundLayoutValue(iconX, dpi.DpiScaleX);
                iconY = RoundLayoutValue(iconY, dpi.DpiScaleY);
            }
            var iconRect = new Rect(iconX, iconY, IconSize, IconSize);
            drawingContext.DrawImage(_cachedIconImage, iconRect);
        }

        // ---------------------
        // Name
        // ---------------------
        var nameX = leftTextStartX;
        var nameY = textBaselineY;
        drawingContext.DrawText(name, new Point(nameX, nameY));
        
        // ---------------------
        // type
        // ---------------------
        var typeX = nameX + name.WidthIncludingTrailingWhitespace + SpacingBetweenNameAndType;
        if (typeX < descX) {
            drawingContext.DrawText(type, new Point(typeX, textBaselineY + (FontSize - hintFontSize)));
        }

        // ---------------------
        // Description
        // ---------------------
        drawingContext.DrawText(description, new Point(descX, textBaselineY + (FontSize - hintFontSize)));
    }

    protected override Size MeasureOverride(Size constraint) {
        // Desired height accommodates icon and text comfortably
        var height = Math.Max(IconSize, FontSize) + 4; // matches ListBoxItem padding (0,2)
        return new Size(constraint.Width, height);
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
        _viewModel = DataContext as ListItemViewModel;
        Unsubscribe(e.OldValue as INotifyPropertyChanged);
        Subscribe(e.NewValue as INotifyPropertyChanged);
        _cachedIconImage = null;
        _cachedMoniker = default;
        _backColor = VSColorTheme.GetThemedColor(EnvironmentColors.ToolWindowBackgroundColorKey);
        var moniker = _viewModel.IconMoniker;
        if (_cachedIconImage is null || moniker.Id != _cachedMoniker.Id) {
            var dpiInfo = VisualTreeHelper.GetDpi(this);
            var dpiScale = dpiInfo.DpiScaleX;
            var requestedSize = (int)Math.Round(IconSize * dpiScale);
            var requestedDpi = 96;
            _cachedIconImage = IconImageCacheService.Instance.Get(moniker, requestedSize, (uint)_backColor.ToArgb(), requestedDpi);
            _cachedMoniker = moniker;
        }
        InvalidateVisual();
    }

    private void Subscribe(INotifyPropertyChanged npc) {
        if (npc == null) return;
        npc.PropertyChanged += OnVmPropertyChanged;
    }

    private void Unsubscribe(INotifyPropertyChanged npc) {
        if (npc == null) return;
        npc.PropertyChanged -= OnVmPropertyChanged;
    }

    private void OnVmPropertyChanged(object sender, PropertyChangedEventArgs e) {
        if (e.PropertyName == nameof(ListItemViewModel.IsSelected)
            || e.PropertyName == nameof(ListItemViewModel.NameForeground)
            || e.PropertyName == nameof(ListItemViewModel.Item)) {
            InvalidateVisual();
        }
    }

    private static double RoundLayoutValue(double value, double dpiScale) {
        var isScaleOne = Math.Abs(dpiScale - 1.0) < 0.0001;
        var newValue = isScaleOne ? Math.Round(value) : Math.Round(value * dpiScale) / dpiScale;
        if (double.IsNaN(newValue) || double.IsInfinity(newValue) || Math.Abs(newValue - double.MaxValue) < 0.0001) {
            return value;
        }
        return newValue;
    }
}
