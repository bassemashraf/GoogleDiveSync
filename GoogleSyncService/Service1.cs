
using System.ServiceProcess;
using System;
using System.Diagnostics;
using System.ComponentModel;

namespace GoogleSyncService
{
    public partial class Service1 : ServiceBase
    {
        public Service1()
        {
            InitializeComponent();
        }


        protected override void OnStart(string[] args)
        {
            CloudToLocalSync.StartSync(DriveConnect.Service);
            LocalToCloudSync.StartSync(DriveConnect.Service);

        }

        protected override void OnStop()
        {


        }





    }
}
