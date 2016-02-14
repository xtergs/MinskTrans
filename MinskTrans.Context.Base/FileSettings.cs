using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MinskTrans.Utilites.Base.IO;

namespace MinskTrans.Context
{
    class FileSettings
    {
        public string TimeFile { get; set; } = "time.txt";
        public TypeFolder TimeTypefolder { get; set; } = TypeFolder.Local;
        public string TimeLink { get; set; }
        public string RouteFile { get; set; } = "route.txt";
        public TypeFolder RouteFileFolder { get; set; } = TypeFolder.Local;
        public string RouteLink { get; set; } 
        public string StopsFile { get; set; } = "stops.txt";
        public TypeFolder StopsTypeFolder { get; set; } = TypeFolder.Local;
        public string StopsLink { get; set; }


        public string LastUpdatedFile { get; set; } = "LastUpdated.txt";
        public TypeFolder LastUpdatedTypefolder { get; set; } = TypeFolder.Local;
        public string LastUpdatedLink { get; set; }
        public string HotNewsFile { get; set; } = "HotNews.txt";
        public TypeFolder HotNewsFileFolder { get; set; } = TypeFolder.Local;
        public string HotNewsLink { get; set; }
        public string MainNewsFile { get; set; } = "MainNews.txt";
        public TypeFolder MainNewsTypeFolder { get; set; } = TypeFolder.Local;
        public string MainNewsLink { get; set; }
    }
}
