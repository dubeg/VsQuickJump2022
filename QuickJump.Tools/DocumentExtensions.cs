using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;
using EnvDTE;
using QuickJump2022.Tools;

namespace QuickJump2022.QuickJump.Tools;

public static class DocumentExtensions {

    public static void GoToLine(this ProjectItem projectItem, int lineNo) {
        ThreadHelper.ThrowIfNotOnUIThread();
        var fullPath = projectItem.TryGetProperty<string>("FullPath");
        if (string.IsNullOrEmpty(fullPath)) return; 
        QuickJumpData.Instance.Dte.ItemOperations.OpenFile(fullPath, "{00000000-0000-0000-0000-000000000000}");
        var document = projectItem.Document;
        document.GoToLine(lineNo);
    }

    public static void GoToLine(this Document document, int lineNo) {
        ThreadHelper.ThrowIfNotOnUIThread("Go");
        if (document != null) {
            var selection = document.Selection;
            if (selection is TextSelection txtSel) {
                txtSel.GotoLine(lineNo, false);
                txtSel.StartOfLine((vsStartOfLineOptions)1, false);
            }
        }
    }
}
