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
        private static OutputPanelController OutputPanel = new OutputPanelController("Haskell GHCi");
        public override int CommandId { get; } = 257;
        public static RunSelectedFunctionCommand Instance { get; internal set; }
        private PowershellProcess _process;

        private string _sourcePath = "";
        private string _sourceFileName = "";
        private string _selectedText = "";
        private bool _isReading = false;
        private Guid _statusPanelGuid;

        private RunSelectedFunctionCommand(AsyncPackage package, OleMenuCommandService commandService) : base(package, commandService)
        {
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

            _statusPanelGuid = HaskellEditorMargin.SubscribePanel();
            HaskellEditorMargin.UpdatePanel(_statusPanelGuid, $"Executing '{_sourceFileName}' and function '{_selectedText}'", new SolidColorBrush(Colors.Gray), true);

            this.package.JoinableTaskFactory.RunAsync(async delegate
            {
                OutputPanel.Initialize();
                OutputPanel.ClearOutput();
                OutputPanel.WriteLineInvoke("Running Function with GHCi...");
                _isReading = false;
                await RunAsync();
            });
        }

        private async Task RunAsync()
        {
            var timeoutSpan = TimeSpan.FromSeconds(OptionsAccessor.HaskellFileExecutionTimeout);

            _process = new PowershellProcess();
            _process.ErrorDataRecieved += RecieveErrorData;
            _process.OutputDataRecieved += RecieveOutputData;
            _process.OutputTimeout = timeoutSpan;
            await _process.StartProcessAsync();

            await RunSetupCommandsAsync();

            var res = await _process.WaitForExitAsync(timeoutSpan);
            if (res == ProcessCompleteReson.ForceKilled)
            {
                OutputPanel.WriteLine($"ERROR! Function ran for longer than {timeoutSpan}! Killing process...");
                HaskellEditorMargin.UpdatePanel(_statusPanelGuid, $"Execution of '{_sourceFileName}' and function '{_selectedText}' failed!", new SolidColorBrush(Colors.LightPink), false);
            }
            else
            {
                OutputPanel.WriteLineInvoke("Function ran to completion!");
                OutputPanel.ActivateOutputWindow();
                HaskellEditorMargin.UpdatePanel(_statusPanelGuid, $"Successfully ran the file '{_sourceFileName}' and function '{_selectedText}'", new SolidColorBrush(Colors.LightGreen), false);
            }
        }


        private void RecieveErrorData(object sender, DataReceivedEventArgs e)
        {
            OutputPanel.WriteLineInvoke($"ERROR! {e.Data}");
        }

        private void RecieveOutputData(object sender, DataReceivedEventArgs e)
        {
            string line = $"{e.Data}";
            if (line.Contains("Leaving GHCi"))
                _isReading = false;
            if (_isReading)
                OutputPanel.WriteLineInvoke(line);
            if (line.Contains("module loaded"))
                _isReading = true;
        }

        private async Task RunSetupCommandsAsync()
        {
            await _process.WriteLineAsync($"cd '{_sourcePath}'");
            if (OptionsAccessor.GHCUPPath == "")
                await _process.WriteLineAsync($"& ghci");
            else
                await _process.WriteLineAsync($"& '{DirHelper.CombinePathAndFile(OptionsAccessor.GHCUPPath, "bin\\ghci.exe")}'");
            await _process.WriteLineAsync($":load \"{_sourceFileName}\"");
            await _process.WriteLineAsync($"{_selectedText}");
            await _process.WriteLineAsync($":quit");
            await _process.WriteLineAsync($"exit");
        }
    }
}
