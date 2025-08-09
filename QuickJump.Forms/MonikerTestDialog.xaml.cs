using Microsoft.VisualStudio.PlatformUI;
using System.Windows;

namespace QuickJump2022.Forms;

public partial class MonikerTestDialog : DialogWindow {
    public MonikerTestDialog() {
        InitializeComponent();
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e) {
        Close();
    }
}
