using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Controls;
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

    public static readonly DependencyProperty ContentTypeProperty =
        DependencyProperty.Register(nameof(ContentType), typeof(string), typeof(UserControl1),
    new PropertyMetadata(string.Empty, OnContentTypeChanged));

    public static readonly DependencyProperty TextBufferProperty =
        DependencyProperty.Register(nameof(TextBuffer), typeof(ITextBuffer), typeof(UserControl1),
            new PropertyMetadata(null));

    public string Text {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public string ContentType {
        get => (string)GetValue(ContentTypeProperty);
        set => SetValue(ContentTypeProperty, value);
    }

    /// <summary>
    /// TextBuffer property should be set in other to support syntax highlighting for languages like C#.
    /// </summary>
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
    }

    private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
        if (d is UserControl1 control && e.NewValue is string newText) {
            if (control.TextViewHost is null) 
                control.ContentType = "text";
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

    private static void OnContentTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
        if (d is UserControl1 control && e.NewValue is string newContentType) {
            if (control.TextBuffer == null) {
                var contentType = control.ContentTypeRegistryService.GetContentType(newContentType);
                control.TextBuffer = control.TextBufferFactoryService.CreateTextBuffer(contentType);
            }
            var textViewRoleSet = control.TextEditorFactoryService.CreateTextViewRoleSet(
                PredefinedTextViewRoles.Editable,
                PredefinedTextViewRoles.Interactive
            );
            var textView = control.TextEditorFactoryService.CreateTextView(control.TextBuffer, textViewRoleSet);
            textView.Options.SetOptionValue(DefaultTextViewHostOptions.EnableFileHealthIndicatorOptionId, false);
            textView.Options.SetOptionValue(DefaultTextViewHostOptions.EditingStateMarginOptionId, false);
            textView.Options.SetOptionValue(DefaultTextViewHostOptions.SourceImageMarginEnabledOptionId, false);
            textView.Options.SetOptionValue(DefaultTextViewHostOptions.ShowChangeTrackingMarginOptionId, false);
            textView.Options.SetOptionValue(DefaultTextViewHostOptions.ChangeTrackingId, false);
            textView.Options.SetOptionValue(DefaultTextViewHostOptions.VerticalScrollBarId, false);
            textView.Options.SetOptionValue(DefaultTextViewHostOptions.HorizontalScrollBarId, false);
            textView.Options.SetOptionValue(DefaultTextViewHostOptions.LineNumberMarginId, false);
            textView.Options.SetOptionValue(DefaultTextViewHostOptions.OutliningMarginId, false);
            textView.Options.SetOptionValue(DefaultTextViewHostOptions.GlyphMarginId, false);
            textView.Options.SetOptionValue(DefaultTextViewOptions.WordWrapStyleId, WordWrapStyles.WordWrap);
            control.TextViewHost = control.TextEditorFactoryService.CreateTextViewHost(textView, true);
            control.EditorHost.Content = control.TextViewHost.HostControl;
            control.UpdateText(control.Text);   
        }
    }
    
    public new void Focus() {
        var element = TextViewHost.TextView.VisualElement;
        element.Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(() => element.Focus()));
    }
}
