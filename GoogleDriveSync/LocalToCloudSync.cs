using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;



namespace GoogleDriveSync
{
    public    class  LocalToCloudSync : AbSync
    {
        public override void UpdateFile()
        {
            throw new NotImplementedException();
        }

        public override void DeleteFile()
        {
            throw new NotImplementedException();
        }

        public override void CreateFolder( )
        {
            throw new NotImplementedException();
        }

        public override void DeleteFolder()
        {
            throw new NotImplementedException();
        }

        public override void Start()
        {
            //string[] AllFiles = Directory.GetFiles(ConfigManager.LocalFilePath, "*.*", SearchOption.AllDirectories);
            //foreach (var FilePath in AllFiles)
            //{

            //    FileInfo Info = new FileInfo(FilePath);
            //    Google.Apis.Drive.v3.Data.File DriveFile = HelperMethods.GetFileWithName(Service, FilePath);
            //    if (DriveFile != null)
            //    {
            //        if (Info.LastWriteTime.Date < DriveFile.ModifiedTime)
            //            LocalFileChanged(FilePath);
            //    }
            //    else
            //        LocalFileChanged(FilePath);
            //}


        }



    }
}
