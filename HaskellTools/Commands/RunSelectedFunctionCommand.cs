using HaskellTools.Commands;
using HaskellTools.Helpers;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Threading;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using System.Xml;

namespace HaskellTools.Commands
{
    internal sealed class RunSelectedFunctionCommand : BaseCommand
    {
        private DispatcherTimer _loopTimer = new DispatcherTimer();
        private static OutputPanelController OutputPanel = new OutputPanelController("Haskell GHCi");
        public override int CommandId { get; } = 257;
        public static RunSelectedFunctionCommand Instance { get; internal set; }
        private Process _process;
        private HaskellToolsPackage _toolPackage;

        private string _sourcePath = "";
        private string _sourceFileName = "";
        private string _selectedText = "";
        private bool _isReading = false;
        private bool _enableReading = true;

        private RunSelectedFunctionCommand(AsyncPackage package, OleMenuCommandService commandService) : base(package, commandService)
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
            Instance = new RunSelectedFunctionCommand(package, await InitializeCommandServiceAsync(package));
        }

        public override void Execute(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (DTE2Helper.GetSourceFileExtension() != ".hs")
            {
                MessageBox.Show("File must be a '.hs' file!");
                return;
            }

            _sourcePath = DTE2Helper.GetSourcePath();
            _sourceFileName = DTE2Helper.GetSourceFileName();
            _selectedText = DTE2Helper.GetSelectedText();
            _enableReading = true;

            this.package.JoinableTaskFactory.RunAsync(async delegate
            {
                OutputPanel.Initialize();
                OutputPanel.ClearOutput();
                OutputPanel.WriteLineInvoke("Running Function with GHCi...");
                _loopTimer.Interval = TimeSpan.FromSeconds(_toolPackage.HaskellFileExecutionTimeout);
                _isReading = false;
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

            await RunSetupCommandsAsync();

            await _process.WaitForExitAsync();

            OutputPanel.WriteLineInvoke("Function ran to completion!");

            _loopTimer.Stop();
        }

        private void SetupProcess()
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = @"powershell.exe";
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;
            startInfo.RedirectStandardInput = true;
            startInfo.UseShellExecute = false;
            startInfo.CreateNoWindow = true;
            _process = new Process();
            _process.StartInfo = startInfo;
            _process.OutputDataReceived += RecieveOutputData;
            _process.ErrorDataReceived += RecieveErrorData;
        }

        private void RecieveErrorData(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null && _enableReading)
                OutputPanel.WriteLineInvoke($"ERROR! {e.Data}");
        }

        private void RecieveOutputData(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null && _enableReading)
            {
                string line = $"{e.Data}";
                if (line.Contains("Leaving GHCi"))
                    _isReading = false;
                if (_isReading)
                    OutputPanel.WriteLineInvoke(line);
                if (line.Contains("module loaded"))
                    _isReading = true;
            }
        }

        private async Task RunSetupCommandsAsync()
        {
            await _process.StandardInput.WriteLineAsync($"cd '{_sourcePath}'");
            await _process.StandardInput.WriteLineAsync($"& '{_toolPackage.GHCIPath}'");
            await _process.StandardInput.WriteLineAsync($":load \"{_sourceFileName}\"");
            await _process.StandardInput.WriteLineAsync($"{_selectedText}");
            await _process.StandardInput.WriteLineAsync($":quit");
            await _process.StandardInput.WriteLineAsync($"exit");
        }
    }
}
