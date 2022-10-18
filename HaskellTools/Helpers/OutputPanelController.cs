using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Threading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace HaskellTools.Helpers
{
    public class OutputPanelController
    {
        public string OutputPaneName { get; set; }
        private OutputWindowPane targetPanel = null;

        public OutputPanelController(string outputPanelName)
        {
            OutputPaneName = outputPanelName;
        }

        public async Task InitializeAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            if (targetPanel != null)
                return;

            EnvDTE80.DTE2 _applicationObject = DTE2Helper.GetDTE2();
            var uih = _applicationObject.ToolWindows.OutputWindow;
            bool found = false;
            foreach (OutputWindowPane pane in uih.OutputWindowPanes)
            {
                if (pane.Name == OutputPaneName)
                {
                    targetPanel = pane;
                    found = true;
                    break;
                }
            }
            if (!found)
                targetPanel = uih.OutputWindowPanes.Add(OutputPaneName);
            targetPanel.Activate();
            targetPanel.Clear();
        }

        public async Task ClearOutputAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            if (targetPanel == null)
                throw new ArgumentNullException("Panel not initialized!");

            targetPanel.Clear();
        }

        private void WriteLine(string text)
        {
            if (targetPanel == null)
                throw new ArgumentNullException("Panel not initialized!");

            targetPanel.Activate();
            if (text.Length > 1024)
                targetPanel.OutputString($"{text.Substring(0,1024)} ... (Output too long to show here!){Environment.NewLine}");
            else
                targetPanel.OutputString($"{text}{Environment.NewLine}");
        }

        public async Task WriteLineAsync(string text)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            WriteLine(text);
        }

        public async Task ActivateOutputWindowAsync()
        {
            if (targetPanel != null)
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                DTE dte = DTE2Helper.GetDTE2();
                var window = dte.Windows.Item(EnvDTE.Constants.vsWindowKindOutput);
                window.Activate();
                targetPanel.Activate();
            }
        }
    }
}
