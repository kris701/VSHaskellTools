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
        private bool _checkForGHCiAtStartup = true;
        private bool _GHCiFound = false;
        private bool _isFirstStart = true;

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

        [Category("Haskell Tools")]
        [DisplayName("Check GHCi At Startup")]
        [Description("Checks if GHCi is registered in path, if not the user will be asked to provide the path to it instead.")]
        public bool CheckForGHCiAtStartup
        {
            get { return _checkForGHCiAtStartup; }
            set { _checkForGHCiAtStartup = value; }
        }

        [Category("Haskell Tools")]
        [DisplayName("Is GHCi found?")]
        [Description("Indication that the extension have found GHC.")]
        internal bool GHCiFound
        {
            get { return _GHCiFound; }
            set { _GHCiFound = value; }
        }

        [Category("Haskell Tools")]
        [DisplayName("Is this the first time the extension starts?")]
        [Description("Indication that the extention have been installed.")]
        internal bool IsFirstStart
        {
            get { return _isFirstStart; }
            set { _isFirstStart = value; }
        }
    }
}
