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
            LineLabel.Content = text;
        }

        private void BreakPoint_Click(object sender, RoutedEventArgs e)
        {
            DoBreak = (bool)(sender as CheckBox).IsChecked;
        }
    }
}
