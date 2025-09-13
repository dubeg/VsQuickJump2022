using Microsoft.VisualStudio.PlatformUI;
using QuickJump2022.TextEditor;
using System;
using System.Windows;

namespace QuickJump2022.Forms;

public partial class InputForm : DialogWindow {
    public string ResultText { get; private set; }
    
    public InputForm(string initialText = "") {
        InitializeComponent();
        
        // Set initial text if provided
        if (!string.IsNullOrEmpty(initialText)) {
            CodeView.Text = initialText;
        }
        
        // Subscribe to key events from the InputTextEditor
        CodeView.EscapePressed += OnEscapePressed;
        CodeView.EnterPressed += OnEnterPressed;
        
        // Focus the editor when loaded
        Loaded += (s, e) => CodeView.Focus();
    }
    
    private void OnEscapePressed(object sender, EventArgs e) {
        // Close the dialog when escape is pressed
        DialogResult = false;
        Close();
    }
    
    // Optional: Add a method to handle Enter key if needed
    private void OnEnterPressed(object sender, EventArgs e) {
        // Save the text and close on Enter
        ResultText = CodeView.Text;
        DialogResult = true;
        Close();
    }
    
    protected override void OnClosed(EventArgs e) {
        // Clean up event subscriptions
        if (CodeView != null) {
            CodeView.EscapePressed -= OnEscapePressed;
            CodeView.EnterPressed -= OnEnterPressed;
        }
        base.OnClosed(e);
    }
    
    // Static helper method to show the dialog
    public static string ShowModalEx(string initialText = "") {
        var dialog = new InputForm(initialText);
        dialog.ShowModal(); // ShowModal is an instance method from DialogWindow base class
        return dialog.ResultText;
    }
}