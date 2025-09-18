using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.VisualStudio;

namespace QuickJump2022.TextEditor;

[Serializable]
public class VsKeyInfo {
    public static VsKeyInfo Create(Key key,
        char keyChar,
        byte virtualKey,
        KeyStates keyStates = default(KeyStates),
        bool shiftPressed = false,
        bool controlPressed = false,
        bool altPressed = false,
        bool capsLockToggled = false,
        bool numLockToggled = false) {

        return new VsKeyInfo {
            Key = key,
            KeyChar = keyChar,
            VirtualKey = virtualKey,
            KeyStates = keyStates,
            ShiftPressed = shiftPressed,
            ControlPressed = controlPressed,
            AltPressed = altPressed,
            CapsLockToggled = capsLockToggled,
            NumLockToggled = numLockToggled
        };
    }

    public Key Key { get; init; }
    public char KeyChar { get; init; }
    public byte VirtualKey { get; init; }
    public KeyStates KeyStates { get; init; }
    public bool ShiftPressed { get; init; }
    public bool ControlPressed { get; init; }
    public bool AltPressed { get; init; }
    public bool CapsLockToggled { get; init; }
    public bool NumLockToggled { get; init; }

    // --

    [DllImport("user32.dll")]
    public static extern short VkKeyScanEx(char ch, IntPtr dwhkl);

    [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
    public static extern IntPtr GetKeyboardLayout(int dwLayout);

    public static VsKeyInfo GetVsKeyInfo(IntPtr pvaIn, VSConstants.VSStd2KCmdID commandID) {
        var capsLockToggled = Keyboard.IsKeyToggled(Key.CapsLock);
        var numLockToggled = Keyboard.IsKeyToggled(Key.NumLock);
        var shiftPressed = Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);
        var controlPressed = Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);
        var altPressed = Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt);
        switch (commandID) {
            case VSConstants.VSStd2KCmdID.RETURN: return CreateSpecialKeyInfo(Key.Return, '\r', 0x0D);
            case VSConstants.VSStd2KCmdID.BACKSPACE: return CreateSpecialKeyInfo(Key.Back, '\b', 0x08);
            case VSConstants.VSStd2KCmdID.TAB: return CreateSpecialKeyInfo(Key.Tab, '\t', 0x09);
            case VSConstants.VSStd2KCmdID.BACKTAB: return CreateSpecialKeyInfo(Key.Tab, '\t', 0x09);
            case VSConstants.VSStd2KCmdID.CANCEL: return CreateSpecialKeyInfo(Key.Escape, '\x1B', 0x1B);
            case VSConstants.VSStd2KCmdID.UP: return CreateSpecialKeyInfo(Key.Up, '\0', 0x26);
            case VSConstants.VSStd2KCmdID.DOWN: return CreateSpecialKeyInfo(Key.Down, '\0', 0x28);
            case VSConstants.VSStd2KCmdID.PAGEUP: return CreateSpecialKeyInfo(Key.PageUp, '\0', 0x21);
            case VSConstants.VSStd2KCmdID.PAGEDN: return CreateSpecialKeyInfo(Key.PageDown, '\0', 0x22);
        }
        // For TYPECHAR / other commands with character data.
        if (pvaIn != IntPtr.Zero) {
            var keyChar = (char)(ushort)Marshal.GetObjectForNativeVariant(pvaIn);
            // convert from char to virtual key, using current thread's input locale
            var _pKeybLayout = new Lazy<IntPtr>(() => GetKeyboardLayout(0));
            var keyScan = VkKeyScanEx(keyChar, _pKeybLayout.Value);
            var virtualKey = (byte)(keyScan & 0x00ff);
            var key = KeyInterop.KeyFromVirtualKey(virtualKey);
            return VsKeyInfo.Create(
                key,
                keyChar,
                virtualKey,
                keyStates: KeyStates.Down,
                capsLockToggled: capsLockToggled,
                numLockToggled: numLockToggled,
                shiftPressed: shiftPressed,
                controlPressed: controlPressed,
                altPressed: altPressed
            );
        }
        Debug.Assert(false, $"Unexpected command: {commandID} with null pvaIn");
        return CreateSpecialKeyInfo(Key.None, '\0', 0);

        // Local helper function
        VsKeyInfo CreateSpecialKeyInfo(Key key, char keyChar, byte virtualKey) {
            return VsKeyInfo.Create(
                key,
                keyChar,
                virtualKey,
                keyStates: KeyStates.Down,
                capsLockToggled: capsLockToggled,
                numLockToggled: numLockToggled,
                shiftPressed: shiftPressed,
                controlPressed: controlPressed,
                altPressed: altPressed);
        }
    }
}
