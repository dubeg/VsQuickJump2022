using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;

namespace QuickJump2022.QuickJump.Tools;
public static class WindowUtils {

    [Flags]
    public enum ExtendedWindowStyles {
        // ...
        WS_EX_TOOLWINDOW = 0x00000080,
        WS_EX_CLIENTEDGE = 0x00000200,
        WS_EX_DLGMODALFRAME = 0x00000001,
        WS_EX_NOACTIVATE = 0x08000000,
        WS_EX_WINDOWEDGE = 0x00000100,
        WS_EX_STATICEDGE = 0x00020000,
    }

    public enum GetWindowLongFields {
        // ...
        GWL_EXSTYLE = (-20),
        // ...
    }

    [DllImport("user32.dll")]
    public static extern IntPtr GetWindowLong(IntPtr hWnd, int nIndex);


    [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr", SetLastError = true)]
    private static extern IntPtr IntSetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

    [DllImport("user32.dll", EntryPoint = "SetWindowLong", SetLastError = true)]
    private static extern Int32 IntSetWindowLong(IntPtr hWnd, int nIndex, Int32 dwNewLong);

    private static int IntPtrToInt32(IntPtr intPtr) {
        return unchecked((int)intPtr.ToInt64());
    }

    [DllImport("kernel32.dll", EntryPoint = "SetLastError")]
    public static extern void SetLastError(int dwErrorCode);

    public static IntPtr SetWindowLong(IntPtr hWnd, int nIndex, IntPtr dwNewLong) {
        int error = 0;
        IntPtr result = IntPtr.Zero;
        // Win32 SetWindowLong doesn't clear error on success
        SetLastError(0);
        if (IntPtr.Size == 4) {
            // use SetWindowLong
            Int32 tempResult = IntSetWindowLong(hWnd, nIndex, IntPtrToInt32(dwNewLong));
            error = Marshal.GetLastWin32Error();
            result = new IntPtr(tempResult);
        }
        else {
            result = IntSetWindowLongPtr(hWnd, nIndex, dwNewLong);
            error = Marshal.GetLastWin32Error();
        }
        if ((result == IntPtr.Zero) && (error != 0)) {
            throw new System.ComponentModel.Win32Exception(error);
        }
        return result;
    }

    [DllImport("user32.dll", SetLastError = true)]
    public static extern IntPtr SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    public static extern uint GetWindowThreadProcessId(IntPtr hWnd, IntPtr ProcessId);

    [DllImport("user32.dll")]
    public static extern bool AttachThreadInput(uint idAttach, uint idAttachTo, bool fAttach);

    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    public static extern int GetSystemMetrics(int smIndex);
    public const int SM_CMONITORS = 80;

    [DllImport("user32.dll")]
    public static extern bool SystemParametersInfo(int nAction, int nParam, ref RECT rc, int nUpdate);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    public static extern bool GetMonitorInfo(HandleRef hmonitor, [In, Out] MONITORINFOEX info);

    [DllImport("user32.dll")]
    public static extern IntPtr MonitorFromWindow(HandleRef handle, int flags);

    public struct RECT {
        public int left;
        public int top;
        public int right;
        public int bottom;
        public int width { get { return right - left; } }
        public int height { get { return bottom - top; } }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4, CharSet = CharSet.Auto)]
    public class MONITORINFOEX {
        public int cbSize = Marshal.SizeOf(typeof(MONITORINFOEX));
        public RECT rcMonitor = new RECT();
        public RECT rcWork = new RECT();
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public char[] szDevice = new char[32];
        public int dwFlags;
    }

    public static void ActivateWindow(Window window) {
        var hwnd = new WindowInteropHelper(window).EnsureHandle();
        var threadId1 = GetWindowThreadProcessId(GetForegroundWindow(), IntPtr.Zero);
        var threadId2 = GetWindowThreadProcessId(hwnd, IntPtr.Zero);

        if (threadId1 != threadId2) {
            AttachThreadInput(threadId1, threadId2, true);
            SetForegroundWindow(hwnd);
            AttachThreadInput(threadId1, threadId2, false);
        }
        else
            SetForegroundWindow(hwnd);
    }

    public static Rect GetMaximizedWindowBounds(Window window) {
        var windowHandle = new WindowInteropHelper(window).Handle;
        var screen = Screen.FromHandle(windowHandle);
        // The WorkingArea property provides the bounds of the screen, excluding the taskbar
        var workingArea = screen.WorkingArea;
        return new Rect(workingArea.Left, workingArea.Top, workingArea.Width, workingArea.Height);
    }

    public static Rect GetMaximizedWindowBounds(IntPtr hWnd) {
        Rect r;
        var multimonSupported = GetSystemMetrics(SM_CMONITORS) != 0;
        if (!multimonSupported) {
            var rc = new RECT();
            SystemParametersInfo(48, 0, ref rc, 0);
            r = new Rect(rc.left, rc.top, rc.width, rc.height);
        }
        else {
            var hmonitor = MonitorFromWindow(new HandleRef(null, hWnd), 2);
            var info = new MONITORINFOEX();
            GetMonitorInfo(new HandleRef(null, hmonitor), info);
            r = new Rect(info.rcWork.left, info.rcWork.top, info.rcWork.width, info.rcWork.height);
        }
        return r;
    }

    public static void SetWindowStyle_ToolWindow(Window window) {
        var wndHelper = new WindowInteropHelper(window);
        int exStyle = (int)GetWindowLong(wndHelper.Handle, (int)GetWindowLongFields.GWL_EXSTYLE);
        exStyle |= (int)ExtendedWindowStyles.WS_EX_TOOLWINDOW;
        SetWindowLong(wndHelper.Handle, (int)GetWindowLongFields.GWL_EXSTYLE, (IntPtr)exStyle);
    }

    public static void SetWindowStyle_NoBorder(Window window) {
        var wndHelper = new WindowInteropHelper(window);
        int exStyle = (int)GetWindowLong(wndHelper.Handle, (int)GetWindowLongFields.GWL_EXSTYLE);
        exStyle |= (int)ExtendedWindowStyles.WS_EX_TOOLWINDOW;
        var removedFlags = 
            //ExtendedWindowStyles.WS_EX_DLGMODALFRAME
            ExtendedWindowStyles.WS_EX_CLIENTEDGE
            | ExtendedWindowStyles.WS_EX_STATICEDGE;
        exStyle &= ~((int)removedFlags);
        SetWindowLong(wndHelper.Handle, (int)GetWindowLongFields.GWL_EXSTYLE, (IntPtr)exStyle);
    }
}
