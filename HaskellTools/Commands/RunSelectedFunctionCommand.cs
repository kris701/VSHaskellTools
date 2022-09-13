using EnvDTE;
using HaskellTools.Commands;
using HaskellTools.Helpers;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace HaskellTools.Commands
{
    internal sealed class RunSelectedFunctionCommand : BaseCommand
    {
        private static OutputPanelController OutputPanel = new OutputPanelController("Haskell GHCi");
        public override int CommandId { get; } = 257;
        public static RunSelectedFunctionCommand Instance { get; internal set; }

        private RunSelectedFunctionCommand(AsyncPackage package, OleMenuCommandService commandService) : base(package, commandService)
        {
        }

        public static async Task InitializeAsync(AsyncPackage package)
        {
            Instance = new RunSelectedFunctionCommand(package, await InitializeCommandServiceAsync(package));
        }

        public override void Execute(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (DTE2Helper.GetSourceFileExtension() != ".hs")
            {
                MessageBox.Show("File must be a '.hs' file!");
                return;
            }

            OutputPanel.Initialize();
            OutputPanel.ClearOutput();

            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = @"powershell.exe";
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;
            startInfo.RedirectStandardInput = true;
            startInfo.UseShellExecute = false;
            startInfo.CreateNoWindow = true;
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            process.StartInfo = startInfo;
            process.Start();

            OutputPanel.WriteLine("Running GHCi...");

            HaskellToolsPackage myToolsOptionsPackage = this.package as HaskellToolsPackage;

            process.StandardInput.WriteLine($"cd '{DTE2Helper.GetSourcePath()}'");
            process.StandardInput.WriteLine($"& '{myToolsOptionsPackage.GHCIPath}'");
            System.Threading.Thread.Sleep(1000);
            process.StandardInput.WriteLine($":load {DTE2Helper.GetSourceFileName()}");
            process.StandardInput.WriteLine($"{DTE2Helper.GetSelectedText()}");
            process.StandardInput.WriteLine($":quit");
            System.Threading.Thread.Sleep(1000);
            process.StandardInput.WriteLine($"exit");

            process.WaitForExit();

            if (!process.StandardError.EndOfStream)
            {
                while (!process.StandardError.EndOfStream)
                    OutputPanel.WriteLine($"Error! {process.StandardError.ReadLine()}");
            }
            else
            {
                bool reading = false;
                while (!process.StandardOutput.EndOfStream)
                {
                    string line = process.StandardOutput.ReadLine();
                    if (line.Contains("Leaving GHCi"))
                        reading = true;
                    if (reading)
                        OutputPanel.WriteLine(line);
                    if (line.Contains("module loaded"))
                        reading = true;
                }
            }
        }
    }
}
