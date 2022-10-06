using HaskellTools.Commands;
using HaskellTools.Editor;
using HaskellTools.Helpers;
using HaskellTools.Options;
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
using System.Windows.Media;
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

        private string _sourcePath = "";
        private string _sourceFileName = "";
        private string _selectedText = "";
        private bool _isReading = false;
        private bool _enableReading = true;
        private Guid _statusPanelGuid;

        private RunSelectedFunctionCommand(AsyncPackage package, OleMenuCommandService commandService) : base(package, commandService)
        {
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
                HaskellEditorMargin.UpdatePanel(_statusPanelGuid, $"Execution of '{_sourceFileName}' and function '{_selectedText}' failed!", new SolidColorBrush(Colors.LightPink), false);
            }
        }

        public static async Task InitializeAsync(AsyncPackage package)
        {
            Instance = new RunSelectedFunctionCommand(package, await InitializeCommandServiceAsync(package));
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

            _sourcePath = DTE2Helper.GetSourcePath();
            _sourceFileName = DTE2Helper.GetSourceFileName();
            _selectedText = DTE2Helper.GetSelectedText();
            _enableReading = true;

            _statusPanelGuid = HaskellEditorMargin.SubscribePanel();
            HaskellEditorMargin.UpdatePanel(_statusPanelGuid, $"Executing '{_sourceFileName}' and function '{_selectedText}'", new SolidColorBrush(Colors.LightGreen), true);

            this.package.JoinableTaskFactory.RunAsync(async delegate
            {
                OutputPanel.Initialize();
                OutputPanel.ClearOutput();
                OutputPanel.WriteLineInvoke("Running Function with GHCi...");
                _loopTimer.Interval = TimeSpan.FromSeconds(OptionsAccessor.HaskellFileExecutionTimeout);
                _isReading = false;
                await RunAsync();
                HaskellEditorMargin.UpdatePanel(_statusPanelGuid, $"Successfully ran the file '{_sourceFileName}' and function '{_selectedText}'", new SolidColorBrush(Colors.Gray), false);
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
            if (OptionsAccessor.GHCUPPath == "")
                await _process.StandardInput.WriteLineAsync($"& ghci");
            else
                await _process.StandardInput.WriteLineAsync($"& '{DirHelper.CombinePathAndFile(OptionsAccessor.GHCUPPath, "bin\\ghci.exe")}'");
            await _process.StandardInput.WriteLineAsync($":load \"{_sourceFileName}\"");
            await _process.StandardInput.WriteLineAsync($"{_selectedText}");
            await _process.StandardInput.WriteLineAsync($":quit");
            await _process.StandardInput.WriteLineAsync($"exit");
        }
    }
}
