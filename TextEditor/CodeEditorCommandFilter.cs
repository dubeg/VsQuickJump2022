using Microsoft.VisualStudio.OLE.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickJump2022.TextEditor;

internal sealed class CodeEditorCommandFilter : IOleCommandTarget {
    private readonly Dictionary<Guid, uint[]> allowedCommands;

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
