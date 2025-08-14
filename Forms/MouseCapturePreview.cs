using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Microsoft.VisualStudio.Utilities;

namespace QuickJump2022.Forms;

public class MouseCapturePreview {
    private Window _window;
    private PreProcessInputEventHandler _eventHandler;
    private EventHandler _onActivatedHandler;

    public MouseCapturePreview(Window window) {
        _window = window;
        _onActivatedHandler = (s, e) => { _window.CaptureMouse(); };
        _eventHandler = (s, e) => {
            if (e.StagingItem.Input is MouseButtonEventArgs)
                Handler(s, (MouseButtonEventArgs)e.StagingItem.Input);
        };
        InputManager.Current.PreProcessInput += _eventHandler;
        _window.Activated += _onActivatedHandler;
        _window.Closed += (s, e) => {
            InputManager.Current.PreProcessInput -= _eventHandler;
            
            _window.Activated -= _onActivatedHandler;
        };
    }
    
    void Handler(object sender, MouseButtonEventArgs e) {
        var element = _window;
        if (e.LeftButton == MouseButtonState.Pressed) {
            element.ReleaseMouseCapture();
            // Mouse.Capture(element); // ?

            //var screenPoint = new Point(point.X, point.Y);
            //var logicalPoint = PresentationSource.FromVisual(this).CompositionTarget.TransformFromDevice.Transform(screenPoint);
            //var mainWindow = Application.Current.MainWindow;
            //var mainWindowBounds = new Rect(mainWindow.Left, mainWindow.Top, mainWindow.ActualWidth, mainWindow.ActualHeight);
            //var modalWindowBounds = new Rect(this.Left, this.Top, this.ActualWidth, this.ActualHeight);
            //var inMain = mainWindowBounds.Contains(logicalPoint);
            //var inModal = modalWindowBounds.Contains(logicalPoint);

            var point = e.GetPosition(element);
            var screenPoint = element.PointToScreen(point);
            var screenPoint2 = element.DeviceToLogicalPoint(screenPoint);
            // TODO: handle when mainWindow is maximised.
            var inMain = Application.Current.MainWindow.RestoreBounds.Contains(screenPoint2);
            var inModal = element.RestoreBounds.Contains(screenPoint2);
            if (inMain && !inModal) {
                System.Windows.MessageBox.Show("You clicked outside.");
                element.Close();
            }
        }
        else {
            element.CaptureMouse();
        }
    }
}
