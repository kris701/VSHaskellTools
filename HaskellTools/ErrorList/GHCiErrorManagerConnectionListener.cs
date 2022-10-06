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
    internal class GHCiErrorManagerConnectionListener : ITextViewConnectionListener
    {
        public void SubjectBuffersConnected(ITextView textView, ConnectionReason reason, IReadOnlyCollection<ITextBuffer> subjectBuffers)
        {
            if (GHCiErrorManager.Instance != null)
                if (!GHCiErrorManager.Instance.IsStarted)
                    GHCiErrorManager.Instance.Initialize(textView);
        }

        public void SubjectBuffersDisconnected(ITextView textView, ConnectionReason reason, IReadOnlyCollection<ITextBuffer> subjectBuffers)
        {
            if (GHCiErrorManager.Instance != null)
                if (GHCiErrorManager.Instance.IsStarted)
                    GHCiErrorManager.Instance.Stop();
        }
    }
}
