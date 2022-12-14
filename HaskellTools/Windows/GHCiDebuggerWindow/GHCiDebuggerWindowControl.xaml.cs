using HaskellTools.Helpers;
using HaskellTools.Options;
using HaskellTools.Windows.DebugData;
using HaskellTools.Windows.GHCiDebuggerWindow.UserControls;
using HaskellTools.Windows.UserControls;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Threading;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace HaskellTools
{
    public partial class GHCiDebuggerWindowControl : UserControl
    {
        private enum ReadState { None, Waiting, BackStepping, Tracing, DebugData, EvaluteDebugData }
        private ReadState _currentReadState = ReadState.Waiting;

        private DispatcherTimer _debugTimer = new DispatcherTimer();
        private DispatcherTimer _debugEvaluateTimer = new DispatcherTimer();

        private PowershellProcess _process;
        private string _sourcePath = "";
        private List<DataItem> _debugData = new List<DataItem>();

        public bool IsFileLoaded { get; internal set; } = false;
        public string FileLoaded { get; internal set; } = "None";

        public GHCiDebuggerWindowControl()
        {
            _process = new PowershellProcess();
            _process.ErrorDataRecieved += RecieveErrorData;
            _process.OutputDataRecieved += RecieveNormalData;
            this.InitializeComponent();
            _debugTimer.Interval = TimeSpan.FromMilliseconds(100);
            _debugTimer.Tick += DebugReadOver;
            _debugEvaluateTimer.Interval = TimeSpan.FromMilliseconds(100);
            _debugEvaluateTimer.Tick += EvaluateDebugReadOver;
        }

        private async void StartDebuggingButton_Click(object sender, RoutedEventArgs e)
        {
            await StartDebuggerAsync();
        }

        private async void StopDebuggingButton_Click(object sender, RoutedEventArgs e)
        {
            await StopDebuggerAsync();
        }

        private async void ContinueButton_Click(object sender, RoutedEventArgs e)
        {
            ContinueButton.IsEnabled = false;
            BackButton.IsEnabled = false;
            ResetBreakpointPanel();
            if (!_process.IsRunning)
                return;
            _currentReadState = ReadState.Waiting;
            await _process.WriteLineAsync($":continue");
        }

        private async void BackButton_Click(object sender, RoutedEventArgs e)
        {
            ContinueButton.IsEnabled = false;
            BackButton.IsEnabled = false;
            ResetBreakpointPanel();
            if (!_process.IsRunning)
                return;
            _currentReadState = ReadState.Waiting;
            await _process.WriteLineAsync($":back");
        }

        private async void ForceEvaluate_Click(object sender, RoutedEventArgs e)
        {
            await EvaluateDebugDataAsync();
        }

        private void StepButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_process.IsRunning)
                return;

            ResetBreakpointPanel();
            _currentReadState = ReadState.Waiting;
            _process.WriteLine($":step");
        }

        private void ResetBreakpoints_Click(object sender, RoutedEventArgs e)
        {
            if (_process.IsRunning)
                return;
            foreach (var item in BreakpointPanel.Children)
                if (item is DebuggerLine line)
                    line.SetBreakpoint(false);
        }

        private void OutputTextbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            (sender as RichTextBox).ScrollToEnd();
        }

        private void HistoryTraceBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            (sender as TextBox).ScrollToEnd();
        }

        private async void KillDebuggingButton_Click(object sender, RoutedEventArgs e)
        {
            await StopDebuggerAsync();
        }

        public async Task LoadAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            if (await DTE2Helper.IsValidFileOpenAsync()) {
                if (!_process.IsRunning)
                {
                    var newName = await DTE2Helper.GetSourceFileNameAsync();
                    if (_sourcePath == "")
                        _sourcePath = await DTE2Helper.GetSourcePathAsync();

                    FileLoaded = newName;
                    CurrentlyDebuggingLabel.Content = $"Loaded File: {FileLoaded}";
                    ErrorLabel.Visibility = Visibility.Hidden;
                    MainGrid.Visibility = Visibility.Visible;
                    await FillBreakPointLinesAsync();
                    IsFileLoaded = true;
                    MainGrid.IsEnabled = true;
                }
            }
            else
            {
                ErrorLabel.Visibility = Visibility.Visible;
                MainGrid.Visibility = Visibility.Hidden;
            }
        }

        public async Task UnloadAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            await StopDebuggerAsync();
            BreakpointPanel.Children.Clear();
            IsFileLoaded = false;
            FileLoaded = "None";
        }

        private async void RecieveErrorData(object sender, DataReceivedEventArgs e)
        {
            await OutputTextbox.AppendTextAsync($"{e.Data}{Environment.NewLine}", "#ba4141");
        }

        private async void RecieveNormalData(object sender, DataReceivedEventArgs e)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            EvaluateOutput(e.Data);
        }

        private void EvaluateOutput(string text)
        {
            if (text != null)
            {
                try
                {
                    switch (_currentReadState)
                    {
                        case ReadState.Waiting:
                            if (text.Contains("Stopped in <exception thrown>"))
                            {
                                BackButton.IsEnabled = true;

                                _debugData.Clear();
                                HistoryTraceBox.Text = "";

                                _currentReadState = ReadState.DebugData;
                            }
                            else
                            if (text.Contains("breakpoint at "))
                            {
                                string lineNumber = text.Substring(text.IndexOf("at"));
                                lineNumber = lineNumber.Substring(lineNumber.IndexOf(':') + 1);
                                lineNumber = lineNumber.Substring(0, lineNumber.IndexOf(':'));
                                int number = Int32.Parse(lineNumber);
                                MarkBreakpoint(number);

                                _debugData.Clear();
                                HistoryTraceBox.Text = "";

                                _currentReadState = ReadState.DebugData;
                            }
                            else
                            if (text.Contains("Stopped in "))
                            {
                                string lineNumber = text.Substring(text.IndexOf("in"));
                                lineNumber = lineNumber.Substring(lineNumber.IndexOf(':') + 1);
                                lineNumber = lineNumber.Substring(0, lineNumber.IndexOf(':'));
                                int number = Int32.Parse(lineNumber);
                                MarkBreakpoint(number);

                                _debugData.Clear();
                                HistoryTraceBox.Text = "";

                                _currentReadState = ReadState.DebugData;
                            }
                            break;
                        case ReadState.DebugData:
                            if (!text.Contains(">"))
                            {
                                string name = text.Substring(0, text.IndexOf("::"));
                                string type = text.Substring(text.IndexOf("::") + 2);
                                string value = "";
                                if (text.Contains("="))
                                {
                                    type = type.Substring(0, type.IndexOf("="));
                                    value = text.Substring(text.IndexOf("=") + 1);
                                }
                                _debugData.Add(new DataItem()
                                {
                                    VariableName = name,
                                    Type = type,
                                    EvaluatedValue = value
                                });
                                _debugTimer.Start();
                            }
                            break;
                        case ReadState.EvaluteDebugData:
                            if (text.Contains("="))
                            {
                                string rname = text.Substring(text.IndexOf(">") + 1);
                                rname = rname.Substring(0, rname.IndexOf("="));
                                string rvalue = text.Substring(text.IndexOf("=") + 1);
                                foreach (var item in _debugData)
                                {
                                    if (item.VariableName.Trim() == rname.Trim())
                                    {
                                        item.EvaluatedValue = rvalue;
                                        break;
                                    }
                                }
                                _debugEvaluateTimer.Start();
                            }
                            break;
                        case ReadState.Tracing:
                            if (!text.Contains("<end of history>"))
                            {
                                if (text.Contains("*Main>"))
                                    text = text.Substring(text.IndexOf("*Main>") + 7);
                                HistoryTraceBox.Text += $"{text}{Environment.NewLine}";
                            }
                            else
                                _currentReadState = ReadState.Waiting;
                            break;
                    }
                    OutputTextbox.AppendText($"{text}{Environment.NewLine}", "#ffffff");
                }
                catch (Exception e)
                {
                    OutputTextbox.AppendText($"Error! Something went wrong while in output evaluation step {_currentReadState}!", "#c20e0e");
                    OutputTextbox.AppendText($"{e.Message}{Environment.NewLine}", "#c20e0e");
                }
            }
        }

        private void SetLocalsData()
        {
            DebugDataPanel.Children.Clear();
            foreach (var item in _debugData)
                DebugDataPanel.Children.Add(new LocalsLine(item));
        }

        private async void DebugReadOver(object sender, EventArgs e)
        {
            SetLocalsData();
            _currentReadState = ReadState.Waiting;
            _debugTimer.Stop();
            if ((bool)ForceValueChecks.IsChecked)
                await EvaluateDebugDataAsync();
            else
            {
                _currentReadState = ReadState.Tracing;
                await _process.WriteLineAsync($":hist");
            }
        }

        private async void EvaluateDebugReadOver(object sender, EventArgs e)
        {
            SetLocalsData();
            _debugEvaluateTimer.Stop();
            if ((bool)ForceValueChecks.IsChecked)
            {
                _currentReadState = ReadState.Tracing;
                await _process.WriteLineAsync($":hist");
            }
        }

        private async Task EvaluateDebugDataAsync()
        {
            if (!_process.IsRunning)
                return;

            _currentReadState = ReadState.EvaluteDebugData;
            foreach (var item in _debugData)
            {
                await _process.WriteLineAsync($":force {item.VariableName}");
            }
        }

        public async Task FillBreakPointLinesAsync()
        {
            BreakpointPanel.Children.Clear();
            string fileName = await DTE2Helper.GetSourceFilePathAsync();
            string[] text = File.ReadAllLines(fileName);
            int index = 1;
            foreach (string line in text)
            {
                BreakpointPanel.Children.Add(new DebuggerLine(index, line));
                index++;
            }
        }

        private async Task InsertBreakPointsAsync()
        {
            if (!_process.IsRunning)
                return;

            foreach (var control in BreakpointPanel.Children)
                if (control is DebuggerLine line)
                    if (line.DoBreak)
                        await _process.WriteLineAsync($":break {line.LineNumber}");
        }

        private void ResetBreakpointPanel()
        {
            foreach (var control in BreakpointPanel.Children)
                if (control is DebuggerLine line)
                    line.Background = Brushes.Transparent;
        }
        private void MarkBreakpoint(int lineNumber)
        {
            foreach (var control in BreakpointPanel.Children)
            {
                if (control is DebuggerLine line)
                {
                    if (line.LineNumber == lineNumber)
                    {
                        if (line.DoBreak)
                            line.Background = Brushes.Red;
                        else
                            line.Background = Brushes.Orange;
                        ContinueButton.IsEnabled = true;
                        BackButton.IsEnabled = true;
                        StepButton.IsEnabled = true;
                        break;
                    }
                }
            }
        }

        private async Task StartDebuggerAsync()
        {
            MainGrid.IsEnabled = false;
            StartDebuggingButton.IsEnabled = false;
            StopDebuggingButton.IsEnabled = true;
            BreakpointPanel.IsEnabled = false;
            ResetBreakpoints.IsEnabled = false;
            await LoadSessionAsync();
            if (_process.IsRunning)
            {
                _currentReadState = ReadState.Waiting;
                IsDebuggerOnBorder.BorderBrush = Brushes.Red;
                await InsertBreakPointsAsync();
                await _process.WriteLineAsync($":trace {OptionsAccessor.DebuggerEntryFunctionName}");
            }
            MainGrid.IsEnabled = true;
        }

        private async Task LoadSessionAsync()
        {
            if (_process.IsRunning)
                await StopDebuggerAsync();

            await _process.StartProcessAsync();

            OutputTextbox.AppendText($"Starting GHCi...{Environment.NewLine}", "#787878");
            await RunStartingCommandsAsync();
            OutputTextbox.AppendText($"GHCI started and '{FileLoaded}' loaded!{Environment.NewLine}", "#787878");
        }

        private async Task RunStartingCommandsAsync()
        {
            await _process.WriteLineAsync($"cd '{_sourcePath}'");
            if (OptionsAccessor.GHCUPPath == "")
                await _process.WriteLineAsync($"& ghci");
            else
                await _process.WriteLineAsync($"& '{DirHelper.CombinePathAndFile(OptionsAccessor.GHCUPPath, "bin\\ghci.exe")}'");
            await _process.WriteLineAsync($":load \"{FileLoaded}\"");
            await _process.WriteLineAsync($":set -fbreak-on-exception");
        }

        private async Task StopDebuggerAsync()
        {
            MainGrid.IsEnabled = false;
            StartDebuggingButton.IsEnabled = true;
            StopDebuggingButton.IsEnabled = false;
            if (_process.IsRunning)
                await _process.StopProcessAsync();
            IsDebuggerOnBorder.BorderBrush = Brushes.Transparent;
            BreakpointPanel.IsEnabled = true;
            ResetBreakpoints.IsEnabled = true;
            ContinueButton.IsEnabled = false;
            BackButton.IsEnabled = false;
            StepButton.IsEnabled = false;
            _currentReadState = ReadState.Waiting;
            DebugDataPanel.Children.Clear();
            OutputTextbox.Document.Blocks.Clear();
            HistoryTraceBox.Text = "";
            ResetBreakpointPanel();
            MainGrid.IsEnabled = true;
        }
    }
}