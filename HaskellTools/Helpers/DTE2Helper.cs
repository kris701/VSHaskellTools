using EnvDTE;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public static bool IsValidFileOpen()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            EnvDTE80.DTE2 _applicationObject = GetDTE2();
            if (_applicationObject == null)
                return false;
            var uih = _applicationObject.ActiveDocument;
            if (uih == null)
                return false;
            var extension = GetSourceFileExtension();
            if (extension != "hs")
                return false;
            return true;
        }

        public static string GetSourceFilePath()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            EnvDTE80.DTE2 _applicationObject = GetDTE2();
            var uih = _applicationObject.ActiveDocument;
            return uih.FullName;
        }

        public static string GetSourceFileName()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            EnvDTE80.DTE2 _applicationObject = GetDTE2();
            var uih = _applicationObject.ActiveDocument;
            return uih.Name;
        }

        public static string GetSourceFileExtension()
        {
            string name = GetSourceFileName();
            return name.Substring(name.LastIndexOf('.'));
        }

        public static string GetSourcePath()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            EnvDTE80.DTE2 _applicationObject = GetDTE2();
            var uih = _applicationObject.ActiveDocument;
            return uih.Path;
        }

        public static string GetSelectedText()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            EnvDTE80.DTE2 _applicationObject = GetDTE2();
            var uih = _applicationObject.ActiveDocument;
            var value = ComUtils.Get(uih.Selection, "Text").ToString();
            return value;
        }
    }
}
