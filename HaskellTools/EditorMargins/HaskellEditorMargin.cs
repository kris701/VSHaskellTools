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
        public const string MarginName = "Haskell Editor Margin";

        private Dictionary<Guid, MarginPanel> _panels;
        private bool isDisposed;

        public HaskellEditorMargin(IWpfTextView textView)
        {
            _panels = new Dictionary<Guid, MarginPanel>();

            this.Height = 35;
            this.ClipToBounds = true;
            this.Background = StatusColors.StatusBarBackground();
            this.Orientation = Orientation.Horizontal;
        }

        public void SubscribePanel_Event(Guid newGuid)
        {
            if (this.IsVisible)
            {
                var newPanel = new MarginPanel(_panels, newGuid);
                this.Children.Add(newPanel);
                _panels.Add(newPanel.PanelID, newPanel);
            }
        }

        public async void UnsubscribePanel_Event(Guid panelGuid)
        {
            if (_panels.ContainsKey(panelGuid))
            {
                var panel = _panels[panelGuid];
                await panel.RemoveThisPanelFromParentAsync();
            }
        }

        public void UpdatePanel_Event(Guid panelGuid, string text, SolidColorBrush backgroundColor, bool showLoading = false)
        {
            if (this.IsVisible)
            {
                if (_panels.ContainsKey(panelGuid))
                {
                    var panel = _panels[panelGuid];
                    panel.UpdatePanel(text, backgroundColor, showLoading);
                }
            }
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
                foreach (var panel in _panels.Values)
                    panel.RemoveThisPanelFromParentAsync();
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
