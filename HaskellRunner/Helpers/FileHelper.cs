﻿using EnvDTE;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaskellRunner.Helpers
{
    public static class FileHelper
    {
        public static EnvDTE80.DTE2 GetDTE2()
        {
            return Package.GetGlobalService(typeof(DTE)) as EnvDTE80.DTE2;
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