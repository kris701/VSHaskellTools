using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EnvDTE;
using HaskellTools.HaskellInfo;
using HaskellTools.Options;
using Microsoft.VisualStudio.Core.Imaging;
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Language.StandardClassification;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Adornments;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;

namespace HaskellTools.QuickInfo
{
    internal class HaskellQuickInfoSource : IAsyncQuickInfoSource
    {
        private ITextBuffer _textBuffer;
        private HaskellQuickInfoSourceProvider _toolTipProvider;

        public HaskellQuickInfoSource(ITextBuffer subjectBuffer, HaskellQuickInfoSourceProvider toolTipProvider)
        {
            _textBuffer = subjectBuffer;
            _toolTipProvider = toolTipProvider;
        }

        private bool m_isDisposed;
        public void Dispose()
        {
            if (!m_isDisposed)
            {
                GC.SuppressFinalize(this);
                m_isDisposed = true;
            }
        }

        public Task<QuickInfoItem> GetQuickInfoItemAsync(IAsyncQuickInfoSession session, CancellationToken cancellationToken)
        {
            Task<QuickInfoItem> t = new Task<QuickInfoItem>(() =>
            {
                SnapshotPoint? subjectTriggerPoint = session.GetTriggerPoint(_textBuffer.CurrentSnapshot);
                ITextSnapshot currentSnapshot = subjectTriggerPoint.Value.Snapshot;
                SnapshotSpan querySpan = new SnapshotSpan(subjectTriggerPoint.Value, 0);

                ITextStructureNavigator navigator = _toolTipProvider.NavigatorService.GetTextStructureNavigator(_textBuffer);
                TextExtent extent = navigator.GetExtentOfWord(subjectTriggerPoint.Value);
                string searchText = extent.Span.GetText();

                if (!subjectTriggerPoint.HasValue)
                {
                    return new QuickInfoItem(null, "");
                }

                if (HaskellPreludeInfo.IsLoading)
                {
                    ITrackingSpan applicable = currentSnapshot.CreateTrackingSpan(extent.Span, SpanTrackingMode.EdgeInclusive);
                    return new QuickInfoItem(applicable, "Prelude is loading...");
                }
                else
                {
                    foreach (string key in HaskellPreludeInfo.PreludeContent.Keys.OrderByDescending(x => x.Length))
                    {
                        int foundIdx = searchText.IndexOf(key, StringComparison.CurrentCultureIgnoreCase);
                        if (foundIdx > -1)
                        {
                            ITrackingSpan applicable = currentSnapshot.CreateTrackingSpan(extent.Span.Start + foundIdx, key.Length, SpanTrackingMode.EdgeInclusive);
                            return new QuickInfoItem(applicable, HaskellPreludeInfo.PreludeContent[key]);
                        }
                    }
                    return new QuickInfoItem(null, "");
                }
            });
            t.Start();
            return t;
        }
    }
}
