using Microsoft.VisualStudio.Threading;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaskellTools.Helpers
{
    public class PowershellProcess
    {
        public event DataReceivedEventHandler ErrorDataRecieved;
        public event DataReceivedEventHandler OutputDataRecieved;
        public bool IsRunning { get; private set; }

        private Process _process;

        public async Task WriteInputAsync(string input)
        {
            if (IsRunning)
                await _process.StandardInput.WriteLineAsync(input);
        }

        public void WriteInput(string input)
        {
            if (IsRunning)
                _process.StandardInput.WriteLine(input);
        }

        public async Task StartProcessAsync()
        {
            if (IsRunning)
                await StopProcessAsync();
            SetupProcess();
            _process.Start();
            _process.BeginErrorReadLine();
            _process.BeginOutputReadLine();
            IsRunning = true;
        }

        public void StartProcess()
        {
            if (IsRunning)
                StopProcess();
            SetupProcess();
            _process.Start();
            _process.BeginErrorReadLine();
            _process.BeginOutputReadLine();
            IsRunning = true;
        }

        public async Task StopProcessAsync()
        {
            if (_process != null && !_process.HasExited)
            {
                ProcessHelper.KillProcessAndChildrens(_process.Id);
                await _process.WaitForExitAsync();
            }
            IsRunning = false;
        }

        public void StopProcess()
        {
            if (_process != null && !_process.HasExited)
            {
                ProcessHelper.KillProcessAndChildrens(_process.Id);
                _process.WaitForExit();
            }
            IsRunning = false;
        }

        private void SetupProcess()
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = @"powershell.exe";
            startInfo.RedirectStandardError = true;
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardInput = true;
            startInfo.UseShellExecute = false;
            startInfo.CreateNoWindow = true;
            _process = new Process();
            _process.StartInfo = startInfo;
            _process.ErrorDataReceived += RecieveErrorData;
            _process.OutputDataReceived += RecieveOutputData;
        }

        private void RecieveErrorData(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null && e.Data != "")
            {
                if (ErrorDataRecieved != null)
                    ErrorDataRecieved.Invoke(sender, e);
            }
        }

        private void RecieveOutputData(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null && e.Data != "")
            {
                if (OutputDataRecieved != null)
                    OutputDataRecieved.Invoke(sender, e);
            }
        }
    }
}
