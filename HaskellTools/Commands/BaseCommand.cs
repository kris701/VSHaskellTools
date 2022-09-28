using EnvDTE;
using EnvDTE80;
using HaskellTools.Helpers;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaskellTools.Commands
{
    internal abstract class BaseCommand
    {
        public abstract int CommandId { get; }
        public static readonly Guid CommandSet = new Guid("c8d29eda-f85f-4c3f-8620-6b8c0c6ebd51");
        internal readonly AsyncPackage package;

        public bool CanBeDisabled { get; } = true;

        public BaseCommand(AsyncPackage package, OleMenuCommandService commandService, bool canBeDisabled = true)
        {
            CanBeDisabled = canBeDisabled;
            this.package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandID = new CommandID(CommandSet, CommandId);
            var menuItem = new OleMenuCommand(this.Execute, menuCommandID);
            if (CanBeDisabled)
                menuItem.BeforeQueryStatus += MyQueryStatus;
            commandService.AddCommand(menuItem);
        }

        public static async Task<OleMenuCommandService> InitializeCommandServiceAsync(AsyncPackage package)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);
            var newService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            return newService;
        }

        public abstract void Execute(object sender, EventArgs e);

        private void MyQueryStatus(object sender, EventArgs e)
        {
            var button = (MenuCommand)sender;
            button.Enabled = DTE2Helper.IsValidFileOpen();
        }
    }
}
