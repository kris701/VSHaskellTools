using EnvDTE;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace HaskellTools.Helpers
{
    public static class DTE2Helper
    {
        public static EnvDTE80.DTE2 GetDTE2()
        {
            return Package.GetGlobalService(typeof(DTE)) as EnvDTE80.DTE2;
        }

        public static async Task<bool> IsValidFileOpenAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            try
            {
                EnvDTE80.DTE2 _applicationObject = GetDTE2();
                if (_applicationObject == null)
                    return false;
                var uih = _applicationObject.ActiveDocument;
                if (uih == null)
                    return false;
                var extension = await GetSourceFileExtensionAsync();
                if (extension != Constants.HaskellExt)
                    return false;
                return true;
            } catch
            {
                return false;
            }
        }

        public static async Task<bool> IsFullyVSOpenAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            EnvDTE80.DTE2 _applicationObject = GetDTE2();
            if (_applicationObject == null)
                return false;
            return _applicationObject.Solution.IsOpen;
        }

        public static async Task<string> GetSourceFilePathAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            EnvDTE80.DTE2 _applicationObject = GetDTE2();
            var uih = _applicationObject.ActiveDocument;
            return uih.FullName;
        }

        public static async Task<string> GetSourceFileNameAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            EnvDTE80.DTE2 _applicationObject = GetDTE2();
            var uih = _applicationObject.ActiveDocument;
            return uih.Name;
        }

        public static async Task<string> GetSourceFileExtensionAsync()
        {
            string name = await GetSourceFileNameAsync();
            return name.Substring(name.LastIndexOf('.'));
        }

        public static async Task<string> GetSourcePathAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            EnvDTE80.DTE2 _applicationObject = GetDTE2();
            var uih = _applicationObject.ActiveDocument;
            return uih.Path;
        }

        public static async Task SaveActiveDocumentAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            EnvDTE80.DTE2 _applicationObject = GetDTE2();
            var uih = _applicationObject.ActiveDocument;
            if (uih != null)
                uih.Save();
        }

        public static async Task FocusActiveDocumentAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            EnvDTE80.DTE2 _applicationObject = GetDTE2();
            var uih = _applicationObject.ActiveDocument;
            if (uih != null)
                uih.Activate();
        }

        public static async Task<string> GetSelectedTextAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            EnvDTE80.DTE2 _applicationObject = GetDTE2();
            var uih = _applicationObject.ActiveDocument;
            var value = ComUtils.Get(uih.Selection, "Text").ToString();
            return value;
        }
    }
}
