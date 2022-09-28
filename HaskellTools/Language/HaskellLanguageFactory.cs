using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace HaskellTools
{
    [Guid(Constants.HaskellGuidEditorFactory)]
    [ComVisible(true)]
    internal class HaskellLanguageFactory : EditorFactory
    {
        public HaskellLanguageFactory(Package package) : base(package) { }
    }
}
