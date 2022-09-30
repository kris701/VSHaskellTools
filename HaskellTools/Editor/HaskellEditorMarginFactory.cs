using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;

namespace HaskellTools.Editor
{
    [Export(typeof(IWpfTextViewMarginProvider))]
    [Name(HaskellEditorMargin.MarginName)]
    [Order(After = PredefinedMarginNames.HorizontalScrollBar)]
    [MarginContainer(PredefinedMarginNames.Bottom)]
    [ContentType("haskell")]
    [TextViewRole(PredefinedTextViewRoles.Interactive)]
    internal sealed class HaskellEditorMarginFactory : IWpfTextViewMarginProvider
    {
        public IWpfTextViewMargin CreateMargin(IWpfTextViewHost wpfTextViewHost, IWpfTextViewMargin marginContainer)
        {
            return new HaskellEditorMargin(wpfTextViewHost.TextView);
        }
    }
}
