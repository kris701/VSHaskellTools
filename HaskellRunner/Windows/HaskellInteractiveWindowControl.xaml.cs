using HaskellRunner.Helpers;
using Microsoft.VisualStudio.Shell;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace HaskellRunner
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
                InputTextbox.Text = "";
            }
        }

        private async void MyToolWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadSession();
        }

        private void OutputTextbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            (sender as TextBox).ScrollToEnd();
        }

        private async void ReloadButton_Click(object sender, RoutedEventArgs e)
        {
            await LoadSession();
        }

        private async Task LoadSession()
        {
            enableOutput = false;
            OutputTextbox.Text = "";

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
                    if (enableOutput)
                        OutputTextbox.Text += $"{e1.Data}{Environment.NewLine}";
                    string read = e1.Data;
                }));
            });
            process.OutputDataReceived += new DataReceivedEventHandler((sender2, e2) => {
                Application.Current.Dispatcher.Invoke(new Action(() => {
                    if (enableOutput)
                        OutputTextbox.Text += $"{e2.Data}{Environment.NewLine}";
                    string read = e2.Data;
                }));
            });

            process.Start();

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            OutputTextbox.Text += $"Starting GHCi...{Environment.NewLine}";

            while (GHCiPath == "")
                await Task.Delay(1000);

            process.StandardInput.WriteLine($"cd '{FileHelper.GetSourcePath()}'");
            process.StandardInput.WriteLine($"& '{GHCiPath}'");
            System.Threading.Thread.Sleep(1000);
            process.StandardInput.WriteLine($":load {FileHelper.GetSourceFileName()}");

            await Task.Delay(100);
            OutputTextbox.Text += $"GHCI started and '{FileHelper.GetSourceFileName()}' loaded!{Environment.NewLine}";

            InputTextbox.IsEnabled = true;
            enableOutput = true;
        }
    }
}