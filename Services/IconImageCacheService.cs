using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.VisualStudio.Imaging.Interop;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace QuickJump2022.Services;

public sealed class IconImageCacheService {

	public static IconImageCacheService Instance { get; } = new IconImageCacheService();

	private readonly MemoryCache _cache;
	private readonly object _lock = new object();

	private IconImageCacheService() {
		_cache = new MemoryCache(new MemoryCacheOptions());
		VSColorTheme.ThemeChanged += (_) => Clear();
        var mainWindow = Application.Current.MainWindow;
        var mainWindowSource = PresentationSource.FromVisual(mainWindow) as HwndSource;
        if (mainWindowSource != null) {
            mainWindowSource.AddHook(WndProc);
        }
    }

    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled) {
        const int WM_DPICHANGED = 0x02E0;
        if (msg == WM_DPICHANGED) {
            // Flush icon cache on DPI change
            Clear();
            VS.StatusBar.ShowMessageAsync($"DPI: changed {DateTime.Now}");
        }
        return IntPtr.Zero;
    }

    public ImageSource Get(ImageMoniker moniker, int size, uint backgroundArgb, int dpi) {
		var key = CreateKey(moniker, size, backgroundArgb, dpi);
		if (_cache.Get(key) is ImageSource cached) return cached;
		lock (_lock) {
			if (_cache.Get(key) is ImageSource cachedInside) return cachedInside;
			var created = CreateFromVsImageService(moniker, size, backgroundArgb, dpi);
			if (created is Freezable freezable) freezable.Freeze();
			_cache.Set(key, created);
			return created;
		}
	}

	public void Clear() {
		lock (_lock) {
            _cache.Clear();
		}
	}

	private static string CreateKey(ImageMoniker moniker, int size, uint bg, int dpi) =>
		$"{moniker.Guid:N}:{moniker.Id}:{size}:{bg}:{dpi}";

	private static ImageSource CreateFromVsImageService(ImageMoniker moniker, int size, uint backgroundArgb, int dpi) {
		var imageService = Package.GetGlobalService(typeof(SVsImageService)) as IVsImageService2;
		if (imageService == null) return null;
		var attrs = new ImageAttributes {
			StructSize = Marshal.SizeOf(typeof(ImageAttributes)),
			ImageType = (uint)_UIImageType.IT_Bitmap,
			Format = (uint)_UIDataFormat.DF_WPF,
			Dpi = dpi,
			LogicalHeight = size,
			LogicalWidth = size,
            Flags = (uint)_ImageAttributesFlags.IAF_RequiredFlags | unchecked((uint)_ImageAttributesFlags.IAF_Background),
            Background = backgroundArgb,
		};
		var obj = imageService.GetImage(moniker, attrs);
		if (obj is ImageSource direct) return direct;
		if (obj is IVsUIObject uiObj && uiObj.get_Data(out var data) == 0) {
			return data as ImageSource;
		}
		return null;
	}
}


