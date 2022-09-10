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

namespace HaskellTools
{
    /// <summary>
    /// Interaction logic for GHCiDebuggerWindowControl.
    /// </summary>
    public partial class GHCiDebuggerWindowControl : UserControl
    {
        private enum ReadState { None, Breakpoint, DebugData, EvaluteDebugData }
        private ReadState _currentReadState = ReadState.Breakpoint;

        private Process process;
        private string fileName = "";
        private List<DataItem> debugData = new List<DataItem>();
        private int haveUpdatedCount = 0;

        public string GHCiPath { get; set; } = "";

        /// <summary>
        /// Initializes a new instance of the <see cref="GHCiDebuggerWindowControl"/> class.
        /// </summary>
        public GHCiDebuggerWindowControl()
        {
            this.InitializeComponent();
        }

        private void MyToolWindow_Loaded(object sender, RoutedEventArgs e)
        {
            DebugDataPanel.ItemsSource = debugData;
            FillBreakPointLines();
        }

        private async void StartDebuggingButton_Click(object sender, RoutedEventArgs e)
        {
            await LoadSession();
            foreach(var control in BreakpointPanel.Children)
            {
                if (control is DebuggerLine line)
                {
                    if (line.DoBreak)
                    {
                        process.StandardInput.WriteLine($":break {line.LineNumber}");
                    }
                }
            }
            process.StandardInput.WriteLine($"main");
        }

        private void StopDebuggingButton_Click(object sender, RoutedEventArgs e)
        {
            if (process != null)
                process.Close();
            BreakpointPanel.IsEnabled = true;
            _currentReadState = ReadState.Breakpoint;
            DebugDataPanel.ItemsSource = null;
            OutputTextbox.Document.Blocks.Clear();
            ResetBreakpointPanel();
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
            if (process != null)
                process.Close();
            BreakpointPanel.IsEnabled = false;
            _currentReadState = ReadState.Breakpoint;
            DebugDataPanel.ItemsSource = null;
            OutputTextbox.Document.Blocks.Clear();

            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = @"powershell.exe";
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;
            startInfo.RedirectStandardInput = true;
            startInfo.UseShellExecute = false;
            startInfo.CreateNoWindow = true;
            process = new Process();
            process.StartInfo = startInfo;
            process.EnableRaisingEvents = true;
            process.Exited += new EventHandler((sender1, e1) => {
                BreakpointPanel.IsEnabled = true;
                ResetBreakpointPanel();
                });

            process.ErrorDataReceived += new DataReceivedEventHandler((sender1, e1) =>
            {
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    OutputTextbox.AppendText($"{e1.Data}{Environment.NewLine}", "#ba4141");
                }));
            });
            process.OutputDataReceived += new DataReceivedEventHandler((sender2, e2) =>
            {
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    EvaluateOutput(e2.Data);
                }));
            });

            process.Start();

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            OutputTextbox.AppendText($"Starting GHCi...{Environment.NewLine}", "#787878");

            while (GHCiPath == "")
                await Task.Delay(1000);

            await process.StandardInput.WriteLineAsync($"cd '{FileHelper.GetSourcePath()}'");
            await process.StandardInput.WriteLineAsync($"& '{GHCiPath}'");
            System.Threading.Thread.Sleep(1000);
            if (fileName == "")
                fileName = FileHelper.GetSourceFileName();
            await process.StandardInput.WriteLineAsync($":load {fileName}");

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
                            foreach (var control in BreakpointPanel.Children)
                            {
                                if (control is DebuggerLine line)
                                {
                                    if (line.LineNumber == number)
                                    {
                                        line.Background = Brushes.Red;
                                        break;
                                    }
                                }
                            }
                            _currentReadState = ReadState.DebugData;
                            process.StandardInput.WriteLine($"");
                        }
                        break;
                    case ReadState.DebugData:
                        if (!text.Contains(">"))
                        {
                            string name = text.Substring(0, text.IndexOf("::"));
                            string type = text.Substring(text.IndexOf("::") + 2);
                            type = type.Substring(0, type.IndexOf("="));
                            string value = text.Substring(text.IndexOf("=") + 1);
                            debugData.Add(new DataItem()
                            {
                                VariableName = name,
                                Type = type,
                                EvaluatedValue = value
                            });
                        }
                        else
                        {
                            DebugDataPanel.ItemsSource = null;
                            DebugDataPanel.ItemsSource = debugData;
                            _currentReadState = ReadState.Breakpoint;
                        }
                        break;
                    case ReadState.EvaluteDebugData:
                        if (text.Contains("="))
                        {
                            string rname = text.Substring(text.IndexOf(">") + 1);
                            rname = rname.Substring(0, rname.IndexOf("="));
                            string rvalue = text.Substring(text.IndexOf("=") + 1);
                            foreach (var item in debugData)
                            {
                                if (item.VariableName.Trim() == rname.Trim())
                                {
                                    item.EvaluatedValue = rvalue;
                                    haveUpdatedCount++;
                                    break;
                                }
                            }
                            if (haveUpdatedCount == debugData.Count)
                            {
                                haveUpdatedCount = 0;

                                DebugDataPanel.ItemsSource = null;
                                DebugDataPanel.ItemsSource = debugData;

                                _currentReadState = ReadState.Breakpoint;
                            }
                        }
                        break;
                }
                OutputTextbox.AppendText($"{text}{Environment.NewLine}", "#ffffff");
            }
        }

        private void ContinueButton_Click(object sender, RoutedEventArgs e)
        {
            ResetBreakpointPanel();
            process.StandardInput.WriteLine($":continue");
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

        private void ForceEvaluate_Click(object sender, RoutedEventArgs e)
        {
            _currentReadState = ReadState.EvaluteDebugData;
            foreach (var item in debugData)
            {
                process.StandardInput.WriteLine($":force {item.VariableName}");
            }
        }
    }
}