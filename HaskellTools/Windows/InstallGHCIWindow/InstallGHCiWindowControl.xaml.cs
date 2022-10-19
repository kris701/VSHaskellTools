using HaskellTools.Checkers;
using HaskellTools.Options;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;

namespace HaskellTools
{
    public partial class InstallGHCiWindowControl : UserControl
    {
        public InstallGHCiWindowControl()
        {
            this.InitializeComponent();
        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        private async void CheckAgainButton_Click(object sender, RoutedEventArgs e)
        {
            var checker = new GHCiChecker();
            SetPanelsVisibility(await checker.CheckForGHCiAsync());
        }

        private void HaskellGHCNotFound_Loaded(object sender, RoutedEventArgs e)
        {
            SetPanelsVisibility(OptionsAccessor.GHCiFound);
        }

        private void SetPanelsVisibility(bool isFound)
        {
            if (isFound)
            {
                FoundPanel.Visibility = Visibility.Visible;
                NotFoundPanel.Visibility = Visibility.Collapsed;
            }
            else
            {
                FoundPanel.Visibility = Visibility.Collapsed;
                NotFoundPanel.Visibility = Visibility.Visible;
            }
        }
    }
}