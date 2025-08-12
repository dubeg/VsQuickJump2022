using System;
using System.Collections.Immutable;
using System.Windows.Media;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;

namespace QuickJump2022.Tools;

/// <summary>
/// https://github.com/ryzngard/Carnation/blob/main/Carnation/Helpers/FontsAndColorsHelpers.cs
/// </summary>
public static class FontsAndColorsHelper {
    // GUID is stored here: "HKEY_USERS\.DEFAULT\Software\Microsoft\VisualStudio\[VS_VER]_Config\FontAndColo‌​rs\Text Editor"
    // where VS_VER is the actual Visual Studio version: 10.0, 11.0, 12.0, 14.0, etc.
    // Or via api:
    // Microsoft.VisualStudio.Editor.DefGuidList.guidTextEditorFont‌​Category
    public static readonly Guid TextEditorCategory = new Guid("A27B4E24-A735-4d1d-B8E7-9716E1E3D8E0");
    public static readonly Guid TextEditorMEFItemsCategory = new Guid("75A05685-00A8-4DED-BAE5-E7A50BFA929A");
    public static readonly Guid TextEditorLanguageServiceCategory = new Guid("E0187991-B458-4F7E-8CA9-42C9A573B56C");
    public static readonly Guid TextEditorManagerCategory = new Guid("58E96763-1D3B-4E05-B6BA-FF7115FD0B7B");
    public static readonly Guid TextEditorMarkerCategory = new Guid("FF349800-EA43-46C1-8C98-878E78F46501");

    internal static readonly (FontFamily FontFamily, double FontSize) DefaultFontInfo = (new FontFamily("Consolas"), 13.0);
    private static readonly (Color Foreground, Color Background) DefaultTextColors = (Colors.Black, Colors.White);

    private static IVsFontAndColorStorage s_fontsAndColorStorage;
    private static IVsFontAndColorDefaultsProvider s_fontsAndColorDefaultsProvider;
    private static IVsUIShell2 s_vsUIShell2;

    private const uint InvalidColorRef = 0xff000000;

    private static readonly Guid[] s_categories = {
        TextEditorManagerCategory,
        TextEditorMEFItemsCategory,
        // TextEditorMarkerCategory,
        // TextEditorCategory,
        // TextEditorLanguageServiceCategory
    };

    public static ImmutableDictionary<Guid, ImmutableArray<AllColorableItemInfo>> GetTextEditorInfos() {
        ThreadHelper.ThrowIfNotOnUIThread();
        EnsureInitialized();
        return s_categories.ToImmutableDictionary(catagory => catagory, catagory => GetCategoryItems(catagory));
        static ImmutableArray<AllColorableItemInfo> GetCategoryItems(Guid category) {
            if (s_fontsAndColorDefaultsProvider.GetObject(category, out var obj) != VSConstants.S_OK) {
                return ImmutableArray<AllColorableItemInfo>.Empty;
            }
            var fontAndColorDefaults = (IVsFontAndColorDefaults)obj;
            if (fontAndColorDefaults.GetItemCount(out var count) != VSConstants.S_OK) {
                return ImmutableArray<AllColorableItemInfo>.Empty;
            }
            var builder = ImmutableArray.CreateBuilder<AllColorableItemInfo>();
            var items = new AllColorableItemInfo[1];
            for (var index = 0; index < count; index++) {
                if (fontAndColorDefaults.GetItem(index, items) == VSConstants.S_OK) {
                    builder.Add(items[0]);
                }
            }
            return builder.ToImmutable();
        }
    }
    
    public static (Color Foreground, Color Background) GetPlainTextColors() {
        ThreadHelper.ThrowIfNotOnUIThread();
        EnsureInitialized();
        if (s_fontsAndColorStorage.OpenCategory(TextEditorManagerCategory, (uint)(__FCSTORAGEFLAGS.FCSF_READONLY | __FCSTORAGEFLAGS.FCSF_LOADDEFAULTS)) != VSConstants.S_OK) {
            return DefaultTextColors;
        }
        try {
            var colorItems = new ColorableItemInfo[1];
            if (s_fontsAndColorStorage.GetItem("Plain Text", colorItems) != VSConstants.S_OK) {
                return DefaultTextColors;
            }
            var colorItem = colorItems[0];
            var foreground = TryGetColor(colorItem.crForeground) ?? DefaultTextColors.Foreground;
            var background = TryGetColor(colorItem.crBackground) ?? DefaultTextColors.Background;
            return (foreground, background);
        }
        finally {
            s_fontsAndColorStorage.CloseCategory();
        }
    }
    
    public static (FontFamily FontFamily, double FontSize) GetEditorFontInfo(bool scaleFontSize = true) {
        ThreadHelper.ThrowIfNotOnUIThread();
        EnsureInitialized();
        var fontsAndColorStorage = ServiceProvider.GlobalProvider.GetService<SVsFontAndColorStorage, IVsFontAndColorStorage>();
        if (fontsAndColorStorage is null) {
            return DefaultFontInfo;
        }
        // Open Text Editor category for readonly access.
        if (fontsAndColorStorage.OpenCategory(TextEditorMEFItemsCategory, (uint)(__FCSTORAGEFLAGS.FCSF_READONLY | __FCSTORAGEFLAGS.FCSF_LOADDEFAULTS)) != VSConstants.S_OK) {
            return DefaultFontInfo;
        }
        try {
            var logFont = new LOGFONTW[1];
            var fontInfo = new FontInfo[1];
            if (fontsAndColorStorage.GetFont(logFont, fontInfo) != VSConstants.S_OK) {
                return DefaultFontInfo;
            }
            var fontFamily = fontInfo[0].bFaceNameValid == 1
                ? new FontFamily(fontInfo[0].bstrFaceName)
                : DefaultFontInfo.FontFamily;
            var fontSize = fontInfo[0].bPointSizeValid == 1
                ? scaleFontSize
                    ? Math.Abs(logFont[0].lfHeight) * GetDipsPerPixel()
                    : fontInfo[0].wPointSize
                : DefaultFontInfo.FontSize;
            return (fontFamily, DefaultFontInfo.FontSize);
        }
        finally {
            fontsAndColorStorage.CloseCategory();
        }
    }
    
    private static double GetDipsPerPixel() {
        var dc = UnsafeNativeMethods.GetDC(IntPtr.Zero);
        if (dc != IntPtr.Zero) {
            // Getting the DPI from the desktop is bad, but some callers just have no context for what monitor they are on.
            double fallbackDpi = UnsafeNativeMethods.GetDeviceCaps(dc, UnsafeNativeMethods.LOGPIXELSX);
            UnsafeNativeMethods.ReleaseDC(IntPtr.Zero, dc);
            return fallbackDpi / 96.0;
        }
        return 1;
    }
    
    private static void EnsureInitialized() {
        ThreadHelper.ThrowIfNotOnUIThread();
        if (s_fontsAndColorStorage is null) {
            s_fontsAndColorStorage = ServiceProvider.GlobalProvider.GetService<SVsFontAndColorStorage, IVsFontAndColorStorage>();
            s_vsUIShell2 = ServiceProvider.GlobalProvider.GetService<SVsUIShell, IVsUIShell2>();
            s_fontsAndColorDefaultsProvider = (IVsFontAndColorDefaultsProvider)ServiceProvider.GlobalProvider.GetService(Guid.Parse("DAF27B38-80B3-4C58-8133-AFD41C36C79A"));
        }
    }
    
    public static Color? TryGetColor(uint colorRef) {
        ThreadHelper.ThrowIfNotOnUIThread();
        var fontAndColorUtilities = (IVsFontAndColorUtilities)s_fontsAndColorStorage;
        if (fontAndColorUtilities.GetColorType(colorRef, out var colorType) != VSConstants.S_OK) {
            return null;
        }
        uint? win32Color = null;
        if (colorType == (int)__VSCOLORTYPE.CT_INVALID) {
            return null;
        }
        else if (colorType == (int)__VSCOLORTYPE.CT_AUTOMATIC) {
            return null;
        }
        else if (colorType == (int)__VSCOLORTYPE.CT_RAW) {
            win32Color = colorRef;
        }
        else if (colorType == (int)__VSCOLORTYPE.CT_COLORINDEX) {
            var encodedIndex = new COLORINDEX[1];
            if (fontAndColorUtilities.GetEncodedIndex(colorRef, encodedIndex) == VSConstants.S_OK &&
                fontAndColorUtilities.GetRGBOfIndex(encodedIndex[0], out var decoded) == VSConstants.S_OK) {
                if (encodedIndex[0] == COLORINDEX.CI_SYSTEXT_BK ||
                    encodedIndex[0] == COLORINDEX.CI_SYSTEXT_FG) {
                    return null;
                }
                win32Color = encodedIndex[0] == COLORINDEX.CI_USERTEXT_BK
                    ? decoded & 0x00ffffff
                    : decoded | 0xff000000;
            }
        }
        else if (colorType == (int)__VSCOLORTYPE.CT_SYSCOLOR) {
            if (fontAndColorUtilities.GetEncodedSysColor(colorRef, out var sysColor) == VSConstants.S_OK) {
                win32Color = (uint)sysColor;
            }
        }
        else if (colorType == (int)__VSCOLORTYPE.CT_VSCOLOR) {
            if (fontAndColorUtilities.GetEncodedVSColor(colorRef, out var vsSysColor) == VSConstants.S_OK &&
                s_vsUIShell2.GetVSSysColorEx(vsSysColor, out var rgbColor) == VSConstants.S_OK) {
                win32Color = rgbColor;
            }
        }
        return win32Color.HasValue
            ? (Color?)FromWin32Color((int)win32Color.Value)
            : null;
        Color FromWin32Color(int color) {
            var drawingColor = System.Drawing.ColorTranslator.FromWin32(color);
            return Color.FromRgb(drawingColor.R, drawingColor.G, drawingColor.B);
        }
    }
    
    internal static uint GetColorRef(Color color, Color defaultColor) {
        return (color == defaultColor)
            ? InvalidColorRef
            : ToWin32Color(color);
        uint ToWin32Color(Color color) {
            return (uint)(color.R | color.G << 8 | color.B << 16);
        }
    }
}