using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System.Threading;


namespace GoogleDriveSync
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        static string ApplicationName = "certain-tangent-329512";   
        private void button1_Click(object sender, EventArgs e)
        {
            UserCredential credential;



            credential = GetCardianlities();

            //FolderBrowserDialog FBD = new FolderBrowserDialog();
            //if (FBD.ShowDialog() == DialogResult.OK)
            //{


            //    String[] files = Directory.GetFiles(@"D:\LiveRepReportsWithSSIS\SBS");
            //    foreach (string file in files)
            //    {
            //        listBox1.Items.Add(credential);
            //    }

            //}

            var service = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });
            //  FilesResource.ListRequest listRequest = service.Files.List();
            //  listRequest.PageSize = 10;
            //  listRequest.Fields = "nextPageToken, files(id, name)";
            //  IList<Google.Apis.Drive.v3.Data.File> files = listRequest.Execute()
            //     .Files;
            //  Console.WriteLine("Files:");
            //  if (files != null && files.Count > 0)
            //  {
            //      foreach (var file in files)
            //      {
            //          listBox1.Items.Add(file.Name);
            //      }
            //  }
            //  else
            //  {
            //      Console.WriteLine("No files found.");
            //  }


            var response = service.Changes.GetStartPageToken().Execute();
            Console.WriteLine("Start token: " + response.StartPageTokenValue);

            string pageToken = response.StartPageTokenValue;
            while (pageToken != null)
            {
                var request = service.Changes.List(pageToken);
                request.Spaces = "drive";
                var changes = request.Execute();
                foreach (var change in changes.Changes)
                {
                    // Process change
                    listBox1.Items.Add("Change found for file: " + change.File.Name.ToLower());
                  //Console.WriteLine("Change found for file: " + change.FileId);
                }
                if (changes.NewStartPageToken != null)
                {
                    // Last page, save this token for the next polling interval
                    pageToken = changes.NewStartPageToken;

                }
                pageToken = changes.NextPageToken;
            }


        }











        public UserCredential GetCardianlities()
        {
            UserCredential credential;
            string[] Scopes = { DriveService.Scope.DriveReadonly };
            using (var stream = new FileStream("credentials.json", FileMode.Open, FileAccess.Read))
            {

                string credPath = "token.json";
                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets, Scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;
                Console.WriteLine(credential);
              
            }
            return credential;

        }

        private void fileSystemWatcher1_Changed(object sender, FileSystemEventArgs e)
        {
            listBox1.Items.Add(e.FullPath.ToString());
            
        }
    }
}
