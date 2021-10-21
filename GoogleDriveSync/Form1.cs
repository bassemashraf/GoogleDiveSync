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
        static UserCredential credential;

        DriveService service ;
        

     
       
        public Form1()
        {
            InitializeComponent();
           

        }


        static string ApplicationName = "certain-tangent-329512";   
        private void button1_Click(object sender, EventArgs e)
        {
            credential = GetCardianlities();

            service = new DriveService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });

            var watcher = new FileSystemWatcher(@"E:\work\LocalDrive");

            watcher.NotifyFilter = NotifyFilters.Attributes
                                 | NotifyFilters.CreationTime
                                 | NotifyFilters.DirectoryName
                                 | NotifyFilters.FileName
                                 | NotifyFilters.LastAccess
                                 | NotifyFilters.LastWrite
                                 | NotifyFilters.Security
                                 | NotifyFilters.Size;

            watcher.Changed += OnChanged;
            watcher.Created += OnCreated;
            watcher.Deleted += OnDeleted;
            watcher.Renamed += OnRenamed;



            watcher.IncludeSubdirectories = true;
            watcher.EnableRaisingEvents = true;

            Console.WriteLine("Press enter to exit.");
            Console.ReadLine();



            //FolderBrowserDialog FBD = new FolderBrowserDialog();
            //if (FBD.ShowDialog() == DialogResult.OK)
            //{


            //    String[] files = Directory.GetFiles(@"D:\LiveRepReportsWithSSIS\SBS");
            //    foreach (string file in files)
            //    {
            //        listBox1.Items.Add(credential);
            //    }

            //}



           
            //FilesResource.ListRequest listRequest = service.Files.List();
            //listRequest.PageSize = 10;
            //listRequest.Fields = "nextPageToken, files(id, name)";
            //IList<Google.Apis.Drive.v3.Data.File> files = listRequest.Execute()
            //   .Files;
            //Console.WriteLine("Files:");
            //if (files != null && files.Count > 0)
            //{
            //    foreach (var file in files)
            //    {
            //        listBox1.Items.Add(file.Name);
            //    }
            //}
            //else
            //{
            //    Console.WriteLine("No files found.");
            //}


            //var response = service.Changes.GetStartPageToken().Execute();
            //Console.WriteLine("Start token: " + response.StartPageTokenValue);

            //string pageToken = response.StartPageTokenValue;
            //while (pageToken != null)
            //{
            //    var request = service.Changes.List(pageToken);
            //    request.Spaces = "drive";
            //    var changes = request.Execute();
            //    foreach (var change in changes.Changes)
            //    {
            //        // Process change
            //        listBox1.Items.Add("Change found for file: " + change.File.Name.ToString());
            //      //Console.WriteLine("Change found for file: " + change.FileId);
            //    }
            //    if (changes.NewStartPageToken != null)
            //    {
            //        // Last page, save this token for the next polling interval
            //        pageToken = changes.NewStartPageToken;

            //    }
            //    pageToken = changes.NextPageToken;
            //}



        }
        private static void OnChanged(object sender, FileSystemEventArgs e)
        {
            if (e.ChangeType != WatcherChangeTypes.Changed)
            {
                return;
            }
            Console.WriteLine($"Changed: {e.FullPath}");
        }

        private  void OnCreated(object sender, FileSystemEventArgs e)
        {
            string value = $"Created: {e.FullPath}";
            Console.WriteLine(value);
            Console.WriteLine(GetMimeType(e.FullPath));
           //uploadFile(service, e.FullPath);
            createnewfolder(service, e.FullPath);
        }

        private static void OnDeleted(object sender, FileSystemEventArgs e) =>
            Console.WriteLine($"Deleted: {e.FullPath}");

        private static void OnRenamed(object sender, RenamedEventArgs e)
        {
            Console.WriteLine($"Renamed:");
            Console.WriteLine($"    Old: {e.OldFullPath}");
            Console.WriteLine($"    New: {e.FullPath}");
        }





        public void createnewfolder(DriveService service, string _uploadFile)
        {
          

            Google.Apis.Drive.v3.Data.File FileMetaData = new Google.Apis.Drive.v3.Data.File();
            FileMetaData.Name = System.IO.Path.GetFileName(_uploadFile);
            FileMetaData.MimeType = "application/vnd.google-apps.folder";
            FileMetaData.Parents = new List<string>
                {
                   "1aNidvO2hxTqsSN-_7QKYz4CAZTdTJ3Kd"
                };
            Google.Apis.Drive.v3.FilesResource.CreateRequest request;

            request = service.Files.Create(FileMetaData);
            request.Fields = "id";
            var file = request.Execute();
            Console.WriteLine(file.Id.ToString());
        }

        public  void uploadFile(DriveService service, string _uploadFile)
        {
            if (true)
            {
                
                Google.Apis.Drive.v3.Data.File body = new Google.Apis.Drive.v3.Data.File();
                body.Name = System.IO.Path.GetFileName(_uploadFile);

                body.MimeType = GetMimeType(_uploadFile);//"application/vnd.google-apps.folder";// GetMimeType(_uploadFile);
                body.Parents = new List<string>
                {
                   "1aNidvO2hxTqsSN-_7QKYz4CAZTdTJ3Kd"
                };
                try
                {




                    var stream = new System.IO.FileStream(_uploadFile, System.IO.FileMode.Open);
                                        //var stream = new FileStream(_uploadFile, FileMode.OpenOrCreate, FileAccess.ReadWrite);



                     var request = service.Files.Create(body, stream, GetMimeType(_uploadFile));
                    request.Fields = "*";
                    // You can bind event handler with progress changed event and response recieved(completed event)
                 
                    var result =request.Upload();
                    Console.WriteLine(result.Status.ToString());
               

                    //if (result.Exception.Message )
                    //Console.WriteLine(result.Exception.Message);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message+"Error Occured");
                   
                }
            }
            else
            {
                Console.WriteLine("succesyukrjt");
                
            }
        }



        private void Request_ProgressChanged(Google.Apis.Upload.IUploadProgress obj)



        {
            Console.WriteLine(obj.Status + " " + obj.BytesSent);
        }

        private void Request_ResponseReceived(Google.Apis.Drive.v3.Data.File obj)
        {
            if (obj != null)
            {
              Console.WriteLine(obj.Id.ToString());
            }
        }



        public UserCredential GetCardianlities()
        {
            UserCredential credential;
            string[] Scopes = {DriveService.Scope.Drive, DriveService.Scope.DriveFile};
            using (var stream = new FileStream("client_secret.json", FileMode.Open, FileAccess.ReadWrite))
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



        private string GetMimeType(string fileName)
        {
            string mimeType = "application /unknown";
            string ext = System.IO.Path.GetExtension(fileName).ToLower();
            Microsoft.Win32.RegistryKey regKey = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(ext);
            if (regKey != null && regKey.GetValue("Content Type") != null)
                mimeType = regKey.GetValue("Content Type").ToString();
            return mimeType;
        }


    }
}
