using HaskellTools.Helpers;
using HaskellTools.Options;
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
    public partial class HaskellInteractiveWindowControl : UserControl
    {
        private PowershellProcess _process;
        private bool _isLoaded = false;
        private List<string> _previousText = new List<string>() { "" };
        private int _previousTextIndex = 0;

        public HaskellInteractiveWindowControl()
        {
            this.InitializeComponent();
        }

        private async void InputTextbox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (_isLoaded && _process.IsRunning)
            {
                if (e.Key == System.Windows.Input.Key.Enter)
                {
                    if (InputTextbox.Text != "")
                    {
                        await SendInputAsync(InputTextbox.Text);
                        InputTextbox.Text = "";
                    }
                } 
                else if (e.Key == System.Windows.Input.Key.Down)
                {
                    if (_previousText.Count > 0)
                    {
                        _previousTextIndex--;
                        if (_previousTextIndex < 0)
                            _previousTextIndex = 0;
                        InputTextbox.Text = _previousText[_previousTextIndex];
                    }
                }
                else if (e.Key == System.Windows.Input.Key.Up)
                {
                    if (_previousText.Count > 0)
                    {
                        _previousTextIndex++;
                        if (_previousTextIndex >= _previousText.Count)
                            _previousTextIndex = _previousText.Count - 1;
                        InputTextbox.Text = _previousText[_previousTextIndex];
                    }
                }
            }
        }

        private async void EnterHistoryButton_Click(object sender, RoutedEventArgs e)
        {
            if (InputTextbox.Text != "")
            {
                await SendInputAsync(InputTextbox.Text);
                InputTextbox.Text = "";
            }
        }

        private async Task SendInputAsync(string text)
        {
            OutputTextbox.AppendText($"> {text}{Environment.NewLine}", "#4e6fb5");
            await _process.WriteLineAsync(text);
            await _process.WriteLineAsync("");
            _previousText.Insert(1, text);
            _previousTextIndex = 0;
        }

        private void OutputTextbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            (sender as RichTextBox).ScrollToEnd();
        }

        private async void ReloadButton_Click(object sender, RoutedEventArgs e)
        {
            await UnloadAsync();
            await LoadAsync();
        }

        private async void HaskellInteractiveWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await Task.Delay(1000);
            await LoadAsync();
        }

        public async Task LoadAsync()
        {
            if (!_isLoaded)
            {
                LoadedFileNameLabel.Content = $"Starting...";
                _process = new PowershellProcess();
                _process.ErrorDataRecieved += RecieveErrorData;
                _process.OutputDataRecieved += RecieveOutputData;
                await _process.StartProcessAsync();

                await _process.WriteLineAsync($"cd '{await DTE2Helper.GetSourcePathAsync()}'");
                if (OptionsAccessor.GHCUPPath == "")
                    await _process.WriteLineAsync($"& ghci");
                else
                    await _process.WriteLineAsync($"& '{DirHelper.CombinePathAndFile(OptionsAccessor.GHCUPPath, "bin\\ghci.exe")}'");
                string fileName = await DTE2Helper.GetSourceFileNameAsync();
                _isLoaded = true;
                await _process.WriteLineAsync($":load \"{fileName}\"");
                LoadedFileNameLabel.Content = $"File Loaded: '{fileName}'";

                InputTextbox.IsEnabled = true;
            }
        }

        public async Task UnloadAsync()
        {
            InputTextbox.IsEnabled = false;
            OutputTextbox.Document.Blocks.Clear();
            if (_process != null)
                await _process.StopProcessAsync();
            LoadedFileNameLabel.Content = $"GHCi Unloaded";
            _isLoaded = false;
        }

        private async void RecieveErrorData(object sender, DataReceivedEventArgs e)
        {
            if (_isLoaded)
            {
                string data = CleanErrorDataString(e.Data);
                if (data != "")
                    await OutputTextbox.AppendTextAsync($"{data}{Environment.NewLine}", "#ba4141");
            }
        }

        private async void RecieveOutputData(object sender, DataReceivedEventArgs e)
        {
            if (_isLoaded)
            {
                string data = CleanDataString(e.Data);
                if (data != "")
                    await OutputTextbox.AppendTextAsync($"{data}{Environment.NewLine}", "#ffffff");
            }
        }

        private string CleanDataString(string data)
        {
            if (data.Contains("*Main>"))
            {
                data = data.Substring(data.LastIndexOf("*Main>") + 6);
                data = data.Trim();
            }
            return data;
        }

        private string CleanErrorDataString(string data)
        {
            data = CleanDataString(data);
            if (data.Contains("error:"))
            {
                data = data.Substring(data.LastIndexOf("error:") + 6);
                data = data.Trim();
            }
            return data;
        }

        private void ClearHistoryButton_Click(object sender, RoutedEventArgs e)
        {
            _previousText.Clear();
            _previousText.Add("");
            _previousTextIndex = 0;
        }
    }
}