using Microsoft.VisualStudio.Shell;
using System;
using System.Runtime.InteropServices;

namespace HaskellTools
{
    /// <summary>
    /// This class implements the tool window exposed by this package and hosts a user control.
    /// </summary>
    /// <remarks>
    /// In Visual Studio tool windows are composed of a frame (implemented by the shell) and a pane,
    /// usually implemented by the package implementer.
    /// <para>
    /// This class derives from the ToolWindowPane class provided from the MPF in order to use its
    /// implementation of the IVsUIElementPane interface.
    /// </para>
    /// </remarks>
    [Guid("39870e3c-4df7-484e-a548-274d6d694954")]
    public class GHCiDebuggerWindow : ToolWindowPane
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GHCiDebuggerWindow"/> class.
        /// </summary>
        public GHCiDebuggerWindow() : base(null)
        {
            this.Caption = "GHCi Debugger";

            // This is the user control hosted by the tool window; Note that, even if this class implements IDisposable,
            // we are not calling Dispose on this object. This is because ToolWindowPane calls Dispose on
            // the object returned by the Content property.
            this.Content = new GHCiDebuggerWindowControl();
        }

        public void SetData(string path)
        {
            (this.Content as GHCiDebuggerWindowControl).GHCiPath = path;
        }
    }
}
