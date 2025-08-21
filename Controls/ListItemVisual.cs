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
    private double IconLeftMargin => 5.0;
    private double IconRightMargin => 5.0;
    private double TextRightMargin => 5.0;
    private double SpacingBetweenNameAndType => 0.0;
    
    private ListItemViewModel _viewModel;
    private ImageSource _cachedIconImage;
    private ImageMoniker _cachedMoniker;
    private System.Drawing.Color _backColor;

    /// <summary>
    /// Font size of type & description.
    /// </summary>
    public static readonly DependencyProperty HintFontSizeProperty = DependencyProperty.Register(
        name: nameof(HintFontSize),
        propertyType: typeof(double),
        ownerType: typeof(ListItemVisual),
        new FrameworkPropertyMetadata(0d, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure)
    );

    /// <summary>
    /// Font size of type & description.
    /// </summary>
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
        var dpi = VisualTreeHelper.GetDpi(Application.Current.MainWindow);
        var pixelsPerDip = dpi.PixelsPerDip;

        var vm = _viewModel;
        if (vm == null) return;

        // Brushes
        var defaultTextBrush = (Brush)Application.Current.TryFindResource(ThemedDialogColors.ListBoxTextBrushKey);
        var disabledTextBrush = (Brush)Application.Current.TryFindResource(ThemedDialogColors.ListItemDisabledTextBrushKey);
        var nameBrush = vm.NameForeground ?? defaultTextBrush ?? Brushes.Black;
        var typeBrush = (vm.IsSelected ? defaultTextBrush : disabledTextBrush) ?? Brushes.Gray;
        var descBrush = typeBrush;

        // Fonts
        var nameTypeface = new Typeface(FontFamily, FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);
        var hintFontSize = HintFontSize;
        if (hintFontSize == 0) hintFontSize = FontSize;

        // Layout metrics
        var iconX = IconLeftMargin;
        var iconY = (ActualHeight - IconSize) / 2.0;
        var leftTextStartX = iconX + IconSize + IconRightMargin;
        var rightLimitX = ActualWidth - TextRightMargin;

        // Measure texts without constraints
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
            MaxLineCount = 1,
        };
        if (_viewModel.Item is ListItemSymbol) {
            // For example: "methodName(param1, param2, param3)"
            // We want to color parameters and punctuation differently.
            var text = nameText;
            int openParen = text.IndexOf('(');
            int closeParen = text.LastIndexOf(')');
            if (openParen >= 0 && closeParen > openParen) {
                // Color parameters
                var parametersForeground = _viewModel.NameParametersForeground;
                var punctuationForeground = _viewModel.NamePunctuationMarksForeground;

                // Scan between parentheses
                int paramStart = openParen + 1;
                int i = paramStart;
                while (i < closeParen) {
                    // Skip whitespace
                    while (i < closeParen && char.IsWhiteSpace(text[i])) i++;
                    int paramEnd = i;
                    // Find next comma or closing paren
                    while (paramEnd < closeParen && text[paramEnd] != ',') paramEnd++;
                    int length = paramEnd - i;
                    if (length > 0 && parametersForeground != null) {
                        // Set parameter color
                        name.SetForegroundBrush(parametersForeground, i, length);
                    }
                    // Set comma color
                    if (paramEnd < closeParen && text[paramEnd] == ',' && punctuationForeground != null) {
                        name.SetForegroundBrush(punctuationForeground, paramEnd, 1);
                    }
                    i = paramEnd + 1;
                }
                // Set color for opening and closing parenthesis
                name.SetForegroundBrush(punctuationForeground, openParen, 1);
                name.SetForegroundBrush(punctuationForeground, closeParen, 1);
            }
        }

        var typeText = vm.Type ?? string.Empty;
        var type = new FormattedText(
            typeText,
            CultureInfo.CurrentUICulture,
            FlowDirection.LeftToRight,
            nameTypeface,
            hintFontSize,
            typeBrush,
            pixelsPerDip
        ) {
            Trimming = TextTrimming.CharacterEllipsis,
            MaxLineCount = 1,
        };

        var descriptionText = vm.Description ?? string.Empty;
        var description = new FormattedText(
            descriptionText,
            CultureInfo.CurrentUICulture,
            FlowDirection.LeftToRight,
            nameTypeface,
            hintFontSize,
            descBrush,
            pixelsPerDip
        ) { 
            Trimming = TextTrimming.CharacterEllipsis,
            MaxLineCount = 1,
        };

        var totalAvailable = Math.Max(0, rightLimitX - leftTextStartX);
        var nameDesired = name.WidthIncludingTrailingWhitespace;
        var typeDesired = type.WidthIncludingTrailingWhitespace;
        var descDesired = description.WidthIncludingTrailingWhitespace;

        // Allocate widths by priority: Name > Type > Desc
        var nameWidth = Math.Min(nameDesired, totalAvailable);
        var spaceAfterName = nameWidth > 0 && totalAvailable - nameWidth > SpacingBetweenNameAndType && typeDesired > 0 ? SpacingBetweenNameAndType : 0.0;
        var remainingAfterName = Math.Max(0, totalAvailable - nameWidth - spaceAfterName);

        var typeWidth = Math.Min(typeDesired, remainingAfterName);
        var spaceAfterType = typeWidth > 0 && remainingAfterName - typeWidth > SpacingBetweenNameAndType && descDesired > 0 ? SpacingBetweenNameAndType : 0.0;
        var remainingAfterType = Math.Max(0, remainingAfterName - typeWidth - spaceAfterType);

        var descWidth = Math.Min(descDesired, remainingAfterType);

        // Apply constraints so FormattedText computes trimmed glyphs
        name.MaxTextWidth = Math.Max(0, nameWidth);
        type.MaxTextWidth = Math.Max(0, typeWidth);
        description.MaxTextWidth = Math.Max(0, descWidth);

        // Compute positions (desc right-aligned)
        var descX = rightLimitX - descWidth;
        var typeX = leftTextStartX + nameWidth + (typeWidth > 0 ? spaceAfterName : 0.0);
        var nameX = leftTextStartX;
        var textBaselineY = (ActualHeight - Math.Max(IconSize, Math.Max(name.Height, Math.Max(type.Height, description.Height)))) / 2.0;

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
        // Draw texts with prioritized allocated widths
        // ---------------------
        drawingContext.DrawText(name, new Point(nameX, textBaselineY));
        if (typeWidth > 0)
            drawingContext.DrawText(type, new Point(typeX, textBaselineY + (FontSize - hintFontSize)));
        if (descWidth > 0)
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
