using HaskellTools.Helpers;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace HaskellTools.Checkers
{
    public class GHCiChecker
    {
        private HaskellToolsPackage _package;
        private Process _process;
        private bool _isStarted = false;
        private bool _isGood = true;
        public GHCiChecker(HaskellToolsPackage package)
        {
            _package = package;
        }

        public async Task CheckForGHCi()
        {
            SetupProcess();
            _isStarted = true;
            if (_package.GHCUPPath == "")
                await _process.StandardInput.WriteLineAsync($"& ghci");
            else
                await _process.StandardInput.WriteLineAsync($"& '{DirHelper.CombinePathAndFile(_package.GHCUPPath, "bin/ghci.exe")}'");
            await Task.Delay(500);
            if (_isGood)
                _package.CheckForGHCiAtStartup = false;
            else
            {
                MessageBox.Show("Not found!");
            }
        }

        private void SetupProcess()
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = @"powershell.exe";
            startInfo.RedirectStandardError = true;
            startInfo.RedirectStandardInput = true;
            startInfo.UseShellExecute = false;
            startInfo.CreateNoWindow = true;
            _process = new Process();
            _process.StartInfo = startInfo;
            _process.ErrorDataReceived += RecieveErrorData;
        }

        private void RecieveErrorData(object sender, DataReceivedEventArgs e)
        {
            if (_isStarted)
                _isGood = false;
        }
    }
}
