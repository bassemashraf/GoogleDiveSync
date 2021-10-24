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

            //var watcher = new FileSystemWatcher(@"D:\LiveRepReportsWithSSIS\SBS\LocalDrive");

            //watcher.NotifyFilter = NotifyFilters.Attributes
            //                     | NotifyFilters.CreationTime
            //                     | NotifyFilters.DirectoryName
            //                     | NotifyFilters.FileName
            //                     | NotifyFilters.LastAccess
            //                     | NotifyFilters.LastWrite
            //                     | NotifyFilters.Security
            //                     | NotifyFilters.Size;

            //watcher.Changed += OnChanged;
            //watcher.Created += OnCreated;
            //watcher.Deleted += OnDeleted;
            //watcher.Renamed += OnRenamed;



            //watcher.IncludeSubdirectories = true;
            //watcher.EnableRaisingEvents = true;

            //Console.WriteLine("Press enter to exit.");
            //Console.ReadLine();



            //FolderBrowserDialog FBD = new FolderBrowserDialog();
            //if (FBD.ShowDialog() == DialogResult.OK)
            //{


            //    String[] files = Directory.GetFiles(@"D:\LiveRepReportsWithSSIS\SBS");
            //    foreach (string file in files)
            //    {
            //        listBox1.Items.Add(credential);
            //    }

            //}










        }
        private  void OnChanged(object sender, FileSystemEventArgs e)
        {
            if (e.ChangeType != WatcherChangeTypes.Changed)
            {
               
                return;
            }
            Console.WriteLine($"Changed: {e.FullPath}");
            FileAttributes attr = System.IO.File.GetAttributes(e.FullPath);
            //detect whether its a directory or file
            if ((attr & FileAttributes.Directory) != FileAttributes.Directory)
            {
                 
                uploadFile(service, e.FullPath, getperants(e.FullPath)[0]);
            }


        }

        private  void OnCreated(object sender, FileSystemEventArgs e)
        {
            string value = $"Created: {e.FullPath}";
            Console.WriteLine(value);
            //get the file attributes for file or directory
            FileAttributes attr = System.IO.File.GetAttributes(e.FullPath);
            //detect whether its a directory or file
            if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
            {
                createnewfolder(service, e.FullPath, getperants(e.FullPath)[0]);
                Console.WriteLine(getperants(e.FullPath)[0]);
            }
            else
                uploadFile(service, e.FullPath, getperants(e.FullPath)[0]);


            // uploadFile(service, e.FullPath ,getperants(e.FullPath)[0]);
            // createnewfolder(service, e.FullPath);
            //uploadFile(service, e.FullPath);

        }

        private static void OnDeleted(object sender, FileSystemEventArgs e) =>
            Console.WriteLine($"Deleted: {e.FullPath}");

        private static void OnRenamed(object sender, RenamedEventArgs e)
        {
            Console.WriteLine($"Renamed:");
            Console.WriteLine($"    Old: {e.OldFullPath}");
            Console.WriteLine($"    New: {e.FullPath}");
        }





        public void createnewfolder(DriveService service, string _uploadFile , String parentfolder)
        {

            String id = getfolderid(parentfolder);

            Google.Apis.Drive.v3.Data.File FileMetaData = new Google.Apis.Drive.v3.Data.File();
            FileMetaData.Name = System.IO.Path.GetFileName(_uploadFile);
            FileMetaData.MimeType = "application/vnd.google-apps.folder" ;
            FileMetaData.Parents = new List<string>
                {
                   id
                };
            Google.Apis.Drive.v3.FilesResource.CreateRequest request;

            request = service.Files.Create(FileMetaData);
            request.Fields = "id";
            var file = request.Execute();
        }

        public  void uploadFile(DriveService service, string _uploadFile  ,String parentfolder)
        {



            String id = getfolderid(parentfolder);
            


            Google.Apis.Drive.v3.Data.File body = new Google.Apis.Drive.v3.Data.File();
                body.Name = System.IO.Path.GetFileName(_uploadFile);

                body.MimeType = GetMimeType(_uploadFile);//"application/vnd.google-apps.folder";// GetMimeType(_uploadFile);
                body.Parents = new List<string>
                {
                  id
                };
                try
                {
                  
                  



                    var stream = new System.IO.FileStream(_uploadFile, System.IO.FileMode.Open);
                                        //var stream = new FileStream(_uploadFile, FileMode.OpenOrCreate, FileAccess.ReadWrite);


                     
                     var request = service.Files.Create(body, stream, GetMimeType(_uploadFile));
                    request.Fields = "*";
                 
                    var result =request.Upload();
  
               
       
                }
                catch 
                {
                  
                   
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

        private void button2_Click(object sender, EventArgs e)
        {
        
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
                    //listBox1.Items.Add("Change found for file: " + change.File.Name.ToString());
                    Console.WriteLine("Change found for file: " + change.File);
                    Console.WriteLine("Change found for file: " + change.File.Parents[0]);
                    Console.WriteLine("Change found for file: " + change.Type + "for " + change.File.Name);
                    DownloadFile(service, change.File, @"D:\LiveRepReportsWithSSIS\SBS\LocalDrive\"+change.File.Parents.ToString()+change.File.Name);
                }
                if (changes.NewStartPageToken != null)
                {
                    // Last page, save this token for the next polling interval
                    pageToken = changes.NewStartPageToken;

                }
                pageToken = changes.NextPageToken;
                
            }

            





        }











        private  void DownloadFile(Google.Apis.Drive.v3.DriveService service, Google.Apis.Drive.v3.Data.File file, string saveTo)
        {

            var request = service.Files.Get(file.Id);
            var stream = new System.IO.MemoryStream();

            // Add a handler which will be notified on progress changes.
            // It will notify on each chunk download and when the
            // download is completed or failed.
            request.MediaDownloader.ProgressChanged += (Google.Apis.Download.IDownloadProgress progress) =>
            {
                switch (progress.Status)
                {
                    case Google.Apis.Download.DownloadStatus.Downloading:
                        {
                            Console.WriteLine(progress.BytesDownloaded);
                            break;
                        }
                    case Google.Apis.Download.DownloadStatus.Completed:
                        {
                            Console.WriteLine("Download complete.");
                            SaveStream(stream, saveTo);
                            break;
                        }
                    case Google.Apis.Download.DownloadStatus.Failed:
                        {
                            Console.WriteLine("Download failed.");
                            break;
                        }
                }
            };
            request.Download(stream);

        }
        private static void SaveStream(System.IO.MemoryStream stream, string saveTo)
        {
            using (System.IO.FileStream file = new System.IO.FileStream(saveTo, System.IO.FileMode.Create, System.IO.FileAccess.Write))
            {
                stream.WriteTo(file);
            }
        }





        private List<string> getperants( string filepath ) 
        {
            List<string> perants= new List<string>();
            DirectoryInfo di = new DirectoryInfo(filepath);
            while (true)
            {
                di = di.Parent;
                if (di.Name == "SBS")
                {
                   
                    break;
                }
                perants.Add(di.Name);
            }
            return perants; 
        }





        private List<string> listfilesfromfolder(String FolderNAme)
        {
            FilesResource.ListRequest listRequest = service.Files.List();
            listRequest.Fields = "nextPageToken, files(id, name)";
            List<string> folders = new List<string>();
            string id = getfolderid(FolderNAme);

            listRequest.Q = "mimeType = 'application/vnd.google-apps.folder' and '" + id + "' in parents";
            IList<Google.Apis.Drive.v3.Data.File> files = listRequest.Execute()
               .Files;
            Console.WriteLine("Files:");
            if (files != null && files.Count > 0)
            {
                foreach (var file in files)
                {
                    folders.Add(file.Name);
                }
            }
            else
            {
                Console.WriteLine("No files found.");
            }

            return folders;
        }




        private string getfolderid( string foldername ) 
        {
        FilesResource.ListRequest listRequest = service.Files.List();
        listRequest.Q = "mimeType = 'application/vnd.google-apps.folder' and name = '"+foldername+"'";
        listRequest.Fields = "nextPageToken, files(id, name)";

        IList<Google.Apis.Drive.v3.Data.File> files = listRequest.Execute()
        .Files;
            String id = " ";
            if (files != null && files.Count > 0)
            {
                foreach (var file in files)
                {   
                    id = file.Id;
                }
            }
            else 
            {
                return "File Not Found";
            }
            return id;

        }
       
    }
}
