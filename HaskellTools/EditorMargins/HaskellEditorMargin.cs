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

namespace HaskellTools.Editor
{
    public class HaskellEditorMargin : StackPanel, IWpfTextViewMargin
    {
        private Dictionary<Guid, Border> _panels;
        private Dictionary<Guid, DispatcherTimer> _panelTimers;
        private List<Label> _loadingLabels;
        private string _loadingChar = "/";
        private DispatcherTimer _loadingTimer = new DispatcherTimer();

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
            _panels = new Dictionary<Guid, Border>();
            _panelTimers = new Dictionary<Guid, DispatcherTimer>();
            _loadingLabels = new List<Label>();
            UpdatePanelEvent += UpdatePanel_Event;
            SubscribePanelEvent += SubscribePanel_Event;
            UnsubscribePanelEvent += UnsubscribePanel_Event;

            this.Height = 35;
            this.ClipToBounds = true;
            this.Background = StatusColors.StatusBarBackground();
            this.Orientation = Orientation.Horizontal;
            _loadingTimer.Interval = TimeSpan.FromMilliseconds(100);
            _loadingTimer.Tick += LoadingLabel_Cycle;
            _loadingTimer.Stop();
        }

        public static Guid SubscribePanel()
        {
            if (SubscribePanelEvent != null)
                return SubscribePanelEvent.Invoke();
            return Guid.Empty;
        }
        private Guid SubscribePanel_Event()
        {
            var newBorder = new Border();
            newBorder.CornerRadius = new CornerRadius(10);
            newBorder.BorderThickness = new Thickness(2);
            newBorder.BorderBrush = new SolidColorBrush(Colors.Black);

            var newPanel = new StackPanel();
            newPanel.Orientation = Orientation.Horizontal;
            newPanel.Margin = new Thickness(2);

            var loadingLabel = new Label()
            {
                Content = _loadingChar,
                HorizontalAlignment = HorizontalAlignment.Center,
                Width = 30,
                Visibility = Visibility.Hidden,
                Name = "LoadingLabel"
            };
            loadingLabel.IsVisibleChanged += LoadingLabel_IsVisibileChanged;
            newPanel.Children.Add(loadingLabel);
            _loadingLabels.Add(loadingLabel);

            var statusLabel = new Label()
            {
                Content = "",
                HorizontalAlignment = HorizontalAlignment.Center,
                FontWeight = FontWeights.Bold,
                Name = "StatusLabel"
            };
            newPanel.Children.Add(statusLabel);

            newBorder.Child = newPanel;

            var newGuid = Guid.NewGuid();
            _panels.Add(newGuid, newBorder);
            var newTimer = new DispatcherTimer();
            newTimer.Tag = newGuid;
            newTimer.Tick += RemovePanelEvent;
            newTimer.Interval = TimeSpan.FromSeconds(10);
            newTimer.Start();
            _panelTimers.Add(newGuid, newTimer);
            this.Children.Add(newBorder);
            return newGuid;
        }

        public static void UnsubscribePanel(Guid panelGuid)
        {
            if (UnsubscribePanelEvent != null)
                UnsubscribePanelEvent.Invoke(panelGuid);
        }
        private void UnsubscribePanel_Event(Guid panelGuid)
        {
            _panelTimers[panelGuid].Stop();
            _panelTimers.Remove(panelGuid);

            var panel = _panels[panelGuid];
            foreach (UIElement child in (panel.Child as StackPanel).Children)
            {
                if (child is Label label)
                {
                    if (label.Name == "LoadingLabel")
                    {
                        _loadingLabels.Remove(label);
                        break;
                    }
                }
            }
            this.Children.Remove(panel);
            _panels.Remove(panelGuid);
        }

        public static void UpdatePanel(Guid panelGuid, string text, SolidColorBrush backgroundColor, bool showLoading = false)
        {
            if (UpdatePanelEvent != null)
                UpdatePanelEvent.Invoke(panelGuid, text, backgroundColor, showLoading);
        }
        private void UpdatePanel_Event(Guid panelGuid, string text, SolidColorBrush backgroundColor, bool showLoading = false)
        {
            if (_panels.ContainsKey(panelGuid))
            {
                if (showLoading)
                    _panelTimers[panelGuid].Stop();
                else
                    _panelTimers[panelGuid].Start();
                var panel = _panels[panelGuid];
                panel.Background = backgroundColor;
                foreach (UIElement child in (panel.Child as StackPanel).Children)
                {
                    if (child is Label label)
                    {
                        if (label.Name == "LoadingLabel")
                        {
                            if (showLoading)
                                label.Visibility = Visibility.Visible;
                            else
                                label.Visibility = Visibility.Hidden;
                        }
                        else if (label.Name == "StatusLabel")
                        {
                            label.Content = text;
                        }
                    }
                }
            }
        }

        private void RemovePanelEvent(object sender, EventArgs e)
        {
            if (sender is DispatcherTimer timer)
                if (timer.Tag is Guid id)
                    UnsubscribePanel_Event(id);
        }

        private void LoadingLabel_IsVisibileChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            bool isAnyVisible = false;
            foreach(var label in _loadingLabels)
            {
                if (label.Visibility == Visibility.Visible)
                {
                    isAnyVisible = true;
                    break;
                }
            }
            if (isAnyVisible)
                _loadingTimer.Start();
            else
                _loadingTimer.Stop();
        }

        private void LoadingLabel_Cycle(object sender, EventArgs e)
        {
            switch (_loadingChar)
            {
                case "/":
                    _loadingChar = "-";
                    break;
                case "-":
                    _loadingChar = "\\";
                    break;
                case "\\":
                    _loadingChar = "|";
                    break;
                case "|":
                    _loadingChar = "/";
                    break;
            }
            foreach (var label in _loadingLabels)
            {
                if (label.Visibility == Visibility.Visible)
                {
                    label.Content = _loadingChar;
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
