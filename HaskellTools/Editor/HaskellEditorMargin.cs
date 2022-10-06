using HaskellTools.Helpers;
using HaskellTools.Options;
using Microsoft.VisualStudio.Text.Editor;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace HaskellTools.Editor
{
    public enum GHCiRunningState { None, Running, Finished, Failed };
    public enum GHCiCheckingState { None, Checking, Finished, Failed };

    public class HaskellEditorMargin : StackPanel, IWpfTextViewMargin
    {
        private delegate void ChangeRunningStatusHandler(GHCiRunningState toStatus, string message);
        private delegate void ChangeCheckingStatusHandler(GHCiCheckingState toStatus, int errorCount);
        private static event ChangeRunningStatusHandler ChangeRunningStatusEvent;
        private static event ChangeCheckingStatusHandler ChangeCheckingStatusEvent;

        public const string MarginName = "Haskell Editor Margin";

        private bool isDisposed;

        private StackPanel _checkerStatusPanel;
        private StackPanel _statusPanel;

        private Label _statusLabel;
        private Label _checkerStatusLabel;
        private Label _messageLabel;
        private Label _loadingLabel;
        private Label _checkerLoadingLabel;
        private DispatcherTimer _loadingTimer = new DispatcherTimer();

        public HaskellEditorMargin(IWpfTextView textView)
        {
            ChangeRunningStatusEvent += ChangeRunningStatus_Event;
            ChangeCheckingStatusEvent += ChangeCheckingStatus_Event;

            _loadingTimer.Interval = TimeSpan.FromMilliseconds(100);
            _loadingTimer.Tick += LoadingLabel_Cycle;
            _loadingTimer.Stop();

            this.Height = 30;
            this.ClipToBounds = true;
            this.Background = new SolidColorBrush(Colors.Gray);

            _statusPanel = new StackPanel();
            _statusPanel.Margin = new Thickness(2);
            _statusPanel.Orientation = Orientation.Horizontal;

            _loadingLabel = new Label()
            {
                Content = "/",
                HorizontalAlignment = HorizontalAlignment.Center,
                Width = 30,
                Visibility = Visibility.Hidden
            };
            _loadingLabel.IsVisibleChanged += LoadingLabel_IsVisibileChanged;
            _statusPanel.Children.Add(_loadingLabel);
            _statusPanel.Children.Add(new Separator()
            {
                Width = 30,
                Height = 0,
                HorizontalAlignment = HorizontalAlignment.Center
            });
            _statusLabel = new Label
            {
                Content = "",
                HorizontalAlignment = HorizontalAlignment.Center,
                FontWeight = FontWeights.Bold
            };
            _statusPanel.Children.Add(_statusLabel);
            _messageLabel = new Label
            {
                Content = "Waiting for execution...",
                HorizontalAlignment = HorizontalAlignment.Center
            };
            _statusPanel.Children.Add(_messageLabel);

            this.Children.Add(_statusPanel);


            _checkerStatusPanel = new StackPanel();
            _checkerStatusPanel.Margin = new Thickness(2);
            _checkerStatusPanel.Orientation = Orientation.Horizontal;

            _checkerLoadingLabel = new Label()
            {
                Content = "/",
                HorizontalAlignment = HorizontalAlignment.Center,
                Width = 30,
                Visibility = Visibility.Hidden
            };
            _checkerStatusLabel = new Label
            {
                Content = "",
                HorizontalAlignment = HorizontalAlignment.Center,
                FontWeight = FontWeights.Bold
            };
            _checkerStatusPanel.Children.Add(_checkerStatusLabel);
            _checkerLoadingLabel.IsVisibleChanged += LoadingLabel_IsVisibileChanged;
            _checkerStatusPanel.Children.Add(_checkerLoadingLabel);

            this.Children.Add(_checkerStatusPanel);
        }

        public static void ChangeRunningStatus(GHCiRunningState toStatus, string message)
        {
            if (ChangeRunningStatusEvent != null)
                ChangeRunningStatusEvent.Invoke(toStatus, message);
        }

        public static void ChangeCheckingStatus(GHCiCheckingState toStatus, int errorCount)
        {
            if (ChangeCheckingStatusEvent != null)
                ChangeCheckingStatusEvent.Invoke(toStatus, errorCount);
        }

        private void ChangeRunningStatus_Event(GHCiRunningState toStatus, string message)
        {
            switch (toStatus)
            {
                case GHCiRunningState.None:
                    _loadingLabel.Visibility = Visibility.Hidden;
                    _statusLabel.Content = "";
                    _messageLabel.Content = message;
                    _statusPanel.Background = new SolidColorBrush(Colors.Gray);
                    break;
                case GHCiRunningState.Running:
                    _loadingLabel.Visibility = Visibility.Visible;
                    _statusLabel.Content = "Running...";
                    _messageLabel.Content = message;
                    _statusPanel.Background = new SolidColorBrush(Colors.LightGreen);
                    break;
                case GHCiRunningState.Failed:
                    _loadingLabel.Visibility = Visibility.Hidden;
                    _statusLabel.Content = "Run aborted!";
                    _messageLabel.Content = message;
                    _statusPanel.Background = new SolidColorBrush(Colors.LightPink);
                    break;
                case GHCiRunningState.Finished:
                    _loadingLabel.Visibility = Visibility.Hidden;
                    _statusLabel.Content = "Run Finished!";
                    _messageLabel.Content = message;
                    _statusPanel.Background = new SolidColorBrush(Colors.Gray);
                    break;
            }
        }

        private void ChangeCheckingStatus_Event(GHCiCheckingState toStatus, int errorCount)
        {
            switch (toStatus)
            {
                case GHCiCheckingState.None:
                    _checkerLoadingLabel.Visibility = Visibility.Hidden;
                    _checkerStatusLabel.Content = "";
                    _checkerStatusPanel.Background = new SolidColorBrush(Colors.Gray);
                    break;
                case GHCiCheckingState.Checking:
                    _checkerLoadingLabel.Visibility = Visibility.Visible;
                    _checkerStatusLabel.Content = "Checking document...";
                    _checkerStatusPanel.Background = new SolidColorBrush(Colors.BlueViolet);
                    break;
                case GHCiCheckingState.Failed:
                    _checkerLoadingLabel.Visibility = Visibility.Hidden;
                    _checkerStatusLabel.Content = $"Compile Errors: {errorCount}";
                    _checkerStatusPanel.Background = new SolidColorBrush(Colors.LightPink);
                    break;
                case GHCiCheckingState.Finished:
                    _checkerLoadingLabel.Visibility = Visibility.Hidden;
                    _checkerStatusLabel.Content = "Compile Errors: None";
                    _checkerStatusPanel.Background = new SolidColorBrush(Colors.Gray);
                    break;
            }
        }

        private void LoadingLabel_IsVisibileChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (_loadingLabel.IsVisible || _checkerLoadingLabel.IsVisible)
                _loadingTimer.Start();
            else
                _loadingTimer.Stop();
        }

        private void LoadingLabel_Cycle(object sender, EventArgs e)
        {
            switch (_loadingLabel.Content)
            {
                case "/":
                    _loadingLabel.Content = "-";
                    _checkerLoadingLabel.Content = "-";
                    break;
                case "-":
                    _loadingLabel.Content = "\\";
                    _checkerLoadingLabel.Content = "\\";
                    break;
                case "\\":
                    _loadingLabel.Content = "|";
                    _checkerLoadingLabel.Content = "|";
                    break;
                case "|":
                    _loadingLabel.Content = "/";
                    _checkerLoadingLabel.Content = "/";
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
