using HaskellTools.Helpers;
using HaskellTools.Options;
using Microsoft.VisualStudio.Text.Editor;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Linq;
using static System.Net.Mime.MediaTypeNames;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.PlatformUI;
using System.Threading.Tasks;

namespace HaskellTools.EditorMargins
{
    public class HaskellEditorMargin : StackPanel, IWpfTextViewMargin
    {
        private Dictionary<Guid,MarginPanel> _panels;

        private delegate Guid SubscribePanelEventHandler();
        private static event SubscribePanelEventHandler SubscribePanelEvent;

        private delegate void UnsubscribePanelEventHandler(Guid panelGuid);
        private static event UnsubscribePanelEventHandler UnsubscribePanelEvent;

        private delegate void UpdatePanelEventHandler(Guid panelGuid, string text, SolidColorBrush backgroundColor, bool showLoading = false);
        private static event UpdatePanelEventHandler UpdatePanelEvent;

        public const string MarginName = "Haskell Editor Margin";

        private bool isDisposed;

        public HaskellEditorMargin(IWpfTextView textView)
        {
            _panels = new Dictionary<Guid, MarginPanel>();
            UpdatePanelEvent += UpdatePanel_Event;
            SubscribePanelEvent += SubscribePanel_Event;
            UnsubscribePanelEvent += UnsubscribePanel_Event;

            this.Height = 35;
            this.ClipToBounds = true;
            this.Background = StatusColors.StatusBarBackground();
            this.Orientation = Orientation.Horizontal;
        }

        public static Guid SubscribePanel()
        {
            if (SubscribePanelEvent != null)
                return SubscribePanelEvent.Invoke();
            return Guid.Empty;
        }
        private Guid SubscribePanel_Event()
        {
            var newPanel = new MarginPanel(_panels);
            this.Children.Add(newPanel);
            _panels.Add(newPanel.PanelID, newPanel);
            return newPanel.PanelID;
        }

        public static void UnsubscribePanel(Guid panelGuid)
        {
            if (UnsubscribePanelEvent != null)
                UnsubscribePanelEvent.Invoke(panelGuid);
        }
        private async void UnsubscribePanel_Event(Guid panelGuid)
        {
            if (_panels.ContainsKey(panelGuid))
            {
                var panel = _panels[panelGuid];
                await panel.RemoveThisPanelFromParentAsync();
            }
        }

        public static void UpdatePanel(Guid panelGuid, string text, SolidColorBrush backgroundColor, bool showLoading = false)
        {
            if (UpdatePanelEvent != null)
                UpdatePanelEvent.Invoke(panelGuid, text, backgroundColor, showLoading);
        }
        private async void UpdatePanel_Event(Guid panelGuid, string text, SolidColorBrush backgroundColor, bool showLoading = false)
        {
            if (_panels.ContainsKey(panelGuid))
            {
                var panel = _panels[panelGuid];
                while (!panel.IsLoaded)
                    await Task.Delay(100);
                panel.UpdatePanel(text, backgroundColor, showLoading);
            }
        }

        private void RemovePanelEvent(object sender, EventArgs e)
        {
            if (sender is DispatcherTimer timer)
                if (timer.Tag is Guid id)
                    UnsubscribePanel_Event(id);
        }

        #region IWpfTextViewMargin

        public FrameworkElement VisualElement
        {
            get
            {
                this.ThrowIfDisposed();
                return this;
            }
        }

        #endregion

        #region ITextViewMargin

        public double MarginSize
        {
            get
            {
                this.ThrowIfDisposed();

                return this.ActualHeight;
            }
        }

        public bool Enabled
        {
            get
            {
                this.ThrowIfDisposed();
                return true;
            }
        }
        public ITextViewMargin GetTextViewMargin(string marginName)
        {
            return string.Equals(marginName, HaskellEditorMargin.MarginName, StringComparison.OrdinalIgnoreCase) ? this : null;
        }

        public void Dispose()
        {
            if (!this.isDisposed)
            {
                GC.SuppressFinalize(this);
                this.isDisposed = true;
            }
        }

        #endregion

        private void ThrowIfDisposed()
        {
            if (this.isDisposed)
            {
                throw new ObjectDisposedException(MarginName);
            }
        }
    }
}
