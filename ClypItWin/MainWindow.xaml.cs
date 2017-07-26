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
using System.Net;
using System.Windows.Controls.Primitives;
using System.Net.Http;

namespace ClypItWin
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public ClypSession Clyp = new ClypSession();

        public MainWindow()
        {
            InitializeComponent();
        }

        public static string ClypQuery(string Url, HttpMethod Method, Dictionary<string, string> Headers, Dictionary<string, string> Parameters, ClypSession Clyp)
        {
            using (var client = new HttpClient())
            {
                var request = new HttpRequestMessage();
                request.RequestUri = new Uri(Url);
                request.Method = Method;

                request.Content = new FormUrlEncodedContent(Parameters);

                foreach (var header in Headers)
                {
                    request.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }

                var response = new HttpResponseMessage();

                if(Method == HttpMethod.Get)
                {
                    foreach (var header in Headers)
                    {
                        client.DefaultRequestHeaders.TryAddWithoutValidation(header.Key, header.Value);
                        return client.GetAsync(Url).Result.Content.ReadAsStringAsync().Result;
                    }
                } else
                {
                    var task = client.SendAsync(request)
                    .ContinueWith((taskWithMessage) =>
                    {
                        response = taskWithMessage.Result;
                    });
                    task.Wait();
                }

                if (response == new HttpResponseMessage())
                {
                    return "Failed";
                } else
                {
                    return response.Content.ReadAsStringAsync().Result;
                }         
            }
        }

        private void client_handleRequest(object sender, UploadStringCompletedEventArgs e)
        {

        }

        private void closeButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void notificationBell_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Notifications notificationsWindow = new Notifications(this.Clyp);
            notificationsWindow.Top = this.Top;
            notificationsWindow.Left = this.Left;
            notificationsWindow.ShowDialog();
        }

        private void settingsButton_Click(object sender, RoutedEventArgs e)
        {
            Settings settingsWindow = new Settings();
            settingsWindow.Top = this.Top;
            settingsWindow.Left = this.Left;
            settingsWindow.Clyp = Clyp;
            settingsWindow.ShowDialog();

            if(Clyp.login_attempt)
            {
                Clyp = JsonConvert.DeserializeObject<ClypSession>(ClypQuery("https://api.clyp.it/oauth2/token",
                    HttpMethod.Post,
                    new Dictionary<string, string>()
                    { { "Authorization", "Basic MjkzMTE5Og==" }, { "Content-Type", "application/x-www-form-urlencoded" } },
                    new Dictionary<string, string>()
                    { { "grant_type", "password" }, { "username", settingsWindow.usernameTextbox.Text }, { "password", settingsWindow.passwordTextbox.Password.ToString() } },
                    this.Clyp));
                Clyp.user = JsonConvert.DeserializeObject<ClypUser>(ClypQuery("https://api.clyp.it/me",
                    HttpMethod.Get,
                    new Dictionary<string, string>()
                    { { "authorization", "Bearer " + Clyp.access_token }, { "Content-Type", "application/x-www-form-urlencoded" }, { "x-client-type", "WebAlfa" } },
                    new Dictionary<string, string>(),
                    this.Clyp));
                username.Content = "Logged in as: " + Clyp.user.FirstName;

                if(Clyp.user.NotificationsSummary.Count > 0)
                {
                    BitmapImage unreadNotificationsBell = new BitmapImage();
                    unreadNotificationsBell.BeginInit();
                    unreadNotificationsBell.UriSource = new Uri("pack://application:,,,/ClypItWin;component/assets/notification_unread.png");
                    unreadNotificationsBell.EndInit();
                    notificationBell.Source = unreadNotificationsBell;
                    notificationBell.IsEnabled = true;
                    notificationsCircle.Visibility = Visibility.Visible;
                    numberOfNotifications.Visibility = Visibility.Visible;
                    numberOfNotifications.Content = Clyp.user.NotificationsSummary.Count;
                }

                while (Clyp.access_token == null && Clyp.login_attempt)
                {
                    settingsButton = null;
                    settingsButton_Click(new object(), new RoutedEventArgs());
                }
            }  
        }

        public class ClypSession
        {
            public bool login_attempt = false;
            public string access_token { get; set; }
            public int expires_in { get; set; }
            public string refresh_token { get; set; }
            public string token_type { get; set; }
            public ClypUser user { get; set; }
        }

        public class ClypUser
        {
            public string Biography { get; set; }
            public bool ContentAdministrator { get; set; }
            public string DefaultAudioFileStatus { get; set; }
            public string EmailAddress { get; set; }
            public bool EmailAddressVerified { get; set; }
            public bool EmailMarketingEnabled { get; set; }
            public string FirstName { get; set; }
            public NotificationsSummaryClass NotificationsSummary { get; set; }
            public string ProfilePictureUrl { get; set; }
            public string PublicProfileUrl { get; set; }
            public bool PublicProfileVisible { get; set; }
            public string SubscriptionState { get; set; }
            public double UploadSecondsRemaining { get; set; }
            public string UserId { get; set; }
            public string Website { get; set; }

            public class NotificationsSummaryClass
            {
                public int Count = 0;
            }
        }

        private void dragAndDropGrid_Drop(object sender, System.Windows.DragEventArgs e)
        {
            string[] FileList = (string[])e.Data.GetData(System.Windows.Forms.DataFormats.FileDrop, false);
            e.Handled = true;
            var files = FileList.ToList();
            Uploading uploadingWindow = new Uploading(this.Clyp, files[0]);
            uploadingWindow.Top = this.Top;
            uploadingWindow.Left = this.Left;
            uploadingWindow.Show();
        }
    }
}