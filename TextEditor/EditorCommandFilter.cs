using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace QuickJump2022.TextEditor;

public class EditorCommandFilter : IOleCommandTarget {
    public delegate void InputEditorSpecialKeyHandler(object sender, VsKeyInfo keyInfo);
    private readonly Dictionary<Guid, uint[]> _allowedCommands;
    public bool HasFocus { private get; set; }
    public event InputEditorSpecialKeyHandler KeyPressed;
    public event EventHandler PreCommand;
    public event EventHandler PostCommand;
    public IOleCommandTarget NextCommandTarget { private get; set; }

    [DllImport("user32.dll")]
    public static extern short VkKeyScanEx(char ch, IntPtr dwhkl);

    [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
    public static extern IntPtr GetKeyboardLayout(int dwLayout);

    public EditorCommandFilter(Dictionary<Guid, uint[]> allowedCommands) => _allowedCommands = allowedCommands;

    public int QueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText) {
        ThreadHelper.ThrowIfNotOnUIThread(nameof(QueryStatus));
        if (HasFocus) {
            if (cCmds == 1U && !IsCommandAllowed(ref pguidCmdGroup, prgCmds[0].cmdID)) {
                prgCmds[0].cmdf |= 17U;
                return 0;
            }
            if (NextCommandTarget != null)
                return NextCommandTarget.QueryStatus(ref pguidCmdGroup, cCmds, prgCmds, pCmdText);
        }
        return -2147221248;
    }

    public int Exec(
        ref Guid pguidCmdGroup,
        uint nCmdID,
        uint nCmdexecopt,
        IntPtr pvaIn,
        IntPtr pvaOut
    ) {
        ThreadHelper.ThrowIfNotOnUIThread(nameof(Exec));
        if (HasFocus) {
            if (pguidCmdGroup == VSConstants.VSStd2K) {
                var commandID = (VSConstants.VSStd2KCmdID)nCmdID;
                switch (commandID) {
                    case VSConstants.VSStd2KCmdID.TYPECHAR: break;
                    case VSConstants.VSStd2KCmdID.BACKSPACE: break;
                    // --
                    case VSConstants.VSStd2KCmdID.TAB:
                    case VSConstants.VSStd2KCmdID.BACKTAB:
                    case VSConstants.VSStd2KCmdID.UP:
                    case VSConstants.VSStd2KCmdID.PAGEUP:
                    case VSConstants.VSStd2KCmdID.DOWN:
                    case VSConstants.VSStd2KCmdID.PAGEDN:
                    case VSConstants.VSStd2KCmdID.RETURN:
                    case VSConstants.VSStd2KCmdID.CANCEL:
                        var keyInfo = GetVsKeyInfo(pvaIn, commandID);
                        KeyPressed?.Invoke(this, keyInfo);
                        return VSConstants.S_OK;
                }
            }
            if (!IsCommandAllowed(ref pguidCmdGroup, nCmdID)) {
                return 0;
            }
            if (NextCommandTarget != null) {
                PreCommand?.Invoke(this, EventArgs.Empty);
                int num = NextCommandTarget.Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);
                PostCommand?.Invoke(this, EventArgs.Empty);
                return num;
            }
        }
        return -2147221244;
    }

    private VsKeyInfo GetVsKeyInfo(IntPtr pvaIn, VSConstants.VSStd2KCmdID commandID) {
        // catch current modifiers as early as possible
        bool capsLockToggled = Keyboard.IsKeyToggled(Key.CapsLock);
        bool numLockToggled = Keyboard.IsKeyToggled(Key.NumLock);
        bool shiftPressed = Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);
        bool controlPressed = Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);
        bool altPressed = Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt);

        // Direct mapping for navigation and special keys
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

        // For TYPECHAR and other commands with character data
        if (pvaIn != IntPtr.Zero) {
            char keyChar = (char)(ushort)Marshal.GetObjectForNativeVariant(pvaIn);
            
            // convert from char to virtual key, using current thread's input locale
            Lazy<IntPtr> _pKeybLayout = new Lazy<IntPtr>(() => GetKeyboardLayout(0));
            short keyScan = VkKeyScanEx(keyChar, _pKeybLayout.Value);
            
            byte virtualKey = (byte)(keyScan & 0x00ff);
            Key key = KeyInterop.KeyFromVirtualKey(virtualKey);
            
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

        // Fallback - should not reach here in normal operation
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

    internal bool IsCommandAllowed(ref Guid pguidCmdGroup, uint cmdID) {
        if (_allowedCommands == null) {
            return pguidCmdGroup != Guid.Empty;
        }
        if (_allowedCommands.TryGetValue(pguidCmdGroup, out uint[] array) && Array.IndexOf<uint>(array, cmdID) > -1) {
            return true;
        }
        return false;
    }
}