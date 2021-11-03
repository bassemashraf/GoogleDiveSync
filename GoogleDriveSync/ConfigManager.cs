using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoogleDriveSync
{
    class ConfigManager
    {
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
