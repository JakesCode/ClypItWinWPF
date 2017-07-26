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
using System.IO;
using System.Web;
using System.Net.Http;
using System.Net.Http.Headers;


namespace ClypItWin
{
    /// <summary>
    /// Interaction logic for Uploading.xaml
    /// </summary>
    public partial class Uploading : Window
    {
        public bool authenticated = false;

        public Uploading(MainWindow.ClypSession Clyp, string FilePath)
        {
            InitializeComponent();
            try
            {
                username.Content = "Logged in as: " + Clyp.user.FirstName;
                authenticated = true;
                Task.Factory.StartNew(() => { string test = postToClypIt(Clyp, FilePath); });
            }
            catch
            {
                // Not logged in and should upload anonymously //
                // authenticated stays as false //
            }
        }

        private string postToClypIt(MainWindow.ClypSession Clyp, string FilePath)
        {
            using (var client = new HttpClient())
            {
                MultipartFormDataContent form = new MultipartFormDataContent();
                FileStream file = File.OpenRead(FilePath);
                var droppedFile = new ByteArrayContent(new StreamContent(file).ReadAsByteArrayAsync().Result);
                droppedFile.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/form-data; boundary=----WebKitFormBoundary7MA4YWxkTrZu0gW");

                client.DefaultRequestHeaders.TryAddWithoutValidation("postman-token", "f6065275-baf8-91a9-e816-188061ac03a1");
                client.DefaultRequestHeaders.TryAddWithoutValidation("cache-control", "no-cache");
                client.DefaultRequestHeaders.TryAddWithoutValidation("content-type", "multipart/form-data; boundary=----WebKitFormBoundary7MA4YWxkTrZu0gW");
                client.DefaultRequestHeaders.TryAddWithoutValidation("authorization", "Bearer " + Clyp.access_token);
                client.DefaultRequestHeaders.TryAddWithoutValidation("x-client-type", "WebAlfa");

                form.Add(droppedFile, "file", Path.GetFileName(FilePath));
                var response = client.PostAsync("https://upload.clyp.it/upload", form).Result;
                return response.Content.ReadAsStringAsync().Result;
            }
        }

        private void closeButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void saveButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
