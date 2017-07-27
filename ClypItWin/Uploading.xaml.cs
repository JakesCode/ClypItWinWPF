using Microsoft.VisualStudio.Services.WebApi;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ClypItWin
{
    /// <summary>
    /// Interaction logic for Uploading.xaml
    /// </summary>
    public partial class Uploading : Window
    {
        public bool authenticated = false;
        public MainWindow.ClypSession Clyp;
        public string filepath = "";
        public string postResult = "";

        public Uploading(MainWindow.ClypSession Clyp, string FilePath)
        {
            InitializeComponent();
            this.Clyp = Clyp;
            this.filepath = FilePath;
            try
            {
                username.Content = "Logged in as: " + Clyp.user.FirstName;
                authenticated = true;
                notLoggedIn.Visibility = Visibility.Hidden;
            }
            catch
            {
                // Not logged in and should upload anonymously //
                // authenticated stays as false //
                notLoggedIn.Visibility = Visibility.Visible;
            }
        }

        public string patchClypItUpload(MainWindow.ClypSession Clyp, ClypUploadResponse Response)
        {
            using (var client = new HttpClient())
            {
                MultipartFormDataContent content = new MultipartFormDataContent();

                Dictionary<string, string> formData = new Dictionary<string, string>()
                {
                    { "title", trackTitle.Text },
                    { "description", trackDescription.Text }
                };

                if (publicButton.Foreground == new SolidColorBrush(Colors.White))
                {
                    // Must be public //
                    formData.Add("status", "Public");
                }
                else
                {
                    // Must be private then //
                    formData.Add("status", "Private");
                }

                client.DefaultRequestHeaders.TryAddWithoutValidation("postman-token", "f6065275-baf8-91a9-e816-188061ac03a1");
                client.DefaultRequestHeaders.TryAddWithoutValidation("cache-control", "no-cache");
                client.DefaultRequestHeaders.TryAddWithoutValidation("authorization", "Bearer " + Clyp.access_token);
                client.DefaultRequestHeaders.TryAddWithoutValidation("x-client-type", "WebAlfa");
                client.DefaultRequestHeaders.TryAddWithoutValidation("content-type", "application/x-www-form-urlencoded; charset=UTF-8");

                return client.PatchAsync("https://api.clyp.it/" + Response.AudioFileId, content).Result.Content.ReadAsStringAsync().Result;
            }
        }

        private void artworkDropZone_Drop(object sender, DragEventArgs e)
        {
            filepath = ((string[])e.Data.GetData(DataFormats.FileDrop, false))[0];
            if (filepath.EndsWith(".jpg") || filepath.EndsWith(".png"))
            {
                BitmapImage droppedArtwork = new BitmapImage();
                droppedArtwork.BeginInit();
                droppedArtwork.UriSource = new Uri(filepath);
                droppedArtwork.EndInit();
                artworkDropZone.Source = droppedArtwork;
                artworkLabel.Visibility = Visibility.Hidden;
            }
        }

        private void closeButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private string postToClypIt(MainWindow.ClypSession Clyp, string FilePath, bool auth)
        {
            using (var client = new HttpClient())
            {
                MultipartFormDataContent form = new MultipartFormDataContent();
                FileStream file = File.OpenRead(FilePath);
                var droppedFile = new ByteArrayContent(new StreamContent(file).ReadAsByteArrayAsync().Result);
                droppedFile.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/form-data; boundary=----WebKitFormBoundaryB27eCApzWWdpf4x3");

                client.DefaultRequestHeaders.TryAddWithoutValidation("postman-token", "f6065275-baf8-91a9-e816-188061ac03a1");
                client.DefaultRequestHeaders.TryAddWithoutValidation("cache-control", "no-cache");

                if (auth)
                {
                    client.DefaultRequestHeaders.TryAddWithoutValidation("authorization", "Bearer " + Clyp.access_token);
                }

                client.DefaultRequestHeaders.TryAddWithoutValidation("x-client-type", "WebAlfa");

                form.Add(droppedFile, "file", Path.GetFileName(FilePath));
                var response = client.PostAsync("https://upload.clyp.it/upload", form).Result;
                return response.Content.ReadAsStringAsync().Result;
            }
        }

        private void privateButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            privateButton.Foreground = new SolidColorBrush(Colors.White);
            publicButton.Foreground = new SolidColorBrush(Color.FromRgb(199, 199, 199));
        }

        private void publicButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            publicButton.Foreground = new SolidColorBrush(Colors.White);
            privateButton.Foreground = new SolidColorBrush(Color.FromRgb(199, 199, 199));
        }

        private void saveButton_Click(object sender, RoutedEventArgs e)
        {
            Task.Factory.StartNew(() => { postResult = postToClypIt(Clyp, filepath, false); }).Wait();
            ClypUploadResponse Response = JsonConvert.DeserializeObject<ClypUploadResponse>(postResult);
            if (Response.Successful)
            {
                Clipboard.SetText(patchClypItUpload(this.Clyp, Response));
                System.Windows.Forms.MessageBox.Show("Test");
            }
        }

        public class ClypUploadResponse
        {
            public string AudioFileId { get; set; }
            public string DateCreated { get; set; }
            public string Description { get; set; }
            public double Duration { get; set; }
            public string Mp3Url { get; set; }
            public string OggUrl { get; set; }
            public string PlaylistId { get; set; }
            public string PlaylistUploadToken { get; set; }
            public string SecureMp3Url { get; set; }
            public string SecureOggUrl { get; set; }
            public bool Successful { get; set; }
            public string Title { get; set; }
            public string Url { get; set; }
        }
    }
}