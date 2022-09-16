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
        private string _runHaskellPath = "runhaskell";
        private string _ghciPath = "ghci";
        private int _haskellFileExecutionTimeout = 10;
        private string _debuggerEntryFunctionName = "main";

        [Category("Haskell Tools")]
        [DisplayName("'runhaskell' path")]
        [Description("The path to the 'runhaskell.exe' file to compile and run haskell files")]
        public string RunHaskellPath
        {
            get { return _runHaskellPath; }
            set { _runHaskellPath = value; }
        }

        [Category("Haskell Tools")]
        [DisplayName("'GHCi' path")]
        [Description("The path to the 'GHCi.exe' file to interpret haskell files")]
        public string GHCIPath
        {
            get { return _ghciPath; }
            set { _ghciPath = value; }
        }

        [Category("Haskell Tools")]
        [DisplayName("Haskell File Execution Timeout")]
        [Description("How much time should pass before the GHCi instance is killed in seconds.")]
        public int HaskellFileExecutionTimeout
        {
            get { return _haskellFileExecutionTimeout; }
            set { _haskellFileExecutionTimeout = value; }
        }

        [Category("Haskell Tools")]
        [DisplayName("Debugger Entry Function Name")]
        [Description("The name of the function that the debugger should enter on")]
        public string DebuggerEntryFunctionName
        {
            get { return _debuggerEntryFunctionName; }
            set { _debuggerEntryFunctionName = value; }
        }
    }
}
