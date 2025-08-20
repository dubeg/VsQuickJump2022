using System.Linq;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Formatting;
using Microsoft.VisualStudio.TextManager.Interop;
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
                textView.Caret.MoveTo(point);
                // -------
                // Ensure span is visible
                // TODO: make sure this isn't too slow.
                // If it is, we'll revert to the fastest solution,
                // even if it's annoying visually.
                // -------
                textView.TryGetTextViewLineContainingBufferPosition(point, out var textViewLine);
                var isLineWithinViewport = false;
                var isLineBelowViewportCenter = false;
                if (textViewLine is not null) {
                    var isLineBelowViewport = textViewLine.Bottom > textView.ViewportBottom;
                    var isLineAboveViewport = textViewLine.Bottom < textView.ViewportTop;
                    isLineWithinViewport = !isLineBelowViewport && !isLineAboveViewport;
                    isLineBelowViewportCenter = textViewLine.Bottom > (textView.ViewportTop + textView.ViewportHeight / 2);
                }

                if (!isLineWithinViewport || isLineBelowViewportCenter) {
                    var marginInPixels = 75;
                    var span = new SnapshotSpan(point, 0);
                    var vspan = new VirtualSnapshotSpan(span); // I don't know why we have to do this.
                    vspan = vspan.TranslateTo(textView.TextSnapshot);
                    textView.DisplayTextLineContainingBufferPosition(vspan.SnapshotSpan.Start, marginInPixels, ViewRelativePosition.Top);
                }
                else {
                    textView.ViewScroller.EnsureSpanVisible(
                        new SnapshotSpan(point, 0),
                        EnsureSpanVisibleOptions.ShowStart
                    );
                }
            }
        }
    }
}
