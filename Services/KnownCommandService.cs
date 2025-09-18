using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Linq;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Core.Imaging;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.Imaging.Interop;
using QuickJump2022.Models;
using static Microsoft.VisualStudio.VSConstants;

namespace QuickJump2022.Services;

public class KnownCommandService {
    private List<KnownCommandItem> _commands = new();

    public List<KnownCommandItem> GetCommands() => _commands;

    public List<KnownCommandItem> PreloadCommandsCache(CommandService commandService, PackageInfoService packageInfoService) {
        _commands.AddRange([
            //new (KnownCommands.Build_BatchBuild,"Build: Batch Build...",KnownMonikers.BuildSolution),
            //new (KnownCommands.Build_BuildOnlyProject,"Build: Build Project",KnownMonikers.BuildSelection),
            //new (KnownCommands.Build_BuildSelection,"Build: Build Selection",KnownMonikers.BuildSelection),
            new (KnownCommands.Build_BuildSolution,"Build: Build Solution",KnownMonikers.BuildSolution),
            new (KnownCommands.Build_Cancel,"Build: Cancel",KnownMonikers.Cancel),
            //new (KnownCommands.Build_CleanOnlyProject,"Build: Clean Project",KnownMonikers.CleanData),
            //new (KnownCommands.Build_CleanSelection,"Build: Clean Selection",KnownMonikers.CleanData),
            new (KnownCommands.Build_CleanSolution,"Build: Clean Solution",KnownMonikers.CleanData),
            new (KnownCommands.Build_ConfigurationManager,"Build: Configuration Manager...",KnownMonikers.ConfigurationEditor),
            //new (KnownCommands.Build_DeploySelection,"Build: Deploy Selection",default),
            //new (KnownCommands.Build_DeploySolution,"Build: Deploy Solution",default),
            //new (KnownCommands.Build_ProjectPickerBuild,"Build: Project Picker Build",default),
            //new (KnownCommands.Build_ProjectPickerRebuild,"Build: Project Picker Rebuild", default),
            //new (KnownCommands.Build_PublishSelection,"Build: Publish Selection",default),
            //new (KnownCommands.Build_RebuildOnlyProject,"Build: Rebuild Project",default),
            //new (KnownCommands.Build_RebuildSelection,"Build: Rebuild Selection",default),
            new (KnownCommands.Build_RebuildSolution,"Build: Rebuild Solution",default),
            //new (KnownCommands.Build_RunCodeAnalysisonProject,"Build: Run Code Analysis on Project",KnownMonikers.Analysis),
            //new (KnownCommands.Build_RunCodeAnalysisonSelection,"Build: Run Code Analysis on Selection",KnownMonikers.Analysis),
        ]);

        _commands.AddRange([
            new (KnownCommands.Debug_AddWatch,"Debug: Add watch",KnownMonikers.Watch),
            new (KnownCommands.Debug_AttachtoProcess,"Debug: Attach to process",KnownMonikers.Process),
            new (KnownCommands.Debug_Autos,"Debug: Autos",KnownMonikers.LocalsWindow),
            new (KnownCommands.Debug_BreakAll,"Debug: Break all",default),
            new (KnownCommands.Debug_CallStack,"Debug: Callstack",KnownMonikers.CallStackWindow),
            new (KnownCommands.Debug_ToggleBreakpoint,"Debug: Toggle breakpoint",KnownMonikers.BreakpointEnabled),
            //new (KnownCommands.Debug_EnableBreakpoint,"Debug: Enable breakpoint",KnownMonikers.BreakpointEnabled),
            new (
                new CommandID(new Guid("{C9DD4A59-47FB-11D2-83E7-00C04F9902C1}"), 0x123),
                "Debug: Enable all breakpoints",
                KnownMonikers.EnableAllBreakpoints
            ),
            new (
                new CommandID(new Guid("{C9DD4A59-47FB-11D2-83E7-00C04F9902C1}"), 0x122),
                "Debug: Disable all breakpoints",
                KnownMonikers.DisableAllBreakpoints
            ),
            new (KnownCommands.Debug_DeleteAllBreakpoints,"Debug: Delete all breakpoints",KnownMonikers.DeleteBreakpoint),
            new (KnownCommands.Debug_DetachAll,"Debug: Detach all",default),
            new (KnownCommands.Debug_ExceptionSettings,"Debug: Exception settings",KnownMonikers.ExceptionSettings),
            new (KnownCommands.Debug_Immediate,"Debug: Immediate window",KnownMonikers.ImmediateWindow),
            new (KnownCommands.Debug_Locals,"Debug: Locals",KnownMonikers.LocalsWindow),
            new (KnownCommands.Debug_Print,"Debug: Print",KnownMonikers.PrintPreview),
            new (KnownCommands.Debug_QuickWatch,"Debug: QuickWatch",default),
            new (KnownCommands.Debug_Restart,"Debug: Restart",KnownMonikers.Restart),
            new (KnownCommands.Debug_RunToCursor,"Debug: Run to cursor",default),
            new (KnownCommands.Debug_Start,"Debug: Start",KnownMonikers.Run),
            new (KnownCommands.Debug_StartWithoutDebugging,"Debug: Start without debugging",KnownMonikers.Run),
            new (KnownCommands.Debug_StepInto,"Debug: Step Into",KnownMonikers.StepInto),
            new (KnownCommands.Debug_StepOut,"Debug: Step Out",KnownMonikers.StepOut),
            new (KnownCommands.Debug_StepOver,"Debug: Step Over",KnownMonikers.StepOver),
            new (KnownCommands.Debug_StopDebugging,"Debug: Stop Debugging",KnownMonikers.Stop),
            new (KnownCommands.Debug_Threads,"Debug: Threads",KnownMonikers.Thread),
        ]);

        _commands.AddRange([
            //new (KnownCommands.Edit_AddResource,"Edit: Add Resource...",default),
            //new (KnownCommands.Edit_AddTagHandler,"Edit: Add Tag Handler",default),
            //new (KnownCommands.Edit_BreakLine,"Edit: Break Line",default),
            new (KnownCommands.Edit_Capitalize,"Edit: Capitalize",default),
            //new (KnownCommands.Edit_CharLeft,"Edit: Char Left",default),
            //new (KnownCommands.Edit_CharLeftExtend,"Edit: Char Left Extend",default),
            //new (KnownCommands.Edit_CharLeftExtendColumn,"Edit: Char Left Extend Column",default),
            //new (KnownCommands.Edit_CharRight,"Edit: Char Right",default),
            //new (KnownCommands.Edit_CharRightExtend,"Edit: Char Right Extend",default),
            //new (KnownCommands.Edit_CharRightExtendColumn,"Edit: Char Right Extend Column",default),
            //new (KnownCommands.Edit_CharTranspose,"Edit: Char Transpose",default),
            //new (KnownCommands.Edit_CheckMnemonics,"Edit: Check Mnemonics",default),
            //new (KnownCommands.Edit_ClearAll,"Edit: Clear All",default),
            new (KnownCommands.Edit_ClearAllBookmarksInDocument,"Edit: Clear All Bookmarks In Document",default),
            new (KnownCommands.Edit_ClearBookmarks,"Edit: Clear Bookmarks",default),
            //new (KnownCommands.Edit_ClearFindResults1,"Edit: Clear All",default),
            //new (KnownCommands.Edit_ClearFindResults2,"Edit: Clear All",default),
            new (KnownCommands.Edit_ClearOutputWindow,"Edit: Clear All",default),
            new (KnownCommands.Edit_CollapseTag,"Edit: Collapse Tag",default),
            new (KnownCommands.Edit_CollapsetoDefinitions,"Edit: Collapse to Definitions",default),
            new (KnownCommands.Edit_CommentSelection,"Edit: Comment Selection",default),
            new (KnownCommands.Edit_CompleteWord,"Edit: Complete Word",KnownMonikers.CompleteWord),
            new (KnownCommands.Edit_ContractSelection,"Edit: Contract Selection",default),
            new (KnownCommands.Edit_ConvertSpacesToTabs,"Edit: Convert Spaces To Tabs",default),
            new (KnownCommands.Edit_ConvertTabsToSpaces,"Edit: Convert Tabs To Spaces",default),
            new (KnownCommands.Edit_Copy,"Edit: Copy",KnownMonikers.Copy),
            new (KnownCommands.Edit_CopyParameterTip,"Edit: Copy Parameter Tip",default),
            new (KnownCommands.Edit_Cut,"Edit: Cut",KnownMonikers.Cut),
            new (KnownCommands.Edit_CycleClipboardRing,"Edit: Cycle Clipboard Ring",default),
            //new (KnownCommands.Edit_DecreaseFilterLevel,"Edit: Decrease Filter Level",default),
            //new (KnownCommands.Edit_DecreaseLineIndent,"Edit: Decrease Line Indent",default),
            new (KnownCommands.Edit_Delete,"Edit: Delete",default),
            new (KnownCommands.Edit_DeleteBackwards,"Edit: Delete Backwards",default),
            new (KnownCommands.Edit_DeleteBlankLines,"Edit: Delete Blank Lines",default),
            new (KnownCommands.Edit_DeleteHorizontalWhiteSpace,"Edit: Delete Horizontal White Space",default),
            new (KnownCommands.Edit_DeleteToBOL,"Edit: Delete To BOL",default),
            new (KnownCommands.Edit_DeleteToEOL,"Edit: Delete To EOL",default),
            new (KnownCommands.Edit_DeleteVersionInfoBlock,"Edit: Delete Version Info Block",default),
            //new (KnownCommands.Edit_DocumentEnd,"Edit: Document End",default),
            //new (KnownCommands.Edit_DocumentEndExtend,"Edit: Document End Extend",default),
            //new (KnownCommands.Edit_DocumentStart,"Edit: Document Start",default),
            //new (KnownCommands.Edit_DocumentStartExtend,"Edit: Document Start Extend",default),
            //new (KnownCommands.Edit_DoubleClick,"Edit: Double Click",default),
            //new (KnownCommands.Edit_EditIDs,"Edit: Edit IDs",default),
            //new (KnownCommands.Edit_EditNames,"Edit: Edit Names",default),
            //new (KnownCommands.Edit_EditTagHandler,"Edit: Edit Tag Handler",default),
            //new (KnownCommands.Edit_ExpandSelection,"Edit: Expand Selection",default),
            new (KnownCommands.Edit_Find,"Edit: Find",default),
            new (KnownCommands.Edit_FindAllReferences,"Edit: Find All References",default),
            new (KnownCommands.Edit_FindinFiles,"Edit: Find in Files",KnownMonikers.SearchFolderClosed),
            new (KnownCommands.Edit_FindNext,"Edit: Find Next",KnownMonikers.FindNext),
            //new (KnownCommands.Edit_FindNextSelected,"Edit: Find Next (Selected)",default),
            new (KnownCommands.Edit_FindPrevious,"Edit: Find Previous",KnownMonikers.FindPrevious),
            //new (KnownCommands.Edit_FindPreviousSelected,"Edit: Find Previous (Selected)",default),
            new (KnownCommands.Edit_FindSymbol,"Edit: Find Symbol",KnownMonikers.FindSymbol),
            new (KnownCommands.Edit_FormatDocument,"Edit: Format Document",KnownMonikers.FormatDocument),
            new (KnownCommands.Edit_FormatSelection,"Edit: Format Selection",KnownMonikers.FormatSelection),
            new (KnownCommands.Edit_GoTo,"Edit: Go To Line",default),
            new (KnownCommands.Edit_GotoBrace,"Edit: Go To Brace",default),
            new (KnownCommands.Edit_GoToDeclaration,"Edit: Go To Declaration",KnownMonikers.GoToDeclaration),
            new (KnownCommands.Edit_GoToDefinition,"Edit: Go To Definition",KnownMonikers.GoToDefinition),
            new (
                new CommandID(EditorConstants.EditorCommandSet, (int)EditorConstants.EditorCommandID.GoToLastEditLocation),
                "Edit: Go To Last Edit Location",
                default
            ),
            new (
                new CommandID(EditorConstants.EditorCommandSet, (int)EditorConstants.EditorCommandID.GoToBase),
                "Edit: Go To Base",
                default
            ),
            new (
                new CommandID(EditorConstants.EditorCommandSet, (int)EditorConstants.EditorCommandID.GoToContainingDeclaration),
                "Edit: Go To Containing Block",
                default
            ),
            new (
                new CommandID(EditorConstants.EditorCommandSet, 66),
                "Edit: Expand Selection to Line",
                default
            ),
            new (
                new CommandID(EditorConstants.EditorCommandSet, (int)EditorConstants.EditorCommandID.JoinLines),
                "Edit: Join Lines",
                default
            ),
            new (
                new CommandID(EditorConstants.EditorCommandSet, (int)EditorConstants.EditorCommandID.SortLines),
                "Edit: Sort Lines",
                default
            ),
            new (
                new CommandID(EditorConstants.EditorCommandSet, 94),
                "Edit: Next Suggestion",
                default
            ),
            new (
                new CommandID(EditorConstants.EditorCommandSet, 93),
                "Edit: Previous Suggestion",
                default
            ),
            new (
                new CommandID(EditorConstants.EditorCommandSet, (int)EditorConstants.EditorCommandID.ToggleLineComments),
                "Edit: Toggle Line Comment",
                default
            ),
            new (
                new CommandID(EditorConstants.EditorCommandSet, (int)EditorConstants.EditorCommandID.ToggleBlockComments),
                "Edit: Toggle Block Comment",
                default
            ),
            new (
                new CommandID(EditorConstants.EditorCommandSet, 59),
                "Edit: Toggle Spell Checker",
                default
            ),
            new (KnownCommands.View_QuickActions,"Edit: Quick Fixes",default),
            new (KnownCommands.Edit_PeekDefinition,"Edit: Peek Definition",default),
            //new (KnownCommands.Edit_GoToFindResults1Location,"Edit: Go To Location",default),
            //new (KnownCommands.Edit_GoToFindResults1NextLocation,"Edit: Go To Next Location",default),
            //new (KnownCommands.Edit_GoToFindResults1PrevLocation,"Edit: Go To Previous Location",default),
            //new (KnownCommands.Edit_GoToFindResults2Location,"Edit: Go To Location",default),
            //new (KnownCommands.Edit_GoToFindResults2NextLocation,"Edit: Go To Next Location",default),
            //new (KnownCommands.Edit_GoToFindResults2PrevLocation,"Edit: Go To Previous Location",default),
            //new (KnownCommands.Edit_GoToNextLocation,"Edit: Go To Next Location",default),
            //new (KnownCommands.Edit_GoToPrevLocation,"Edit: Go To Previous Location",default),
            //new (KnownCommands.Edit_GoToOutputWindowLocation,"Edit: Go To Location",default),
            //new (KnownCommands.Edit_GoToOutputWindowNextLocation,"Edit: Go To Next Location",default),
            //new (KnownCommands.Edit_GoToOutputWindowPrevLocation,"Edit: Go To Previous Location",default),
            new (KnownCommands.Edit_GoToReference,"Edit: Go To Reference",KnownMonikers.GoToReference),
            new (KnownCommands.Edit_GoToTypeDefinition,"Edit: Go To Type Definition",KnownMonikers.GoToTypeDefinition),
            new (
                new CommandID(new Guid("{B61E1A20-8C13-49A9-A727-A0EC091647DD}"),0x200),
                "Edit: Go To Implementation",
                default
            ),
            //new (KnownCommands.Edit_HideAdvancedCompletionMembers,"Edit: Hide Advanced Completion Members",default),
            //new (KnownCommands.Edit_HideSelection,"Edit: Hide Selection",default),
            //new (KnownCommands.Edit_HideSnippetHighlighting,"Edit: Hide Snippet Highlighting",default),
            //new (KnownCommands.Edit_IncreaseFilterLevel,"Edit: Increase Filter Level",default),
            //new (KnownCommands.Edit_IncreaseLineIndent,"Edit: Increase Line Indent",default),
            new (KnownCommands.Edit_IncrementalSearch,"Edit: Incremental Search",default),
            //new (KnownCommands.Edit_InsertComment,"Edit: Insert Comment",default),
            //new (KnownCommands.Edit_InsertFileAsText,"Edit: Insert File As Text",default),
            //new (KnownCommands.Edit_InsertNew,"Edit: Insert New",default),
            //new (KnownCommands.Edit_InsertSeparator,"Edit: Insert Separator",default),
            //new (KnownCommands.Edit_InsertSnippet,"Edit: Insert Snippet",default),
            //new (KnownCommands.Edit_InsertTab,"Edit: Insert Tab",default),
            //new (KnownCommands.Edit_InvokeSnippetFromShortcut,"Edit: Invoke Snippet From Shortcut",default),
            //new (KnownCommands.Edit_LineCut,"Edit: Line Cut",default),
            //new (KnownCommands.Edit_LineDelete,"Edit: Line Delete",default),
            //new (KnownCommands.Edit_LineDown,"Edit: Line Down",default),
            //new (KnownCommands.Edit_LineDownExtend,"Edit: Line Down Extend",default),
            //new (KnownCommands.Edit_LineDownExtendColumn,"Edit: Line Down Extend Column",default),
            //new (KnownCommands.Edit_LineEnd,"Edit: Line End",default),
            //new (KnownCommands.Edit_LineEndExtend,"Edit: Line End Extend",default),
            //new (KnownCommands.Edit_LineEndExtendColumn,"Edit: Line End Extend Column",default),
            //new (KnownCommands.Edit_LineFirstColumn,"Edit: Line First Column",default),
            //new (KnownCommands.Edit_LineFirstColumnExtend,"Edit: Line First Column Extend",default),
            //new (KnownCommands.Edit_LineLastChar,"Edit: Line Last Char",default),
            //new (KnownCommands.Edit_LineLastCharExtend,"Edit: Line Last Char Extend",default),
            //new (KnownCommands.Edit_LineOpenAbove,"Edit: Line Open Above",default),
            //new (KnownCommands.Edit_LineOpenBelow,"Edit: Line Open Below",default),
            //new (KnownCommands.Edit_LineStart,"Edit: Line Start",default),
            //new (KnownCommands.Edit_LineStartAfterIndentation,"Edit: Line Start After Indentation",default),
            //new (KnownCommands.Edit_LineStartAfterIndentationExtend,"Edit: Line Start After Indentation Extend",default),
            //new (KnownCommands.Edit_LineStartAfterIndentationNext,"Edit: Line Start After Indentation Next",default),
            //new (KnownCommands.Edit_LineStartAfterIndentationPrev,"Edit: Line Start After Indentation Prev",default),
            //new (KnownCommands.Edit_LineStartExtend,"Edit: Line Start Extend",default),
            //new (KnownCommands.Edit_LineStartExtendColumn,"Edit: Line Start Extend Column",default),
            //new (KnownCommands.Edit_LineTranspose,"Edit: Line Transpose",default),
            //new (KnownCommands.Edit_LineUp,"Edit: Line Up",default),
            //new (KnownCommands.Edit_LineUpExtend,"Edit: Line Up Extend",default),
            //new (KnownCommands.Edit_LineUpExtendColumn,"Edit: Line Up Extend Column",default),
            new (KnownCommands.Edit_ListMembers,"Edit: List Members",KnownMonikers.ListMembers),
            new (KnownCommands.Edit_MakeLowercase,"Edit: Make Lowercase",default),
            new (KnownCommands.Edit_MakeUppercase,"Edit: Make Uppercase",default),
            //new (KnownCommands.Edit_MoveControlDown,"Edit: Move Control Down",default),
            //new (KnownCommands.Edit_MoveControlDownGrid,"Edit: Move Control Down Grid",default),
            //new (KnownCommands.Edit_MoveControlLeft,"Edit: Move Control Left",default),
            //new (KnownCommands.Edit_MoveControlLeftGrid,"Edit: Move Control Left Grid",default),
            //new (KnownCommands.Edit_MoveControlRight,"Edit: Move Control Right",default),
            //new (KnownCommands.Edit_MoveControlRightGrid,"Edit: Move Control Right Grid",default),
            //new (KnownCommands.Edit_MoveControlUp,"Edit: Move Control Up",default),
            //new (KnownCommands.Edit_MoveControlUpGrid,"Edit: Move Control Up Grid",default),
            new (KnownCommands.Edit_NewAccelerator,"Edit: New Accelerator",default),
            //new (KnownCommands.Edit_NewString,"Edit: New String",default),
            new (KnownCommands.Edit_NewVersionInfoBlock,"Edit: New Version Info Block",default),
            new (KnownCommands.Edit_NextBookmark,"Edit: Next Bookmark",KnownMonikers.NextBookmark),
            new (KnownCommands.Edit_NextBookmarkInDocument,"Edit: Next Bookmark In Document",default),
            //new (KnownCommands.Edit_NextKeyTyped,"Edit: Next Key Typed",default),
            new (KnownCommands.Edit_NextMethod,"Edit: Next Method",default),
            new (KnownCommands.Edit_OpenFile,"Edit: Open File",KnownMonikers.OpenFile),
            //new (KnownCommands.Edit_OvertypeMode,"Edit: Overtype Mode",default),
            //new (KnownCommands.Edit_PageDown,"Edit: Page Down",default),
            //new (KnownCommands.Edit_PageDownExtend,"Edit: Page Down Extend",default),
            //new (KnownCommands.Edit_PageUp,"Edit: Page Up",default),
            //new (KnownCommands.Edit_PageUpExtend,"Edit: Page Up Extend",default),
            new (KnownCommands.Edit_ParameterInfo,"Edit: Parameter Info",default),
            new (KnownCommands.Edit_Paste,"Edit: Paste",KnownMonikers.Paste),
            //new (KnownCommands.Edit_PasteMovesCaret,"Edit: Paste Moves Caret",default),
            //new (KnownCommands.Edit_PasteParameterTip,"Edit: Paste Parameter Tip",default),
            new (KnownCommands.Edit_PreviousBookmark,"Edit: Previous Bookmark",KnownMonikers.PreviousBookmark),
            new (KnownCommands.Edit_PreviousBookmarkInDocument,"Edit: Previous Bookmark In Document",default),
            new (KnownCommands.Edit_PreviousMethod,"Edit: Previous Method",default),
            //new (KnownCommands.Edit_QuickFindSymbol,"Edit: Quick Find Symbol",default),
            new (KnownCommands.Edit_QuickInfo,"Edit: Quick Info",default),
            new (KnownCommands.Edit_Redo,"Edit: Redo",KnownMonikers.Redo),
            //new (KnownCommands.Edit_RedoLastGlobalAction,"Edit: Redo Last Global Action",default),
            new (KnownCommands.Edit_Remove,"Edit: Remove",KnownMonikers.Remove),
            //new (KnownCommands.Edit_RemoveTagHandler,"Edit: Remove Tag Handler",default),
            new (KnownCommands.Edit_FindAllReferences,"Edit: Find All References",default),
            new (KnownCommands.Edit_FindinFiles,"Edit: Find In Files",KnownMonikers.FindInFile),
            new (KnownCommands.Edit_Find,"Edit: Quick Find",KnownMonikers.QuickFind),
            new (KnownCommands.Edit_Replace,"Edit: Quick Replace",KnownMonikers.QuickReplace),
            new (KnownCommands.Edit_ReplaceinFiles,"Edit: Replace in Files",KnownMonikers.ReplaceInFolder),
            //new (KnownCommands.Edit_ResourceIncludes,"Edit: Resource Includes...",default),
            //new (KnownCommands.Edit_ResourceSymbols,"Edit: Resource Symbols...",default),
            //new (KnownCommands.Edit_ReverseCancel,"Edit: Reverse Cancel",default),
            new (KnownCommands.Edit_ReverseIncrementalSearch,"Edit: Reverse Incremental Search",default),
            //new (KnownCommands.Edit_ScrollColumnLeft,"Edit: Scroll Column Left",default),
            //new (KnownCommands.Edit_ScrollColumnRight,"Edit: Scroll Column Right",default),
            //new (KnownCommands.Edit_ScrollLineBottom,"Edit: Scroll Line Bottom",default),
            //new (KnownCommands.Edit_ScrollLineCenter,"Edit: Scroll Line Center",default),
            //new (KnownCommands.Edit_ScrollLineDown,"Edit: Scroll Line Down",default),
            //new (KnownCommands.Edit_ScrollLineTop,"Edit: Scroll Line Top",default),
            //new (KnownCommands.Edit_ScrollLineUp,"Edit: Scroll Line Up",default),
            //new (KnownCommands.Edit_ScrollPageDown,"Edit: Scroll Page Down",default),
            //new (KnownCommands.Edit_ScrollPageUp,"Edit: Scroll Page Up",default),
            new (KnownCommands.Edit_SelectAll,"Edit: Select All",KnownMonikers.SelectAll),
            new (KnownCommands.Edit_SelectCurrentWord,"Edit: Select Current Word",default),
            //new (KnownCommands.Edit_SelectionCancel,"Edit: Selection Cancel",default),
            //new (KnownCommands.Edit_SelectNextControl,"Edit: Select Next Control",default),
            //new (KnownCommands.Edit_SelectPreviousControl,"Edit: Select Previous Control",default),
            //new (KnownCommands.Edit_SelectToLastGoBack,"Edit: Select To Last Go Back",default),
            //new (KnownCommands.Edit_ShowGrid,"Edit: Show Grid",KnownMonikers.ShowGrid),
            //new (KnownCommands.Edit_ShowSnippetHighlighting,"Edit: Show Snippet Highlighting",default),
            //new (KnownCommands.Edit_ShowTileGrid,"Edit: Show Tile Grid",default),
            //new (KnownCommands.Edit_SizeControlDown,"Edit: Size Control Down",default),
            //new (KnownCommands.Edit_SizeControlDownGrid,"Edit: Size Control Down Grid",default),
            //new (KnownCommands.Edit_SizeControlLeft,"Edit: Size Control Left",default),
            //new (KnownCommands.Edit_SizeControlLeftGrid,"Edit: Size Control Left Grid",default),
            //new (KnownCommands.Edit_SizeControlRight,"Edit: Size Control Right",default),
            //new (KnownCommands.Edit_SizeControlRightGrid,"Edit: Size Control Right Grid",default),
            //new (KnownCommands.Edit_SizeControlUp,"Edit: Size Control Up",default),
            //new (KnownCommands.Edit_SizeControlUpGrid,"Edit: Size Control Up Grid",default),
            new (KnownCommands.Edit_StartAutomaticOutlining,"Edit: Start Automatic Outlining",default),
            //new (KnownCommands.Edit_StopHidingCurrent,"Edit: Stop Hiding Current",default),
            //new (KnownCommands.Edit_StopOutlining,"Edit: Stop Outlining",default),
            //new (KnownCommands.Edit_StopOutliningTag,"Edit: Stop Outlining Tag",default),
            //new (KnownCommands.Edit_StopSearch,"Edit: Stop Search",default),
            new (KnownCommands.Edit_SurroundWith,"Edit: Surround With...",default),
            //new (KnownCommands.Edit_SwapAnchor,"Edit: Swap Anchor",default),
            // new (KnownCommands.Edit_SwitchbetweenautomaticandtabonlyIntelliSensecompletion,"Switch between automatic and tab-only,IntelliSense Edit: completion",default),
            //new (KnownCommands.Edit_SwitchtoFindinFiles,"Edit: Find in Files",KnownMonikers.SearchFolderClosed),
            //new (KnownCommands.Edit_SwitchtoQuickFind,"Edit: Quick Find",default),
            //new (KnownCommands.Edit_SwitchtoQuickReplace,"Edit: Quick Replace",default),
            //new (KnownCommands.Edit_SwitchtoReplaceinFiles,"Edit: Replace in Files",default),
            new (KnownCommands.Edit_TabifySelectedLines,"Edit: Tabify Selected Lines",default),
            new (KnownCommands.Edit_TabLeft,"Edit: Tab Left",default),
            new (KnownCommands.Edit_ToggleAllOutlining,"Edit: Toggle All Outlining",default),
            new (KnownCommands.Edit_ToggleBookmark,"Edit: Toggle Bookmark",default),
            new (KnownCommands.Edit_ToggleCase,"Edit: Toggle Case",default),
            new (KnownCommands.Edit_ToggleOutliningExpansion,"Edit: Toggle Outlining Expansion",default),
            new (KnownCommands.Edit_ToggleTaskListShortcut,"Edit: Toggle Task List Shortcut",default),
            new (KnownCommands.Edit_ToggleWordWrap,"Edit: Toggle Word Wrap",default),
            new (KnownCommands.Edit_UncommentSelection,"Edit: Uncomment Selection",default),
            new (KnownCommands.Edit_Undo,"Edit: Undo",KnownMonikers.Undo),
            new (KnownCommands.Edit_UntabifySelectedLines,"Edit: Untabify Selected Lines",default),
            //new (KnownCommands.Edit_ValidateDocument,"Edit: Validate Document",KnownMonikers.ValidateDocument),
            //new (KnownCommands.Edit_ViewAsPopup,"Edit: View As Popup",default),
            //new (KnownCommands.Edit_ViewBottom,"Edit: View Bottom",KnownMonikers.ViewBottom),
            //new (KnownCommands.Edit_ViewTop,"Edit: View Top",KnownMonikers.ViewTop),
            new (KnownCommands.Edit_ViewWhiteSpace,"Edit: View White Space",default),
            new (KnownCommands.Edit_WordDeleteToEnd,"Edit: Word Delete To End",default),
            new (KnownCommands.Edit_WordDeleteToStart,"Edit: Word Delete To Start",default),
            new (KnownCommands.Edit_WordNext,"Edit: Word Next",default),
            new (KnownCommands.Edit_WordPrevious,"Edit: Word Previous",default),
            new (KnownCommands.Edit_WordTranspose,"Edit: Word Transpose",default),
        ]);

        _commands.AddRange([
            new (KnownCommands.File_AddExistingProject,"File: Add Existing Project...",default),
            new (KnownCommands.File_AddNewProject,"File: Add New Project...",default),
            //new (KnownCommands.File_AdvancedSaveOptions,"File: Advanced Save Options...",default),
            new (KnownCommands.File_BrowseWith,"File: Browse With...",default),
            new (KnownCommands.File_Close,"File: Close",KnownMonikers.Close),
            //new (KnownCommands.File_CloseAllButThis,"File: Close Other Tabs",default),
            new (KnownCommands.File_CloseProject,"File: Close Project",default),
            new (KnownCommands.File_CloseSolution,"File: Close Solution",KnownMonikers.CloseSolution),
            new (KnownCommands.File_CopyFullPath,"File: Copy Full Path",default),
            new (KnownCommands.File_CopyRelativePath,"File: Copy Relative Path",default),
            new (KnownCommands.File_Exit,"File: Exit",KnownMonikers.Exit),
            new (KnownCommands.File_NewFile,"File: New File...",KnownMonikers.NewDocument),
            new (KnownCommands.File_NewProject,"File: New Project...",KnownMonikers.NewDocumentCollection),
            new (KnownCommands.File_OpenContainingFolder,"File: Open Containing Folder",default),
            new (KnownCommands.File_OpenFile,"File: Open File...",KnownMonikers.OpenFile),
            new (KnownCommands.File_OpenProject,"File: Open Project...",KnownMonikers.OpenDocumentGroup),
            //new (KnownCommands.File_PageSetup,"File: Page Setup...",default),
            //new (KnownCommands.File_Print,"File: Print...",KnownMonikers.Print),
            //new (KnownCommands.File_PrintPreview,"File: Print Preview",KnownMonikers.PrintPreview),
            //new (KnownCommands.File_ProjectPickerMoveInto,"File: Project Picker Move Into",default),
            new (KnownCommands.File_Rename,"File: Rename",KnownMonikers.Rename),
            new (KnownCommands.File_SaveAll,"File: Save All",KnownMonikers.SaveAll),
            //new (KnownCommands.File_SaveCopyofSelectedItemsAs,"File: Save Copy of Selected Item(s) As...",default),
            //new (KnownCommands.File_SaveSelectedItems,"File: Save Selected Items",default),
            //new (KnownCommands.File_SaveSelectedItemsAs,"File: Save Selected Items As...",default),
            //new (KnownCommands.File_SaveSelection,"File: Save Selection",default),
            //new (KnownCommands.File_SelectProjectTemplate,"File: Select Project Template...",default),
            new (KnownCommands.File_ViewinBrowser,"File: View in Browser",KnownMonikers.ViewInBrowser),
            // --
            new (new CommandID(new Guid("{EC3F30E6-52A0-4A93-97BD-2660FF3A7AD5}"), 0x1),"File: Add quick file...",KnownMonikers.NewDocument),
        ]);

        _commands.AddRange([
            //new (KnownCommands.Format_AlignBottoms,"Align Bottoms",default),
            //new (KnownCommands.Format_AlignCenters,"Align Centers",KnownMonikers.AlignCenter),
            //new (KnownCommands.Format_AlignLefts,"Align Lefts",default),
            //new (KnownCommands.Format_AlignMiddles,"Align Middles",KnownMonikers.AlignMiddle),
            //new (KnownCommands.Format_AlignRights,"Align Rights",default),
            //new (KnownCommands.Format_AligntoGrid,"Align to Grid",default),
            //new (KnownCommands.Format_AlignTops,"Align Tops",default),
            //new (KnownCommands.Format_BackgroundColor,"Background Color...",KnownMonikers.BackgroundColor),
            //new (KnownCommands.Format_Bold,"Format: Bold",KnownMonikers.Bold),
            //new (KnownCommands.Format_BringtoFront,"Bring to Front",default),
            //new (KnownCommands.Format_ButtonBottom,"Button Bottom",default),
            //new (KnownCommands.Format_ButtonRight,"Button Right",default),
            //new (KnownCommands.Format_CenterHorizontal,"Center Horizontal",default),
            //new (KnownCommands.Format_CenterHorizontally,"Center Horizontally",KnownMonikers.CenterHorizontally),
            //new (KnownCommands.Format_CenterVertical,"Center Vertical",default),
            //new (KnownCommands.Format_CenterVertically,"Center Vertically",KnownMonikers.CenterVertically),
            //new (KnownCommands.Format_CheckMnemonics,"Check Mnemonics",default),
            //new (KnownCommands.Format_ConverttoHyperlink,"Convert to Hyperlink...",default),
            //new (KnownCommands.Format_DecreaseHorizontalSpacing,"Decrease Horizontal Spacing",KnownMonikers.DecreaseHorizontalSpacing),
            //new (KnownCommands.Format_DecreaseIndent,"Decrease Indent",KnownMonikers.DecreaseIndent),
            //new (KnownCommands.Format_DecreaseVerticalSpacing,"Decrease Vertical Spacing",KnownMonikers.DecreaseVerticalSpacing),
            //new (KnownCommands.Format_FixedWidth,"Fixed Width",default),
            //new (KnownCommands.Format_Flip,"Flip",default),
            //new (KnownCommands.Format_ForegroundColor,"Foreground Color...",KnownMonikers.ForegroundColor),
            //new (KnownCommands.Format_GuideSettings,"Format: Guide Settings...",default),
            //new (KnownCommands.Format_IncreaseHorizontalSpacing,"Increase Horizontal Spacing",KnownMonikers.IncreaseHorizontalSpacing),
            //new (KnownCommands.Format_IncreaseIndent,"Increase Indent",KnownMonikers.IncreaseIndent),
            //new (KnownCommands.Format_IncreaseVerticalSpacing,"Increase Vertical Spacing",KnownMonikers.IncreaseVerticalSpacing),
            //new (KnownCommands.Format_InsertBookmark,"Format: Insert Bookmark...",default),
            //new (KnownCommands.Format_Italic,"Format: Italic",KnownMonikers.Italic),
            //new (KnownCommands.Format_Justify,"Justify",default),
            //new (KnownCommands.Format_JustifyCenter,"Justify Center",default),
            //new (KnownCommands.Format_JustifyLeft,"Justify Left",default),
            //new (KnownCommands.Format_JustifyRight,"Justify Right",default),
            //new (KnownCommands.Format_LockControls,"Lock Controls",default),
            //new (KnownCommands.Format_MakeHorizontalSpacingEqual,"Make Horizontal Spacing (new Equal",KnownMonikers.DistributeHorizontalCenter),
            //new (KnownCommands.Format_MakeSameHeight,"Make Same Height",KnownMonikers.MakeSameHeight),
            //new (KnownCommands.Format_MakeSameSize,"Make Same Size",default),
            //new (KnownCommands.Format_MakeSameWidth,"Make Same Width",default),
            //new (KnownCommands.Format_MakeVerticalSpacingEqual,"Make Vertical Spacing Equal",KnownMonikers.DistributeVerticalCenter),
            //new (KnownCommands.Format_Optimize,"Format: Optimize",default),
            //new (KnownCommands.Format_RemoveHorizontalSpacing,"Remove Horizontal Spacing",KnownMonikers.RemoveHorizontalSpacing),
            //new (KnownCommands.Format_RemoveVerticalSpacing,"Remove Vertical Spacing",KnownMonikers.RemoveVerticalSpacing),
            //new (KnownCommands.Format_SendtoBack,"Send to Back",default),
            //new (KnownCommands.Format_SizetoContent,"Size to Content",default),
            //new (KnownCommands.Format_SizetoGrid,"Size to Grid",default),
            //new (KnownCommands.Format_SpaceAcross,"Space Across",KnownMonikers.SpaceAcross),
            //new (KnownCommands.Format_SpaceDown,"Space Down",KnownMonikers.SpaceDown),
            //new (KnownCommands.Format_Stretch,"Stretch",default),
            //new (KnownCommands.Format_Subscript,"Subscript",KnownMonikers.Subscript),
            //new (KnownCommands.Format_Superscript,"Superscript",KnownMonikers.Superscript),
            new (KnownCommands.Format_ToggleGuides,"Format: Toggle Guides",KnownMonikers.ToggleGuides),
            //new (KnownCommands.Format_Underline,"Underline",KnownMonikers.Underline),
        ]);

        _commands.AddRange([
            new (KnownCommands.Help_About,"Help: About...",default),
            new (KnownCommands.Help_DebugHelpContext,"Help: Debug Help Context",default),
            new (KnownCommands.Help_F1Help,"Help: F1 Help",KnownMonikers.F1Help),
            new (KnownCommands.Help_RegisterProduct,"Help: Register Visual Studio",default),
            new (KnownCommands.Help_TechnicalSupport,"Help: Technical Support",default),
            new (KnownCommands.Help_WindowHelp,"Help: Window Help",default),
        ]);

        _commands.AddRange([
            new (KnownCommands.Project_AddAssemblyReference,"Project: Add Assembly Reference...",KnownMonikers.Assembly),
            new (KnownCommands.Project_AddClass,"Project: Add Class...",KnownMonikers.AddClass),
            //new (KnownCommands.Project_AddComponent,"Project: Add Component...",KnownMonikers.AddComponent),
            new (KnownCommands.Project_AddCOMReference,"Project: Add COM Reference",KnownMonikers.COM),
            //new (KnownCommands.Project_AddConnectionPoint,"Project: Add Connection Point...",default),
            //new (KnownCommands.Project_AddContentPage,"Project: Add Content Page",default),
            //new (KnownCommands.Project_AddEvent,"Project: Add Event...",KnownMonikers.AddEvent),
            new (KnownCommands.Project_AddExistingItem,"Project: Add Existing Item...",default),
            //new (KnownCommands.Project_AddFormWindowsForms,"Project: Add Form (Windows Forms)...",default),
            //new (KnownCommands.Project_AddFunction,"Project: Add Function...",default),
            //new (KnownCommands.Project_AddHTMLPage,"Project: Add HTML Page",KnownMonikers.AddHTMLPage),
            //new (KnownCommands.Project_AddIndexer,"Project: Add Indexer...",KnownMonikers.AddIndexer),
            new (KnownCommands.Project_AddInterface,"Project: Add Interface...",KnownMonikers.AddInterface),
            //new (KnownCommands.Project_AddMasterPage,"Project: Add Master Page",default),
            new (KnownCommands.Project_AddMethod,"Project: Add Method...",KnownMonikers.AddMethod),
            new (KnownCommands.Project_AddModule,"Project: Add Module...",KnownMonikers.AddModule),
            new (KnownCommands.Project_AddNestedClass,"Project: Add Nested Class...",default),
            new (KnownCommands.Project_AddNewItem,"Project: Add New Item...",KnownMonikers.NewItem),
            new (KnownCommands.Project_AddNewSolutionFolder,"Project: Add New Solution Folder",default),
            new (KnownCommands.Project_AddProjectOutputs,"Project: Add Project Outputs...",default),
            new (KnownCommands.Project_AddProjectReference,"Project: Add Project Reference...",KnownMonikers.AddReference),
            //new (KnownCommands.Project_AddProperty,"Project: Add Property...",KnownMonikers.AddProperty),
            new (KnownCommands.Project_AddReference,"Project: Add Reference...",KnownMonikers.AddReference),
            new (KnownCommands.Project_AddSDKReference,"Project: Add SDK Reference...",KnownMonikers.SDK),
            //new (KnownCommands.Project_AddServiceReference,"Project: Add Service Reference...",default),
            //new (KnownCommands.Project_AddSharedProjectReference,"Project: Add Shared Project Reference...",KnownMonikers.SharedProject),
            //new (KnownCommands.Project_AddStyleSheet,"Project: Add Style Sheet",default),
            //new (KnownCommands.Project_AddUserControlWindowsForms,"Project: Add User Control (Windows Forms)...",default),
            //new (KnownCommands.Project_AddVariable,"Project: Add Variable...",KnownMonikers.AddVariable),
            //new (KnownCommands.Project_AddWebForm,"Project: Add Web Form",KnownMonikers.AddWebForm),
            //new (KnownCommands.Project_AddWebReference,"Project: Add Web Reference...",default),
            //new (KnownCommands.Project_AddWebService,"Project: Add Web Service",KnownMonikers.AddWebService),
            //new (KnownCommands.Project_AddWebUserControl,"Project: Add Web User Control",KnownMonikers.AddWebUserControl),
            //new (KnownCommands.Project_ConfigureServiceReference,"Project: Configure Service Reference...",default),
            new (KnownCommands.Project_EditProjectFile,"Project: Edit Project File",default),
            //new (KnownCommands.Project_ExcludeFromProject,"Project: Exclude From Project",default),
            //new (KnownCommands.Project_HideFolder,"Project: Hide Folder",default),
            //new (KnownCommands.Project_ImplementInterface,"Project: Implement Interface...",KnownMonikers.ImplementInterface),
            //new (KnownCommands.Project_IncludeInProject,"Project: Include In Project",default),
            //new (KnownCommands.Project_NestRelatedFiles,"Project: Nest Related Files",default),
            new (KnownCommands.Project_NewFolder,"Project: New Folder",KnownMonikers.NewFolder),
            //new (KnownCommands.Project_Override,"Project: Override",default),
            new (KnownCommands.Project_ProjectBuildOrder,"Project: Project Build Order...",default),
            new (KnownCommands.Project_ProjectDependencies,"Project: Project Dependencies...",default),
            new (KnownCommands.Project_Properties,"Project: Properties",default),
            //new (KnownCommands.Project_RecalculateLinks,"Project: Recalculate Links",default),
            //new (KnownCommands.Project_ReloadProject,"Project: Reload Project",default),
            //new (KnownCommands.Project_RunCustomTool,"Project: Run Custom Tool",default),
            //new (KnownCommands.Project_SetAsStartPage,"Project: Set As Start Page",default),
            //new (KnownCommands.Project_SetasStartupProject,"Project: Set as Startup Project",default),
            //new (KnownCommands.Project_ShowAllFiles,"Project: Show All Files",KnownMonikers.ShowAllFiles),
            new (KnownCommands.Project_StartOptions,"Project: Start Options...",KnownMonikers.Settings),
            //new (KnownCommands.Project_UnhideFolders,"Project: Unhide Folders",default),
            //new (KnownCommands.Project_UnloadProject,"Project: Unload Project",default),
            //new (KnownCommands.Project_UpdateServiceReference,"Project: Update Service Reference",default),
            //new (KnownCommands.Project_UpdateWebReference,"Project: Update Web Reference",default),
            // --
            new (new CommandID(new Guid("{25FD982B-8CAE-4CBD-A440-E03FFCCDE106}"), 0x100),"Project: Manage NuGet packages...",KnownMonikers.NuGet),
        ]);

        _commands.AddRange([
            new(new CommandID(new Guid("{25FD982B-8CAE-4CBD-A440-E03FFCCDE106}"), 0x200), "Solution: Manage NuGet packages...", KnownMonikers.NuGet),
        ]);

        //commands.AddRange([
        //    new (KnownCommands.Refactor_EncapsulateField,"Refactor: Encapsulate Field...",KnownMonikers.EncapsulateField),
        //    new (KnownCommands.Refactor_ExtractInterface,"Refactor: Extract Interface...",KnownMonikers.ExtractInterface),
        //    new (KnownCommands.Refactor_ExtractMethod,"Refactor: Extract Method...",KnownMonikers.ExtractMethod),
        //    new (KnownCommands.Refactor_RemoveParameters,"Refactor: Remove Parameters...",default),
        //    new (KnownCommands.Refactor_Rename,"Refactor: Rename...",KnownMonikers.Rename),
        //    new (KnownCommands.Refactor_ReorderParameters,"Refactor: Reorder Parameters...",KnownMonikers.ReorderParameters),
        //]);


        //new (KnownCommands.RepeatFind,"Modify Find",KnownMonikers.Edit),
        //new (KnownCommands.SwitchtoFindSymbol,"Find Symbol",default),

        _commands.AddRange([
            //new (KnownCommands.Tools_AddRemoveToolboxItems,"Tools: Choose Toolbox Items...",default),
            //new (KnownCommands.Tools_Alias,"Alias",default),
            //new (KnownCommands.Tools_CodeSnippetsManager,"Tools: Code Snippets Manager...",default),
            //new (KnownCommands.Tools_Customize,"Tools: Customize Toolbars...",default),
            new (KnownCommands.Tools_CustomizeKeyboard,"Tools: Customize Keyboard...",KnownMonikers.Settings),
            //new (KnownCommands.Tools_GoToCommandLine,"Tools: Go To Command Line",default),
            //new (KnownCommands.Tools_ImmediateMode,"Tools: Immediate Mode",default),
            new (KnownCommands.Tools_ImportandExportSettings,"Tools: Import and Export Settings...",KnownMonikers.Settings),
            new (KnownCommands.Tools_Open,"Tools: Open",KnownMonikers.Open),
            //new (KnownCommands.Tools_OpenWith,"Tools: Open With...",default),
            new (KnownCommands.Tools_Options,"Tools: Options",KnownMonikers.Settings),
            new (KnownCommands.ManageExtensions,"Tools: Manage Extensions...",KnownMonikers.Extension),
        ]);

        _commands.AddRange([
            new (
                new CommandID(new Guid("{E286548F-5085-4E2B-A4FD-5984CCB553BC}"), 0x300),
                "View: Call Hierarchy",
                KnownMonikers.CallHierarchy
            ),
            //new (KnownCommands.View_AddRemoveColumns,"View: Add/Remove Columns...",default),
            //new (KnownCommands.View_Autosize,"View: Autosize",default),
            //new (KnownCommands.View_Backward,"View: Backward",default), // Is it for a browser?
            new (KnownCommands.View_BookmarkWindow,"View: Bookmark Window",KnownMonikers.BookmarkMainMenuItem),
            new (KnownCommands.View_BrowseDefinition,"View: Browse Definition",KnownMonikers.BrowseDefinition),
            new (KnownCommands.View_BrowseNext,"View: Browse Next",KnownMonikers.BrowseNext),
            new (KnownCommands.View_BrowsePrevious,"View: Browse Previous",KnownMonikers.BrowsePrevious),
            new (KnownCommands.View_BuildStartupProjectsOnlyOnRun,"View: Build Startup Projects Only On Run",default),
            new (KnownCommands.View_ChooseEncoding,"View: Class View Show Project References",default),
            new (KnownCommands.View_ClassView,"View: Class View",KnownMonikers.ClassDetails),
            new (KnownCommands.View_CodeDefinitionWindow,"View: Code Definition Window",KnownMonikers.CodeDefinitionWindow),
            new (KnownCommands.View_CommandWindow,"View: Command window",default),
            new (KnownCommands.View_Details,"View: View Details",default),
            new (KnownCommands.View_DocumentOutline,"View: Document Outline",KnownMonikers.DocumentOutline),
            //new (KnownCommands.View_EditDefinition,"View: Edit Definition",default),
            //new (KnownCommands.View_EditLabel,"View: Edit Label",KnownMonikers.EditLabel),
            //new (KnownCommands.View_EditMaster,"View: Edit Master",default),
            new (KnownCommands.View_ErrorList,"View: Error List",default),
            new (KnownCommands.View_ExpandAll,"View: Expand All",KnownMonikers.ExpandAll),
            //new (KnownCommands.View_FindResults1,"View: Find Results 1",KnownMonikers.FindInFile),
            //new (KnownCommands.View_FindResults2,"View: Find Results 2",KnownMonikers.FindInFile),
            //new (KnownCommands.View_FindSymbolResults,"View: Find Symbol Results",KnownMonikers.FindSymbol),
            //new (KnownCommands.View_Forward,"View: Forward",default), // Is it for a browser?
            //new (KnownCommands.View_ForwardBrowseContext,"View: Forward Browse Context",default),
            new (KnownCommands.View_FullScreen,"View: Full Screen",KnownMonikers.FullScreen),
            new (KnownCommands.View_NavigateBackward,"View: Navigate Backward",default),
            new (KnownCommands.View_NavigateForward,"View: Navigate Forward",default),
            //new (KnownCommands.View_NextView,"View: Next View",default),
            new (KnownCommands.View_ObjectBrowser,"View: Object Browser",KnownMonikers.ApplicationClass),
            //new (KnownCommands.View_Open,"View: Open",KnownMonikers.Open),
            //new (KnownCommands.View_OpenWith,"View: Open With...",default),
            new (KnownCommands.View_Output,"View: Output",KnownMonikers.Output),
            new (KnownCommands.View_PropertiesWindow,"View: Properties Window",KnownMonikers.Property),
            new (KnownCommands.View_PropertyPages,"View: Property Pages",default),
            new (KnownCommands.View_ResourceView,"View: Resource View",KnownMonikers.ResourceView),
            new (KnownCommands.View_ServerExplorer,"View: Server Explorer",default),
            new (KnownCommands.View_ShowReferences,"View: Show References",default),
            new (KnownCommands.View_SolutionExplorer,"View: Solution Explorer",default),
            new (KnownCommands.View_TaskList,"View: Task List",KnownMonikers.TaskList),
            new (KnownCommands.View_ToggleDesigner,"View: Toggle Designer",default),
            new (KnownCommands.View_Toolbox,"View: Toolbox",KnownMonikers.ToolBox),
            new (KnownCommands.View_ViewCode,"View: View Code",default),
            new (KnownCommands.View_ViewComponentDesigner,"View: View Component Designer",default),
            new (KnownCommands.View_ViewDesigner,"View: View Designer",default),
            new (KnownCommands.View_ViewMarkup,"View: View Markup",default),
            new (KnownCommands.View_Zoom,"View: Zoom...",KnownMonikers.Zoom),
            new (KnownCommandsEx.File_StartWindow,"View: Start Window",KnownMonikers.ShowStartWindow),
        ]);

        _commands.AddRange([
            new (KnownCommands.Window_ActivateDocumentWindow,"Window: Activate Document Window",default),
            new (KnownCommands.Window_AutoHide,"Window: Auto Hide",default),
            new (KnownCommands.Window_AutoHideAll,"Window: Auto Hide All",default),
            new (KnownCommands.Window_Cascade,"Window: Cascade",default),
            new (KnownCommands.Window_Close,"Window: Close",KnownMonikers.Close),
            new (KnownCommands.Window_CloseAllDocuments,"Window: Close All Tabs",default),
            new (KnownCommands.Window_CloseDocumentWindow,"Window: Close Document Window",default),
            new (KnownCommands.Window_CloseToolWindow,"Window: Close Tool Window",default),
            new (KnownCommands.Window_MovetoNavigationBar,"Window: Move to Navigation Bar",default),
            new (KnownCommands.Window_MovetoNextTabGroup,"Window: Move to Next Document Group",default),
            new (KnownCommands.Window_MovetoPreviousTabGroup,"Window: Move to Previous Document Group",default),
            new (KnownCommands.Window_NewHorizontalTabGroup,"Window: New Horizontal Document Group",default),
            new (KnownCommands.Window_NewVerticalTabGroup,"Window: New Vertical Document Group",default),
            new (KnownCommands.Window_NewWindow,"Window: New Window",KnownMonikers.NewWindow),
            new (KnownCommands.Window_NextDocumentWindow,"Window: Next Document Window",default),
            new (KnownCommands.Window_NextDocumentWindowNav,"Window: Next Document Window Nav",default),
            new (KnownCommands.Window_NextPane,"Window: Next Pane",default),
            new (KnownCommands.Window_NextSplitPane,"Window: Next Split Pane",default),
            new (KnownCommands.Window_NextSubpane,"Window: Next Subpane",default),
            new (KnownCommands.Window_NextTab,"Window: Next Tab",default),
            new (KnownCommands.Window_NextToolWindow,"Window: Next Tool Window",default),
            new (KnownCommands.Window_PreviousDocumentWindow,"Window: Previous Document Window",default),
            new (KnownCommands.Window_PreviousDocumentWindowNav,"Window: Previous Document Window Nav",default),
            new (KnownCommands.Window_PreviousPane,"Window: Previous Pane",default),
            new (KnownCommands.Window_PreviousSplitPane,"Window: Previous Split Pane",default),
            new (KnownCommands.Window_PreviousSubpane,"Window: Previous Subpane",default),
            new (KnownCommands.Window_PreviousTab,"Window: Previous Tab",default),
            new (KnownCommands.Window_PreviousToolWindow,"Window: Previous Tool Window",default),
            new (KnownCommands.Window_PreviousToolWindowNav,"Window: Previous Tool Window Nav",default),
            new (KnownCommands.Window_Split,"Window: Split window",KnownMonikers.Split),
            new (KnownCommands.Window_Windows,"Window: Windows...",default),
        ]);

        // ----------
        // Related to the 'Error List' window.
        // ----------
        //_commands.AddRange([
        //    new (KnownCommands.Messages,"Messages",KnownMonikers.Message),
        //    new (KnownCommands.Warnings,"Warnings",default),
        //    new (KnownCommands.Errors,"Errors",default),
        //]);

        // ----------
        // Tests
        // ----------
        {
            _commands.AddRange([
                new (KnownCommandsEx.TestExplorer_OpenToolWindow,"View: Test Explorer",KnownMonikers.TestResult),
                new (KnownCommandsEx.TestExplorer_RunTests,"Test: Run tests",KnownMonikersEx.TestExplorer_RunTests),
                new (KnownCommandsEx.TestExplorer_DebugTests,"Test: Debug tests",KnownMonikers.DebugSelection),
                new (KnownCommandsEx.TestExplorer_ClearResults,"Test: Clear results",default),
            ]);
        }

        // ----------
        // VS Extensions
        // ----------
        var packages = packageInfoService.GetPackages();
        if (packages.Any(x => x.PackageGuid == KnownPackages.VsDbg)) {
            _commands.AddRange([
                new (KnownCommandsEx.VsDbg_ToggleJSDebugging,"Debug: Toggle JavaScript Debugging",KnownMonikers.Settings),
                new (KnownCommandsEx.VsDbg_ToggleJustMyCode,"Debug: Toggle Just My Code",KnownMonikers.Settings),
                new (KnownCommandsEx.VsDbg_ToggleXamlHotReload,"Debug: Toggle XAML Hot Reload",KnownMonikers.Settings),
                new (KnownCommandsEx.VsDbg_ToggleCodeLens,"Text Editor: Toggle CodeLens",KnownMonikers.Settings),
                new (KnownCommandsEx.VsDbg_ToggleCSharpFadeOut,"Text Editor: Toggle C# Fade Out",KnownMonikers.Settings),
                new (KnownCommandsEx.VsDbg_ToggleCSharpInlineParameterNameHint,"Text Editor: C# Inline Parameter Name Hint",KnownMonikers.Settings),
                new (KnownCommandsEx.VsDbg_ToggleAspnetIntegratedTerminal,"Debug: Toggle AspNet Integrator Terminal",KnownMonikers.Settings),
            ]);
        }

        if (packages.Any(x => x.PackageGuid == KnownPackages.SettingsStoreExplorer)) {
            _commands.AddRange([
                new (KnownCommandsEx.SettingsStoreExplorer_OpenToolWindow,"View: Settings Store Explorer", KnownMonikersEx.SettingsStoreExplorer_OpenToolWindow),
            ]);
        }

        if (packages.Any(x => x.PackageGuid == KnownPackages.KnownMonikerExplorer)) {
            _commands.AddRange([
                new (KnownCommandsEx.KnownMonikerExplorer_OpenToolWindow,"View: Known Monikers Explorer",KnownMonikers.Image),
            ]);
        }

        if (packages.Any(x => x.PackageGuid == KnownPackages.CommandTableInfo)) {
            _commands.AddRange([
                new (KnownCommandsEx.CommandTableInfo_OpenToolWindow,"View: Command Explorer",KnownMonikers.CommandUIOption),
            ]);
        }

        if (packages.Any(x => x.PackageGuid == KnownPackages.AddAnyFile)) {
            _commands.AddRange([
                new (KnownCommandsEx.AddAnyFile_NewEmptyFile,"Project: Add Quick File...",KnownMonikers.AddTextFile),
            ]);
        }

        var bindings = commandService.GetCommands();
        var bindingDict = new Dictionary<(Guid, int), string>();
        foreach (var x in bindings) {
            bindingDict[(new Guid(x.Guid), x.ID)] = x.Shortcuts?.FirstOrDefault() ?? "";
        };
        foreach (var cmd in _commands) {
            if (bindingDict.TryGetValue((cmd.Command.Guid, cmd.Command.ID), out var shortcut)) {
                cmd.Shortcut = shortcut;
            }
        }
        return _commands;
    }

    static class KnownPackages {
        public static Guid AddAnyFile = new Guid("{27dd9dea-6dd2-403e-929d-3ff20d896c5e}"); // Mads' AddQuickFile
        public static Guid CommandTableInfo = new Guid("{82d76b14-dcc0-423c-8b05-bac944909ebd}"); // Mads' Commands Explorer
        public static Guid KnownMonikerExplorer = new Guid("{4256ca61-2162-4ca2-8d10-4c6a2794521c}"); // Mads' Known Moniker Explorer
        public static Guid VsDbg = new Guid("{bbda3f82-23d0-4ed9-8244-3701c4042139}"); // Dubeg's misc commands
        public static Guid SelectNextOccurrence = new Guid("{70ceadaa-6e24-4d6d-92aa-6e5a836ca8f2}");
        public static Guid SettingsStoreExplorer = new Guid("{e8762000-5824-4411-bc19-417b39b309f5}");
        // TODO:
        // Add packages for text editing:
        // - Subword nav
        // - VSTricks
        // - ...
    }

    static class KnownMonikersEx {
        public static ImageMoniker SettingsStoreExplorer_OpenToolWindow = new ImageMoniker() { Guid = new Guid("{3da9ddb5-b35b-4ed6-9d52-73aa4c30127e}"), Id = 1 };
        public static ImageMoniker TestExplorer_RunTests = new ImageMoniker() { Guid = new Guid("{b23d3054-21dc-479f-a9fa-ebd0fb383112}"), Id = 1003 };
    }

    static class KnownCommandsEx {
        public static Guid AddAnyFile_CmdSet = new Guid("{32af8a17-bbbc-4c56-877e-fc6c6575a8cf}");
        public static CommandID AddAnyFile_NewEmptyFile = new CommandID(AddAnyFile_CmdSet, 0x100);

        public static Guid CommandTableInfo_CmdSet = new Guid("{a5bb06f5-7f0a-4194-b06f-0bb25d48f268}");
        public static CommandID CommandTableInfo_OpenToolWindow = new CommandID(CommandTableInfo_CmdSet, 0x200);

        public static CommandID KnownMonikerExplorer_OpenToolWindow = new CommandID(KnownPackages.KnownMonikerExplorer, 0x100);

        public static Guid SettingsStoreExplorer_CmdSet = new Guid("{9fc9f69d-174d-4876-b28b-dc1e4fac89dc}");
        public static CommandID SettingsStoreExplorer_OpenToolWindow = new CommandID(SettingsStoreExplorer_CmdSet, 0x100);

        public static CommandID VsDbg_ToggleJSDebugging = new CommandID(KnownPackages.VsDbg, 0x100);
        public static CommandID VsDbg_ToggleJustMyCode = new CommandID(KnownPackages.VsDbg, 0x200);
        public static CommandID VsDbg_ToggleXamlHotReload = new CommandID(KnownPackages.VsDbg, 0x300);
        public static CommandID VsDbg_ToggleCodeLens = new CommandID(KnownPackages.VsDbg, 0x400);
        public static CommandID VsDbg_ToggleCSharpFadeOut = new CommandID(KnownPackages.VsDbg, 0x500);
        public static CommandID VsDbg_ToggleCSharpInlineParameterNameHint = new CommandID(KnownPackages.VsDbg, 0x600);
        public static CommandID VsDbg_ToggleAspnetIntegratedTerminal = new CommandID(KnownPackages.VsDbg, 0x700);

        private static Guid TestExplorer_CmdSet = new Guid("{1E198C22-5980-4E7E-92F3-F73168D1FB63}");
        public static CommandID TestExplorer_OpenToolWindow = new CommandID(TestExplorer_CmdSet, 0x200);
        public static CommandID TestExplorer_RunTests = new CommandID(TestExplorer_CmdSet, 0x310);
        public static CommandID TestExplorer_DebugTests = new CommandID(TestExplorer_CmdSet, 0x315);
        public static CommandID TestExplorer_ClearResults = new CommandID(TestExplorer_CmdSet, 0x322);

        public static CommandID File_StartWindow = new CommandID(new Guid("{7C57081E-4F31-4EBF-A96F-4769E1D688EC}"), 0x120);
    }
}
