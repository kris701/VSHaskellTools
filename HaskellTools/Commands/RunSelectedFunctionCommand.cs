using HaskellTools.Commands;
using HaskellTools.EditorMargins;
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
        private bool _isRunning = false;

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

            if (_isRunning)
            {
                HaskellEditorMarginFactory.UpdatePanel(HaskellEditorMarginFactory.SubscribePanel(), $"A Haskell file is already executing", StatusColors.StatusItemBadBackground(), false);
                return;
            }

            if (!DTE2Helper.IsValidFileOpen())
            {
                MessageBox.Show("File must be a '.hs' file!");
                return;
            }
            DTE2Helper.SaveActiveDocument();

            _sourcePath = DTE2Helper.GetSourcePath();
            _sourceFileName = DTE2Helper.GetSourceFileName();
            _selectedText = DTE2Helper.GetSelectedText();

            _statusPanelGuid = HaskellEditorMarginFactory.SubscribePanel();
            HaskellEditorMarginFactory.UpdatePanel(_statusPanelGuid, $"Executing '{_sourceFileName}' and function '{_selectedText}'", StatusColors.StatusItemNormalBackground(), true);
            _isRunning = true;

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
            _process.StopOnError = true;
            await _process.StartProcessAsync();

            await RunSetupCommandsAsync();

            var res = await _process.WaitForExitAsync(timeoutSpan);
            if (res == ProcessCompleteReson.ForceKilled)
            {
                OutputPanel.WriteLine($"ERROR! Function ran for longer than {timeoutSpan}! Killing process...");
                HaskellEditorMarginFactory.UpdatePanel(_statusPanelGuid, $"Execution of '{_sourceFileName}' and function '{_selectedText}' failed!", StatusColors.StatusItemBadBackground(), false);
            }
            else
            {
                OutputPanel.WriteLineInvoke("Function ran to completion!");
                OutputPanel.ActivateOutputWindow();
                HaskellEditorMarginFactory.UpdatePanel(_statusPanelGuid, $"Successfully ran the file '{_sourceFileName}' and function '{_selectedText}'", StatusColors.StatusItemGoodBackground(), false);
            }
            _isRunning = false;
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
