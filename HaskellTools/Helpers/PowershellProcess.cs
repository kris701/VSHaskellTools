using Microsoft.VisualStudio.PlatformUI;
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
    public enum ProcessCompleteReson { None, ProcessNotRunning, RanToCompletion, StoppedOnError, ForceKilled }
    public class PowershellProcess
    {
        public event DataReceivedEventHandler ErrorDataRecieved;
        public event DataReceivedEventHandler OutputDataRecieved;
        public bool IsRunning { get; private set; }
        public bool StopOnError { get; set; } = false;
        public TimeSpan OutputTimeout { get; set; } = TimeSpan.Zero;

        private Process _process;
        DispatcherTimer _timer;
        DispatcherTimer _OutputTimer;
        private bool _didForceKill = false;
        private bool _didEncounterError = false;

        public async Task WriteLineAsync(string input)
        {
            if (IsRunning)
            {
                try
                {
                    await _process.StandardInput.WriteLineAsync(input);
                }
                catch { }
            }
        }

        public void WriteLine(string input)
        {
            if (IsRunning)
            {
                try
                {
                    _process.StandardInput.WriteLine(input);
                }
                catch { }
            }
        }

        public async Task StartProcessAsync(string args = "")
        {
            if (IsRunning)
                await StopProcessAsync();
            SetupProcess(args);
            IsRunning = true;
        }

        public void StartProcess(string args = "")
        {
            if (IsRunning)
                StopProcess();
            SetupProcess(args);
            IsRunning = true;
        }

        public async Task StopProcessAsync()
        {
            if (_process != null && !_process.HasExited)
            {
                if (_timer != null)
                    _timer.Stop();
                if (_OutputTimer != null)
                    _OutputTimer.Stop();
                ProcessHelper.KillProcessAndChildrens(_process.Id);
                await _process.WaitForExitAsync();
            }
            IsRunning = false;
        }

        public void StopProcess()
        {
            if (_process != null && !_process.HasExited)
            {
                if (_timer != null)
                    _timer.Stop();
                if (_OutputTimer != null)
                    _OutputTimer.Stop();
                ProcessHelper.KillProcessAndChildrens(_process.Id);
                _process.WaitForExit();
            }
            IsRunning = false;
        }

        public async Task<ProcessCompleteReson> WaitForExitAsync(TimeSpan timeout)
        {
            if (_didForceKill)
                return ProcessCompleteReson.ForceKilled;
            if (_didEncounterError)
                return ProcessCompleteReson.StoppedOnError;
            if (IsRunning)
            {
                _timer = new DispatcherTimer();
                _timer.Interval = timeout;
                _timer.Tick += ForceKillTimer;
                _timer.Start();
                await WaitForExitAsync();
                if (_didForceKill)
                    return ProcessCompleteReson.ForceKilled;
                if (_didEncounterError)
                    return ProcessCompleteReson.StoppedOnError;
                return ProcessCompleteReson.RanToCompletion;
            }
            else return ProcessCompleteReson.ProcessNotRunning;
        }

        public async Task<ProcessCompleteReson> WaitForExitAsync()
        {
            if (_didForceKill)
                return ProcessCompleteReson.ForceKilled;
            if (_didEncounterError)
                return ProcessCompleteReson.StoppedOnError;
            if (IsRunning)
            {
                await _process.WaitForExitAsync();
                if (_didForceKill)
                    return ProcessCompleteReson.ForceKilled;
                if (_didEncounterError)
                    return ProcessCompleteReson.StoppedOnError;
                return ProcessCompleteReson.RanToCompletion;
            }
            else
                return ProcessCompleteReson.ProcessNotRunning;
        }

        private void ForceKillTimer(object sender, EventArgs e)
        {
            if (sender is DispatcherTimer timer)
            {
                timer.Stop();
                _didForceKill = true;
                ProcessHelper.KillProcessAndChildrens(_process.Id);
                IsRunning = false;
            }
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

            if (OutputTimeout > TimeSpan.Zero)
            {
                _OutputTimer = new DispatcherTimer();
                _OutputTimer.Interval = OutputTimeout;
                _OutputTimer.Tick += ForceKillTimer;
            }

            _process.Start();
            _process.BeginErrorReadLine();
            _process.BeginOutputReadLine();
            if (OutputTimeout > TimeSpan.Zero)
                _OutputTimer.Start();
            _didForceKill = false;
            _didEncounterError = false;
        }

        private async void RecieveErrorData(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null && e.Data != "")
            {
                if (OutputTimeout > TimeSpan.Zero)
                    _OutputTimer.Start();
                if (ErrorDataRecieved != null)
                    ErrorDataRecieved.Invoke(sender, e);
                if (StopOnError)
                {
                    _didEncounterError = true;
                    await StopProcessAsync();
                }
            }
        }

        private void RecieveOutputData(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null && e.Data != "")
            {
                if (OutputTimeout > TimeSpan.Zero)
                    _OutputTimer.Start();
                if (OutputDataRecieved != null)
                    OutputDataRecieved.Invoke(sender, e);
            }
        }
    }
}
