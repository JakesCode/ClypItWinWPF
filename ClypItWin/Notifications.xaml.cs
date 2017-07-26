using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Forms;
using Newtonsoft.Json;
using RestSharp;
using System.Threading;
using System.Net.Http;

namespace ClypItWin

{
    /// <summary>
    /// Interaction logic for Notifications.xaml
    /// </summary>
    public partial class Notifications : Window
    {
        public Notifications(MainWindow.ClypSession Clyp)
        {
            InitializeComponent();
            if(Clyp.access_token == null)
            {
                notLoggedInLabel.Visibility = Visibility.Visible;
            } else
            {
                notLoggedInLabel.Visibility = Visibility.Hidden;
                if(Clyp.user.NotificationsSummary.Count > 0)
                {
                    ClypNotifications Notifications = JsonConvert.DeserializeObject<ClypNotifications>(MainWindow.ClypQuery("https://api.clyp.it/me/notifications",
                        HttpMethod.Get,
                        new Dictionary<string, string>()
                        { { "authorization", "Bearer " + Clyp.access_token }, { "Content-Type", "application/x-www-form-urlencoded" }, { "x-client-type", "WebAlfa" } },
                        new Dictionary<string, string>()
                        { { "count", "5" } },
                        Clyp));
                    createNotifications(Notifications);
                }
                
            }

        }

        private void createNotifications(ClypNotifications Notifications)
        {
            int notificationNumber = 0;

            foreach (var notification in Notifications.Data)
            {
                Grid container = new Grid();
                container.Height = 42;
                container.Width = 260;
                if(notificationNumber > 0)
                {
                    container.Margin = new Thickness() { Top = 42 * notificationNumber };
                } else
                {
                    container.Margin = new Thickness() { Top = 0 };
                }
                notificationsCanvas.Children.Add(container);

                Rectangle backgroundBox = new Rectangle();
                backgroundBox.Fill = Brushes.White;
                backgroundBox.Height = container.Height;
                backgroundBox.Width = container.Width;
                container.Children.Add(backgroundBox);

                Border blackBorder = new Border();
                blackBorder.BorderBrush = Brushes.Black;
                blackBorder.BorderThickness = new Thickness(1);
                blackBorder.Height = container.Height - 1;
                blackBorder.Width = container.Width - 1;
                container.Children.Add(blackBorder);

                TextBlock details = new TextBlock();
                details.VerticalAlignment = VerticalAlignment.Center;
                details.FontFamily = new FontFamily("Raleway");
                details.FontSize = 13;
                details.Foreground = Brushes.Black;
                if(notification.Text.Length > 40)
                {
                    details.Text = notification.Text.Substring(0, 37) + "...";
                } else
                {
                    details.Text = notification.Text;
                }
                container.Children.Add(details);

                notificationNumber++;   
            }
        }

        private void closeButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        public class Datum
        {
            public string NotificationId { get; set; }
            public string DateCreated { get; set; }
            public bool Acknowledged { get; set; }
            public string ImageUrl { get; set; }
            public string Text { get; set; }
            public string TargetUrl { get; set; }
        }

        public class Paging
        {
        }

        public class ClypNotifications
        {
            public List<Datum> Data { get; set; }
            public int TotalCount { get; set; }
            public Paging Paging { get; set; }
        }
    }
}
