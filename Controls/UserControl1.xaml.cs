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
using QuickJump2022.TextEditor;
using QuickJump2022.Tools;
using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace QuickJump2022.Controls;
/// <summary>
/// Interaction logic for UserControl1.xaml
/// </summary>
public partial class UserControl1 : UserControl {
    public UserControl1() {
        InitializeComponent();
        InitializeEditor();
    }

    public static readonly DependencyProperty TextProperty =
    DependencyProperty.Register(nameof(Text), typeof(string), typeof(UserControl1),
new PropertyMetadata(string.Empty, OnTextChanged));

    //public static readonly DependencyProperty ContentTypeProperty =
    //    DependencyProperty.Register(nameof(ContentType), typeof(string), typeof(UserControl1),
    //new PropertyMetadata(string.Empty, OnContentTypeChanged));

    public static readonly DependencyProperty TextBufferProperty =
        DependencyProperty.Register(nameof(TextBuffer), typeof(ITextBuffer), typeof(UserControl1),
            new PropertyMetadata(null));

    public string Text {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    //public string ContentType {
    //    get => (string)GetValue(ContentTypeProperty);
    //    set => SetValue(ContentTypeProperty, value);
    //}

    public ITextBuffer TextBuffer {
        get => (ITextBuffer)GetValue(TextBufferProperty);
        set => SetValue(TextBufferProperty, value);
    }

    [Import]
    internal ITextEditorFactoryService TextEditorFactoryService { get; set; }

    [Import]
    internal ITextBufferFactoryService TextBufferFactoryService { get; set; }

    [Import]
    internal IClassificationTypeRegistryService ClassificationTypeRegistryService { get; set; }

    [Import]
    internal IClassificationFormatMapService ClassificationFormatMapService { get; set; }

    [Import]
    internal IEditorFormatMapService EditorFormatMapService { get; set; }

    [Import]
    internal IClassifierAggregatorService ClassifierAggregatorService { get; set; }

    [Import]
    internal IContentTypeRegistryService ContentTypeRegistryService { get; set; }

    public IWpfTextViewHost TextViewHost { get; private set; }

    private void InitializeEditor() {
        var componentModel = (IComponentModel)Microsoft.VisualStudio.Shell.Package.GetGlobalService(typeof(SComponentModel));
        if (componentModel == null) {
            EditorHost.Content = "Design view is not supported because MEF services are not available in design mode.";
            return;
        }
        componentModel.DefaultCompositionService.SatisfyImportsOnce(this);
        // --
        // https://learn.microsoft.com/en-us/dotnet/api/microsoft.visualstudio.editor.fontsandcolorscategory.appearancecategory
        // Text Editor = "text"
        // Output Window = "output"
        // --
        //var appearanceCategory = QuickJumpClassifications.InputEditor;
        //var qjFormatMap = ClassificationFormatMapService.GetClassificationFormatMap(appearanceCategory);
        //if (qjFormatMap is not null) {
        //    var size = 16 * 96 / 72;
            
        //    var textFormatMap = ClassificationFormatMapService.GetClassificationFormatMap("text");
        //    qjFormatMap.DefaultTextProperties = textFormatMap.DefaultTextProperties.SetFontRenderingEmSize(
        //        textFormatMap.DefaultTextProperties.FontRenderingEmSize + 6
        //    );
        //    //qjFormatMap.SetExplicitTextProperties(ClassificationTypeRegistryService.GetClassificationType(ClassificationTypeNames.Text), textFormatMap.DefaultTextProperties);
        //}
        //else {
        //    appearanceCategory = "text";
        //}
        // --
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
                //view.Options.SetOptionValue(DefaultWpfViewOptions.AppearanceCategory, appearanceCategory); // Unused
            },
            false
        );
        var textViewHost = hoster.GetTextViewHost();
        SetStyles(textViewHost.TextView);
        TextViewHost = textViewHost;
        EditorHost.Content = TextViewHost.HostControl;
        // Content = textViewHost.HostControl;
        Focus();
    }

    private void SetStyles(IWpfTextView textView) {
        var fontSize = 24;
        var fontFamilyName = "Consolas";
        var fontFamily = new FontFamily(fontFamilyName);
        // --
        //textView.VisualElement.Resources["CollapsedTextForeground"] = new SolidColorBrush(Color.FromRgb(0xA5, 0xA5, 0xA5));
        //textView.VisualElement.Resources
        // --
        var formatMap = EditorFormatMapService.GetEditorFormatMap(textView);
        //var textProperties = formatMap.GetProperties("Text");
        var textProperties = formatMap.GetProperties("Plain Text");
        formatMap.BeginBatchUpdate();
        textProperties[ClassificationFormatDefinition.TypefaceId] = new Typeface("Arial");
        textProperties[ClassificationFormatDefinition.FontRenderingSizeId] = 20.0;
        formatMap.SetProperties("Plain Text", textProperties);
        formatMap.EndBatchUpdate();
        // --
        //var classFormatMap = ClassificationFormatMapService.GetClassificationFormatMap(textView);
        //var textFormattingRun = classFormatMap.DefaultTextProperties.SetFontRenderingEmSize(fontSize * 96 / 72);
        //var textFormattingRun = TextFormattingRunProperties.CreateTextFormattingRunProperties(
        //    new Typeface(
        //        classFormatMap.DefaultTextProperties.Typeface.FontFamily,
        //        classFormatMap.DefaultTextProperties.Typeface.Style,
        //        classFormatMap.DefaultTextProperties.Typeface.Weight,
        //        classFormatMap.DefaultTextProperties.Typeface.Stretch
        //    //FontStretches.Normal
        //    ),
        //    16,
        //    classFormatMap.DefaultTextProperties.ForegroundBrush.
        //);
        //classFormatMap.SetTextProperties(ClassificationTypeRegistryService.GetClassificationType(ClassificationTypeNames.Text), textFormattingRun);
        // --
        //ResourceDictionary regularCaretProperties = formatMap.GetProperties("Caret");
        //ResourceDictionary overwriteCaretProperties = formatMap.GetProperties("Overwrite Caret");
        //ResourceDictionary indicatorMargin = formatMap.GetProperties("Indicator Margin");
        //ResourceDictionary visibleWhitespace = formatMap.GetProperties("Visible Whitespace");
        //ResourceDictionary selectedText = formatMap.GetProperties("Selected Text");
        //ResourceDictionary inactiveSelectedText = formatMap.GetProperties("Inactive Selected Text");

        //formatMap.BeginBatchUpdate();

        //textProperties[ClassificationFormatDefinition.TypefaceId] = new Typeface(fontFamilyName);
        //textProperties[ClassificationFormatDefinition.FontRenderingSizeId] = fontSize * 96 / 72;
        //formatMap.SetProperties("Text", textProperties);

        //regularCaretProperties[EditorFormatDefinition.ForegroundBrushId] = Brushes.Magenta;
        //formatMap.SetProperties("Caret", regularCaretProperties);

        //overwriteCaretProperties[EditorFormatDefinition.ForegroundBrushId] = Brushes.Turquoise;
        //formatMap.SetProperties("Overwrite Caret", overwriteCaretProperties);

        //indicatorMargin[EditorFormatDefinition.BackgroundColorId] = Colors.LightGreen;
        //formatMap.SetProperties("Indicator Margin", indicatorMargin);

        //visibleWhitespace[EditorFormatDefinition.ForegroundColorId] = Colors.Red;
        //formatMap.SetProperties("Visible Whitespace", visibleWhitespace);

        //selectedText[EditorFormatDefinition.BackgroundBrushId] = Brushes.LightPink;
        //formatMap.SetProperties("Selected Text", selectedText);

        //inactiveSelectedText[EditorFormatDefinition.BackgroundBrushId] = Brushes.DeepPink;
        //formatMap.SetProperties("Inactive Selected Text", inactiveSelectedText);

        //formatMap.EndBatchUpdate();
    }

    private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
        if (d is UserControl1 control && e.NewValue is string newText) {
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

