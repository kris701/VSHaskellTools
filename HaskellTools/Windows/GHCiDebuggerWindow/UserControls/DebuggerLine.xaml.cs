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
using System.Windows.Resources;
using System.Windows.Shapes;

namespace HaskellTools.Windows.UserControls
{
    /// <summary>
    /// Interaction logic for DebuggerLine.xaml
    /// </summary>
    public partial class DebuggerLine : UserControl
    {
        public int LineNumber { get; set; }
        public bool DoBreak { get; set; } = false;

        public DebuggerLine(int lineNumber, string text)
        {
            LineNumber = lineNumber;
            InitializeComponent();
            LineNumberLabel.Content = LineNumber;
            LineLabel.Text = text;
        }

        private void ToggleBreakpoint_Click(object sender, RoutedEventArgs e)
        {
            SetBreakpoint(!DoBreak);
        }

        public void SetBreakpoint(bool toValue)
        {
            DoBreak = toValue;
            if (DoBreak)
                HoverCanvas.Visibility = Visibility.Visible;
            else
                HoverCanvas.Visibility = Visibility.Hidden;
        }

        private void BreakPointButton_MouseEnter(object sender, MouseEventArgs e)
        {
            //BreakPointButton.Visibility = Visibility.Visible;
            BreakPointButton.Opacity = 1;
        }

        private void BreakPointButton_MouseLeave(object sender, MouseEventArgs e)
        {
            //BreakPointButton.Visibility = Visibility.Hidden;
            BreakPointButton.Opacity = 0.01;
        }
    }
}
