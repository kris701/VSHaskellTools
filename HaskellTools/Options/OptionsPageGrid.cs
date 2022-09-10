using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaskellRunner.Options
{
    public class OptionPageGrid : DialogPage
    {
        private string runHaskellPath = "runhaskell";
        private string ghciPath = "ghci";

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
    }
}
