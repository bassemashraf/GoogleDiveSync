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


        public   static string GetMimeType(string fileName)
        {
            string mimeType = "application /unknown";
            string ext = System.IO.Path.GetExtension(fileName).ToLower();
            Microsoft.Win32.RegistryKey regKey = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(ext);
            if (regKey != null && regKey.GetValue("Content Type") != null)
                mimeType = regKey.GetValue("Content Type").ToString();
            return mimeType;
        }

        public  static  List<string> GetParentsFolders(string FilePath)
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

        public  static List<string> ListFilesFromFolder(DriveService Service ,  String foldername)
        {
            FilesResource.ListRequest listRequest = Service.Files.List();
            listRequest.Fields = "nextPageToken, files(id, name)";
            List<string> folders = new List<string>();
            string id = GetFolderId(Service,foldername);
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
        public static  void CreateNotFoundFolders(DriveService Service , String filepath)
        {
            List<String> Parents = GetParentsFolders(filepath);

            if (Parents.Count > 1)
            {
                for (int i = Parents.Count; i-- > 0;)
                {
                    if (i == 0)
                        break;
                    if (!ListFilesFromFolder(Service,Parents[i]).Contains(Parents[i - 1]))
                    {
                        CreateNewFolder(Service, Parents[i - 1], Parents[i]);
                    }
                }
            }
        }

        public  static  void UpdateOldFile(DriveService Service,  String NewFilePath, String OldFilePath)
        {
            String fileId = "";
            Google.Apis.Drive.v3.Data.File updatedFileMetadata = new Google.Apis.Drive.v3.Data.File();
            updatedFileMetadata.Name = Path.GetFileName(NewFilePath);
            FilesResource.UpdateRequest updateRequest;
            if (IsFolder(NewFilePath))
                fileId = GetFolderId(Service, Path.GetFileName(OldFilePath));
            else
                fileId = GetFileId(Service, OldFilePath);
            updateRequest = Service.Files.Update(updatedFileMetadata, fileId);
            updateRequest.Execute();
        }
        public   static string GetFolderId(DriveService Service, string foldername)
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


        public  static string GetFileId(DriveService Service, string path)
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

        public  static  bool IsFolder(String path)
        {
            FileAttributes attr = System.IO.File.GetAttributes(path);
            if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
                return true;
            else
                return false;
        }
        public static  void CreateNewFolder(DriveService Service, String filename, String parentfolder)
        {
            String ID = GetFolderId(Service,parentfolder);
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




    }
}
