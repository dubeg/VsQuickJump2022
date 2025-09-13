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

public partial class InputTextEditor : UserControl, IDisposable {
    
    // Event that fires when Escape key is pressed
    public event EventHandler EscapePressed;
    
    // Event that fires when Enter key is pressed
    public event EventHandler EnterPressed;
    
    public InputTextEditor() {
        InitializeComponent();
        InitializeEditor();
    }

    public static readonly DependencyProperty TextProperty =
    DependencyProperty.Register(nameof(Text), typeof(string), typeof(InputTextEditor),
new PropertyMetadata(string.Empty, OnTextChanged));

    public static readonly DependencyProperty TextBufferProperty =
        DependencyProperty.Register(nameof(TextBuffer), typeof(ITextBuffer), typeof(InputTextEditor),
            new PropertyMetadata(null));

    public string Text {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public ITextBuffer TextBuffer {
        get => (ITextBuffer)GetValue(TextBufferProperty);
        set => SetValue(TextBufferProperty, value);
    }

    [Import] internal ITextEditorFactoryService TextEditorFactoryService { get; set; }
    [Import] internal ITextBufferFactoryService TextBufferFactoryService { get; set; }
    [Import] internal IClassificationTypeRegistryService ClassificationTypeRegistryService { get; set; }
    [Import] internal IClassificationFormatMapService ClassificationFormatMapService { get; set; }
    [Import] internal IEditorFormatMapService EditorFormatMapService { get; set; }
    [Import] internal IClassifierAggregatorService ClassifierAggregatorService { get; set; }
    [Import] internal IContentTypeRegistryService ContentTypeRegistryService { get; set; }

    private Hoster _hoster;
    public IWpfTextViewHost TextViewHost { get; private set; }

    private void InitializeEditor() {
        var componentModel = (IComponentModel)Microsoft.VisualStudio.Shell.Package.GetGlobalService(typeof(SComponentModel));
        if (componentModel == null) {
            EditorHost.Content = "Design view is not supported because MEF services are not available in design mode.";
            return;
        }
        componentModel.DefaultCompositionService.SatisfyImportsOnce(this);
        _hoster = TextEditor.HostEditorHelper.CreateHoster(
            "FILTER...",
            "text", // Content type (language)
            textViewRoles: [
                PredefinedTextViewRoles.Editable,
            PredefinedTextViewRoles.Interactive,
            ],
            TextEditor.AllowedCommands.Instance,
            (view) => {
                view.Options.SetOptionValue(DefaultTextViewHostOptions.EnableFileHealthIndicatorOptionId, false);
                view.Options.SetOptionValue(DefaultTextViewHostOptions.EditingStateMarginOptionId, false);
                view.Options.SetOptionValue(DefaultTextViewHostOptions.SourceImageMarginEnabledOptionId, false);
                view.Options.SetOptionValue(DefaultTextViewHostOptions.ShowChangeTrackingMarginOptionId, false);
                view.Options.SetOptionValue(DefaultTextViewHostOptions.ChangeTrackingId, false);
                view.Options.SetOptionValue(DefaultTextViewHostOptions.VerticalScrollBarId, false);
                view.Options.SetOptionValue(DefaultTextViewHostOptions.HorizontalScrollBarId, false);
                view.Options.SetOptionValue(DefaultTextViewHostOptions.LineNumberMarginId, false);
                view.Options.SetOptionValue(DefaultTextViewHostOptions.OutliningMarginId, false);
                view.Options.SetOptionValue(DefaultTextViewHostOptions.GlyphMarginId, false);
                view.Options.SetOptionValue(DefaultTextViewHostOptions.SuggestionMarginId, false);
                view.Options.SetOptionValue(DefaultTextViewHostOptions.SelectionMarginId, false);
                view.Options.SetOptionValue(DefaultTextViewHostOptions.ZoomControlId, false);
                view.Options.SetOptionValue(DefaultTextViewHostOptions.ShowMarksOptionId, false);
                view.Options.SetOptionValue(DefaultTextViewOptions.WordWrapStyleId, WordWrapStyles.None);
                view.Options.SetOptionValue(DefaultTextViewOptions.UseVisibleWhitespaceId, true);
                view.Options.SetOptionValue(DefaultOptions.FallbackFontId, "Consolas"); // Unused
            },
            false
        );
        var textViewHost = _hoster.GetTextViewHost();
        // --
        var classificationFormatMap = ClassificationFormatMapService.GetClassificationFormatMap(textViewHost.TextView);
        var defaultTextProps = classificationFormatMap.DefaultTextProperties;
        var baseEmSize = defaultTextProps.FontRenderingEmSize;
        if (baseEmSize > 0) {
            var targetZoom = (24.0 / baseEmSize) * 100.0;
            textViewHost.TextView.ZoomLevel = targetZoom;
        }
        textViewHost.TextView.ZoomLevelChanged += (_, __) => UpdateHeightFromTextView(textViewHost.TextView);
        
        // Initial height calculation
        UpdateHeightFromTextView(textViewHost.TextView);

        TextViewHost = textViewHost;
        EditorHost.Content = TextViewHost.HostControl;
        
        // Subscribe to key press events from the command filter
        if (_hoster.CommandFilter != null) {
            _hoster.CommandFilter.EscapePressed += OnEscapePressed;
            _hoster.CommandFilter.EnterPressed += OnEnterPressed;
        }
        
        // TODO: What's the difference?
        // Content = textViewHost.HostControl; 
        Focus();
    }

    private void UpdateHeightFromTextView(IWpfTextView textView) {
        var height = CalculateSingleLineHeight(textView)
            + BorderThickness.Top
            + BorderThickness.Bottom
            ;
        this.Height = height;
        //EditorHost.Height = height;
    }

    private double CalculateSingleLineHeight(IWpfTextView textView) {
        // Method 1: Try to get an actual text view line (most accurate)
        // This works when the text view has been laid out and contains content
        //var snapshot = textView.TextBuffer.CurrentSnapshot;
        //if (snapshot.Length > 0) {
        //    var firstLineStart = snapshot.GetLineFromLineNumber(0).Start;
        //    var point = new SnapshotPoint(snapshot, firstLineStart);
            
        //    if (textView.TryGetTextViewLineContainingBufferPosition(point, out var textViewLine)) {
        //        // The line height already includes zoom level and all formatting
        //        return textViewLine.Height;
        //    }
        //}

        // Method 2: Calculate from font metrics if no line is available
        // This is used when the text view is empty or not yet laid out
        var classificationFormatMap = ClassificationFormatMapService.GetClassificationFormatMap(textView);
        var props = classificationFormatMap.DefaultTextProperties;
        var typeface = props.Typeface;
        var emSize = props.FontRenderingEmSize;
        
        if (typeface != null && typeface.TryGetGlyphTypeface(out var glyph)) {
            // Calculate line height from glyph metrics
            // Height represents the em height (typically 1.0), so we need to estimate line spacing
            var lineSpacingFactor = 1.2; // Common line spacing factor
            var lineHeight = glyph.Height * lineSpacingFactor * emSize;
            var zoom = textView.ZoomLevel / 100.0;
            return lineHeight * zoom; // DIPs
        }
        
        // Method 3: Fallback to LineHeight property
        // This is the simplest but may not reflect custom formatting
        return textView.LineHeight;
    }

    private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
        if (d is InputTextEditor control && e.NewValue is string newText) {
            control.UpdateText(newText);
        }
    }

    private void UpdateText(string text) {
        if (TextViewHost != null) {
            var textBuffer = TextViewHost.TextView.TextBuffer;
            using (var edit = textBuffer.CreateEdit()) {
                edit.Replace(0, textBuffer.CurrentSnapshot.Length, text ?? string.Empty);
                edit.Apply();
            }
        }
    }

    public new void Focus() {
        var element = TextViewHost.TextView.VisualElement;
        element.Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(() => element.Focus()));
    }
    
    private void OnEscapePressed(object sender, EventArgs e) {
        // Bubble the escape event up to any listeners
        EscapePressed?.Invoke(this, EventArgs.Empty);
    }
    
    private void OnEnterPressed(object sender, EventArgs e) {
        // Bubble the enter event up to any listeners
        EnterPressed?.Invoke(this, EventArgs.Empty);
    }

    // --

    private bool disposedValue;

    protected virtual void Dispose(bool disposing) {
        if (!disposedValue) {
            if (disposing) {
                // Dispose managed state (managed objects)
                // e.g., Unsubscribe from events, dispose child controls if they implement IDisposable
                
                // Unsubscribe from key events
                if (_hoster?.CommandFilter != null) {
                    _hoster.CommandFilter.EscapePressed -= OnEscapePressed;
                    _hoster.CommandFilter.EnterPressed -= OnEnterPressed;
                }
                
                _hoster?.Dispose();
            }

            // Free unmanaged resources (unmanaged objects) and override finalizer
            // e.g., Close file handles, release COM objects
            disposedValue = true;
        }
    }

    public void Dispose() {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

}

