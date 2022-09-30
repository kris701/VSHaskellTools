using Microsoft.VisualStudio.Shell;
using System;
using System.Runtime.InteropServices;

namespace HaskellTools
{
    [Guid("8ada821f-5ccf-47ea-a372-a9434f9b8d60")]
    public class InstallGHCiWindow : ToolWindowPane
    {
        public InstallGHCiWindow() : base(null)
        {
            this.Caption = "Haskell Tools - GHC not found!";

            this.Content = new InstallGHCiWindowControl();
        }
    }
}
