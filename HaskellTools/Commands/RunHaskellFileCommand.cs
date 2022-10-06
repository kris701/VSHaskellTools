using HaskellTools.Commands;
using HaskellTools.Editor;
using HaskellTools.Helpers;
using HaskellTools.Options;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Threading;
using Newtonsoft.Json.Linq;
using System;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Globalization;
using System.IO.Packaging;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Task = System.Threading.Tasks.Task;

namespace HaskellTools.Commands
{
    internal sealed class RunHaskellFileCommand : BaseCommand
    {
        private static OutputPanelController OutputPanel = new OutputPanelController("Haskell");
        public override int CommandId { get; } = 256;
        public static RunHaskellFileCommand Instance { get; internal set; }
        private PowershellProcess _process;

        private string _sourceFilePath = "";
        private string _sourceFileName = "";
        private bool _enableReading = true;

        private RunHaskellFileCommand(AsyncPackage package, OleMenuCommandService commandService) : base(package, commandService)
        {
        }

        public static async Task InitializeAsync(AsyncPackage package)
        {
            Instance = new RunHaskellFileCommand(package, await InitializeCommandServiceAsync(package));
        }

        public override void Execute(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (!DTE2Helper.IsValidFileOpen())
            {
                MessageBox.Show("File must be a '.hs' file!");
                return;
            }
            DTE2Helper.SaveActiveDocument();

            _sourceFilePath = DTE2Helper.GetSourceFilePath();
            _sourceFileName = DTE2Helper.GetSourceFileName();
            _enableReading = true;

            HaskellEditorMargin.ChangeRunningStatus(GHCiRunningState.Running, $"Executing '{_sourceFileName}'");

            this.package.JoinableTaskFactory.RunAsync(async delegate
            {
                OutputPanel.Initialize();
                OutputPanel.ClearOutput();
                OutputPanel.WriteLineInvoke("Executing Haskell File");
                await RunAsync();
            });
        }

        private async Task RunAsync()
        {
            _process = new PowershellProcess();
            _process.ErrorDataRecieved += RecieveErrorData;
            _process.OutputDataRecieved += RecieveOutputData;
            if (OptionsAccessor.GHCUPPath == "")
                await _process.StartProcessAsync($"& 'runhaskell' '{_sourceFilePath}'");
            else
                await _process.StartProcessAsync($"& '{DirHelper.CombinePathAndFile(OptionsAccessor.GHCUPPath, "bin\\runhaskell.exe")}' '{_sourceFilePath}'");

            var timeoutSpan = TimeSpan.FromSeconds(OptionsAccessor.HaskellFileExecutionTimeout);
            var res = await _process.WaitForExitAsync(timeoutSpan);
            if (res == ProcessCompleteReson.ForceKilled)
            {
                OutputPanel.WriteLine($"ERROR! Function ran for longer than {timeoutSpan}! Killing process...");
                HaskellEditorMargin.ChangeRunningStatus(GHCiRunningState.Failed, $"Execution of '{_sourceFileName}' failed!");
            }
            else
            {
                OutputPanel.WriteLineInvoke("Function ran to completion!");
                HaskellEditorMargin.ChangeRunningStatus(GHCiRunningState.Finished, $"Successfully ran the file '{_sourceFileName}'");
            }
        }

        private void RecieveErrorData(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null && _enableReading)
                OutputPanel.WriteLineInvoke($"ERROR! {e.Data}");
        }

        private void RecieveOutputData(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null && _enableReading)
                OutputPanel.WriteLineInvoke($"{e.Data}");
        }
    }
}
