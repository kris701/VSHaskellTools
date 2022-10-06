using HaskellTools.Helpers;
using HaskellTools.Options;
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
        private PowershellProcess _process;
        private bool _isStarted = false;
        private bool _isGood = true;
        public GHCiChecker(HaskellToolsPackage package)
        {
            _package = package;
        }

        public async Task CheckForGHCiAsync()
        {
            OptionsAccessor.GHCiFound = false;
            _process = new PowershellProcess();
            _process.ErrorDataRecieved += RecieveErrorData;
            await _process.StartProcessAsync();
            _isStarted = true;
            if (OptionsAccessor.GHCUPPath == "")
                await _process.WriteLineAsync($"& ghci");
            else
                await _process.WriteLineAsync($"& '{DirHelper.CombinePathAndFile(OptionsAccessor.GHCUPPath, "bin\\ghci.exe")}'");
            await Task.Delay(500);
            if (_isGood)
            {
                OptionsAccessor.CheckForGHCiAtStartup = false;
                OptionsAccessor.GHCiFound = true;
            }
            else
            {
                await InstallGHCiWindowCommand.InitializeAsync(_package);
                InstallGHCiWindowCommand.Instance.Execute(null, null);
            }
            await _process.StopProcessAsync();
        }

        private void RecieveErrorData(object sender, DataReceivedEventArgs e)
        {
          if (_isStarted)
                _isGood = false;
        }
    }
}
