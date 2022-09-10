using HaskellTools.Helpers;
using Microsoft.VisualStudio.Shell;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace HaskellTools
{
    /// <summary>
    /// Interaction logic for HaskellInteractiveWindowControl.
    /// </summary>
    public partial class HaskellInteractiveWindowControl : UserControl
    {
        private Process process;
        private bool enableOutput = false;
        public string GHCiPath { get; set; } = "";
        /// <summary>
        /// Initializes a new instance of the <see cref="HaskellInteractiveWindowControl"/> class.
        /// </summary>
        public HaskellInteractiveWindowControl()
        {
            this.InitializeComponent();
        }

        private void InputTextbox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                process.StandardInput.WriteLine(InputTextbox.Text);
                OutputTextbox.AppendText($"> {InputTextbox.Text}{Environment.NewLine}", "#4e6fb5");
                InputTextbox.Text = "";
            }
        }

        private async void MyToolWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadSession();
        }

        private void OutputTextbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            (sender as RichTextBox).ScrollToEnd();
        }

        private async void ReloadButton_Click(object sender, RoutedEventArgs e)
        {
            await LoadSession();
        }

        private async Task LoadSession()
        {
            enableOutput = false;
            InputTextbox.IsEnabled = false;
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

            process.ErrorDataReceived += new DataReceivedEventHandler((sender1, e1) => {
                Application.Current.Dispatcher.Invoke(new Action(() => {
                    if (enableOutput && e1.Data != "")
                        OutputTextbox.AppendText($"{e1.Data}{Environment.NewLine}", "#ba4141");
                }));
            });
            process.OutputDataReceived += new DataReceivedEventHandler((sender2, e2) => {
                Application.Current.Dispatcher.Invoke(new Action(() => {
                    if (enableOutput && e2.Data != "")
                        OutputTextbox.AppendText($"{e2.Data}{Environment.NewLine}", "#ffffff");
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
            process.StandardInput.WriteLine($":load {FileHelper.GetSourceFileName()}");

            await Task.Delay(100);
            OutputTextbox.AppendText($"GHCI started and '{FileHelper.GetSourceFileName()}' loaded!{Environment.NewLine}", "#787878");

            InputTextbox.IsEnabled = true;
            enableOutput = true;
        }
    }
}