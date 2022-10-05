using HaskellTools.Helpers;
using HaskellTools.Options;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Threading;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management.Instrumentation;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Threading;

namespace HaskellTools.ErrorList
{
    internal class GHCiErrorManager
    {
        public static GHCiErrorManager Instance;
        public string FileName { get; set; }
        public string SourcePath { get; set; }

        private HaskellToolsPackage _package;
        private ErrorListProvider _errorProvider;
        private List<TaskListItem> _currentErrors;
        private Process _process;
        private bool _isReading = false;
        private int _readCounter = 0;

        private string _buffer;
        private bool _foundAny = false;

        List<object> events = new List<object>();

        public GHCiErrorManager(HaskellToolsPackage package)
        {
            Instance = this;
            _package = package;
            _errorProvider = new ErrorListProvider(package);
            _currentErrors = new List<TaskListItem>();

            var dte2 = DTE2Helper.GetDTE2();
            var docEvent = dte2.Events.DocumentEvents;
            events.Add(docEvent);
            docEvent.DocumentSaved += CheckGHCi;
        }

        private void SetupProcess()
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = @"powershell.exe";
            startInfo.RedirectStandardError = true;
            startInfo.RedirectStandardInput = true;
            startInfo.UseShellExecute = false;
            startInfo.CreateNoWindow = true;
            _process = new Process();
            _process.StartInfo = startInfo;
            _process.ErrorDataReceived += RecieveErrorData;
        }

        private async Task RunSetupCommandsAsync(string ghcPath, string readFile)
        {
            await _process.StandardInput.WriteLineAsync($"cd '{SourcePath}'");
            if (ghcPath == "")
                await _process.StandardInput.WriteLineAsync($"& ghci");
            else
                await _process.StandardInput.WriteLineAsync($"& '{DirHelper.CombinePathAndFile(ghcPath, "bin/ghci.exe")}'");
            await Task.Delay(1000);
            _isReading = true;
            _foundAny = false;
            _buffer = "";
            await _process.StandardInput.WriteLineAsync($":load \"{readFile}\"");
            while (_isReading)
            {
                await Task.Delay(200);
                _readCounter++;
                if (_readCounter > 4)
                    break;
            }
            if (_foundAny)
                AddErrorFromBuffer();
            await _process.StandardInput.WriteLineAsync($":quit");
            await _process.StandardInput.WriteLineAsync($"exit");
        }

        private void RecieveErrorData(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null && e.Data != "")
            {
                _foundAny = true;
                if (e.Data.StartsWith(FileName))
                {
                    if (_buffer != "")
                        AddErrorFromBuffer();
                }
                else
                    _buffer += e.Data + Environment.NewLine;
                _readCounter = 0;
            }
        }

        private void AddErrorFromBuffer()
        {
            var errorLines = _buffer.Split('|');

            TaskListItem newError = new TaskListItem();
            newError.Category = TaskCategory.BuildCompile;
            newError.Text = errorLines[0];
            newError.Line = Convert.ToInt32(errorLines[1]) - 1;
            _buffer = "";
            _currentErrors.Add(newError);
        }

        private async void CheckGHCi(EnvDTE.Document document)
        {
            if (DTE2Helper.IsValidFileOpen() && _process == null)
            {
                FileName = DTE2Helper.GetSourceFileName();
                SourcePath = DTE2Helper.GetSourcePath();
                _currentErrors.Clear();
                SetupProcess();
                _process.Start();
                _process.BeginErrorReadLine();
                await RunSetupCommandsAsync(OptionsAccessor.GHCUPPath, FileName);
                await _process.WaitForExitAsync();
                _process = null;
                _errorProvider.Tasks.Clear();
                foreach (var error in _currentErrors)
                    _errorProvider.Tasks.Add(error);
                if (_currentErrors.Count > 0)
                    _errorProvider.Show();
            }
        }
    }
}
