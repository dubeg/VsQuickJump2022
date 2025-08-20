using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace QuickJump2022.TextEditor;

public static class QuickJumpClassifications {
    public const string InputEditor = "QuickJumpClassifications/InputEditor";

    [Export]
    [Name(InputEditor)]
    public static ClassificationTypeDefinition InputEditorType = null;

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = InputEditor)]
    [UserVisible(true)]  // Note: must be user-visible to be themed!
    [Name(InputEditor)]
    public sealed class InputEditorFormatDefinition : ClassificationFormatDefinition {
        public InputEditorFormatDefinition() {
            ForegroundColor = Color.FromRgb(0xFF, 0x22, 0x22);  // default colour in all themes
            DisplayName = "QuickJump Input Editor";  // appears in Fonts and Colors options
        }
    }
}
