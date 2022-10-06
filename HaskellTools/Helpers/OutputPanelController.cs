using EnvDTE;
using Microsoft.VisualStudio.Shell;
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

        public void Initialize()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (targetPanel != null)
                return;

            EnvDTE80.DTE2 _applicationObject = GetDTE2();
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

        public void ClearOutput()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (targetPanel == null)
                throw new ArgumentNullException("Panel not initialized!");

            targetPanel.Clear();
        }

        public void WriteLine(string text)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (targetPanel == null)
                throw new ArgumentNullException("Panel not initialized!");

            targetPanel.Activate();
            if (text.Length > 1024)
                targetPanel.OutputString($"{text.Substring(0,1024)} ... (Output too long to show here!){Environment.NewLine}");
            else
                targetPanel.OutputString($"{text}{Environment.NewLine}");
        }

        public void WriteLineInvoke(string text)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                WriteLine(text);
            }));
        }

        private static EnvDTE80.DTE2 GetDTE2()
        {
            return Package.GetGlobalService(typeof(DTE)) as EnvDTE80.DTE2;
        }

        public void ActivateOutputWindow()
        {
            if (targetPanel != null)
                targetPanel.Activate();
        }
    }
}
