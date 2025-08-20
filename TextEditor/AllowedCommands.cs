using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Editor;
using System.Collections.Generic;

namespace QuickJump2022.TextEditor;

public static class AllowedCommands {
    public static Dictionary<Guid, uint[]> Instance = new() {
        //https://learn.microsoft.com/en-us/dotnet/api/microsoft.visualstudio.vsconstants.vsstd2kcmdid?view=visualstudiosdk-2022
        //https://learn.microsoft.com/en-us/dotnet/api/microsoft.visualstudio.vsconstants.vsstd2kcmdid
        {
            VSConstants.GUID_VSStandardCommandSet97,
            new uint[]{ 
                (uint)VSConstants.VSStd97CmdID.SelectAll
            }
        },
        {
            VSConstants.VSStd2K,
            new uint[]
            {
                (uint)VSConstants.VSStd2KCmdID.TYPECHAR,
                (uint)VSConstants.VSStd2KCmdID.BACKSPACE,
                //(uint)VSConstants.VSStd2KCmdID.RETURN,
                //(uint)VSConstants.VSStd2KCmdID.TAB,
                //(uint)VSConstants.VSStd2KCmdID.BACKTAB,
                (uint)VSConstants.VSStd2KCmdID.DELETE,
                (uint)VSConstants.VSStd2KCmdID.LEFT,
                (uint)VSConstants.VSStd2KCmdID.LEFT_EXT,
                (uint)VSConstants.VSStd2KCmdID.RIGHT,
                (uint)VSConstants.VSStd2KCmdID.RIGHT_EXT,
                (uint)VSConstants.VSStd2KCmdID.HOME,
                (uint)VSConstants.VSStd2KCmdID.HOME_EXT,
                (uint)VSConstants.VSStd2KCmdID.END,
                (uint)VSConstants.VSStd2KCmdID.END_EXT,
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
