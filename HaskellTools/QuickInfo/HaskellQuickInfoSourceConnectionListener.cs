using HaskellTools.Options;
using HaskellTools.QuickInfo.HaskellInfo;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaskellTools.ErrorList
{
    [Export(typeof(ITextViewConnectionListener))]
    [ContentType(Constants.HaskellLanguageName)]
    [TextViewRole(PredefinedTextViewRoles.Interactive)]
    internal class HaskellQuickInfoSourceConnectionListener : ITextViewConnectionListener
    {
        public void SubjectBuffersConnected(ITextView textView, ConnectionReason reason, IReadOnlyCollection<ITextBuffer> subjectBuffers)
        {
            if (HaskellPreludeInfo.PreludeContent.Count == 0)
            {
                HaskellPreludeInfo.IsLoading = true;
                HaskellPreludeInitializer initializer = new HaskellPreludeInitializer();
                Task.Run(async () => await initializer.InitializePreludeContentAsync(OptionsAccessor.GHCUPPath));
            }
        }

        public void SubjectBuffersDisconnected(ITextView textView, ConnectionReason reason, IReadOnlyCollection<ITextBuffer> subjectBuffers)
        {

        }
    }
}
