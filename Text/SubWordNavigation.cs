using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace QuickJump2022.Text;

/// <summary>
/// Adapted from:
/// https://github.com/vasama/vs-subword-navigation/
/// </summary>

public enum SkipConnectedWhitespace { Never, BeforeAnything, AfterAnything }
public enum SkipConnectedUnderscores { Never, BeforeSubwords, AfterSubwords }
public enum SkipConnectedOperators { Never, BeforeWords, AfterWords }
public enum SkipConnectedBrackets { Never, BeforeWords, AfterWords }


class SubwordNavigationOptions {
    public bool UseWordNav { get; set; } = false;
    public bool RecognizePascal { get; set; } = true;
    public bool StopBetweenUpperAndPascal { get; set; } = true;
    public bool StopBetweenOperators { get; set; } = false;
    public bool StopBetweenBrackets { get; set; } = true;
    public bool StopBetweenOperatorsAndBrackets { get; set; } = true;
    public SkipConnectedWhitespace SkipConnectedWhitespace { get; set; } = SkipConnectedWhitespace.Never;
    public SkipConnectedUnderscores SkipConnectedUnderscores { get; set; } = SkipConnectedUnderscores.AfterSubwords;
    public SkipConnectedOperators SkipConnectedOperators { get; set; } = SkipConnectedOperators.Never;
    public SkipConnectedBrackets SkipConnectedBrackets { get; set; } = SkipConnectedBrackets.Never;
}

public enum SubwordNavigationAction {
    Move,
    Extend,
    Delete,
}

public enum SubwordNavigationDirection {
    Forward,
    Backward,
}

static class SubwordNavigation {
    
    public static void ExecuteWholeWords(
        SubwordNavigationAction action,
        SubwordNavigationDirection direction,
        TextBox textbox
    ) {
        var scanner = new Scanner();
        scanner.SetOptionsForWholeWord(); // TODO: make configurable (?)
        Execute(
            action,
            direction,
            textbox,
            scanner
        );
    }

    public static void ExecuteSubWords(
        SubwordNavigationAction action,
        SubwordNavigationDirection direction,
        TextBox textbox
    ) {
        var scanner = new Scanner();
        var options = new SubwordNavigationOptions();
        if (action == SubwordNavigationAction.Move) {
            options.SkipConnectedWhitespace = SkipConnectedWhitespace.Never;
        }
        scanner.SetOptionsForSubWord(options); // TODO: make configurable (?)
        Execute(
            action,
            direction,
            textbox,
            scanner
        );
    }

    private static void Execute(
        SubwordNavigationAction action,
        SubwordNavigationDirection direction,
        TextBox textbox,
        Scanner scanner
    ) {
        var textLine = textbox.Text;
        var selStart = textbox.SelectionStart;
        var selEnd = selStart + textbox.SelectionLength;
        var pos = textbox.CaretIndex;
        if (textbox.SelectionLength > 0) {
            pos = direction == SubwordNavigationDirection.Forward ? selEnd : selStart;
        }

        var newpos = direction == SubwordNavigationDirection.Forward
            ? scanner.GetNextBoundary(textLine, pos)
            : scanner.GetPrevBoundary(textLine, pos);

        switch (action) {
            case SubwordNavigationAction.Move:
                if (newpos != pos) {
                    textbox.CaretIndex = newpos;
                }
                break;

            case SubwordNavigationAction.Extend:
                if (newpos != pos) {
                    if (newpos < selStart) selStart = newpos;
                    else if (newpos > selEnd) selEnd = newpos;
                    textbox.Select(selStart, selEnd - selStart);
                }
                break;

            case SubwordNavigationAction.Delete: 
                // TODO: implement.
                break;
        }
        textbox.InvalidateVisual();
    }
}
