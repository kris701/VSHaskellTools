using HaskellTools.Options;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Runtime.InteropServices;

namespace HaskellTools
{
    [Guid("ad431514-27ad-42ad-a5c8-e7187a8ddebb")]
    public class WelcomeWindow : ToolWindowPane, IVsWindowFrameNotify2
    {
        public WelcomeWindow() : base(null)
        {
            this.Caption = "Haskell Tools - Welcome";

            this.Content = new WelcomeWindowControl();
        }

        public int OnClose(ref uint pgrfSaveOptions)
        {
            OptionsAccessor.IsFirstStart = false;
            return Microsoft.VisualStudio.VSConstants.S_OK;
        }
    }
}
