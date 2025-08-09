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
    private int _defaultIconSize = 32; // Default icon size, can be adjusted
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
        var cacheKey = $"{moniker.Guid}_{moniker.Id}_{size}";
        if (_iconCache.TryGetValue(cacheKey, out var cachedIcon)) {
            return cachedIcon;
        }
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
        return await moniker.ToBitmapSourceAsync(size);
    }

    public async Task<BitmapSource> GetCodeIconAsync(Enums.EBindType bindType) {
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
        return await GetMonikerImageAsync(moniker, _defaultIconSize);
    }

    public async Task<BitmapSource> GetFileIconAsync(string fileExtension) {
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
        return await GetMonikerImageAsync(moniker, _defaultIconSize);
    }

    public async Task PreloadCommonIconsAsync() {
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
            await GetMonikerImageAsync(moniker, _defaultIconSize);
        }
    }
}
