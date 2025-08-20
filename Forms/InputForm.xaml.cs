using Microsoft.VisualStudio.PlatformUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace QuickJump2022.Forms;
/// <summary>
/// Interaction logic for InputForm.xaml
/// </summary>
public partial class InputForm : DialogWindow {
    public InputForm() {
        InitializeComponent();
        Width = 300;
    }

    protected override void OnClosed(EventArgs e) {
        base.OnClosed(e);
        CodeView?.Dispose();
    }
}
