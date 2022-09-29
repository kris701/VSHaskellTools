using HaskellTools.Commands;
using HaskellTools.Helpers;
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
        private DispatcherTimer _loopTimer = new DispatcherTimer();
        private Process _process;
        private HaskellToolsPackage _toolPackage;

        private string _sourceFilePath = "";
        private bool _enableReading = true;

        private RunHaskellFileCommand(AsyncPackage package, OleMenuCommandService commandService) : base(package, commandService)
        {
            _toolPackage = this.package as HaskellToolsPackage;
            _loopTimer.Tick += ForceKillProcess;
        }

        private void ForceKillProcess(object sender, EventArgs e)
        {
            _enableReading = false;
            _loopTimer.Stop();
            if (!_process.HasExited)
            {
                ProcessHelper.KillProcessAndChildrens(_process.Id);
                OutputPanel.WriteLine($"ERROR! Function ran for longer than {_loopTimer.Interval}! Killing process...");
            }
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
            _enableReading = true;

            this.package.JoinableTaskFactory.RunAsync(async delegate
            {
                OutputPanel.Initialize();
                OutputPanel.ClearOutput();
                OutputPanel.WriteLineInvoke("Executing Haskell File");
                _loopTimer.Interval = TimeSpan.FromSeconds(_toolPackage.HaskellFileExecutionTimeout);
                await RunAsync();
            });
        }

        private async Task RunAsync()
        {
            _loopTimer.Start();

            SetupProcess();
            _process.Start();
            _process.BeginErrorReadLine();
            _process.BeginOutputReadLine();

            await _process.WaitForExitAsync();

            OutputPanel.WriteLineInvoke("Function ran to completion!");

            _loopTimer.Stop();
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

        private void SetupProcess()
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = @"powershell.exe";
            if (_toolPackage.GHCUPPath == "")
                startInfo.Arguments = $"& 'runhaskell' '{_sourceFilePath}'";
            else
                startInfo.Arguments = $"& '{DirHelper.CombinePathAndFile(_toolPackage.GHCUPPath, "bin\\runhaskell.exe")}' '{_sourceFilePath}'";
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;
            startInfo.UseShellExecute = false;
            startInfo.CreateNoWindow = true;
            _process = new Process();
            _process.StartInfo = startInfo;
            _process.OutputDataReceived += RecieveOutputData;
            _process.ErrorDataReceived += RecieveErrorData;
        }
    }
}
