using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace QuickJump2022.Forms;

/// <summary>
/// A frameless window that captures keyboard input and forwards it to the main search window
/// </summary>
public partial class InputCaptureWindow : Window, INotifyPropertyChanged {
    public event PropertyChangedEventHandler PropertyChanged;
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    private MainSearchWindow _ownerWnd;
    private string _searchFontFamily;
    private double _searchFontSize;
    public string SearchFontFamily { 
        get => _searchFontFamily;
        set {
            _searchFontFamily = value;
            OnPropertyChanged();
        }
    }
    public double SearchFontSize { 
        get => _searchFontSize;
        set {
            _searchFontSize = value;
            OnPropertyChanged();
        }
    }

    public InputCaptureWindow(MainSearchWindow ownerWnd) {
        _ownerWnd = ownerWnd;
        InitializeComponent();
        SearchFontFamily = ownerWnd.SearchFontFamily;
        SearchFontSize = ownerWnd.SearchFontSize;
        DataContext = this;
        Loaded += (s, e) => {
            var wndHelper = new WindowInteropHelper(this);
            int exStyle = (int)GetWindowLong(wndHelper.Handle, (int)GetWindowLongFields.GWL_EXSTYLE);
            exStyle |= (int)ExtendedWindowStyles.WS_EX_TOOLWINDOW;
            SetWindowLong(wndHelper.Handle, (int)GetWindowLongFields.GWL_EXSTYLE, (IntPtr)exStyle);

            txtInput.Focus();
            Keyboard.Focus(txtInput);
            _ownerWnd.Dispatcher.BeginInvoke(() => _ownerWnd.OnInputWindowLoaded());
        };
    }

    private void txtInput_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e) { 
        var input = txtInput.Text.Trim();
        _ownerWnd.Dispatcher.BeginInvoke(() => _ownerWnd.OnInputTextChanged(input));
    }

    private void txtInput_PreviewKeyDown(object sender, KeyEventArgs e) {
        switch (e.Key) {
            case Key.Escape:
                _ownerWnd.Dispatcher.BeginInvoke(_ownerWnd.OnEscapePressed);
                e.Handled = true;
                break;

            case Key.Return:
                _ownerWnd.Dispatcher.BeginInvoke(_ownerWnd.OnEnterPressed);
                e.Handled = true;
                break;
            case Key.Up:
                _ownerWnd.Dispatcher.BeginInvoke(_ownerWnd.OnArrowUp);
                e.Handled = true;
                break;
            case Key.Down:
                _ownerWnd.Dispatcher.BeginInvoke(_ownerWnd.OnArrowDown);
                e.Handled = true;
                break;
            case Key.PageUp:
                _ownerWnd.Dispatcher.BeginInvoke(_ownerWnd.OnPageUp);
                e.Handled = true;
                break;
            case Key.PageDown:
                _ownerWnd.Dispatcher.BeginInvoke(_ownerWnd.OnPageDown);
                e.Handled = true;
                break;
            case Key.A when Keyboard.Modifiers == ModifierKeys.Control:
                txtInput.SelectAll();
                e.Handled = true;
                break;
            case Key.Back when Keyboard.Modifiers == ModifierKeys.Control:
                txtInput.Text = "";
                e.Handled = true;
                break;
        }
    }

    public void UpdateRectangle(Rect rect) {
        Left = rect.Left;
        Top = rect.Top;
        Width = rect.Width;
        Height = rect.Height;
    }



    #region Window styles
    [Flags]
    public enum ExtendedWindowStyles {
        // ...
        WS_EX_TOOLWINDOW = 0x00000080,
        // ...
    }

    public enum GetWindowLongFields {
        // ...
        GWL_EXSTYLE = (-20),
        // ...
    }

    [DllImport("user32.dll")]
    public static extern IntPtr GetWindowLong(IntPtr hWnd, int nIndex);

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
            // use SetWindowLongPtr
            result = IntSetWindowLongPtr(hWnd, nIndex, dwNewLong);
            error = Marshal.GetLastWin32Error();
        }

        if ((result == IntPtr.Zero) && (error != 0)) {
            throw new System.ComponentModel.Win32Exception(error);
        }

        return result;
    }

    [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr", SetLastError = true)]
    private static extern IntPtr IntSetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

    [DllImport("user32.dll", EntryPoint = "SetWindowLong", SetLastError = true)]
    private static extern Int32 IntSetWindowLong(IntPtr hWnd, int nIndex, Int32 dwNewLong);

    private static int IntPtrToInt32(IntPtr intPtr) {
        return unchecked((int)intPtr.ToInt64());
    }

    [DllImport("kernel32.dll", EntryPoint = "SetLastError")]
    public static extern void SetLastError(int dwErrorCode);
    #endregion
}
