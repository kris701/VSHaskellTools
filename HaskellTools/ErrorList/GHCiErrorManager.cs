using HaskellTools.EditorMargins;
using HaskellTools.Helpers;
using HaskellTools.Options;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Threading;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management.Instrumentation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Threading;

namespace HaskellTools.ErrorList
{
    internal class GHCiErrorManager
    {
        public static GHCiErrorManager Instance;
        public string FileName { get; set; }
        public bool IsStarted { get; set; }
        public ITextView TextField { get; set; }

        private string _previousDocument = "";

        private HaskellToolsPackage _package;
        private ErrorListProvider _errorProvider;
        private List<TaskListItem> _currentErrors;
        private PowershellProcess _process;
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
            _process = new PowershellProcess();

            ThreadHelper.ThrowIfNotOnUIThread();
            var dte2 = DTE2Helper.GetDTE2();
            var docEvent = dte2.Events.DocumentEvents;
            events.Add(docEvent);
            docEvent.DocumentSaved += CheckGHCi;
        }

        public void Initialize(ITextView textField)
        {
            if (_process.IsRunning)
                _process.StopProcess();
            TextField = textField;

            _process.StartProcess();
            _process.ErrorDataRecieved += RecieveErrorData;
            RunSetupCommands(OptionsAccessor.GHCUPPath);
            IsStarted = true;
            CheckGHCi(null);
        }

        public void Stop()
        {
            _process.StopProcess();
            foreach(var item in _currentErrors)
                _errorProvider.Tasks.Remove(item);
            _currentErrors.Clear();
            IsStarted = false;
        }

        private void RunSetupCommands(string ghcPath)
        {
            if (ghcPath == "")
                _process.WriteLine($"& ghci");
            else
                _process.WriteLine($"& '{DirHelper.CombinePathAndFile(ghcPath, "bin/ghci.exe")}'");
        }

        private void RecieveErrorData(object sender, DataReceivedEventArgs e)
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

        private void AddErrorFromBuffer()
        {
            if (_buffer.Contains("|"))
            {
                var errorLines = _buffer.Split('|');
                if (errorLines.Length > 2)
                {
                    ErrorTask newError = new ErrorTask();
                    newError.ErrorCategory = TaskErrorCategory.Error;
                    newError.Text = errorLines[0];
                    newError.Line = Convert.ToInt32(errorLines[1]) - 1;
                    newError.Navigate += JumpToError;
                    newError.Document = errorLines[2].Replace("\n", "").Replace("\r", "").Trim();
                    newError.Priority = TaskPriority.High;
                    _currentErrors.Add(newError);
                }
            }
            _buffer = "";
        }

        private async void JumpToError(object sender, EventArgs e)
        {
            if (sender is ErrorTask item) {
                foreach(var line in TextField.TextViewLines)
                {
                    var lineText = line.Extent.GetText().Trim();
                    if (lineText == item.Document)
                    {
                        var newSpan = new Microsoft.VisualStudio.Text.SnapshotSpan(line.Extent.Snapshot, line.Extent.Span);
                        TextField.Selection.Select(newSpan, false);
                        await DTE2Helper.FocusActiveDocumentAsync();
                        break;
                    }
                }
            }
        }

        private async void CheckGHCi(EnvDTE.Document document)
        {
            if (IsStarted)
            {
                if (await DTE2Helper.IsValidFileOpenAsync())
                {
                    FileName = await DTE2Helper.GetSourceFilePathAsync();
                    var newBuffer = File.ReadAllText(FileName);
                    if (newBuffer != _previousDocument)
                    {
                        _previousDocument = newBuffer;

                        Guid statusPanelGuid = HaskellEditorMarginFactory.SubscribePanel();
                        HaskellEditorMarginFactory.UpdatePanel(statusPanelGuid, $"Checking file...", StatusColors.StatusItemNormalBackground(), true);

                        _currentErrors.Clear();
                        _foundAny = false;
                        _buffer = "";
                        _readCounter = 0;

                        _process.WriteLine($":load \"{FileName.Replace("\\", "/")}\"");
                        while (_readCounter < 5)
                        {
                            await Task.Delay(100);
                            _readCounter++;
                        }
                        if (_foundAny)
                            AddErrorFromBuffer();
                        _errorProvider.Tasks.Clear();
                        foreach (var error in _currentErrors)
                            _errorProvider.Tasks.Add(error);
                        if (_currentErrors.Count > 0)
                        {
                            _errorProvider.Show();
                            HaskellEditorMarginFactory.UpdatePanel(statusPanelGuid, $"Compile Errors: {_currentErrors.Count}", StatusColors.StatusItemBadBackground(), false);
                        }
                        else
                        {
                            HaskellEditorMarginFactory.UpdatePanel(statusPanelGuid, $"No compile errors found!", StatusColors.StatusItemGoodBackground(), false);
                        }
                    }
                }
            }
        }
    }
}
