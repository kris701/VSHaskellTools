using EnvDTE;
using HaskellTools.Commands;
using HaskellTools.Helpers;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Task = System.Threading.Tasks.Task;

namespace HaskellTools.Commands
{
    internal sealed class RunHaskellFile : BaseCommand
    {
        private static OutputPanelController OutputPanel = new OutputPanelController("Haskell");
        public override int CommandId { get; } = 256;
        public static RunHaskellFile Instance { get; internal set; }

        private RunHaskellFile(AsyncPackage package, OleMenuCommandService commandService) : base(package, commandService)
        {
        }

        public static async Task InitializeAsync(AsyncPackage package)
        {
            Instance = new RunHaskellFile(package, await InitializeCommandServiceAsync(package));
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

            string value = DTE2Helper.GetSourceFilePath();

            HaskellToolsPackage myToolsOptionsPackage = this.package as HaskellToolsPackage;

            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = @"powershell.exe";
            startInfo.Arguments = $"& '{myToolsOptionsPackage.RunHaskellPath}' '{value}'";
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;
            startInfo.UseShellExecute = false;
            startInfo.CreateNoWindow = true;
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            process.StartInfo = startInfo;
            process.Start();

            OutputPanel.WriteLine($"Executing Haskell File '{value}'");
            if (!process.StandardError.EndOfStream)
            {
                
                while (!process.StandardError.EndOfStream)
                    OutputPanel.WriteLine($"Error! {process.StandardError.ReadLine()}");
            }
            else
            {
                while (!process.StandardOutput.EndOfStream)
                    OutputPanel.WriteLine(process.StandardOutput.ReadLine());
            }
        }
    }
}
