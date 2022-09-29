using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Controls;

namespace HaskellTools
{
    public partial class WelcomeWindowControl : UserControl
    {
        private static string ReadmeURL = "https://github.com/kris701/VSHaskellTools/blob/master/README.md";
        private static string ReadmeStart = "<article class=\"markdown-body entry-content container-lg\" itemprop=\"text\">";
        private static string ReadmeEnd = "</article>";

        public WelcomeWindowControl()
        {
            this.InitializeComponent();
        }

        private async void WelcomeWindow_Loaded(object sender, RoutedEventArgs e)
        {
            using (var client = new WebClient())
            {
                var res = await client.DownloadDataTaskAsync(new System.Uri(ReadmeURL));
                var rawHtml = System.Text.Encoding.Default.GetString(res);
                var sub1 = rawHtml.Substring(rawHtml.IndexOf(ReadmeStart) + ReadmeStart.Length);
                var sub2 = sub1.Substring(0, sub1.IndexOf(ReadmeEnd));

                var htmlText = "<html><body style='background-color:#1e1e1e;color:White'>" + sub2 + "</body></html>";
                BrowserView.NavigateToString(htmlText);
                BrowserView.Visibility = Visibility.Visible;
            }
        }
    }
}