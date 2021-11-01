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
        static UserCredential Credential = GetCardianlities();
        DriveService Service = new DriveService(new BaseClientService.Initializer
        {
            HttpClientInitializer = Credential,
            ApplicationName = ApplicationName,
        });
        static string ApplicationName = "GoogleDriveSync";

        String LocalFilePath = ConfigurationManager.AppSettings.Get("LocalFilePath").ToString();
        String WayOneAdd =ConfigurationManager.AppSettings.Get("WayOneAdd").ToString();
        String WayOneEdit=ConfigurationManager.AppSettings.Get("WayOneEdit").ToString();
        String WayOneDelete=ConfigurationManager.AppSettings.Get("WayOneDelete").ToString();
        String WayTwoAdd=ConfigurationManager.AppSettings.Get("WayTwoAdd").ToString();
        String WayTwoEdit=ConfigurationManager.AppSettings.Get("WayTwoEdit").ToString();
        String WayTwoDelete=ConfigurationManager.AppSettings.Get("WayTwoDelete").ToString();
        String DriveFolderName = ConfigurationManager.AppSettings.Get("DriveFolderName").ToString();
        String PathBeforeLocalFolder = ConfigurationManager.AppSettings.Get("PathBeforeLocalFolder").ToString();



        public GoogleDriveSync()
        {
            InitializeComponent();
            
        }
        public static UserCredential GetCardianlities()
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
            }
            return credential;
        }

        private void StartLocalFileWatcherButton_Click(object sender, EventArgs e)
        {
           
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
            if (e.ChangeType != WatcherChangeTypes.Changed){
                return;
            }
            if (Directory.Exists(e.FullPath))
            {
                //detect whether its a directory or file
                String FolderName = Path.GetFileName(e.FullPath);
                List<String> Parents = GetParentsFolders(e.FullPath);
                if (WayOneAdd == "enable")
                    CreateNotFoundFolders(e.FullPath);
                if (!ListFilesFromFolder(Parents[0]).Contains(FolderName))
                {
                    if (IsFolder(e.FullPath))
                    {
                        if(WayOneAdd=="enable")
                            CreateNewFolder(Service, FolderName, GetParentsFolders(e.FullPath)[0]);
                    }
                    else
                    {
                        if (WayOneAdd == "enable")
                            UploadFile(Service, e.FullPath, GetParentsFolders(e.FullPath)[0]);
                    }
                }
                else
                {
                    if (!IsFolder(e.FullPath))
                    {
                        if (WayOneEdit == "enable")
                            UpdateOldFile(e.FullPath, FolderName);
                    }
                }
            }
        }


        private void OnCreated(object sender, FileSystemEventArgs e)
        {
            if (WayOneAdd == "enable")
            {
                String foldername = Path.GetFileName(e.FullPath);

                if (!ListFilesFromFolder(GetParentsFolders(e.FullPath)[0]).Contains(foldername))
                {
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
        }

        private void OnDeleted(object sender, FileSystemEventArgs e)
        {
            if (WayOneDelete == "enable")
            {
                try
                {
                    Service.Files.Delete(GetFileId(e.FullPath)).Execute();
                }
                catch
                {
                    Service.Files.Delete(GetFolderId(Path.GetFileName(e.FullPath))).Execute();
                }
            }
        }
           

        private void OnRenamed(object sender, RenamedEventArgs e)

        {
            if (WayOneEdit == "enable")
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
            try{
                var stream = new System.IO.FileStream(_uploadFile, System.IO.FileMode.Open);
                var request = service.Files.Create(body, stream, GetMimeType(_uploadFile));
                // request.Fields = "*";
                var result = request.Upload();
                stream.Close();
            }catch (Exception e){
                Console.WriteLine(e.Message);
            }
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
                return folders;

            return folders;
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
                return id;
            
            return id;
        }


        private string GetFileId(string path)
        {

            String FileName = Path.GetFileName(path);
            String MimeType = GetMimeType(FileName);
            FilesResource.ListRequest listRequest = Service.Files.List();
            listRequest.Q = "mimeType = '" + MimeType + "' and name = '" + FileName + "'";
            listRequest.Fields = "nextPageToken, files(id, name)";
            IList<Google.Apis.Drive.v3.Data.File> files = listRequest.Execute()
            .Files;
            String id = "";
            if (files != null && files.Count > 0)
            {
                foreach (var file in files)
                {
                    id = file.Id;
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
                    return false;
        }


     

        //Way 2 From Drive To Local
        private void StartDriveWatcherButton_Click(object sender, EventArgs e)
        {
            var response = Service.Changes.GetStartPageToken().Execute();
            string pageToken = response.StartPageTokenValue;
            while (pageToken != null)
            {
                var request = Service.Changes.List(pageToken);
                request.Spaces = "Drive";
                request.Fields = "*";
                request.PageSize = 100;
                var changes = request.Execute();
                foreach (var change in changes.Changes)
                {
                    if (change.File is null || change.Removed == true) 
                    {
                        continue;
                    }
                    if (change.File.Trashed == false && !(change.File  is  null))
                    {
                        IList<string> ParentsList = GetALLParents(change.File.Name);
                        if (ParentsList.Contains(DriveFolderName))
                        {
                            String SaveTo = MapPathFromDriveTOLocal(ParentsList);
                            //Create Missed Folders
                            if (!Directory.Exists(SaveTo) && WayTwoAdd == "enable")
                                Directory.CreateDirectory(SaveTo);
                            //Handle If file Edited Or Added

                            if (System.IO.File.Exists(SaveTo + change.File.Name) && change.File.MimeType != "application/vnd.google-apps.folder" && WayTwoEdit =="enable")
                            {
                                System.IO.File.Delete(SaveTo + change.File.OriginalFilename);
                                DownloadFile(Service, change.File, SaveTo + change.File.Name);
                            }
                            else if (change.File.MimeType != "application/vnd.google-apps.folder" && WayTwoAdd=="enable")
                            {
                                DownloadFile(Service, change.File, SaveTo + change.File.Name);
                            }
                            //Handle Rename Files

                            if (change.File.OriginalFilename != null && change.File.Name != change.File.OriginalFilename && WayTwoEdit == "enable")
                            {
                                System.IO.File.Delete(SaveTo + change.File.OriginalFilename);
                                DownloadFile(Service, change.File, SaveTo + change.File.Name);
                            }
                            //Handle If Folder Added OR File 
                            if (!Directory.Exists(SaveTo + change.File.Name) && change.File.MimeType == "application/vnd.google-apps.folder" && WayTwoAdd=="enable")
                                Directory.CreateDirectory(SaveTo + change.File.Name);
                        }
                    }
                    else 
                    {
                        if(WayTwoDelete == "enable")
                            DeleteLocalFile(change.File.Name);
                    }
                    if (changes.NewStartPageToken != null)
                    {
                        // Last page, save this token for the next polling interval
                        pageToken = changes.NewStartPageToken;
                    }
                    pageToken = changes.NextPageToken;
                }
            }
        }


        private void DownloadFile(Google.Apis.Drive.v3.DriveService service, Google.Apis.Drive.v3.Data.File file, string saveTo)
        {
            var jetStream = new System.IO.MemoryStream();
            FilesResource.GetRequest request = new FilesResource.GetRequest(service, file.Id);

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
                           
                            using (System.IO.FileStream Lfile = new System.IO.FileStream(saveTo, System.IO.FileMode.Create, System.IO.FileAccess.Write))
                            {
                                jetStream.WriteTo(Lfile);
                            }
                            break;
                        }
                    case Google.Apis.Download.DownloadStatus.Failed:
                        {
                           
                            break;
                        }
                }
            };

            request.DownloadWithStatus(jetStream);
        }

        private string GetOneParent(string filename)
        {
            FilesResource.ListRequest listRequest = Service.Files.List();
            listRequest.Q = "trashed = false AND name = '" + filename +"'";
            listRequest.Fields = "*";
             string Parent = null;
            IList<Google.Apis.Drive.v3.Data.File> files = listRequest.Execute()
            .Files;
            if (files != null && files.Count > 0)
            {
                foreach (var file in files)
                {
                    Parent = file.Parents[0];
                }
            }
            else
            {
                return Parent;
            }
            return Parent;
        }


        private string MapPathFromDriveTOLocal(IList<String> Parents)
        {
            String LocalFilePath = PathBeforeLocalFolder;
            List<String> reversepath = Enumerable.Reverse(Parents).ToList();
            foreach (String name in reversepath)
            {
                LocalFilePath = LocalFilePath + name + @"\";
            }
            return LocalFilePath;
        }

        private List<string> GetALLParents(string filename)
        {
            List<string> Parents = new List<string>();
            String NextParentId = " ";
            while (filename != DriveFolderName)
            {
                NextParentId = GetOneParent(filename);
                filename = Service.Files.Get(NextParentId).Execute().Name;
                Parents.Add(filename);
                
            }
            return Parents;
        }

        private void DeleteLocalFile(String filename)
        {
            String  sDir = LocalFilePath; 
            string[] files = Directory.GetFiles(sDir, filename, SearchOption.AllDirectories);
            string[] directors = Directory.GetDirectories(sDir, filename, SearchOption.AllDirectories);
            try{
                try{
                    System.IO.File.Delete(files[0]);
                }catch{
                    Directory.Delete(directors[0]);
                }
            }catch {
                Console.WriteLine("File Not Created Before"); 
            }
           
        }
    }
}
