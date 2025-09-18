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
    private readonly Dictionary<Guid, uint[]> _allowedCommands;
    public event InputEditorSpecialKeyHandler KeyPressed;
    public event EventHandler PreCommand;
    public event EventHandler PostCommand;
    public IOleCommandTarget NextCommandTarget { private get; set; }

    public EditorCommandFilter(Dictionary<Guid, uint[]> allowedCommands) => _allowedCommands = allowedCommands;

    public int QueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText) {
        ThreadHelper.ThrowIfNotOnUIThread(nameof(QueryStatus));
        if (cCmds == 1U && !IsCommandAllowed(ref pguidCmdGroup, prgCmds[0].cmdID)) {
            prgCmds[0].cmdf |= 17U;
            return 0;
        }
        if (NextCommandTarget != null)
            return NextCommandTarget.QueryStatus(ref pguidCmdGroup, cCmds, prgCmds, pCmdText);
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
        if (pguidCmdGroup == VSConstants.VSStd2K) {
            var commandID = (VSConstants.VSStd2KCmdID)nCmdID;
            switch (commandID) {
                case VSConstants.VSStd2KCmdID.TAB:
                case VSConstants.VSStd2KCmdID.BACKTAB:
                case VSConstants.VSStd2KCmdID.UP:
                case VSConstants.VSStd2KCmdID.PAGEUP:
                case VSConstants.VSStd2KCmdID.DOWN:
                case VSConstants.VSStd2KCmdID.PAGEDN:
                case VSConstants.VSStd2KCmdID.RETURN:
                case VSConstants.VSStd2KCmdID.CANCEL:
                    var keyInfo = VsKeyInfo.GetVsKeyInfo(pvaIn, commandID);
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
        return -2147221244;
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