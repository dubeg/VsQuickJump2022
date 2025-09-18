using EnvDTE;
using Microsoft.CodeAnalysis.Classification;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Formatting;
using Microsoft.VisualStudio.Utilities;
using QuickJump2022.Tools;
using System.ComponentModel.Composition;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace QuickJump2022.TextEditor;

public partial class InputTextEditor : UserControl {
    public event EventHandler TextChanged;
    public event InputEditorSpecialKeyHandler SpecialKeyPressed;
    public static readonly DependencyProperty TextProperty = DP.Register<InputTextEditor, string>(nameof(Text), string.Empty, OnTextChanged);
    public static readonly DependencyProperty BorderBackgroundProperty = DP.Register<InputTextEditor, Brush>(nameof(BorderBackground), Brushes.Transparent, null);

    public string Text {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public Brush BorderBackground {
        get => (Brush)GetValue(BorderBackgroundProperty);
        set => SetValue(BorderBackgroundProperty, value);
    }

    //[Import] internal ITextEditorFactoryService TextEditorFactoryService { get; set; }
    //[Import] internal ITextBufferFactoryService TextBufferFactoryService { get; set; }
    [Import] internal IClassificationTypeRegistryService ClassificationTypeRegistryService { get; set; }
    [Import] internal IClassificationFormatMapService ClassificationFormatMapService { get; set; }
    [Import] internal IEditorFormatMapService EditorFormatMapService { get; set; }
    //[Import] internal IClassifierAggregatorService ClassifierAggregatorService { get; set; }
    [Import] internal IContentTypeRegistryService ContentTypeRegistryService { get; set; }

    private EditorHost _editorHost;
    private IWpfTextView TextView => _editorHost?.WpfTextView;
    private ITextBuffer TextBuffer => _editorHost?.WpfTextView?.TextBuffer;

    public InputTextEditor() {
        InitializeComponent();
        InitializeEditor();
        Unloaded += (_, _) => {
            _editorHost?.Close();
        };
    }


    private void InitializeEditor() {
        var componentModel = (IComponentModel)Package.GetGlobalService(typeof(SComponentModel));
        if (componentModel == null) {
            EditorHost.Content = "Design view is not supported because MEF services are not available in design mode.";
            return;
        }
        componentModel.DefaultCompositionService.SatisfyImportsOnce(this);
        _editorHost = new EditorHost();
        var textViewHost = _editorHost.InitializeHost();
        // --
        var classificationFormatMap = ClassificationFormatMapService.GetClassificationFormatMap(textViewHost.TextView);
        var defaultTextProps = classificationFormatMap.DefaultTextProperties;
        var baseEmSize = defaultTextProps.FontRenderingEmSize;
        if (baseEmSize > 0) {
            var targetZoom = (FontSize / baseEmSize) * 100.0;
            textViewHost.TextView.ZoomLevel = targetZoom;
        }
        Height = CalculateSingleLineHeight(textViewHost.TextView)
            + Padding.Top
            + Padding.Bottom
            + BorderThickness.Top
            + BorderThickness.Bottom
            + EditorBorder.BorderThickness.Top
            + EditorBorder.BorderThickness.Bottom
            ;
        EditorHost.Content = textViewHost.HostControl;
        _editorHost.KeyPressed += (_, args) => SpecialKeyPressed?.Invoke(this, args);
        _editorHost.CommandFilter.KeyPressed += (_, args) => SpecialKeyPressed?.Invoke(this, args);
        _editorHost.CommandFilter.PreCommand += (_, _) => _textLastValue = Text;
        _editorHost.CommandFilter.PostCommand += (_, _) => {
            Text = TextBuffer.CurrentSnapshot.GetText();
            if (Text != _textLastValue) {
                TextChanged?.Invoke(this, EventArgs.Empty);
            }
        };
        // --
        var formatMap = EditorFormatMapService.GetEditorFormatMap(TextView);
        var resources = formatMap.GetProperties("TextView Background");
        if (resources.Contains(EditorFormatDefinition.BackgroundBrushId)) {
            var backgroundBrush = resources[EditorFormatDefinition.BackgroundBrushId] as Brush;
            if (backgroundBrush is SolidColorBrush solidBrush) {
                BorderBackground = backgroundBrush;
            }
        }
        else { 
            // TODO: try to get it using FontsAndColors service.
            // I'm not sure it's needed though. The code here might never run.
        }
        // --
        Focus();
    }

    private string _textLastValue;

    private double CalculateSingleLineHeight(IWpfTextView textView) {
        var classificationFormatMap = ClassificationFormatMapService.GetClassificationFormatMap(textView);
        var props = classificationFormatMap.DefaultTextProperties;
        var typeface = props.Typeface;
        var emSize = props.FontRenderingEmSize;
        if (typeface != null && typeface.TryGetGlyphTypeface(out var glyph)) {
            // Calculate line height from glyph metrics
            // Height represents the em height (typically 1.0), so we need to estimate line spacing.
            var lineSpacingFactor = 1;
            var lineHeight = glyph.Height * lineSpacingFactor * emSize;
            var zoom = textView.ZoomLevel / 100.0;
            return lineHeight * zoom; // DIPs
        }
        // Fallback
        return textView.LineHeight;
    }

    public void SuspendProcessing() => _editorHost.StopProcessing();

    private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
        if (d is InputTextEditor control && e.NewValue is string newText) {
            control.UpdateText(newText);
        }
    }

    private void UpdateText(string text) {
        if (TextBuffer != null) {
            var textBuffer = TextBuffer;
            using (var edit = textBuffer.CreateEdit()) {
                edit.Replace(0, textBuffer.CurrentSnapshot.Length, text ?? string.Empty);
                edit.Apply();
            }
        }
    }

    public void SelectAll() {
        if (TextBuffer != null && TextBuffer.CurrentSnapshot.Length > 0) {
            var textView = TextView;
            if (textView != null) {
                var snapshot = textView.TextBuffer.CurrentSnapshot;
                textView.Selection.Select(new SnapshotSpan(snapshot, 0, snapshot.Length), false);
                textView.Caret.MoveTo(new SnapshotPoint(snapshot, snapshot.Length));
            }
        }
    }

    public void SetCaretToEnd() {
        var textView = TextView;
        if (textView != null) {
            var snapshot = textView.TextBuffer.CurrentSnapshot;
            textView.Caret.MoveTo(new SnapshotPoint(snapshot, snapshot.Length));
        }
    }

    public new void Focus() {
        var element = TextView.VisualElement;
        element.Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(() => element.Focus()));
    }
}

