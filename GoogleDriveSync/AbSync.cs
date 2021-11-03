using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoogleDriveSync
{
   public  abstract class AbSync
    {
        public abstract void UpdateFile();

        public abstract void DeleteFile();

        public abstract void CreateFolder();

        public abstract void DeleteFolder();

        public abstract void Start();
    }
}
