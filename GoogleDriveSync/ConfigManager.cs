using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace GoogleDriveSync
{
    class ConfigManager
    {


      public static   String LocalFilePath = ConfigurationManager.AppSettings.Get("LocalFilePath").ToString();
      public static   String WayOneAdd = ConfigurationManager.AppSettings.Get("WayOneAdd").ToString();
      public static   String WayOneEdit = ConfigurationManager.AppSettings.Get("WayOneEdit").ToString();
      public static   String WayOneDelete = ConfigurationManager.AppSettings.Get("WayOneDelete").ToString();
      public static   String WayTwoAdd = ConfigurationManager.AppSettings.Get("WayTwoAdd").ToString();
      public static   String WayTwoEdit = ConfigurationManager.AppSettings.Get("WayTwoEdit").ToString();
      public static   String WayTwoDelete = ConfigurationManager.AppSettings.Get("WayTwoDelete").ToString();
      public static   String DriveFolderName = ConfigurationManager.AppSettings.Get("DriveFolderName").ToString();
      public static   String PathBeforeLocalFolder = ConfigurationManager.AppSettings.Get("PathBeforeLocalFolder").ToString();
        public static void UpdateConfigs(string key, string value)
        {
            var confFile = ".\\conf.cnf";
            System.IO.File.AppendAllLines(confFile, new string[] { string.Format("{0}: {1}", key, value) });
        }

        public static string GetConfigs(string key)
        {
            var confFile = ".\\conf.cnf";
            var lines = System.IO.File.ReadAllLines(confFile);
            if (lines != null)
            {
                var fm = string.Format("{0}: ", key);
                var val = lines.FirstOrDefault(p => p.StartsWith(fm));
                return val;
            }
            return null;
        }
    }
}
