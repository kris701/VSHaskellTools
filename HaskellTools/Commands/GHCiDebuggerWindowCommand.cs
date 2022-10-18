using HaskellTools.Helpers;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.ComponentModel.Design;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Task = System.Threading.Tasks.Task;

namespace HaskellTools.Commands
{
    internal sealed class GHCiDebuggerWindowCommand : BaseCommand
    {
        public override int CommandId { get; } = 260;
        public static GHCiDebuggerWindowCommand Instance { get; internal set; }

        private GHCiDebuggerWindowCommand(AsyncPackage package, OleMenuCommandService commandService) : base(package, commandService)
        {
        }

        public static async Task InitializeAsync(AsyncPackage package)
        {
            Instance = new GHCiDebuggerWindowCommand(package, await InitializeCommandServiceAsync(package));
        }

        public override async Task ExecuteAsync()
        {
            if (!await DTE2Helper.IsValidFileOpenAsync())
            {
                MessageBox.Show("File must be a '.hs' file!");
                return;
            }
            await DTE2Helper.SaveActiveDocumentAsync();

            ToolWindowPane window = await this.package.ShowToolWindowAsync(typeof(GHCiDebuggerWindow), 0, true, this.package.DisposalToken);
            if ((null == window) || (null == window.Frame))
            {
                throw new NotSupportedException("Cannot create tool window");
            }
        }
    }
}
