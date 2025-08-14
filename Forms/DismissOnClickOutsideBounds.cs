using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using QuickJump2022.Tools;

namespace QuickJump2022.Interop;
public class DismissOnClickOutsideBounds {
    private Window _window;
    private HwndSource _hwndSource;
    private EventHandler DeactivationHandler;

    public DismissOnClickOutsideBounds(Window window) {
        _window = window;
        DeactivationHandler = (s, e) => DetectClickOutsideBounds(_window);
        _window.Deactivated += DeactivationHandler;
        _window.Closing += (s, e) => {
            _window.Deactivated -= DeactivationHandler;
            UnsafeNativeMethods.ReleaseCapture();
            _hwndSource?.RemoveHook(HwndHook);
        };
        _window.SourceInitialized += (s, e) => {
            var helper = new WindowInteropHelper(_window);
            _hwndSource = HwndSource.FromHwnd(helper.Handle);
            _hwndSource.AddHook(HwndHook);
            UnsafeNativeMethods.SetCapture(helper.Handle);
            _window.Activated += (s, e) => UnsafeNativeMethods.SetCapture(helper.Handle);
        };
    }

    private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled) {
        const int WM_LBUTTONDOWN = 0x0201;
        const int WM_RBUTTONDOWN = 0x0204;
        const int WM_MBUTTONDOWN = 0x0207;
        if (msg == WM_LBUTTONDOWN || msg == WM_RBUTTONDOWN || msg == WM_MBUTTONDOWN) {
            DetectClickOutsideBounds(_window);
        }
        return IntPtr.Zero;
    }

    private static void DetectClickOutsideBounds(Window window) {
        if (UnsafeNativeMethods.GetCursorPos(out var point)) {
            var screenPoint = new Point(point.X, point.Y);
            var logicalPoint = PresentationSource.FromVisual(window).CompositionTarget.TransformFromDevice.Transform(screenPoint);
            var mainWindow = Application.Current.MainWindow;
            var mainWindowBounds = new Rect(mainWindow.Left, mainWindow.Top, mainWindow.ActualWidth, mainWindow.ActualHeight);
            var modalWindowBounds = new Rect(window.Left, window.Top, window.ActualWidth, window.ActualHeight);
            var inMain = mainWindowBounds.Contains(logicalPoint);
            var inModal = modalWindowBounds.Contains(logicalPoint);
            if (inMain && !inModal) {
                window.Close();
            }
        }
    }
}
