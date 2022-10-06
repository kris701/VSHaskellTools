using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Runtime.InteropServices;

namespace HaskellTools
{
    [Guid("4c47a506-6fa9-4b7f-90f9-b5d48e6728a1")]
    public class HaskellInteractiveWindow : ToolWindowPane, IVsWindowFrameNotify2
    {
        public HaskellInteractiveWindow() : base(null)
        {
            this.Caption = "Haskell Interactive Window";
            this.Content = new HaskellInteractiveWindowControl();
        }

        public int OnClose(ref uint pgrfSaveOptions)
        {
            (this.Content as HaskellInteractiveWindowControl).UnloadAsync();
            return Microsoft.VisualStudio.VSConstants.S_OK;
        }
    }
}
