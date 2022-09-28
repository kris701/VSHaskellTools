using HaskellTools.Helpers;
using Microsoft.VisualStudio.Language.StandardClassification;
using Microsoft.VisualStudio.Text.Adornments;
using Microsoft.VisualStudio.Threading;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaskellTools.HaskellInfo
{
    public class HaskellPreludeInitializer
    {
        private Process _process;
        private bool _isParsing = false;
        private string _currentKey = "";
        private ClassifiedTextElement _currentElements;
        private ClassifiedTextElement _currentCommentElements;

        public async Task InitializePreludeContentAsync(string ghciPath)
        {
            HaskellPreludeInfo.PreludeContent.Clear();

            SetupProcess();
            _process.Start();
            _process.BeginErrorReadLine();
            _process.BeginOutputReadLine();

            await RunSetupCommandsAsync(ghciPath);

            await _process.WaitForExitAsync();
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
            
        }

        private void RecieveOutputData(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null && _isParsing)
            {
                string line = $"{e.Data}";
                if (line.Contains("::") && !line.StartsWith(" "))
                {
                    if (_currentKey != "")
                    {
                        ContainerElement newElement = new ContainerElement(
                            ContainerElementStyle.Stacked,
                            _currentElements,
                            _currentCommentElements);
                        HaskellPreludeInfo.PreludeContent.Add(_currentKey, newElement);
                    }

                    if (line.StartsWith("type "))
                    {
                        _currentKey = line.Replace("::", ":").Split(':')[0].Replace("type ","").Trim();
                        string typeText = line.Replace("::", ":").Split(':')[1].Trim();

                        _currentElements = new ClassifiedTextElement(
                                new ClassifiedTextRun(PredefinedClassificationTypeNames.Type, $"{_currentKey} :: "),
                                new ClassifiedTextRun(PredefinedClassificationTypeNames.Type, typeText)
                            );
                        _currentCommentElements = new ClassifiedTextElement();
                    }
                    else
                    {
                        _currentKey = line.Replace("::", ":").Split(':')[0].Trim().Replace("(","").Replace(")","");
                        string typeText = line.Replace("::", ":").Split(':')[1].Trim();

                        _currentElements = new ClassifiedTextElement(
                                new ClassifiedTextRun(PredefinedClassificationTypeNames.Keyword, $"{_currentKey} :: "),
                                new ClassifiedTextRun(PredefinedClassificationTypeNames.Type, typeText)
                            );
                        _currentCommentElements = new ClassifiedTextElement();
                    }
                }
                else
                {
                    if (_currentCommentElements != null)
                        _currentCommentElements = new ClassifiedTextElement(_currentCommentElements.Runs.ToArray().Append(new ClassifiedTextRun(PredefinedClassificationTypeNames.Comment, $"{line}{Environment.NewLine}")));
                }
            }
        }

        private async Task RunSetupCommandsAsync(string ghciPath)
        {
            if (ghciPath == "")
                await _process.StandardInput.WriteLineAsync($"& ghci");
            else
                await _process.StandardInput.WriteLineAsync($"& '{DirHelper.CombinePathAndFile(ghciPath, "bin/ghci.exe")}'");
            await Task.Delay(1000);
            _isParsing = true;
            await _process.StandardInput.WriteLineAsync($":browse Prelude");
            await Task.Delay(100000);
            _isParsing = false;
            await _process.StandardInput.WriteLineAsync($":quit");
            await _process.StandardInput.WriteLineAsync($"exit");
        }
    }
}
