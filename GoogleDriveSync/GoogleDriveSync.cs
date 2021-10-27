﻿using System;
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
using System.Configuration;


namespace GoogleDriveSync
{
    public partial class GoogleDriveSync : Form
    {
        static UserCredential Credential;

        DriveService Service;




        public GoogleDriveSync()
        {
            InitializeComponent();
        }

        static string ApplicationName = "GoogleDriveSync";
        private void StartLocalFileWatcherButton_Click(object sender, EventArgs e)
        {

            Credential = GetCardianlities();

            Service = new DriveService(new BaseClientService.Initializer
            {
                HttpClientInitializer = Credential,
                ApplicationName = ApplicationName,
            });
            String LocalFilePath = ConfigurationManager.AppSettings.Get("LocalFilePath").ToString(); 
            var watcher = new FileSystemWatcher(LocalFilePath); //Path of Local Folder  Which Sync with Drive 

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

        }
        private void OnChanged(object sender, FileSystemEventArgs e)
        {
            if (e.ChangeType != WatcherChangeTypes.Changed)
            {
                return;
            }

   
            //detect whether its a directory or file
            String FolderName = Path.GetFileName(e.FullPath);

            List<String> Parents = GetParentsFolders(e.FullPath);

            CreateNotFoundFolders(e.FullPath);

            if (!ListFilesFromFolder(Parents[0]).Contains(FolderName))
            {
                if (IsFolder(e.FullPath))
                {
                    CreateNewFolder(Service, FolderName, GetParentsFolders(e.FullPath)[0]);
                }
                else
                {
                    UploadFile(Service, e.FullPath, GetParentsFolders(e.FullPath)[0]);
                }
            }
            else
            {
                if (!IsFolder(e.FullPath))
                {
                    UpdateOldFile(e.FullPath, FolderName);
                }
            }

        }


        private void OnCreated(object sender, FileSystemEventArgs e)
        {
            String foldername = Path.GetFileName(e.FullPath);
      
            if (!ListFilesFromFolder(GetParentsFolders(e.FullPath)[0]).Contains(foldername))
            {
            
                FileAttributes attr = System.IO.File.GetAttributes(e.FullPath);
                //detect whether its a directory or file
                if (IsFolder(e.FullPath))
                {
                    CreateNewFolder(Service, foldername, GetParentsFolders(e.FullPath)[0]);
                }
                else
                {
                    UploadFile(Service, e.FullPath, GetParentsFolders(e.FullPath)[0]);
                }
            }

        }

        private void OnDeleted(object sender, FileSystemEventArgs e) =>
           Console.WriteLine($"Deleted: {e.FullPath}");

        private void OnRenamed(object sender, RenamedEventArgs e)
        {
            UpdateOldFile(e.FullPath, e.OldFullPath);
        }





        public void CreateNewFolder(DriveService service, String filename, String parentfolder)
        {


            String ID = GetFolderId(parentfolder);

            Google.Apis.Drive.v3.Data.File FileMetaData = new Google.Apis.Drive.v3.Data.File();
            FileMetaData.Name = filename;
            FileMetaData.MimeType = "application/vnd.google-apps.folder";
            FileMetaData.Parents = new List<string>
                {
                   ID
                };
            Google.Apis.Drive.v3.FilesResource.CreateRequest request;

            request = Service.Files.Create(FileMetaData);
            request.Fields = "id";
            var file = request.Execute();

        }



        public void UploadFile(DriveService service, string _uploadFile, String parentfolder)
        {
            String ID = GetFolderId(parentfolder);

            Google.Apis.Drive.v3.Data.File body = new Google.Apis.Drive.v3.Data.File();
            body.Name = System.IO.Path.GetFileName(_uploadFile);

            body.MimeType = GetMimeType(_uploadFile);
            body.Parents = new List<string>
                {
                  ID
                };
            try
            {
                var stream = new System.IO.FileStream(_uploadFile, System.IO.FileMode.Open);
                var request = service.Files.Create(body, stream, GetMimeType(_uploadFile));
                // request.Fields = "*";
                var result = request.Upload();
                stream.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);

            }


        }


        public UserCredential GetCardianlities()
        {
            UserCredential credential;
            string[] Scopes = {DriveService.Scope.Drive, DriveService.Scope.DriveFile,DriveService.Scope.DriveMetadata,DriveService.Scope.DriveAppdata,DriveService.Scope.DriveScripts
            };
            using (var stream = new FileStream("client_secret.json", FileMode.Open, FileAccess.ReadWrite))
            {

                string credPath = "token.json";
                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.FromStream(stream).Secrets, Scopes,
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



        private List<string> GetParentsFolders(string FilePath)
        {
            List<string> perants = new List<string>();
            DirectoryInfo di = new DirectoryInfo(FilePath);
            while (true)
            {
                di = di.Parent;
                String StopParent = ConfigurationManager.AppSettings.Get("StopParent").ToString();
                if (di.Name == StopParent) //The Parent Folder Of Local Folder Which Sync Whith Drive 
                {
                    break;
                }
                perants.Add(di.Name);
            }
            return perants;
        }


        private List<string> ListFilesFromFolder(String foldername)
        {
            FilesResource.ListRequest listRequest = Service.Files.List();
            listRequest.Fields = "nextPageToken, files(id, name)";
            List<string> folders = new List<string>();
            string id = GetFolderId(foldername);

            listRequest.Q = "'" + id + "' in parents AND trashed = false";
            IList<Google.Apis.Drive.v3.Data.File> files = listRequest.Execute()
               .Files;
            if (files != null && files.Count > 0)
            {
                foreach (var file in files)
                {
                    folders.Add(file.Name);
                }
            }
            else
            {
                return folders;
            }

            return folders;
        }




        private string GetFolderId(string foldername)
        {
            FilesResource.ListRequest listRequest = Service.Files.List();
            listRequest.Q = "mimeType = 'application/vnd.google-apps.folder' and name = '" + foldername + "' AND trashed = false";
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
                return id;
            }
            return id;
        }


        private void CreateNotFoundFolders(String filepath)
        {
            List<String> Parents = GetParentsFolders(filepath);

            if (Parents.Count > 1)
            {
                for (int i = Parents.Count; i-- > 0;)
                {
                    if (i == 0)
                        break;
                    if (!ListFilesFromFolder(Parents[i]).Contains(Parents[i - 1]))
                    {
                        CreateNewFolder(Service, Parents[i - 1], Parents[i]);
                    }

                }
            }


        }




       




        private string GetFileId(string path)
        {
            
            
            String FileName = Path.GetFileName(path);
            String MimeType = GetMimeType(FileName);
            FilesResource.ListRequest listRequest = Service.Files.List();
            listRequest.Q = "mimeType = '" + MimeType + "' and name = '" + FileName + "'";
            listRequest.Fields = "nextPageToken, files(id, name,parents)";
            IList<Google.Apis.Drive.v3.Data.File> files = listRequest.Execute()
            .Files;
            String id = "";
            if (files != null && files.Count > 0 )
            {
                foreach (var file in files)
                {
                    id = file.Id;
                    Console.WriteLine(file.Parents[0]);
                }
            }
            return id;

        }

        private bool IsFolder(String path)
        {
            FileAttributes attr = System.IO.File.GetAttributes(path);
            if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
                return true;
            else
                return  false;
        }




        void UpdateOldFile(String NewFilePath, String OldFilePath)
        {
            String fileId = "";
            Google.Apis.Drive.v3.Data.File updatedFileMetadata = new Google.Apis.Drive.v3.Data.File();
            updatedFileMetadata.Name = Path.GetFileName(NewFilePath);
            FilesResource.UpdateRequest updateRequest;
            if (IsFolder(NewFilePath))
                fileId = GetFolderId(Path.GetFileName(OldFilePath));
            else
                fileId = GetFileId(OldFilePath);
            updateRequest = Service.Files.Update(updatedFileMetadata, fileId);
            updateRequest.Execute();

        }

        private void StartDriveWatcherButton_Click(object sender, EventArgs e)
        {
            var response = Service.Changes.GetStartPageToken().Execute();
            Console.WriteLine("Start token: " + response.StartPageTokenValue);

            string pageToken = response.StartPageTokenValue;
            while (pageToken != null)
            {
                var request = Service.Changes.List(pageToken);
                request.Spaces = "Drive";
                request.Fields = "*";
                var changes = request.Execute();
                foreach (var change in changes.Changes)
                {
                    if (change.File.Parents.Contains(GetFolderId("LocalDrive")))
                        Console.WriteLine("ana fe localdrive");
                   
                    //DownloadFile(Service, change.File, @"D:\LiveRepReportsWithSSIS\SBS\LocalDrive\" + change.File.Parents.ToString() + change.File.Name);
                }
                if (changes.NewStartPageToken != null)
                {
                    // Last page, save this token for the next polling interval
                    pageToken = changes.NewStartPageToken;

                }
                pageToken = changes.NextPageToken;


            }
        }



        // The Way From Google Drive To Local Not Completed

        //private void StartDriveWatcherButton_Click(object sender, EventArgs e)
        //{
        //    var response = Service.Changes.GetStartPageToken().Execute();
        //    Console.WriteLine("Start token: " + response.StartPageTokenValue);

        //    string pageToken = response.StartPageTokenValue;
        //    while (pageToken != null)
        //    {
        //        var request = Service.Changes.List(pageToken);
        //        request.Spaces = "1aNidvO2hxTqsSN - _7QKYz4CAZTdTJ3Kd";
        //        var changes = request.Execute();
        //        foreach (var change in changes.Changes)
        //        {
        //            // Process change
        //            //listBox1.Items.Add("Change found for file: " + change.File.Name.ToString());
        //            Console.WriteLine("Change found for file: " + change.File);
        //            Console.WriteLine("Change found for file: " + change.File.Parents[0]);
        //            Console.WriteLine("Change found for file: " + change.Type + "for " + change.File.Name);
        //            //DownloadFile(Service, change.File, @"D:\LiveRepReportsWithSSIS\SBS\LocalDrive\" + change.File.Parents.ToString() + change.File.Name);
        //        }
        //        if (changes.NewStartPageToken != null)
        //        {
        //            // Last page, save this token for the next polling interval
        //            pageToken = changes.NewStartPageToken;

        //        }
        //        pageToken = changes.NextPageToken;

        //    }

        //}




        //private void DownloadFile(Google.Apis.Drive.v3.DriveService service, Google.Apis.Drive.v3.Data.File file, string saveTo)
        //{

        //    var request = service.Files.Get(file.Id);
        //    var stream = new System.IO.MemoryStream();

        //    // Add a handler which will be notified on progress changes.
        //    // It will notify on each chunk download and when the
        //    // download is completed or failed.
        //    request.MediaDownloader.ProgressChanged += (Google.Apis.Download.IDownloadProgress progress) =>
        //    {
        //        switch (progress.Status)
        //        {
        //            case Google.Apis.Download.DownloadStatus.Downloading:
        //                {
        //                    Console.WriteLine(progress.BytesDownloaded);
        //                    break;
        //                }
        //            case Google.Apis.Download.DownloadStatus.Completed:
        //                {
        //                    Console.WriteLine("Download complete.");
        //                    SaveStream(stream, saveTo);
        //                    break;
        //                }
        //            case Google.Apis.Download.DownloadStatus.Failed:
        //                {
        //                    Console.WriteLine("Download failed.");
        //                    break;
        //                }
        //        }
        //    };
        //    request.Download(stream);

        //}
        //private static void SaveStream(System.IO.MemoryStream stream, string saveTo)
        //{
        //    using (System.IO.FileStream file = new System.IO.FileStream(saveTo, System.IO.FileMode.Create, System.IO.FileAccess.Write))
        //    {
        //        stream.WriteTo(file);
        //    }
        //}







    }
}