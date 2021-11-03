using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using  System.IO;

namespace GoogleSyncService
{
    public partial class Service1 : ServiceBase
    {
        public Service1()
        {
            InitializeComponent();
        }
        int i = 0;

        protected override void OnStart(string[] args)
        {
            
            
        }
        protected override void OnContinue()
        {
            i = i + 1;
            File.Create(@"D:\Runnnnnnnnnnnnnnn.txt"+i.ToString());
            base.OnContinue();
        }

        protected override void OnStop()
        {


        }



       

    }
}
