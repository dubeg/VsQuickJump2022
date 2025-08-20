using EnvDTE;
using Microsoft.CodeAnalysis.Classification;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Editor;
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

    public IWpfTextViewHost TextViewHost { get; private set; }

    private void InitializeEditor() {
        var componentModel = (IComponentModel)Microsoft.VisualStudio.Shell.Package.GetGlobalService(typeof(SComponentModel));
        if (componentModel == null) {
            EditorHost.Content = "Design view is not supported because MEF services are not available in design mode.";
            return;
        }
        componentModel.DefaultCompositionService.SatisfyImportsOnce(this);
        var hoster = TextEditor.HostEditorHelper.CreateHoster(
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
        var textViewHost = hoster.GetTextViewHost();
        // --
        var formatMap = EditorFormatMapService.GetEditorFormatMap(textViewHost.TextView);
        var textProperties = formatMap.GetProperties("Plain Text");
        formatMap.BeginBatchUpdate();
        // textProperties[ClassificationFormatDefinition.TypefaceId] = new Typeface("Cascadia Mono");
        textProperties[ClassificationFormatDefinition.FontRenderingSizeId] = 24.0; // MUST BE A DOUBLE.
        formatMap.SetProperties("Plain Text", textProperties);
        formatMap.EndBatchUpdate();
        // --
        TextViewHost = textViewHost;
        EditorHost.Content = TextViewHost.HostControl;
        // TODO: What's the difference?
        // Content = textViewHost.HostControl; 
        this.Height =
            textViewHost.TextView.LineHeight
            + BorderThickness.Top
            + BorderThickness.Bottom
            + EditorBorder.Padding.Top
            + EditorBorder.Padding.Bottom
            + EditorBorder.BorderThickness.Top
            + EditorBorder.BorderThickness.Bottom
            ;
        Focus();
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
}

