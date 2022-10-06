using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using System.Windows;

namespace HaskellTools.EditorMargins
{
    internal class MarginPanel : Border
    {
        public Guid PanelID { get; }

        private Dictionary<Guid, MarginPanel> _panels;

        private DispatcherTimer _clearTimer;
        private DispatcherTimer _loadingTimer;

        private Label LoadingLabel;
        private Label StatusLabel;
        private StackPanel MainPanel;

        public MarginPanel(Dictionary<Guid, MarginPanel> panels) : base()
        {
            _panels = panels;

            PanelID = Guid.NewGuid();

            this.Loaded += MarginPanel_Loaded;
            this.Opacity = 0;
        }

        private async void MarginPanel_Loaded(object sender, EventArgs e)
        {
            _clearTimer = new DispatcherTimer();
            _clearTimer.Interval = TimeSpan.FromSeconds(5);
            _clearTimer.Tick += RemoveFromParent;

            _loadingTimer = new DispatcherTimer();
            _loadingTimer.Interval = TimeSpan.FromMilliseconds(100);
            _loadingTimer.Tick += LoadingLabel_Tick;

            this.CornerRadius = new CornerRadius(10);
            this.BorderThickness = new Thickness(2);
            this.BorderBrush = new SolidColorBrush(Colors.Black);

            MainPanel = new StackPanel();
            MainPanel.Orientation = Orientation.Horizontal;
            MainPanel.Margin = new Thickness(2);
            MainPanel.Background = StatusColors.StatusItemNormalBackground();
            LoadingLabel = new Label()
            {
                Content = "/",
                HorizontalAlignment = HorizontalAlignment.Center,
                Width = 30,
                Visibility = Visibility.Hidden
            };
            LoadingLabel.IsVisibleChanged += LoadingLabel_IsVisibileChanged;
            MainPanel.Children.Add(LoadingLabel);
            StatusLabel = new Label()
            {
                Content = "",
                HorizontalAlignment = HorizontalAlignment.Center,
                FontWeight = FontWeights.Bold
            };
            MainPanel.Children.Add(StatusLabel);

            base.Child = MainPanel;
            _clearTimer.Start();

            for (double i = 0; i <= 1; i += 0.1)
            {
                await Task.Delay(15);
                this.Opacity = i;
            }
        }

        public void UpdatePanel(string text, SolidColorBrush backgroundColor, bool showLoading = false)
        {
            if (showLoading)
            {
                LoadingLabel.Visibility = Visibility.Visible;
                _clearTimer.Stop();
            }
            else
            {
                LoadingLabel.Visibility = Visibility.Hidden;
                _clearTimer.Start();
            }

            MainPanel.Background = backgroundColor;
            StatusLabel.Content = text;
        }

        private async void RemoveFromParent(object sender, EventArgs e)
        {
            await RemoveThisPanelFromParentAsync();
        }

        private void LoadingLabel_IsVisibileChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (LoadingLabel.Visibility == Visibility.Visible)
                _loadingTimer.Start();
            else
                _loadingTimer.Stop();
        }

        private void LoadingLabel_Tick(object sender, EventArgs e)
        {
            switch (LoadingLabel.Content)
            {
                case "/":
                    LoadingLabel.Content = "-";
                    break;
                case "-":
                    LoadingLabel.Content = "\\";
                    break;
                case "\\":
                    LoadingLabel.Content = "|";
                    break;
                case "|":
                    LoadingLabel.Content = "/";
                    break;
            }
        }

        public async Task RemoveThisPanelFromParentAsync()
        {
            if (this.Parent is StackPanel parentPanel)
            {
                for (double i = 1; i >= 0; i -= 0.05)
                {
                    await Task.Delay(25);
                    this.Opacity = i;
                }

                _panels.Remove(PanelID);
                parentPanel.Children.Remove(this);
            }
        }
    }
}
