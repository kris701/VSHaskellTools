using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaskellTools.Options
{
    public class OptionPageGrid : DialogPage
    {
        private string runHaskellPath = "runhaskell";
        private string ghciPath = "ghci";
        private int haskellFileExecutionTimeout = 10;

        [Category("Haskell Tools")]
        [DisplayName("'runhaskell' path")]
        [Description("The path to the 'runhaskell.exe' file to compile and run haskell files")]
        public string RunHaskellPath
        {
            get { return runHaskellPath; }
            set { runHaskellPath = value; }
        }

        [Category("Haskell Tools")]
        [DisplayName("'GHCi' path")]
        [Description("The path to the 'GHCi.exe' file to interpret haskell files")]
        public string GHCIPath
        {
            get { return ghciPath; }
            set { ghciPath = value; }
        }

        [Category("Haskell Tools")]
        [DisplayName("Haskell File Execution Timeout")]
        [Description("How much time should pass before the GHCi instance is killed in seconds.")]
        public int HaskellFileExecutionTimeout
        {
            get { return haskellFileExecutionTimeout; }
            set { haskellFileExecutionTimeout = value; }
        }
    }
}
