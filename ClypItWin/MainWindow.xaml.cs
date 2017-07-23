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
        private string ClypQuery(string Url, Method HttpMethod, Dictionary<string, string> Headers, Dictionary<string, string> Parameters)
        {
            var client = new RestClient(Url);
            var request = new RestRequest(HttpMethod);
            foreach (var header in Headers)
            {
                request.AddHeader(header.Key, header.Value);
            }
            foreach (var parameter in Parameters)
            {
                request.AddParameter(parameter.Key, parameter.Value);
            }
            IRestResponse response = client.Execute(request);
            return response.Content;

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
                Method.POST,
                new Dictionary<string, string>()
                { { "Authorization", "Basic MjkzMTE5Og==" }, { "Content-Type", "application/x-www-form-urlencoded" } },
                new Dictionary<string, string>()
                { { "grant_type", "password" }, { "username", settingsWindow.usernameTextbox.Text }, { "password", settingsWindow.passwordTextbox.Password.ToString() } }));
                Clyp.user = JsonConvert.DeserializeObject<ClypUser>(ClypQuery("https://api.clyp.it/me",
                    Method.GET,
                    new Dictionary<string, string>()
                    { { "authorization", "Bearer " + Clyp.access_token }, { "Content-Type", "application/x-www-form-urlencoded" }, { "x-client-type", "WebAlfa" } },
                    new Dictionary<string, string>()));
                username.Content = "Logged in as: " + Clyp.user.FirstName;

                while (Clyp.access_token == null && Clyp.login_attempt)
                {
                    settingsButton = null;
                    settingsButton_Click(new object(), new RoutedEventArgs());
                }
            }  
        }

        public class ClypSession
        {
            public string access_token { get; set; }
            public int expires_in { get; set; }
            public string refresh_token { get; set; }
            public bool login_attempt = false;
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
                public int Count { get; set; }
            }
        }

        private void closeButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void notificationBell_MouseDown(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Forms.MessageBox.Show("Test");
        }
    }
}