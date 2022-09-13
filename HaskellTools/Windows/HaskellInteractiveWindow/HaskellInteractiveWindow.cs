using Microsoft.VisualStudio.Shell;
using System;
using System.Runtime.InteropServices;

namespace HaskellTools
{
    [Guid("4c47a506-6fa9-4b7f-90f9-b5d48e6728a1")]
    public class HaskellInteractiveWindow : ToolWindowPane
    {
        public HaskellInteractiveWindow() : base(null)
        {
            this.Caption = "Haskell Interactive Window";
            this.Content = new HaskellInteractiveWindowControl();
        }

        public void SetData(string path)
        {
            (this.Content as HaskellInteractiveWindowControl).GHCiPath = path;
        }
    }
}
