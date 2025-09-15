using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickJump2022.TextEditor;
public partial class EditorHost {
    public static Dictionary<Guid, uint[]> AllowedCommands = new() {
        //https://learn.microsoft.com/en-us/dotnet/api/microsoft.visualstudio.vsconstants.vsstd2kcmdid?view=visualstudiosdk-2022
        //https://learn.microsoft.com/en-us/dotnet/api/microsoft.visualstudio.vsconstants.vsstd2kcmdid
        {
            VSConstants.GUID_VSStandardCommandSet97,
            new uint[]{
                (uint)VSConstants.VSStd97CmdID.SelectAll,
                (uint)VSConstants.VSStd97CmdID.Copy,
                (uint)VSConstants.VSStd97CmdID.Paste,
                (uint)VSConstants.VSStd97CmdID.Cut,
            }
        },
        {
            VSConstants.VSStd2K,
            new uint[]
            {
                (uint)VSConstants.VSStd2KCmdID.TYPECHAR,
                (uint)VSConstants.VSStd2KCmdID.BACKSPACE,
                (uint)VSConstants.VSStd2KCmdID.RETURN,
                (uint)VSConstants.VSStd2KCmdID.TAB,
                (uint)VSConstants.VSStd2KCmdID.BACKTAB,
                (uint)VSConstants.VSStd2KCmdID.DELETE,
                (uint)VSConstants.VSStd2KCmdID.LEFT,
                (uint)VSConstants.VSStd2KCmdID.LEFT_EXT,
                (uint)VSConstants.VSStd2KCmdID.RIGHT,
                (uint)VSConstants.VSStd2KCmdID.RIGHT_EXT,
                (uint)VSConstants.VSStd2KCmdID.HOME,
                (uint)VSConstants.VSStd2KCmdID.HOME_EXT,
                (uint)VSConstants.VSStd2KCmdID.END,
                (uint)VSConstants.VSStd2KCmdID.END_EXT,
                (uint)VSConstants.VSStd2KCmdID.EOL,
                (uint)VSConstants.VSStd2KCmdID.EOL_EXT,
                (uint)VSConstants.VSStd2KCmdID.BOL,
                (uint)VSConstants.VSStd2KCmdID.BOL_EXT,
                (uint)VSConstants.VSStd2KCmdID.DELETETOBOL,
                (uint)VSConstants.VSStd2KCmdID.DELETETOEOL,
                (uint)VSConstants.VSStd2KCmdID.SELECTALL,
                (uint)VSConstants.VSStd2KCmdID.CUT,
                (uint)VSConstants.VSStd2KCmdID.COPY,
                (uint)VSConstants.VSStd2KCmdID.PASTE,
                (uint)VSConstants.VSStd2KCmdID.DELETELINE,
                (uint)VSConstants.VSStd2KCmdID.UNDO,
                (uint)VSConstants.VSStd2KCmdID.REDO,
                (uint)VSConstants.VSStd2KCmdID.DELETEWORDRIGHT,
                (uint)VSConstants.VSStd2KCmdID.DELETEWORDLEFT,
                (uint)VSConstants.VSStd2KCmdID.WORDPREV,
                (uint)VSConstants.VSStd2KCmdID.WORDPREV_EXT,
                (uint)VSConstants.VSStd2KCmdID.WORDNEXT,
                (uint)VSConstants.VSStd2KCmdID.WORDNEXT_EXT,
                (uint)VSConstants.VSStd2KCmdID.CANCEL,
                (uint)VSConstants.VSStd2KCmdID.UP,
                (uint)VSConstants.VSStd2KCmdID.DOWN,
                (uint)VSConstants.VSStd2KCmdID.PAGEUP,
                (uint)VSConstants.VSStd2KCmdID.PAGEDN,
            }
        },
        {
            EditorConstants.EditorCommandSet,
            new uint[]{
                (uint)EditorConstants.EditorCommandID.MoveToNextSubWord,
                (uint)EditorConstants.EditorCommandID.MoveToNextSubWordExtend,
                (uint)EditorConstants.EditorCommandID.MoveToPreviousSubWord,
                (uint)EditorConstants.EditorCommandID.MoveToPreviousSubWordExtend,
                (uint)EditorConstants.EditorCommandID.SubwordDeleteToStart,
                (uint)EditorConstants.EditorCommandID.SubwordDeleteToEnd,
                (uint)EditorConstants.EditorCommandID.SubwordExpandSelection,
                (uint)EditorConstants.EditorCommandID.SubwordContractSelection,
                (uint)EditorConstants.EditorCommandID.SelectCurrentSubword,
                (uint)EditorConstants.EditorCommandID.InsertCaretsAtAllMatching,
                (uint)EditorConstants.EditorCommandID.InsertNextMatchingCaret,
            }
        },

    };
}
