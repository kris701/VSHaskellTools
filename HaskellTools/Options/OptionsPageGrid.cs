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
        private string _GHCUPPath = "";
        [Category("Haskell Tools")]
        [DisplayName("Optional GHCUP Path")]
        [Description("Optional path to your GHCUP installation folder. (Leave empty if environment variables is set). Note, restart of Visual Studio is required when changing this setting.")]
        [DefaultValue("")]
        public string GHCUPPath {
            get { return _GHCUPPath; }
            set {
                _GHCUPPath = value;
                GHCiFound = false;
                CheckForGHCiAtStartup = true;
            }
        }

        [Category("Haskell Tools")]
        [DisplayName("Haskell File Execution Timeout")]
        [Description("How much time should pass before the GHCi instance is killed in seconds.")]
        [DefaultValue(10)]
        public int HaskellFileExecutionTimeout { get; set; } = 10;

        [Category("Haskell Tools")]
        [DisplayName("Debugger Entry Function Name")]
        [Description("The name of the function that the debugger should enter on")]
        [DefaultValue("main")]
        public string DebuggerEntryFunctionName { get; set; } = "main";

        [Category("Haskell Tools")]
        [DisplayName("Check GHCi At Startup")]
        [Description("Checks if GHCi is registered in path, if not the user will be asked to provide the path to it instead.")]
        [DefaultValue(true)]
        public bool CheckForGHCiAtStartup { get; set; } = true;

        [Category("Haskell Tools")]
        [DisplayName("Is GHCi found?")]
        [Description("Indication that the extension have found GHC.")]
        [DefaultValue(false)]
#if DEBUG
        public bool GHCiFound { get; set; } = false;
#else
        public bool GHCiFound { get; internal set; } = false;
#endif

        [Category("Haskell Tools")]
        [DisplayName("Is this the first time the extension starts?")]
        [Description("Indication that the extention have been installed.")]
        [DefaultValue(true)]
#if DEBUG
        public bool IsFirstStart { get; set; } = true;
#else
        public bool IsFirstStart { get; internal set; } = true;
#endif
    }
}
