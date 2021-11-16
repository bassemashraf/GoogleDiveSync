using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Google.Apis.Drive.v3;
using System.Threading;

namespace GoogleSyncService
{
    class CloudToLocalSync
    {
        public static void StartSync(DriveService Service)
        {
            var lastToken = ConfigManager.GetConfigs("LastToken");
            while (true)
            {
                var response = Service.Changes.GetStartPageToken().Execute();
                var pageToken = response.StartPageTokenValue;
                if (lastToken != pageToken)
                {

                    ConfigManager.UpdateConfigs("LastToken", lastToken);
                    var request = Service.Changes.List(lastToken);
                    lastToken = pageToken;
                    request.Spaces = "Drive";
                    request.Fields = "*";
                    var changes = request.Execute();
                    foreach (var change in changes.Changes)
                    {
                        if (change.File is null || change.Removed == true)
                        {
                            continue;
                        }
                        if (change.File.Trashed == false && !(change.File is null))
                        {
                            IList<string> ParentsList = HelperMethods.GetALLParents(Service, change.File.Name);
                            if (ParentsList.Contains(ConfigManager.DriveFolderName))
                            {
                                String SaveTo = HelperMethods.MapPathFromDriveTOLocal(ParentsList);
                                //Create Missed Folders
                                if (!Directory.Exists(SaveTo) && ConfigManager.WayTwoAdd == "enable")
                                    Directory.CreateDirectory(SaveTo);
                                //Handle If file Edited Or Added
                                if (System.IO.File.Exists(SaveTo + change.File.Name) && change.File.MimeType != "application/vnd.google-apps.folder" && ConfigManager.WayTwoEdit == "enable")
                                {
                                    System.IO.File.Delete(SaveTo + change.File.OriginalFilename);
                                    DownloadFile(Service, change.File, SaveTo + change.File.Name);
                                }
                                else if (change.File.MimeType != "application/vnd.google-apps.folder" && ConfigManager.WayTwoAdd == "enable")
                                {
                                    DownloadFile(Service, change.File, SaveTo + change.File.Name);
                                }
                                //Handle Rename Files

                                if (change.File.OriginalFilename != null && change.File.Name != change.File.OriginalFilename && ConfigManager.WayTwoEdit == "enable")
                                {
                                    System.IO.File.Delete(SaveTo + change.File.OriginalFilename);
                                    DownloadFile(Service, change.File, SaveTo + change.File.Name);
                                }
                                //Handle If Folder Added OR File 
                                if (!Directory.Exists(SaveTo + change.File.Name) && change.File.MimeType == "application/vnd.google-apps.folder" && ConfigManager.WayTwoAdd == "enable")
                                    Directory.CreateDirectory(SaveTo + change.File.Name);
                            }
                        }
                        else
                        {
                            if (ConfigManager.WayTwoDelete == "enable")
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
                Thread.Sleep(10000);
            }
        }
        public static void DownloadFile(DriveService service, Google.Apis.Drive.v3.Data.File file, string saveTo)
        {
            try
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
                ConfigManager.logger.Info("file" + file.Name + "Downloaded");
            }
            catch (Exception e)
            {
                ConfigManager.logger.Error(e.Message + "Function Name 'DownloadFile'");
            }
        }
        public static void DeleteLocalFile(String filename)
        {
            //logger.Log("DeleteLocalFile {0}", filename);
            String sDir = ConfigManager.LocalFilePath;
            string[] files = Directory.GetFiles(sDir, filename, SearchOption.AllDirectories);
            string[] directors = Directory.GetDirectories(sDir, filename, SearchOption.AllDirectories);
            try
            {
                if (files.Length > 0)
                    System.IO.File.Delete(files[0]);
                if (directors.Length > 0)
                    Directory.Delete(directors[0]);
                ConfigManager.logger.Info("File" + filename + "Deleted");
            }
            catch (Exception ex)
            {
                ConfigManager.logger.Error(ex.Message + "Function Name 'DeleteLocalFile'");

            }

        }

    }
}
