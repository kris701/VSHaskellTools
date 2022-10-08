using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Windows.Media;

namespace HaskellTools.EditorMargins
{
    [Export(typeof(IWpfTextViewMarginProvider))]
    [Name(HaskellEditorMargin.MarginName)]
    [Order(After = PredefinedMarginNames.HorizontalScrollBar)]
    [MarginContainer(PredefinedMarginNames.Bottom)]
    [ContentType(Constants.HaskellLanguageName)]
    [TextViewRole(PredefinedTextViewRoles.Interactive)]
    internal sealed class HaskellEditorMarginFactory : IWpfTextViewMarginProvider
    {
        private delegate void SubscribePanelEventHandler(Guid newID);
        private static event SubscribePanelEventHandler SubscribePanelEvent;

        private delegate void UnsubscribePanelEventHandler(Guid panelGuid);
        private static event UnsubscribePanelEventHandler UnsubscribePanelEvent;

        private delegate void UpdatePanelEventHandler(Guid panelGuid, string text, SolidColorBrush backgroundColor, bool showLoading = false);
        private static event UpdatePanelEventHandler UpdatePanelEvent;

        public IWpfTextViewMargin CreateMargin(IWpfTextViewHost wpfTextViewHost, IWpfTextViewMargin marginContainer)
        {
            var newMargin = new HaskellEditorMargin(wpfTextViewHost.TextView);
            UpdatePanelEvent += newMargin.UpdatePanel_Event;
            SubscribePanelEvent += newMargin.SubscribePanel_Event;
            UnsubscribePanelEvent += newMargin.UnsubscribePanel_Event;
            return newMargin;
        }

        public static Guid SubscribePanel()
        {
            if (SubscribePanelEvent != null)
            {
                Guid newGuid = Guid.NewGuid();
                SubscribePanelEvent(newGuid);
                return newGuid;
            }
            return Guid.Empty;
        }

        public static void UnsubscribePanel(Guid panelGuid)
        {
            if (UnsubscribePanelEvent != null)
                UnsubscribePanelEvent(panelGuid);
        }

        public static void UpdatePanel(Guid panelGuid, string text, SolidColorBrush backgroundColor, bool showLoading = false)
        {
            if (UpdatePanelEvent != null)
                UpdatePanelEvent(panelGuid, text, backgroundColor, showLoading);
        }
    }
}
