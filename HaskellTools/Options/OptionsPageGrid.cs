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
        private string _ghcupPath = "";
        private int _haskellFileExecutionTimeout = 10;
        private string _debuggerEntryFunctionName = "main";

        [Category("Haskell Tools")]
        [DisplayName("Optional GHCUP Path")]
        [Description("Optional path to your GHCUP installation folder. (Leave empty if environment variables is set)")]
        public string GHCUPPath
        {
            get { return _ghcupPath; }
            set { _ghcupPath = value; }
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
