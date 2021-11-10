using System;
using System.IO;
using System.Collections.Generic;
using Google.Apis.Drive.v3;


namespace GoogleDriveSync
{
    public class LocalToCloudSync
    {
        public static void StartSync(DriveService service)
        {
            string[] AllFiles = Directory.GetFiles(ConfigManager.LocalFilePath, "*.*", SearchOption.AllDirectories);
            foreach (var FilePath in AllFiles)
            {
                //Update Changes Happend When Application Stop
                FileInfo Info = new FileInfo(FilePath);
                Google.Apis.Drive.v3.Data.File DriveFile = HelperMethods.GetFileWithName(service, FilePath);
                if (DriveFile != null)
                {
                    if (Info.LastWriteTime.Date < DriveFile.ModifiedTime)
                        UpdateFiles(service, FilePath);
                }
                else
                    UpdateFiles(service, FilePath);

                HelperMethods.StartLocalFileWatcher();
            }
        }
        public static void UpdateFiles(DriveService service, String FilePath)

        {
            String FolderName = Path.GetFileName(FilePath);
            List<String> Parents = HelperMethods.GetParentsFolders(FilePath);
            if (ConfigManager.WayOneAdd == "enable")
                HelperMethods.CreateNotFoundFolders(service, FilePath);
            if (!HelperMethods.ListFilesFromFolder(service, Parents[0]).Contains(FolderName))
            {
                if (HelperMethods.IsFolder(FilePath))
                {
                    if (ConfigManager.WayOneAdd == "enable")
                        HelperMethods.CreateNewFolder(service, FolderName, HelperMethods.GetParentsFolders(FilePath)[0]);
                }
                else
                {
                    if (ConfigManager.WayOneAdd == "enable")
                        HelperMethods.UploadFile(service, FilePath, HelperMethods.GetParentsFolders(FilePath)[0]);
                }
            }
            else
            {
                if (!HelperMethods.IsFolder(FilePath))
                {
                    if (ConfigManager.WayOneEdit == "enable")
                        HelperMethods.UpdateOldFile(service, FilePath, FolderName);
                }
            }
        }
        public static void CreateFiles(DriveService service, String FilePath)
        {

            if (ConfigManager.WayOneAdd == "enable")
            {
                String foldername = Path.GetFileName(FilePath);

                if (!HelperMethods.ListFilesFromFolder(service, HelperMethods.GetParentsFolders(FilePath)[0]).Contains(foldername))
                {
                    //detect whether its a directory or file
                    if (HelperMethods.IsFolder(FilePath))
                    {
                        HelperMethods.CreateNewFolder(service, foldername, HelperMethods.GetParentsFolders(FilePath)[0]);
                    }
                    else
                    {
                        HelperMethods.UploadFile(service, FilePath, HelperMethods.GetParentsFolders(FilePath)[0]);
                    }
                }
            }

        }
        public static void DeleteFiles(DriveService service, String FilePath)
        {
            if (ConfigManager.WayOneDelete == "enable")
            {
                try
                {
                    if (HelperMethods.GetFileId(service, FilePath) == "")
                        service.Files.Delete(HelperMethods.GetFolderId(service, Path.GetFileName(FilePath))).Execute();
                    else
                        service.Files.Delete(HelperMethods.GetFileId(service, FilePath)).Execute();
                }
                catch (Exception e)
                {
                    ConfigManager.logger.Error(e.Message + "Function Name 'DeleteFiles'");
                }
            }

        }
    }
}
