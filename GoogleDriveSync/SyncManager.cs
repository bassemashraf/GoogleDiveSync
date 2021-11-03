using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoogleDriveSync
{
    class SyncManager
    {
        public void StartSync()
        {
            // logger.Log("StartSync Started");
            var cloudToLocalSync = new CloudToLocalSync();
            cloudToLocalSync.Start();
            var localToCloudSync = new LocalToCloudSync();
            cloudToLocalSync.Start();
            //logger.Log("StartSync Ended");
        }
    }
}
