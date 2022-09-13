using HaskellTools.Commands;
using Microsoft.VisualStudio.Shell;
using System;
using System.Threading.Tasks;

namespace HaskellTools.Commands
{
    internal sealed class GitHub : BaseCommand
    {
        public override int CommandId { get; } = 258;
        public static GitHub Instance { get; internal set; }

        private GitHub(AsyncPackage package, OleMenuCommandService commandService) : base(package, commandService)
        {
        }

        public static async Task InitializeAsync(AsyncPackage package)
        {
            Instance = new GitHub(package, await InitializeCommandServiceAsync(package));
        }

        public override void Execute(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            System.Diagnostics.Process.Start("https://github.com/kris701/HaskellRunner");
        }
    }
}
