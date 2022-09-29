using Microsoft.VisualStudio.Shell;
using System;
using System.Runtime.InteropServices;

namespace HaskellTools
{
    [Guid("ad431514-27ad-42ad-a5c8-e7187a8ddebb")]
    public class WelcomeWindow : ToolWindowPane
    {
        public WelcomeWindow() : base(null)
        {
            this.Caption = "Haskell Tools - Welcome";

            this.Content = new WelcomeWindowControl();
        }
    }
}
