using System;
using System.Linq;
using System.Configuration;
using NLog;

namespace GoogleSyncService
{
    class ConfigManager
    {
        public static String LocalFilePath = ConfigurationManager.AppSettings.Get("LocalFilePath").ToString();
        public static String WayOneAdd = ConfigurationManager.AppSettings.Get("WayOneAdd").ToString();
        public static String WayOneEdit = ConfigurationManager.AppSettings.Get("WayOneEdit").ToString();
        public static String WayOneDelete = ConfigurationManager.AppSettings.Get("WayOneDelete").ToString();
        public static String WayTwoAdd = ConfigurationManager.AppSettings.Get("WayTwoAdd").ToString();
        public static String WayTwoEdit = ConfigurationManager.AppSettings.Get("WayTwoEdit").ToString();
        public static String WayTwoDelete = ConfigurationManager.AppSettings.Get("WayTwoDelete").ToString();
        public static String DriveFolderName = ConfigurationManager.AppSettings.Get("DriveFolderName").ToString();
        public static String PathBeforeLocalFolder = ConfigurationManager.AppSettings.Get("PathBeforeLocalFolder").ToString();
        public static String CardinalitiesPath = ConfigurationManager.AppSettings.Get("CardinalitiesPath").ToString();
        public static Logger logger = LogManager.GetCurrentClassLogger();
        public static void UpdateConfigs(string key, string value)
        {
            var confFile = @".\\conf.cnf";
            System.IO.File.WriteAllText(confFile, string.Empty);
            System.IO.File.AppendAllLines(confFile, new string[] { string.Format("{0}:{1}", key, value) });
        }

        public static string GetConfigs(string key)
        {
            var confFile = @".\\conf.cnf";
            var lines = System.IO.File.ReadAllLines(confFile);
            if (lines != null)
            {
                var fm = string.Format("{0}:", key);
                var val = lines.FirstOrDefault(p => p.StartsWith(fm));
                string[] words = val.Split(':');
                return words[1];
            }
            return null;
        }
    }
}
