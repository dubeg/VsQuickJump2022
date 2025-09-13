using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickJump2022.TextEditor;

internal sealed class CodeEditorCommandFilter : IOleCommandTarget {
    private readonly Dictionary<Guid, uint[]> allowedCommands;
    
    // Event that fires when Escape is pressed
    public event EventHandler EscapePressed;
    
    // Event that fires when Enter is pressed
    public event EventHandler EnterPressed;

    internal CodeEditorCommandFilter(
        Dictionary<Guid, uint[]> allowedCommands
    ) {
        this.allowedCommands = allowedCommands;
    }

    internal bool HasFocus { private get; set; }

    internal IOleCommandTarget NextCommandTarget { private get; set; }

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
            // Check if this is the Escape key (CANCEL command)
            if (pguidCmdGroup == VSConstants.VSStd2K && nCmdID == (uint)VSConstants.VSStd2KCmdID.CANCEL) {
                // Fire the escape event
                EscapePressed?.Invoke(this, EventArgs.Empty);
                // Return S_OK to indicate we handled it
                return VSConstants.S_OK;
            }
            
            // Check if this is the Enter key (RETURN command)
            if (pguidCmdGroup == VSConstants.VSStd2K && nCmdID == (uint)VSConstants.VSStd2KCmdID.RETURN) {
                // Fire the enter event
                EnterPressed?.Invoke(this, EventArgs.Empty);
                // Return S_OK to indicate we handled it
                return VSConstants.S_OK;
            }
            
            if (!IsCommandAllowed(ref pguidCmdGroup, nCmdID)) {
                return 0;
            }
            if (NextCommandTarget != null) {
                int num = NextCommandTarget.Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);
                return num;
            }
        }
        return -2147221244;
    }

    internal bool IsCommandAllowed(ref Guid pguidCmdGroup, uint cmdID) {
        if (allowedCommands == null) {
            return pguidCmdGroup != Guid.Empty;
        }

        if (allowedCommands.TryGetValue(pguidCmdGroup, out uint[] array) && Array.IndexOf<uint>(array, cmdID) > -1)
            return true;

        return false;
    }
}
