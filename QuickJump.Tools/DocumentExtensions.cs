using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;
using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using QuickJump2022.Tools;

namespace QuickJump2022.QuickJump.Tools;

public static class DocumentExtensions {

    public static void GoToLine(this ProjectItem projectItem, int lineNo, bool commit = false) {
        ThreadHelper.ThrowIfNotOnUIThread();
        if (!commit) {
            projectItem.PreviewFile();
        }
        else {
            var fullPath = projectItem.TryGetProperty<string>("FullPath");
            if (string.IsNullOrEmpty(fullPath)) return;
            var window = QuickJumpData.Instance.Dte.ItemOperations.OpenFile(fullPath, "{00000000-0000-0000-0000-000000000000}");
            var document = projectItem.Document;
            document.GoToLine(lineNo, commit);
        }
    }

    public static void GoToLine(this Document document, int lineNo, bool commit = false) {
        ThreadHelper.ThrowIfNotOnUIThread("Go");
        if (document != null) {
            var selection = document.Selection;
            if (selection is TextSelection txtSel) {
                txtSel.GotoLine(lineNo, false);
                txtSel.StartOfLine((vsStartOfLineOptions)1, false);
            }
        }
    }

    public static void PreviewFile(this ProjectItem projectItem) {
        var filePath = projectItem.TryGetProperty<string>("FullPath");
        var openDoc = Package.GetGlobalService(typeof(SVsUIShellOpenDocument)) as IVsUIShellOpenDocument;
        Guid logicalView = VSConstants.LOGVIEWID_Primary;
        int hr = openDoc.OpenDocumentViaProject(
            filePath,
            ref logicalView,
            out _,
            out var hierarchy,
            out var itemid,
            out var windowFrame
        );
        if (windowFrame != null) {
            // Set preview mode
            windowFrame.SetProperty((int)__VSFPROPID5.VSFPROPID_IsProvisional, true);
            windowFrame.ShowNoActivate();
        }
    }
}
