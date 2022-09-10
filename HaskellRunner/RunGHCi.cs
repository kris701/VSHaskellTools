using EnvDTE;
using HaskellRunner.Helpers;
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
using Task = System.Threading.Tasks.Task;

namespace HaskellRunner
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class RunGHCi
    {
        private static OutputPanelController OutputPanel = new OutputPanelController("Haskell GHCi");

        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 257;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("c8d29eda-f85f-4c3f-8620-6b8c0c6ebd51");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly AsyncPackage package;

        /// <summary>
        /// Initializes a new instance of the <see cref="RunGHCi"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        private RunGHCi(AsyncPackage package, OleMenuCommandService commandService)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandID = new CommandID(CommandSet, CommandId);
            var menuItem = new MenuCommand(this.Execute, menuCommandID);
            commandService.AddCommand(menuItem);
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static RunGHCi Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private Microsoft.VisualStudio.Shell.IAsyncServiceProvider ServiceProvider
        {
            get
            {
                return this.package;
            }
        }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static async Task InitializeAsync(AsyncPackage package)
        {
            // Switch to the main thread - the call to AddCommand in RunGHCi's constructor requires
            // the UI thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new RunGHCi(package, commandService);
        }

        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private void Execute(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            OutputPanel.Initialize();
            OutputPanel.ClearOutput();

            string value = GetSourceFilePath();

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

            process.StandardInput.WriteLine($"cd '{GetSourcePath()}'");
            process.StandardInput.WriteLine($"GHCi");
            System.Threading.Thread.Sleep(1000);
            process.StandardInput.WriteLine($":load {GetSourceFileName()}");
            process.StandardInput.WriteLine($"{GetSelectedText()}");
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

        private static EnvDTE80.DTE2 GetDTE2()
        {
            return Package.GetGlobalService(typeof(DTE)) as EnvDTE80.DTE2;
        }

        private string GetSourceFilePath()
        {
            EnvDTE80.DTE2 _applicationObject = GetDTE2();
            var uih = _applicationObject.ActiveDocument;
            return uih.FullName;
        }

        private string GetSourceFileName()
        {
            EnvDTE80.DTE2 _applicationObject = GetDTE2();
            var uih = _applicationObject.ActiveDocument;
            return uih.Name;
        }

        private string GetSourcePath()
        {
            EnvDTE80.DTE2 _applicationObject = GetDTE2();
            var uih = _applicationObject.ActiveDocument;
            return uih.Path;
        }

        private string GetSelectedText()
        {
            EnvDTE80.DTE2 _applicationObject = GetDTE2();
            var uih = _applicationObject.ActiveDocument;
            var value = ComUtils.Get(uih.Selection, "Text").ToString();
            return value;
        }
    }
}
