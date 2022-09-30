using Microsoft.VisualStudio.Text.Adornments;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaskellTools.HaskellInfo
{
    public static class HaskellPreludeInfo
    {
        public static bool IsLoading = false;
        public static Dictionary<string, ContainerElement> PreludeContent = new Dictionary<string, ContainerElement>();
    }
}
