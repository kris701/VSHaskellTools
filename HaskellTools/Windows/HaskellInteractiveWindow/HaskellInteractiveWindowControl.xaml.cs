using HaskellTools.Events;
using HaskellTools.Helpers;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO.Packaging;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
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
        private List<string> _previousText = new List<string>();
        private int _previousTextIndex = 0;

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
                    if (_previousText.Count == 0)
                        _previousText.Insert(0, InputTextbox.Text);
                    if (InputTextbox.Text != _previousText[0])
                        _previousText.Insert(0, InputTextbox.Text);
                    InputTextbox.Text = "";
                    _previousTextIndex = 0;
                } 
                else if (e.Key == System.Windows.Input.Key.Up)
                {
                    _previousTextIndex--;
                    if (_previousTextIndex < 0)
                        _previousTextIndex = 0;
                    InputTextbox.Text = _previousText[_previousTextIndex];
                }
                else if (e.Key == System.Windows.Input.Key.Down)
                {
                    _previousTextIndex++;
                    if (_previousTextIndex >= _previousText.Count)
                        _previousTextIndex = _previousText.Count - 1;
                    InputTextbox.Text = _previousText[_previousTextIndex];
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
            await Task.Delay(1000);
            await Load();
        }

        public async Task Load()
        {
            if (!_isLoaded)
            {
                LoadedFileNameLabel.Content = $"Starting...";
                SetupProcess();

                _process.Start();

                _process.BeginOutputReadLine();
                _process.BeginErrorReadLine();

                if (_package == null)
                {
                    _package = RequestSettingsData.Invoke();
                }

                await _process.StandardInput.WriteLineAsync($"cd '{DTE2Helper.GetSourcePath()}'");
                await _process.StandardInput.WriteLineAsync($"& '{GHCiPath}'");
                string fileName = DTE2Helper.GetSourceFileName();
                await _process.StandardInput.WriteLineAsync($":load \"{fileName}\"");
                await Task.Delay(100);
                LoadedFileNameLabel.Content = $"File Loaded: '{fileName}'";

                InputTextbox.IsEnabled = true;
                _isLoaded = true;
            }
        }

        public async Task Unload()
        {
            InputTextbox.IsEnabled = false;
            OutputTextbox.Document.Blocks.Clear();
            ProcessHelper.KillProcessAndChildrens(_process.Id);
            LoadedFileNameLabel.Content = $"GHCi Unloaded";
            _previousText.Clear();
            _previousTextIndex = 0;
            _isLoaded = false;
        }

        private void RecieveErrorData(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null && _isLoaded)
                OutputTextbox.AppendTextInvoke($"{e.Data.Replace("*Main>", "")}{Environment.NewLine}", "#ba4141");
        }

        private void RecieveOutputData(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null && _isLoaded)
                OutputTextbox.AppendTextInvoke($"{e.Data.Replace("*Main>", "")}{Environment.NewLine}", "#ffffff");
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