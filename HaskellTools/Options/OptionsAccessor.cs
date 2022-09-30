using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaskellTools.Options
{
    internal class OptionsAccessor
    {
        public static HaskellToolsPackage Instance = null;

        public static string GHCUPPath
        {
            get
            {
                OptionPageGrid page = (OptionPageGrid)Instance.GetDialogPage(typeof(OptionPageGrid));
                return page.GHCUPPath;
            }
            set
            {
                OptionPageGrid page = (OptionPageGrid)Instance.GetDialogPage(typeof(OptionPageGrid));
                page.GHCUPPath = value;
            }
        }

        public static int HaskellFileExecutionTimeout
        {
            get
            {
                OptionPageGrid page = (OptionPageGrid)Instance.GetDialogPage(typeof(OptionPageGrid));
                return page.HaskellFileExecutionTimeout;
            }
        }

        public static string DebuggerEntryFunctionName
        {
            get
            {
                OptionPageGrid page = (OptionPageGrid)Instance.GetDialogPage(typeof(OptionPageGrid));
                return page.DebuggerEntryFunctionName;
            }
        }

        public static bool CheckForGHCiAtStartup
        {
            get
            {
                OptionPageGrid page = (OptionPageGrid)Instance.GetDialogPage(typeof(OptionPageGrid));
                return page.CheckForGHCiAtStartup;
            }
            set
            {
                OptionPageGrid page = (OptionPageGrid)Instance.GetDialogPage(typeof(OptionPageGrid));
                page.CheckForGHCiAtStartup = value;
            }
        }

        internal static bool GHCiFound
        {
            get
            {
                OptionPageGrid page = (OptionPageGrid)Instance.GetDialogPage(typeof(OptionPageGrid));
                return page.GHCiFound;
            }
            set
            {
                OptionPageGrid page = (OptionPageGrid)Instance.GetDialogPage(typeof(OptionPageGrid));
                page.GHCiFound = value;
            }
        }

        internal static bool IsFirstStart
        {
            get
            {
                OptionPageGrid page = (OptionPageGrid)Instance.GetDialogPage(typeof(OptionPageGrid));
                return page.IsFirstStart;
            }
            set
            {
                OptionPageGrid page = (OptionPageGrid)Instance.GetDialogPage(typeof(OptionPageGrid));
                page.IsFirstStart = value;
            }
        }
    }
}
