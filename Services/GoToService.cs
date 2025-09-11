using System.IO;
using System.Linq;
using System.Windows.Forms;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Formatting;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Utilities;
using QuickJump2022.Models;

namespace QuickJump2022.Services;
public class GoToService {
    public void GoToFile(ListItemFile file) {
        using var scope = new NewDocumentStateScope(__VSNEWDOCUMENTSTATE.NDS_Permanent, VSConstants.NewDocumentStateReason.Navigation);
        VsShellUtilities.OpenDocument(
            ServiceProvider.GlobalProvider,
            file.FilePath,
            VSConstants.LOGVIEWID.Primary_guid,
            out var hierarchy,
            out var itemId,
            out var windowFrame
        );
    }

    public async Task PreviewFileAsync(ListItemFile file) {
        if (!IsTextFile(file.FilePath)) return;
        var openedDoc = await VS.Documents.IsOpenAsync(fileName: file.FilePath);
        if (openedDoc) await VS.Documents.OpenAsync(file.FilePath);
        //else await VS.Documents.OpenInPreviewTabAsync(file.FilePath);
        else OpenDocumentInPreview(file.FilePath);
    }

    public void OpenDocumentInPreview(string filePath) {
        ThreadHelper.ThrowIfNotOnUIThread();
        using var scope = new NewDocumentStateScope(__VSNEWDOCUMENTSTATE2.NDS_TryProvisional, VSConstants.NewDocumentStateReason.Navigation);
        VsShellUtilities.OpenDocumentWithSpecificEditor(
            ServiceProvider.GlobalProvider,
            filePath,
            VSConstants.VsEditorFactoryGuid.TextEditor_guid,
            VSConstants.LOGVIEWID.TextView_guid,
            out var hierarchy,
            out var itemId,
            out var windowFrame
        );
        if (windowFrame.GetProperty((int)__VSFPROPID.VSFPROPID_DocView, out var docView) == VSConstants.S_OK) {
            if (docView is IVsCodeWindow codeWindow) {
                codeWindow.GetPrimaryView(out var vsTextView);
                var componentModel = (IComponentModel)Package.GetGlobalService(typeof(SComponentModel));
                var adaptersFactory = componentModel.GetService<IVsEditorAdaptersFactoryService>();
                var wpfTextView = adaptersFactory.GetWpfTextView(vsTextView);
                if (wpfTextView != null) {
                    var options = wpfTextView.Options;
                    options.SetOptionValue(DefaultTextViewHostOptions.EnableFileHealthIndicatorOptionId, false);
                    options.SetOptionValue(DefaultTextViewHostOptions.EditingStateMarginOptionId, false);
                    options.SetOptionValue(DefaultTextViewHostOptions.SourceImageMarginEnabledOptionId, false);
                    options.SetOptionValue(DefaultTextViewHostOptions.ShowChangeTrackingMarginOptionId, false);
                    options.SetOptionValue(DefaultTextViewHostOptions.ChangeTrackingId, false);
                    options.SetOptionValue(DefaultTextViewHostOptions.VerticalScrollBarId, false);
                    options.SetOptionValue(DefaultTextViewHostOptions.HorizontalScrollBarId, false);
                    options.SetOptionValue(DefaultTextViewHostOptions.LineNumberMarginId, false);
                    options.SetOptionValue(DefaultTextViewHostOptions.OutliningMarginId, false);
                    options.SetOptionValue(DefaultTextViewHostOptions.GlyphMarginId, false);
                    options.SetOptionValue(DefaultTextViewHostOptions.SuggestionMarginId, false);
                    options.SetOptionValue(DefaultTextViewHostOptions.SelectionMarginId, false);
                    options.SetOptionValue(DefaultTextViewHostOptions.ZoomControlId, false);
                    options.SetOptionValue(DefaultTextViewHostOptions.ShowMarksOptionId, false);
                    options.SetOptionValue(DefaultTextViewOptions.DisplayUrlsAsHyperlinksId, false);
                    options.SetOptionValue(DefaultTextViewOptions.WordWrapStyleId, WordWrapStyles.None);
                    options.SetOptionValue(DefaultTextViewOptions.BraceCompletionEnabledOptionId, false);
                }
            }
        }
    }

    public bool IsTextFile(string filePath) {
        var componentModel = (IComponentModel)Package.GetGlobalService(typeof(SComponentModel));
        var contentTypeRegistry = componentModel.GetService<IContentTypeRegistryService>();
        var fileExtensionRegistry = componentModel.GetService<IFileExtensionRegistryService>();
        var extension = Path.GetExtension(filePath);
        if (string.IsNullOrEmpty(extension))
            return false;
        extension = extension.Substring(1);
        var contentType = fileExtensionRegistry.GetContentTypeForExtension(extension);
        if (contentType != null) {
            // Check if it derives from "text" content type
            var textContentType = contentTypeRegistry.GetContentType("text");
            return contentType.IsOfType("text") || contentType.IsOfType("code");
        }
        return false;
    }

    // --

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
                // -------
                // TODO: make this configurable.
                // It may be preferable to avoid being too "smart" & cause unnecessary layout updates,
                // messing with the user's eye-tracking.
                // If we disable this, we should then always call EnsureSpanVisible(ShowStart).
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
