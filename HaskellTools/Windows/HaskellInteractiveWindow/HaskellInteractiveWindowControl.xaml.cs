using HaskellTools.Events;
using HaskellTools.Helpers;
using Microsoft.VisualStudio.Shell;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO.Packaging;
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
        public event RequestSettingsDataHandler RequestSettingsData;

        public string GHCiPath { get; set; } = "";

        private Process _process;
        private bool _isLoaded = false;
        private HaskellToolsPackage _package;

        public HaskellInteractiveWindowControl()
        {
            this.InitializeComponent();
        }

        private async void InputTextbox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (_isLoaded && !_process.HasExited)
            {
                if (e.Key == System.Windows.Input.Key.Enter)
                {
                    await _process.StandardInput.WriteLineAsync(InputTextbox.Text);
                    OutputTextbox.AppendText($"> {InputTextbox.Text}{Environment.NewLine}", "#4e6fb5");
                    InputTextbox.Text = "";
                }
            }
        }

        private void OutputTextbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            (sender as RichTextBox).ScrollToEnd();
        }

        private async void ReloadButton_Click(object sender, RoutedEventArgs e)
        {
            await Unload();
            await Load();
        }

        private async void HaskellInteractiveWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await Load();
        }

        public async Task Load()
        {
            if (!_isLoaded)
            {
                await Task.Delay(1000);
                InputTextbox.IsEnabled = false;
                OutputTextbox.Document.Blocks.Clear();

                SetupProcess();

                _process.Start();

                _process.BeginOutputReadLine();
                _process.BeginErrorReadLine();

                OutputTextbox.AppendText($"Starting GHCi...{Environment.NewLine}", "#787878");

                if (GHCiPath == "")
                {
                    _package = RequestSettingsData.Invoke();
                    GHCiPath = _package.GHCIPath;
                }

                await _process.StandardInput.WriteLineAsync($"cd '{DTE2Helper.GetSourcePath()}'");
                await _process.StandardInput.WriteLineAsync($"& '{GHCiPath}'");
                await _process.StandardInput.WriteLineAsync($":load {DTE2Helper.GetSourceFileName()}");
                OutputTextbox.AppendText($"GHCI started and '{DTE2Helper.GetSourceFileName()}' loaded!{Environment.NewLine}", "#787878");

                InputTextbox.IsEnabled = true;
                _isLoaded = true;
            }
        }

        public async Task Unload()
        {
            ProcessHelper.KillProcessAndChildrens(_process.Id);
            OutputTextbox.AppendText($"GHCi Unloaded{Environment.NewLine}", "#787878");
            _isLoaded = false;
        }

        private void RecieveErrorData(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null && _isLoaded)
                OutputTextbox.AppendTextInvoke($"{e.Data}{Environment.NewLine}", "#ba4141");
        }

        private void RecieveOutputData(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null && _isLoaded)
                OutputTextbox.AppendTextInvoke($"{e.Data}{Environment.NewLine}", "#ffffff");
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

            _process.ErrorDataReceived += RecieveErrorData;
            _process.OutputDataReceived += RecieveOutputData;
        }
    }
}