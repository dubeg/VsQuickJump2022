using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.Imaging.Interop;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.PlatformUI;
using QuickJump2022.Data;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace QuickJump2022.Tools;

public class KnownMonikerService {
    private readonly AsyncPackage _package;
    private readonly Dictionary<string, BitmapSource> _iconCache = new();
    private IVsImageService2 _imageService;

    public KnownMonikerService(AsyncPackage package) {
        _package = package;
    }

    public async Task InitializeAsync() {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
        _imageService = await _package.GetServiceAsync(typeof(SVsImageService)) as IVsImageService2;
    }

    public async Task<BitmapSource> GetMonikerImageAsync(ImageMoniker moniker, int size) {
        // Create a cache key 
        var cacheKey = $"{moniker.Guid}_{moniker.Id}_{size}";

        // Check cache first
        if (_iconCache.TryGetValue(cacheKey, out var cachedIcon)) {
            return cachedIcon;
        }

        // Switch to VS main UI thread to access KnownMonikers
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

        return await moniker.ToBitmapSourceAsync(size);

        //if (_imageService == null) {
        //    return null;
        //}
        //// Use the provided background color or get from theme
        //bgColor = backgroundColor ?? bgColor;
        
        //var imageAttributes = new ImageAttributes {
        //    Flags = unchecked((uint)_ImageAttributesFlags.IAF_RequiredFlags | (uint)_ImageAttributesFlags.IAF_Background),
        //    ImageType = (uint)_UIImageType.IT_Bitmap,
        //    Format = (uint)_UIDataFormat.DF_WPF,
        //    Background = bgColor,
        //    LogicalHeight = size,
        //    LogicalWidth = size,
        //    StructSize = Marshal.SizeOf(typeof(ImageAttributes))
        //};

        //try {
        //    IVsUIObject result = _imageService.GetImage(moniker, imageAttributes);

        //    if (result != null) {
        //        result.get_Data(out var data);

        //        // The bitmap is now safe to use across threads
        //        var bitmapSource = data as BitmapSource;
        //        if (bitmapSource != null) {
        //            if (!bitmapSource.IsFrozen) {
        //                bitmapSource.Freeze(); // Make it thread-safe
        //            }

        //            // Cache the result
        //            _iconCache[cacheKey] = bitmapSource;
        //            return bitmapSource;
        //        }
        //    }
        //}
        //catch (Exception ex) {
        //    // Log error but don't crash
        //    ex.Log();
        //}

        //return null;
    }

    public async Task<BitmapSource> GetCodeIconAsync(Enums.EBindType bindType, int size) {
        var moniker = bindType switch {
            Enums.EBindType.Namespace => KnownMonikers.Namespace,
            Enums.EBindType.Class => KnownMonikers.Class,
            Enums.EBindType.Method => KnownMonikers.Method,
            Enums.EBindType.Property => KnownMonikers.Property,
            Enums.EBindType.Field => KnownMonikers.Field,
            Enums.EBindType.Enum => KnownMonikers.Enumeration,
            Enums.EBindType.Delegate => KnownMonikers.Delegate,
            Enums.EBindType.Event => KnownMonikers.Event,
            Enums.EBindType.Interface => KnownMonikers.Interface,
            Enums.EBindType.Struct => KnownMonikers.Structure,
            _ => KnownMonikers.QuestionMark
        };
        return await GetMonikerImageAsync(moniker, size);
    }

    public async Task<BitmapSource> GetFileIconAsync(string fileExtension, int size) {
        var moniker = fileExtension?.ToLowerInvariant() switch {
            ".cs" => KnownMonikers.CSFileNode,
            ".vb" => KnownMonikers.VBFileNode,
            ".fs" => KnownMonikers.FSFileNode,
            ".cpp" or ".cc" or ".cxx" => KnownMonikers.CPPFileNode,
            ".h" or ".hpp" => KnownMonikers.CPPHeaderFile,
            ".js" or ".ts" => KnownMonikers.JSScript,
            ".json" => KnownMonikers.JSONScript,
            ".xml" => KnownMonikers.XMLFile,
            ".xaml" => KnownMonikers.WPFFile,
            ".html" or ".htm" => KnownMonikers.HTMLFile,
            ".css" => KnownMonikers.StyleSheet,
            ".sql" => KnownMonikers.SQLQueryUnchecked,
            ".txt" => KnownMonikers.TextFile,
            ".md" => KnownMonikers.MarkdownFile,
            ".config" => KnownMonikers.ConfigurationFile,
            ".resx" => KnownMonikers.LocalResources, // Wrong icon
            ".png" or ".jpg" or ".jpeg" or ".gif" or ".bmp" => KnownMonikers.Image,
            ".sln" => KnownMonikers.Solution,
            ".csproj" or ".vbproj" or ".fsproj" => KnownMonikers.CSProjectNode,
            _ => KnownMonikers.Document
        };
        return await GetMonikerImageAsync(moniker, size);
    }

    public async Task PreloadCommonIconsAsync(int size) {
        // Preload common icons to improve performance
        var commonMonikers = new[] {
            KnownMonikers.Namespace,
            KnownMonikers.Class,
            KnownMonikers.Method,
            KnownMonikers.Property,
            KnownMonikers.Field,
            KnownMonikers.Enumeration,
            KnownMonikers.Delegate,
            KnownMonikers.Event,
            KnownMonikers.Interface,
            KnownMonikers.Structure,
            KnownMonikers.QuestionMark,
            KnownMonikers.CSFileNode,
            KnownMonikers.VBFileNode,
            KnownMonikers.FSFileNode,
            KnownMonikers.CPPFileNode,
            KnownMonikers.CPPHeaderFile,
            KnownMonikers.JSScript,
            KnownMonikers.JSONScript,
            KnownMonikers.XMLFile,
            KnownMonikers.WPFFile,
            KnownMonikers.HTMLFile,
            KnownMonikers.StyleSheet,
            KnownMonikers.SQLQueryUnchecked,
            KnownMonikers.TextFile,
            KnownMonikers.MarkdownFile,
            KnownMonikers.ConfigurationFile,
            KnownMonikers.LocalResources, // Wrong icon
            KnownMonikers.Image,
            KnownMonikers.Solution,
            KnownMonikers.CSProjectNode,
            KnownMonikers.Document
        };
        foreach (var moniker in commonMonikers) {
            await GetMonikerImageAsync(moniker, size);
        }
    }
}
