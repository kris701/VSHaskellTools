using HaskellTools.Commands;
using HaskellTools.Helpers;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.ComponentModel.Design;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;

namespace HaskellTools
{
    internal sealed class WelcomeWindowCommand : BaseCommand
    {
        public override int CommandId { get; } = 262;
        public static WelcomeWindowCommand Instance { get; internal set; }

        private WelcomeWindowCommand(AsyncPackage package, OleMenuCommandService commandService) : base(package, commandService, false)
        {
        }

        public static async Task InitializeAsync(AsyncPackage package)
        {
            Instance = new WelcomeWindowCommand(package, await InitializeCommandServiceAsync(package));
        }

        public override void Execute(object sender, EventArgs e)
        {
            this.package.JoinableTaskFactory.RunAsync(async delegate
            {
                while (!DTE2Helper.IsFullyVSOpen())
                    await Task.Delay(1000);
                ToolWindowPane window = await this.package.ShowToolWindowAsync(typeof(WelcomeWindow), 0, true, this.package.DisposalToken);
                if ((null == window) || (null == window.Frame))
                {
                    throw new NotSupportedException("Cannot create tool window");
                }
            });
        }
    }
}
