using HaskellTools.Windows.DebugData;
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

namespace HaskellTools.Windows.GHCiDebuggerWindow.UserControls
{
    /// <summary>
    /// Interaction logic for LocalsLine.xaml
    /// </summary>
    public partial class LocalsLine : UserControl
    {
        public DataItem SourceItem { get; set; }
        public LocalsLine(DataItem sourceItem)
        {
            SourceItem = sourceItem;
            InitializeComponent();
        }
    }
}
