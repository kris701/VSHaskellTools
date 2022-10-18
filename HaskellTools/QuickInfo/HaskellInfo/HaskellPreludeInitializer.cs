using HaskellTools.EditorMargins;
using HaskellTools.Helpers;
using HaskellTools.Options;
using Microsoft.VisualStudio.Language.StandardClassification;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text.Adornments;
using Microsoft.VisualStudio.Threading;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Threading;
using static System.Net.Mime.MediaTypeNames;

namespace HaskellTools.QuickInfo.HaskellInfo
{
    public class HaskellPreludeInitializer
    {
        private PowershellProcess _process;
        private bool _isParsing = false;
        private string _currentKey = "";
        private ClassifiedTextElement _currentElements;
        private ClassifiedTextElement _currentCommentElements;
        private int _parseCounter = 0;

        public async Task InitializePreludeContentAsync(string ghciPath)
        {
            // Little delay to let VS start fully
            await Task.Delay(5000);
            Guid panelID = Guid.Empty;
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            panelID = HaskellEditorMarginFactory.SubscribePanel();
            HaskellEditorMarginFactory.UpdatePanel(panelID, $"Loading QuickInfo from Prelude...", StatusColors.StatusItemNormalBackground(), true);

            _parseCounter = 0;
            HaskellPreludeInfo.PreludeContent.Clear();
            _process = new PowershellProcess();
            _process.OutputDataRecieved += RecieveOutputData;
            await _process.StartProcessAsync();

            await RunSetupCommandsAsync(ghciPath);

            await _process.WaitForExitAsync();

            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            HaskellEditorMarginFactory.UpdatePanel(panelID, $"QuickInfo from Prelude loaded!", StatusColors.StatusItemGoodBackground(), false);

            HaskellPreludeInfo.IsLoading = false;
        }

        private void RecieveOutputData(object sender, DataReceivedEventArgs e)
        {
            _parseCounter = 0;
            string line = $"{e.Data}";
            if (line.Contains("::") && !line.StartsWith(" "))
            {
                if (_currentKey != "")
                {
                    ContainerElement newElement = null;
                    if (_currentCommentElements != null)
                        newElement = new ContainerElement(
                            ContainerElementStyle.Stacked,
                            _currentElements,
                            _currentCommentElements);
                    else
                        newElement = new ContainerElement(
                            ContainerElementStyle.Stacked,
                            _currentElements);
                    HaskellPreludeInfo.PreludeContent.Add(_currentKey, newElement);
                }

                string leftSide = line.Replace("::", ":").Split(':')[0];
                string rightSide = line.Replace("::", ":").Split(':')[1];

                if (line.StartsWith("type "))
                {
                    _currentKey = leftSide.Replace("type ","").Trim();
                    string typeText = rightSide.Trim();

                    _currentElements = new ClassifiedTextElement(
                            new ClassifiedTextRun(PredefinedClassificationTypeNames.Type, $"{_currentKey} :: "),
                            new ClassifiedTextRun(PredefinedClassificationTypeNames.Type, typeText)
                        );
                    _currentCommentElements = null;
                }
                else
                {
                    _currentKey = leftSide.Trim().Replace("(","").Replace(")","");
                    string typeText = rightSide.Trim();

                    _currentElements = new ClassifiedTextElement(
                            new ClassifiedTextRun(PredefinedClassificationTypeNames.Keyword, $"{_currentKey} :: "),
                            new ClassifiedTextRun(PredefinedClassificationTypeNames.Type, typeText)
                        );
                    _currentCommentElements = null;
                }
            }
            else
            {
                if (_currentCommentElements == null)
                    _currentCommentElements = new ClassifiedTextElement();
                _currentCommentElements = new ClassifiedTextElement(_currentCommentElements.Runs.ToArray().Append(new ClassifiedTextRun(PredefinedClassificationTypeNames.Comment, $"{line}{Environment.NewLine}")));
            }
        }

        private async Task RunSetupCommandsAsync(string ghciPath)
        {
            if (ghciPath == "")
                await _process.WriteLineAsync($"& ghci");
            else
                await _process.WriteLineAsync($"& '{DirHelper.CombinePathAndFile(ghciPath, "bin/ghci.exe")}'");
            await Task.Delay(1000);
            _isParsing = true;
            await _process.WriteLineAsync($":browse Prelude");
            while (_isParsing)
            {
                await Task.Delay(500);
                _parseCounter++;
                if (_parseCounter > 4)
                    break;
            }
            await _process.WriteLineAsync($":quit");
            await _process.WriteLineAsync($"exit");
        }
    }
}
