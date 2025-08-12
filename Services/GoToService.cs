using Microsoft.VisualStudio.Text;
using QuickJump2022.Models;

namespace QuickJump2022.Services;
public class GoToService {
    public async Task GoToFileAsync(ListItemFile file) {
        var openedDoc = await VS.Documents.IsOpenAsync(fileName: file.FilePath);
        if (openedDoc) await VS.Documents.OpenAsync(file.FilePath);
        else  await VS.Documents.OpenInPreviewTabAsync(file.FilePath);
    }

    public async Task GoToSymbolAsync(ListItemSymbol symbol) {
        var doc = await VS.Documents.GetActiveDocumentViewAsync();
        await GoToLineAsync(doc, symbol.Line);
    }
    
    public async Task GoToLineAsync(DocumentView documentView, int lineNumber) {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

        var textView = documentView.TextView;
        if (textView?.TextBuffer != null) {
            var snapshot = textView.TextBuffer.CurrentSnapshot;

            // Convert 1-based line number to 0-based
            int zeroBasedLine = Math.Max(0, lineNumber - 1);

            // Ensure line number is within bounds
            if (zeroBasedLine < snapshot.LineCount) {
                var line = snapshot.GetLineFromLineNumber(zeroBasedLine);
                var point = new SnapshotPoint(snapshot, line.Start);

                // Move caret to the line
                textView.Caret.MoveTo(point);

                // Ensure the line is visible
                textView.ViewScroller.EnsureSpanVisible(new SnapshotSpan(point, 0));
            }
        }
    }
}
