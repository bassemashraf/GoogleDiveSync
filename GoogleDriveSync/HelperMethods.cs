using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Google.Apis.Auth.OAuth2;
using System.Threading.Tasks;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System.Threading;
using System.Configuration;
using System.IO;

namespace GoogleDriveSync
{
    class HelperMethods
    {
        public static string GetMimeType(string fileName)
        {
            string mimeType = "application /unknown";
            string ext = System.IO.Path.GetExtension(fileName).ToLower();
            Microsoft.Win32.RegistryKey regKey = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(ext);
            if (regKey != null && regKey.GetValue("Content Type") != null)
                mimeType = regKey.GetValue("Content Type").ToString();
            return mimeType;
        }
        public static List<string> GetParentsFolders(string FilePath)
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
        public static List<string> ListFilesFromFolder(DriveService Service, String foldername)
        {
            List<string> folders = new List<string>();
            try
            {
                FilesResource.ListRequest listRequest = Service.Files.List();
                listRequest.Fields = "nextPageToken, files(id, name)";
                
                string id = GetFolderId(Service, foldername);
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
            }
            catch(Exception ex) 
            {
                ConfigManager.logger.Error(ex.Message + "  Function Name 'ListFilesFromFolder'");
            }
            return folders;
        }
        public static void CreateNotFoundFolders(DriveService Service, String filepath)
        {
            List<String> Parents = GetParentsFolders(filepath);

            if (Parents.Count > 1)
            {
                for (int i = Parents.Count; i-- > 0;)
                {
                    if (i == 0)
                        break;
                    if (!ListFilesFromFolder(Service, Parents[i]).Contains(Parents[i - 1]))
                    {
                        CreateNewFolder(Service, Parents[i - 1], Parents[i]);
                    }
                }
            }
        }
        public static void UpdateOldFile(DriveService Service, String NewFilePath, String OldFilePath)
        {
            String fileId = "";
            Google.Apis.Drive.v3.Data.File updatedFileMetadata = new Google.Apis.Drive.v3.Data.File();
            updatedFileMetadata.Name = Path.GetFileName(NewFilePath);
            try
            {
                FilesResource.UpdateRequest updateRequest;
                if (IsFolder(NewFilePath))
                    fileId = GetFolderId(Service, Path.GetFileName(OldFilePath));
                else
                    fileId = GetFileId(Service, OldFilePath);
                updateRequest = Service.Files.Update(updatedFileMetadata, fileId);
                updateRequest.Execute();
                ConfigManager.logger.Info("File"+ Path.GetFileName(OldFilePath) +"Updated");

            }
            catch (Exception ex) 
            {
                ConfigManager.logger.Error(ex.Message + "  Function Name 'UpdateOldFile'");
            }

        }
        public static string GetFolderId(DriveService Service, string foldername)
        {
            String id = " ";
            try
            {
                FilesResource.ListRequest listRequest = Service.Files.List();
                listRequest.Q = "mimeType = 'application/vnd.google-apps.folder' and name = '" + foldername + "' AND trashed = false";
                listRequest.Fields = "nextPageToken, files(id, name)";
                IList<Google.Apis.Drive.v3.Data.File> files = listRequest.Execute()
                .Files;
                if (files != null && files.Count > 0)
                {
                    foreach (var file in files)
                    {
                        id = file.Id;
                    }
                }
                else
                    return id;
            }
            catch(Exception ex)
            {
               
                ConfigManager.logger.Error(ex.Message + "  Function Name 'GetFolderId'");
            }
            return id;
        }
        public static string GetFileId(DriveService Service, string path)
        {
            String id = "";
            try
            {
                String FileName = Path.GetFileName(path);
                String MimeType = GetMimeType(FileName);
                FilesResource.ListRequest listRequest = Service.Files.List();
                listRequest.Q = "mimeType = '" + MimeType + "' and name = '" + FileName + "'";
                listRequest.Fields = "nextPageToken, files(id, name)";
                IList<Google.Apis.Drive.v3.Data.File> files = listRequest.Execute()
                .Files;

                if (files != null && files.Count > 0)
                {
                    foreach (var file in files)
                    {
                        id = file.Id;
                    }
                }
            }
            catch (Exception ex) 
            {
                ConfigManager.logger.Error(ex.Message + "  Function Name 'GetFileId'");
                
            }
            return id;
        }
        public static bool IsFolder(String path)
        {
            FileAttributes attr = System.IO.File.GetAttributes(path);
            if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
                return true;
            else
                return false;
        }
        public static void CreateNewFolder(DriveService Service, String filename, String parentfolder)
        {
            try
            {
                String ID = GetFolderId(Service, parentfolder);
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
                ConfigManager.logger.Info("File" + filename + " created");
            }
            catch (Exception ex) 
            {
                ConfigManager.logger.Error(ex.Message + " Function Name CreateNewFolder");
            }
        }
        public static Google.Apis.Drive.v3.Data.File GetFileWithName(DriveService Service, string path)
        {
            try
            {
                String FileName = Path.GetFileName(path);
                String MimeType = GetMimeType(FileName);
                FilesResource.ListRequest listRequest = Service.Files.List();
                listRequest.Q = "mimeType = '" + MimeType + "' and name = '" + FileName + "'";
                listRequest.Fields = "*";
                IList<Google.Apis.Drive.v3.Data.File> files = listRequest.Execute()
                .Files;
                Google.Apis.Drive.v3.Data.File drivefile = null;
                if (files != null && files.Count > 0)
                {
                    foreach (var file in files)
                    {
                        drivefile = file;

                    }
                }
                return drivefile;
            }catch (Exception ex)
            {
                ConfigManager.logger.Error(ex.Message);
                return null; 
            }
        }
        public static void UploadFile(DriveService service, string _uploadFile, String parentfolder)
        {
            String ID = GetFolderId(service, parentfolder);

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
                ConfigManager.logger.Info("File" + System.IO.Path.GetFileName(_uploadFile) + "Uploaded" );
            }
            catch (Exception e)
            {
               ConfigManager.logger.Error(e.Message + " Function Name UploadFile");
            }
        }
        public static void OnChanged(object sender, FileSystemEventArgs e)
        {
            LocalToCloudSync.UpdateFiles(DriveConnect.Service, e.FullPath);
        }
        public static void OnCreated(object sender, FileSystemEventArgs e)
        {
            LocalToCloudSync.CreateFiles(DriveConnect.Service, e.FullPath);
        }
        public static void OnDeleted(object sender, FileSystemEventArgs e)
        {
            LocalToCloudSync.DeleteFiles(DriveConnect.Service, e.FullPath);
        }
        public static void OnRenamed(object sender, RenamedEventArgs e)

        {
            if (ConfigManager.WayOneEdit == "enable")
                UpdateOldFile(DriveConnect.Service, e.FullPath, e.OldFullPath);
        }
        public static void StartLocalFileWatcher()
        {
            //Start watcher 
            var watcher = new FileSystemWatcher(ConfigManager.LocalFilePath); //Path of Local Folder  Which Sync with Drive 

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

        public static string GetOneParent(DriveService service,string filename)
        {
            try
            {
                FilesResource.ListRequest listRequest = service.Files.List();
                listRequest.Q = "trashed = false AND name = '" + filename + "'";
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
                    return null;
                }
            }
            catch (Exception ex) 
            {
                ConfigManager.logger.Error(ex.Message + " Function Name GetOneParent");
            }
            return null;
        }
        public static  string MapPathFromDriveTOLocal(IList<String> Parents)
        {
            String LocalFilePath = ConfigManager.PathBeforeLocalFolder;
            List<String> reversepath = Enumerable.Reverse(Parents).ToList();
            foreach (String name in reversepath)
            {
                LocalFilePath = LocalFilePath + name + @"\";
            }
            return LocalFilePath;
        }
        public static List<string> GetALLParents(DriveService service,string filename)
        {
            List<string> Parents = new List<string>();
            String NextParentId = " ";
            while (filename !=ConfigManager.DriveFolderName)
            {
                NextParentId = GetOneParent(service,filename);
                filename = service.Files.Get(NextParentId).Execute().Name;
                Parents.Add(filename);
            }
            return Parents;
        }

    }
}
