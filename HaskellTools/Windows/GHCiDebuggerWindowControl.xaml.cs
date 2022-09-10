using HaskellTools.Helpers;
using HaskellTools.Windows.DebugData;
using HaskellTools.Windows.UserControls;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
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
        private Process process;
        private bool enableOutput = false;
        private string fileName = "";
        private bool isReadingDebugData = false;
        private List<DataItem> debugData = new List<DataItem>();
        private bool isForceReadingDebugData = false;
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
            enableOutput = false;
            if (process != null)
                process.Close();
            BreakpointPanel.IsEnabled = false;
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
            process.Exited += new EventHandler((sender1, e1) => {
                BreakpointPanel.IsEnabled = true;
                ResetBreakpointPanel();
                });

            process.ErrorDataReceived += new DataReceivedEventHandler((sender1, e1) => {
                Application.Current.Dispatcher.Invoke(new Action(() => {
                    if (enableOutput && e1.Data != "")
                        OutputTextbox.AppendText($"{e1.Data}{Environment.NewLine}", "#ba4141");
                }));
            });
            process.OutputDataReceived += new DataReceivedEventHandler((sender2, e2) => {
                Application.Current.Dispatcher.Invoke(new Action(() => {
                    if (enableOutput && e2.Data != "")
                        EvaluateOutput(e2.Data);
                }));
            });

            process.Start();

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            OutputTextbox.AppendText($"Starting GHCi...{Environment.NewLine}", "#787878");

            while (GHCiPath == "")
                await Task.Delay(1000);

            process.StandardInput.WriteLine($"cd '{FileHelper.GetSourcePath()}'");
            process.StandardInput.WriteLine($"& '{GHCiPath}'");
            System.Threading.Thread.Sleep(1000);
            if (fileName == "")
                fileName = FileHelper.GetSourceFileName();
            process.StandardInput.WriteLine($":load {fileName}");

            await Task.Delay(100);
            OutputTextbox.AppendText($"GHCI started and '{FileHelper.GetSourceFileName()}' loaded!{Environment.NewLine}", "#787878");

            enableOutput = true;
        }

        private void EvaluateOutput(string text)
        {
            if (text != null)
            {
                if (isForceReadingDebugData)
                {
                    if (text.Contains("="))
                    {
                        string name = text.Substring(text.IndexOf(">") + 1);
                        name = name.Substring(0, name.IndexOf("="));
                        string value = text.Substring(text.IndexOf("=") + 1);
                        foreach (var item in debugData)
                        {
                            if (item.VariableName.Trim() == name.Trim())
                            {
                                item.Value = value;
                                haveUpdatedCount++;
                                break;
                            }
                        }
                        if (haveUpdatedCount == debugData.Count)
                        {
                            haveUpdatedCount = 0;
                            isForceReadingDebugData = false;

                            DebugDataPanel.ItemsSource = null;
                            DebugDataPanel.ItemsSource = debugData;
                        }
                    }
                }
                else
                {
                    if (text.Contains("*Main"))
                    {
                        isReadingDebugData = false;
                        debugData.Clear();
                    }
                    if (text.Contains("Stopped "))
                    {
                        isReadingDebugData = true;
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
                    }
                    else if (isReadingDebugData)
                    {
                        string name = text.Substring(0, text.IndexOf("::"));
                        string value = text.Substring(text.IndexOf("::") + 2);
                        debugData.Add(new DataItem()
                        {
                            VariableName = name,
                            Value = value
                        });

                        DebugDataPanel.ItemsSource = null;
                        DebugDataPanel.ItemsSource = debugData;
                    }
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
            isForceReadingDebugData = true;
            foreach (var item in debugData)
            {
                process.StandardInput.WriteLine($":force {item.VariableName }");
            }
        }
    }
}