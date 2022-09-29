using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace HaskellTools.Helpers
{
    public static class DirHelper
    {
        public static string CombinePathAndFile(string path, string file)
        {
            path = path.Trim();
            file = file.Trim();
            if (!path.EndsWith("\\"))
                path = $"{path}\\";
            if (file.StartsWith("\\"))
                file = file.Substring(1);
            return $"{path}{file}";
        }
    }
}
