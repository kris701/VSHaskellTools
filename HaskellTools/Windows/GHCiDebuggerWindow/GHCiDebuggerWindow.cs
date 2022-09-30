using HaskellTools.Helpers;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Runtime.InteropServices;

namespace HaskellTools
{
    [Guid("39870e3c-4df7-484e-a548-274d6d694954")]
    public class GHCiDebuggerWindow : ToolWindowPane, IVsWindowFrameNotify3
    {
        public GHCiDebuggerWindow() : base(null)
        {
            this.Caption = "GHCi Debugger";
            this.Content = new GHCiDebuggerWindowControl();
        }

        public int OnClose(ref uint pgrfSaveOptions)
        {
            (this.Content as GHCiDebuggerWindowControl).Unload();
            return Microsoft.VisualStudio.VSConstants.S_OK;
        }

        public int OnDockableChange(int fDockable, int x, int y, int w, int h)
        {
            return Microsoft.VisualStudio.VSConstants.S_OK;
        }

        public int OnMove(int x, int y, int w, int h)
        {
            return Microsoft.VisualStudio.VSConstants.S_OK;
        }

        public int OnShow(int fShow)
        {
            (this.Content as GHCiDebuggerWindowControl).Load();
            return Microsoft.VisualStudio.VSConstants.S_OK;
        }

        public int OnSize(int x, int y, int w, int h)
        {
            return Microsoft.VisualStudio.VSConstants.S_OK;
        }
    }
}
