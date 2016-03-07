using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MetroLog.Layouts;
using MetroLog.Targets;
using PCLStorage;

namespace MinskTrans.Context.Desktop
{
    class PortableFileTargetComp : PortableFileTarget
    {
        public PortableFileTargetComp(IFileSystem fileSystem, StorageType storageType = StorageType.Local, string dirName = "MetroLogs", Layout layout = null)
            : base(fileSystem, storageType, dirName, layout)
        {
        }

       
    }
}
