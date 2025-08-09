using System.Windows;
using System.Windows.Input;

namespace QuickJump2022.Forms;

/// <summary>
/// A frameless window that captures keyboard input and forwards it to the main search window
/// </summary>
public partial class InputCaptureWindow : Window {
    private MainSearchWindow _ownerWnd;

    public InputCaptureWindow(MainSearchWindow ownerWnd) {
        _ownerWnd = ownerWnd;
        InitializeComponent();
        Loaded += (s, e) => {
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

    public void UpdatePosition(double left, double top) {
        Left = left;
        Top = top;
    }
}
