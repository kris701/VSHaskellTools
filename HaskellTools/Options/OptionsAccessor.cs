using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaskellTools.Options
{
    public static class OptionsAccessor
    {
        public static OptionPageGrid Instance = null;

        public static string GHCUPPath
        {
            get
            {
                return Instance.GHCUPPath;
            }
            set
            {
                Instance.GHCUPPath = value;
                Instance.SaveSettingsToStorage();
            }
        }

        public static int HaskellFileExecutionTimeout
        {
            get
            {
                return Instance.HaskellFileExecutionTimeout;
            }
            set 
            { 
                Instance.HaskellFileExecutionTimeout = value;
                Instance.SaveSettingsToStorage();
            }
        }

        public static string DebuggerEntryFunctionName
        {
            get
            {
                return Instance.DebuggerEntryFunctionName;
            }
            set
            {
                Instance.DebuggerEntryFunctionName = value;
                Instance.SaveSettingsToStorage();
            }
        }

        public static bool CheckForGHCiAtStartup
        {
            get
            {
                return Instance.CheckForGHCiAtStartup;
            }
            set
            {
                Instance.CheckForGHCiAtStartup = value;
                Instance.SaveSettingsToStorage();
            }
        }

        public static bool GHCiFound
        {
            get
            {
                return Instance.GHCiFound;
            }
            set
            {
                Instance.GHCiFound = value;
                Instance.SaveSettingsToStorage();
            }
        }

        public static bool IsFirstStart
        {
            get
            {
                return Instance.IsFirstStart;
            }
            set
            {
                Instance.IsFirstStart = value;
                Instance.SaveSettingsToStorage();
            }
        }
    }
}
