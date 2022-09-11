using HaskellTools.Helpers;
using HaskellTools.Windows.DebugData;
using HaskellTools.Windows.UserControls;
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
    /// <summary>
    /// Interaction logic for GHCiDebuggerWindowControl.
    /// </summary>
    public partial class GHCiDebuggerWindowControl : UserControl
    {
        private enum ReadState { None, Breakpoint, Tracing, DebugData, EvaluteDebugData }
        private ReadState _currentReadState = ReadState.Breakpoint;

        private DispatcherTimer _debugTimer = new DispatcherTimer();
        private DispatcherTimer _debugEvaluateTimer = new DispatcherTimer();

        private Process _process;
        private string _sourcePath = "";
        private string _fileName = "";
        private List<DataItem> _debugData = new List<DataItem>();

        public bool IsDebuggerRunning => _process != null && !_process.HasExited;
        public string GHCiPath { get; set; } = "";

        /// <summary>
        /// Initializes a new instance of the <see cref="GHCiDebuggerWindowControl"/> class.
        /// </summary>
        public GHCiDebuggerWindowControl()
        {
            this.InitializeComponent();
            _debugTimer.Interval = TimeSpan.FromMilliseconds(100);
            _debugTimer.Tick += DebugReadOver;
            _debugEvaluateTimer.Interval = TimeSpan.FromMilliseconds(100);
            _debugEvaluateTimer.Tick += EvaluateDebugReadOver;
        }

        private void MyToolWindow_Loaded(object sender, RoutedEventArgs e)
        {
            FillBreakPointLines();
        }

        private async void StartDebuggingButton_Click(object sender, RoutedEventArgs e)
        {
            StartDebuggingButton.IsEnabled = false;
            StopDebuggingButton.IsEnabled = true;
            await StartDebugger();
        }

        private async void StopDebuggingButton_Click(object sender, RoutedEventArgs e)
        {
            StartDebuggingButton.IsEnabled = true;
            StopDebuggingButton.IsEnabled = false;
            await StopDebugger();
        }

        public void FillBreakPointLines()
        {
            BreakpointPanel.Children.Clear();
            string fileName = FileHelper.GetSourceFilePath();
            string[] text = File.ReadAllLines(fileName);
            int index = 1;
            foreach(string line in text)
            {
                BreakpointPanel.Children.Add(new DebuggerLine(index, line));
                index++;
            }
        }

        private async Task LoadSession()
        {
            if (IsDebuggerRunning)
                await StopDebugger();

            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = @"powershell.exe";
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;
            startInfo.RedirectStandardInput = true;
            startInfo.UseShellExecute = false;
            startInfo.CreateNoWindow = true;
            _process = new Process();
            _process.StartInfo = startInfo;
            _process.EnableRaisingEvents = true;
            _process.Exited += new EventHandler((sender1, e1) => {
                StopDebugger();
                });

            _process.ErrorDataReceived += new DataReceivedEventHandler((sender1, e1) =>
            {
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    OutputTextbox.AppendText($"{e1.Data}{Environment.NewLine}", "#ba4141");
                }));
            });
            _process.OutputDataReceived += new DataReceivedEventHandler((sender2, e2) =>
            {
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    EvaluateOutput(e2.Data);
                }));
            });

            _process.Start();

            _process.BeginOutputReadLine();
            _process.BeginErrorReadLine();

            OutputTextbox.AppendText($"Starting GHCi...{Environment.NewLine}", "#787878");

            if (_sourcePath == "")
                _sourcePath = FileHelper.GetSourcePath();
            await _process.StandardInput.WriteLineAsync($"cd '{_sourcePath}'");
            while (GHCiPath == "")
                await Task.Delay(1000);
            await _process.StandardInput.WriteLineAsync($"& '{GHCiPath}'");
            await Task.Delay(1000);
            if (_fileName == "")
                _fileName = FileHelper.GetSourceFileName();
            await _process.StandardInput.WriteLineAsync($":load {_fileName}");

            await Task.Delay(100);
            OutputTextbox.AppendText($"GHCI started and '{FileHelper.GetSourceFileName()}' loaded!{Environment.NewLine}", "#787878");
        }

        private void EvaluateOutput(string text)
        {
            if (text != null)
            {
                switch (_currentReadState)
                {
                    case ReadState.Breakpoint:
                        if (text.Contains("Stopped "))
                        {
                            string lineNumber = text.Substring(text.IndexOf("Stopped"));
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
                            type = type.Substring(0, type.IndexOf("="));
                            string value = text.Substring(text.IndexOf("=") + 1);
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
                            HistoryTraceBox.Text += $"{text}{Environment.NewLine}";
                        }
                        else
                            _currentReadState = ReadState.DebugData;
                        break;
                }
                OutputTextbox.AppendText($"{text}{Environment.NewLine}", "#ffffff");
            }
        }

        private void DebugReadOver(object sender, EventArgs e)
        {
            DebugDataPanel.ItemsSource = null;
            DebugDataPanel.ItemsSource = _debugData;
            _currentReadState = ReadState.Breakpoint;
            _debugTimer.Stop();
            if ((bool)ForceValueChecks.IsChecked)
                EvaluateDebugData();
            else
            {
                _currentReadState = ReadState.Tracing;
                _process.StandardInput.WriteLine($":hist");
            }
        }

        private void EvaluateDebugReadOver(object sender, EventArgs e)
        {
            DebugDataPanel.ItemsSource = null;
            DebugDataPanel.ItemsSource = _debugData;
            _debugEvaluateTimer.Stop();
            if ((bool)ForceValueChecks.IsChecked)
            {
                _currentReadState = ReadState.Tracing;
                _process.StandardInput.WriteLine($":hist");
            }
        }

        private void ContinueButton_Click(object sender, RoutedEventArgs e)
        {
            ContinueButton.IsEnabled = false;
            ResetBreakpointPanel();
            if (!IsDebuggerRunning)
                return;
            _currentReadState = ReadState.Breakpoint;
            _process.StandardInput.WriteLine($":continue");
        }

        private void ForceEvaluate_Click(object sender, RoutedEventArgs e)
        {
            EvaluateDebugData();
        }

        private void EvaluateDebugData()
        {
            if (!IsDebuggerRunning)
                return;

            _currentReadState = ReadState.EvaluteDebugData;
            foreach (var item in _debugData)
            {
                _process.StandardInput.WriteLine($":force {item.VariableName}");
            }
        }

        private void InsertBreakPoints()
        {
            if (!IsDebuggerRunning)
                return;

            foreach (var control in BreakpointPanel.Children)
            {
                if (control is DebuggerLine line)
                {
                    if (line.DoBreak)
                    {
                        _process.StandardInput.WriteLine($":break {line.LineNumber}");
                    }
                }
            }
        }

        private void ResetBreakpointPanel()
        {
            foreach (var control in BreakpointPanel.Children)
            {
                if (control is DebuggerLine line)
                {
                    line.Background = Brushes.Transparent;
                }
            }
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
                        StepButton.IsEnabled = true;
                        break;
                    }
                }
            }
        }

        private async Task StartDebugger()
        {
            BreakpointPanel.IsEnabled = false;
            ResetBreakpoints.IsEnabled = false;
            _currentReadState = ReadState.Breakpoint;
            await LoadSession();
            InsertBreakPoints();
            _process.StandardInput.WriteLine($":trace main");
        }

        private async Task StopDebugger()
        {
            if (IsDebuggerRunning)
                _process.Close();
            BreakpointPanel.IsEnabled = true;
            ResetBreakpoints.IsEnabled = true;
            ContinueButton.IsEnabled = false;
            StepButton.IsEnabled = false;
            _currentReadState = ReadState.Breakpoint;
            DebugDataPanel.ItemsSource = null;
            OutputTextbox.Document.Blocks.Clear();
            HistoryTraceBox.Text = "";
            ResetBreakpointPanel();
            _process = null;
        }

        private void StepButton_Click(object sender, RoutedEventArgs e)
        {
            if (!IsDebuggerRunning)
                return;

            ResetBreakpointPanel();
            _currentReadState = ReadState.Breakpoint;
            _process.StandardInput.WriteLine($":step");
        }

        private void ResetBreakpoints_Click(object sender, RoutedEventArgs e)
        {
            if (IsDebuggerRunning)
                return;
            foreach (var item in BreakpointPanel.Children)
                if (item is DebuggerLine line)
                    line.BreakPoint.IsChecked = false;
        }

        private void OutputTextbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            (sender as RichTextBox).ScrollToEnd();
        }

        private void HistoryTraceBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            (sender as TextBox).ScrollToEnd();
        }
    }
}