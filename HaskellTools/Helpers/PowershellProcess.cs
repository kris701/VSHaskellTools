using Microsoft.VisualStudio.Threading;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace HaskellTools.Helpers
{
    public enum ProcessCompleteReson { None, ProcessNotRunning, RanToCompletion, ForceKilled }
    public class PowershellProcess
    {
        public event DataReceivedEventHandler ErrorDataRecieved;
        public event DataReceivedEventHandler OutputDataRecieved;
        public bool IsRunning { get; private set; }

        private Process _process;
        DispatcherTimer _timer;
        private bool _didForceKill = false;

        public async Task WriteLineAsync(string input)
        {
            if (IsRunning)
                await _process.StandardInput.WriteLineAsync(input);
        }

        public void WriteLine(string input)
        {
            if (IsRunning)
                _process.StandardInput.WriteLine(input);
        }

        public async Task StartProcessAsync(string args = "")
        {
            if (IsRunning)
                await StopProcessAsync();
            SetupProcess(args);
            _process.Start();
            _process.BeginErrorReadLine();
            _process.BeginOutputReadLine();
            IsRunning = true;
        }

        public void StartProcess(string args = "")
        {
            if (IsRunning)
                StopProcess();
            SetupProcess(args);
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

        public async Task<ProcessCompleteReson> WaitForExitAsync(TimeSpan timeout)
        {
            if (IsRunning)
            {
                _didForceKill = false;
                _timer = new DispatcherTimer();
                _timer.Interval = timeout;
                _timer.Tick += ForceKillTimer;
                _timer.Start();
                await WaitForExitAsync();
                if (_didForceKill)
                    return ProcessCompleteReson.ForceKilled;
                else
                    return ProcessCompleteReson.RanToCompletion;
            }
            else return ProcessCompleteReson.ProcessNotRunning;
        }

        public async Task<ProcessCompleteReson> WaitForExitAsync()
        {
            if (IsRunning)
            {
                await _process.WaitForExitAsync();
                return ProcessCompleteReson.RanToCompletion;
            }
            else
                return ProcessCompleteReson.ProcessNotRunning;
        }

        private void ForceKillTimer(object sender, EventArgs e)
        {
            _didForceKill = true;
            ProcessHelper.KillProcessAndChildrens(_process.Id);
        }

        private void SetupProcess(string args = "")
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = @"powershell.exe";
            startInfo.Arguments = args;
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
