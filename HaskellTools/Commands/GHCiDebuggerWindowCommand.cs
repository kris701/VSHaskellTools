﻿using HaskellTools.Helpers;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.ComponentModel.Design;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
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

        public override void Execute(object sender, EventArgs e)
        {
            DTE2Helper.SaveActiveDocument();
            this.package.JoinableTaskFactory.RunAsync(async delegate
            {
                ToolWindowPane window = await this.package.ShowToolWindowAsync(typeof(GHCiDebuggerWindow), 0, true, this.package.DisposalToken);
                HaskellToolsPackage myToolsOptionsPackage = this.package as HaskellToolsPackage;
                (window as GHCiDebuggerWindow).RequestSettingsData += () => { return myToolsOptionsPackage; };
                if ((null == window) || (null == window.Frame))
                {
                    throw new NotSupportedException("Cannot create tool window");
                }
            });
        }
    }
}
