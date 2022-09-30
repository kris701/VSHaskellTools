using HaskellTools.Options;
using Microsoft.VisualStudio.Text.Editor;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace HaskellTools.Editor
{
    public enum GHCiRunningState { None, Running, Finished, Failed };
    

    public class HaskellEditorMargin : StackPanel, IWpfTextViewMargin
    {
        private delegate void ChangeRunningStatusHandler(GHCiRunningState toStatus, string message);
        private static event ChangeRunningStatusHandler ChangeRunningStatusEvent;

        public const string MarginName = "Haskell Editor Margin";

        private bool isDisposed;
        private Label _statusLabel;
        private Label _messageLabel;

        public HaskellEditorMargin(IWpfTextView textView)
        {
            ChangeRunningStatusEvent += ChangeRunningStatus_Event;
            this.Height = 30;
            this.ClipToBounds = true;
            this.Background = new SolidColorBrush(Colors.Gray);
            this.Orientation = Orientation.Horizontal;
            _statusLabel = new Label
            {
                Content = "",
                HorizontalAlignment = HorizontalAlignment.Center
            };
            this.Children.Add(new Separator()
            {
                Width = 30,
                Height = 0,
                HorizontalAlignment = HorizontalAlignment.Center
            });
            this.Children.Add(_statusLabel);
            _messageLabel = new Label
            {
                Content = "Waiting for execution...",
                HorizontalAlignment = HorizontalAlignment.Center
            };
            this.Children.Add(_messageLabel);
        }

        public static void ChangeRunningStatus(GHCiRunningState toStatus, string message)
        {
            if (ChangeRunningStatusEvent != null)
                ChangeRunningStatusEvent.Invoke(toStatus, message);
        }

        private void ChangeRunningStatus_Event(GHCiRunningState toStatus, string message)
        {
            switch (toStatus)
            {
                case GHCiRunningState.None:
                    _statusLabel.Content = "";
                    _messageLabel.Content = message;
                    this.Background = new SolidColorBrush(Colors.Gray);
                    break;
                case GHCiRunningState.Running:
                    _statusLabel.Content = "Running...";
                    _messageLabel.Content = message;
                    this.Background = new SolidColorBrush(Colors.LightGreen);
                    break;
                case GHCiRunningState.Failed:
                    _statusLabel.Content = "Run aborted!";
                    _messageLabel.Content = message;
                    this.Background = new SolidColorBrush(Colors.LightPink);
                    break;
                case GHCiRunningState.Finished:
                    _statusLabel.Content = "Run Finished!";
                    _messageLabel.Content = message;
                    this.Background = new SolidColorBrush(Colors.Gray);
                    break;
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
