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
        public static readonly Guid CommandSet = new Guid(Constants.CommandSetGuid);
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
                menuItem.BeforeQueryStatus += CheckQueryStatus;
            commandService.AddCommand(menuItem);
        }

        public static async Task<OleMenuCommandService> InitializeCommandServiceAsync(AsyncPackage package)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);
            var newService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            return newService;
        }

        public abstract void Execute(object sender, EventArgs e);

        private async void CheckQueryStatus(object sender, EventArgs e)
        {
            var button = (MenuCommand)sender;
            button.Enabled = await DTE2Helper.IsValidFileOpenAsync();
        }
    }
}
