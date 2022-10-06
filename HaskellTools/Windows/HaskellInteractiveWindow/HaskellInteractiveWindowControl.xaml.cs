﻿using HaskellTools.Helpers;
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
        private List<string> _previousText = new List<string>();
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
                    await _process.WriteLineAsync(InputTextbox.Text);
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

                await _process.WriteLineAsync($"cd '{DTE2Helper.GetSourcePath()}'");
                if (OptionsAccessor.GHCUPPath == "")
                    await _process.WriteLineAsync($"& ghci");
                else
                    await _process.WriteLineAsync($"& '{DirHelper.CombinePathAndFile(OptionsAccessor.GHCUPPath, "bin\\ghci.exe")}'");
                string fileName = DTE2Helper.GetSourceFileName();
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
            _previousText.Clear();
            _previousTextIndex = 0;
            _isLoaded = false;
        }

        private void RecieveErrorData(object sender, DataReceivedEventArgs e)
        {
            OutputTextbox.AppendTextInvoke($"{e.Data.Replace("*Main>", "")}{Environment.NewLine}", "#ba4141");
        }

        private void RecieveOutputData(object sender, DataReceivedEventArgs e)
        {
            if (_isLoaded)
                OutputTextbox.AppendTextInvoke($"{e.Data.Replace("*Main>", "")}{Environment.NewLine}", "#ffffff");
        }
    }
}