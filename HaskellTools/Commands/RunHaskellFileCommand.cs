using HaskellTools.Commands;
using HaskellTools.EditorMargins;
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
using System.Windows.Media;
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
        private bool _isRunning = false;

        private string _sourceFilePath = "";
        private string _sourceFileName = "";
        private Guid _statusPanelGuid;

        private RunHaskellFileCommand(AsyncPackage package, OleMenuCommandService commandService) : base(package, commandService)
        {
        }

        public static async Task InitializeAsync(AsyncPackage package)
        {
            Instance = new RunHaskellFileCommand(package, await InitializeCommandServiceAsync(package));
        }

        public override async Task ExecuteAsync()
        {
            if (_isRunning)
            {
                HaskellEditorMarginFactory.UpdatePanel(HaskellEditorMarginFactory.SubscribePanel(), $"A Haskell file is already executing", StatusColors.StatusItemBadBackground(), false);
                return;
            }

            if (!await DTE2Helper.IsValidFileOpenAsync())
            {
                MessageBox.Show("File must be a '.hs' file!");
                return;
            }
            await DTE2Helper.SaveActiveDocumentAsync();

            _sourceFilePath = await DTE2Helper.GetSourceFilePathAsync();
            _sourceFileName = await DTE2Helper.GetSourceFileNameAsync();

            _statusPanelGuid = HaskellEditorMarginFactory.SubscribePanel();
            HaskellEditorMarginFactory.UpdatePanel(_statusPanelGuid, $"Executing '{_sourceFileName}'", StatusColors.StatusItemNormalBackground(), true);
            _isRunning = true;

            await OutputPanel.InitializeAsync();
            await OutputPanel.ClearOutputAsync();
            await OutputPanel.WriteLineAsync("Executing Haskell File");
            await RunAsync();
        }

        private async Task RunAsync()
        {
            _process = new PowershellProcess();
            _process.ErrorDataRecieved += RecieveErrorData;
            _process.OutputDataRecieved += RecieveOutputData;
            _process.StopOnError = true;
            if (OptionsAccessor.GHCUPPath == "")
                await _process.StartProcessAsync($"& 'runhaskell' '{_sourceFilePath}'");
            else
                await _process.StartProcessAsync($"& '{DirHelper.CombinePathAndFile(OptionsAccessor.GHCUPPath, "bin\\runhaskell.exe")}' '{_sourceFilePath}'");

            var timeoutSpan = TimeSpan.FromSeconds(OptionsAccessor.HaskellFileExecutionTimeout);
            var res = await _process.WaitForExitAsync(timeoutSpan);
            await OutputPanel.ActivateOutputWindowAsync();
            switch (res)
            {
                case ProcessCompleteReson.ForceKilled:
                    await OutputPanel.WriteLineAsync($"ERROR! Function ran for longer than {timeoutSpan}! Killing process...");
                    HaskellEditorMarginFactory.UpdatePanel(_statusPanelGuid, $"Execution of '{_sourceFileName}' failed!", StatusColors.StatusItemBadBackground(), false);
                    break;
                case ProcessCompleteReson.StoppedOnError:
                    await OutputPanel.WriteLineAsync($"Errors encountered!");
                    HaskellEditorMarginFactory.UpdatePanel(_statusPanelGuid, $"Execution of '{_sourceFileName}' failed!", StatusColors.StatusItemBadBackground(), false);
                    break;
                case ProcessCompleteReson.RanToCompletion:
                    await OutputPanel.WriteLineAsync("Function ran to completion!");
                    HaskellEditorMarginFactory.UpdatePanel(_statusPanelGuid, $"Successfully ran the file '{_sourceFileName}'", StatusColors.StatusItemGoodBackground(), false);
                    break;
                case ProcessCompleteReson.ProcessNotRunning:
                    await OutputPanel.WriteLineAsync("Process is not running!");
                    HaskellEditorMarginFactory.UpdatePanel(_statusPanelGuid, $"Process is not running", StatusColors.StatusItemBadBackground(), false);
                    break;
            }
            _isRunning = false;
        }

        private async void RecieveErrorData(object sender, DataReceivedEventArgs e)
        {
            await OutputPanel.WriteLineAsync($"ERROR! {e.Data}");
        }

        private async void RecieveOutputData(object sender, DataReceivedEventArgs e)
        {
            await OutputPanel.WriteLineAsync($"{e.Data}");
        }
    }
}
