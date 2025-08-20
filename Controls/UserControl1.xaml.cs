using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Formatting;
using Microsoft.VisualStudio.Utilities;
using QuickJump2022.TextEditor;
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
    internal IClassificationFormatMapService ClassificationFormatMapService { get; set; }

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
        var appearanceCategory = QuickJumpClassifications.InputEditor;
        var qjFormatMap = ClassificationFormatMapService.GetClassificationFormatMap(appearanceCategory);
        if (qjFormatMap is not null) {
            var size = 16 * 96 / 72;
            var textFormatMap = ClassificationFormatMapService.GetClassificationFormatMap(appearanceCategory);
            qjFormatMap.DefaultTextProperties = textFormatMap.DefaultTextProperties.SetFontRenderingEmSize(
                textFormatMap.DefaultTextProperties.FontRenderingEmSize + 2
            );
        }
        else {
            appearanceCategory = "text";
        }
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
                view.Options.SetOptionValue(DefaultWpfViewOptions.AppearanceCategory, appearanceCategory); // Unused
            },
            false
        );
        var textViewHost = hoster.GetTextViewHost();
        //var formatMap = ClassificationFormatMapService.GetClassificationFormatMap(textViewHost.TextView);
        //formatMap.DefaultTextProperties = TextFormattingRunProperties.CreateTextFormattingRunProperties(
        //    new Typeface(
        //        formatMap.DefaultTextProperties.Typeface.FontFamily,
        //        formatMap.DefaultTextProperties.Typeface.Style,
        //        formatMap.DefaultTextProperties.Typeface.Weight,
        //        formatMap.DefaultTextProperties.Typeface.Stretch
        //    //FontStretches.Normal
        //    ),
        //    16,
        //    formatMap.DefaultTextProperties.Foreground
        //);
        // TODO:
        // This doesn't really work well.
        // I think I might have to create a new classification style & use that to set the default size.
        //formatMap.DefaultTextProperties = formatMap.DefaultTextProperties.SetFontRenderingEmSize(16);
        // --
        TextViewHost = textViewHost;
        EditorHost.Content = TextViewHost.HostControl;
        // Content = textViewHost.HostControl;
        Focus();
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

