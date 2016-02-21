using MinskTrans.Utilites.Base.IO;

namespace MinskTrans.Context
{
    public struct FilePathStruct
    {
        public FilePathStruct(string fileName, TypeFolder folder, string originalLink = null, string secondlink = null)
        {
            FileName = fileName;
            Folder = folder;
            OriginalLink = originalLink;
            SecondFormatedLink = secondlink;
        }
        public string FileName { get; set; }
        public TypeFolder Folder { get; set; }
        public string OriginalLink { get; set; }
        public string SecondFormatedLink { get; set; }

        public bool IsHaveOriginalLink => !string.IsNullOrWhiteSpace(OriginalLink);
        public bool IsHaveSecondFormatedLink => !string.IsNullOrWhiteSpace(SecondFormatedLink);
        public string TempFileName => FileName + ".tmp";
        public string NewFileName => FileName + ".new";
        public string OldFileName => FileName + ".old";
    }

    public class FilePathsSettings
    {
        public FilePathsSettings()
        {
            StopsFile = new FilePathStruct("stops.dat", TypeFolder.Local, "http://www.minsktrans.by/city/minsk/stops.txt", "https://hvwkqw-ch3302.files.1drv.com/y3m2ue-xZZv20EqnyXbRVUm4b-tcnCLWCre5b4x0GCTbHBDePahmL9QOhVlDOgUKEDQ1Y6CqQw1YEvwOy5r9qppgCz3m5vcGOH0PRARy_Q_9NWL4TyD79IZOQYJvwWXvPMoecy_GRWduwvvbXD4La4nlQ/stops.dat?download&psid=1");
            RouteFile = new FilePathStruct("route.dat", TypeFolder.Local, "http://www.minsktrans.by/city/minsk/routes.txt", "https://hvwjqw-ch3302.files.1drv.com/y3m8mbmVT4Gkn5aulSslqUvBtdTBxKUt-zsZhhn2TufEX15tkVxq-b-pEzNQN2pjxtP-XwWalZ1ZZIP7CT89D6inMs2tsbU6DdD3JIU2xzu4XzBlGPPd5q-jHqJBRCPG81l74HRPmb9a3FY0ctSQM0aEQ/route.dat?download&psid=1");
            TimeFile = new FilePathStruct("time.dat", TypeFolder.Local, "http://www.minsktrans.by/city/minsk/times.txt", "https://hvwlqw-ch3302.files.1drv.com/y3mZL6AQnCsRop9AfJY6gA6jfsLyxelQabA_KQZYM0XX0ldZPjhjzlUCj2xMIVRzenoY0CY4fCYBDkWnJuofDQnDfcnDkMp38-j-xIkkud__o6Np1Wpl54Me9omwMA6O8ygoOx0WPYfWiz1jjGqMI0rzA/time.dat?download&psid=1");
            FavouriteFile = new FilePathStruct("favourite.dat", TypeFolder.Roaming);
            StatisticFile = new FilePathStruct("statistics.dat", TypeFolder.Roaming);
            LastUpdatedFile = new FilePathStruct("LastNews.txt", TypeFolder.Local, "", "https://onedrive.live.com/download.aspx?cid=27EDF63E3C801B19&resid=27edf63e3c801b19%2111529&authkey=%21ADs9KNHO9TDPE3Q&canary=3P%2F1MinRbysxZGv9ZvRDurX7Th84GvFR4kV1zdateI8%3D4");
            HotNewsFile = new FilePathStruct("days.txt", TypeFolder.Local, @"http://www.minsktrans.by/ru/newsall/news/operativnaya-informatsiya.html", "https://onedrive.live.com/download.aspx?cid=27EDF63E3C801B19&resid=27edf63e3c801b19%2111531&authkey=%21AIJo-8Q4661GpiI&canary=3P%2F1MinRbysxZGv9ZvRDurX7Th84GvFR4kV1zdateI8%3D2");
            MainNewsFile = new FilePathStruct("months.txt", TypeFolder.Local, @"http://www.minsktrans.by/ru/newsall/news/newscity.html", "https://onedrive.live.com/download.aspx?cid=27EDF63E3C801B19&resid=27edf63e3c801b19%2111532&authkey=%21AAQED1sY1RWFib8&canary=3P%2F1MinRbysxZGv9ZvRDurX7Th84GvFR4kV1zdateI8%3D8");
        }
        public FilePathStruct TimeFile { get; set; }
        public FilePathStruct RouteFile { get; set; }
        public FilePathStruct StopsFile { get; set; }

        public FilePathStruct FavouriteFile { get; set; }
        public FilePathStruct StatisticFile { get; set; }
      

        public FilePathStruct LastUpdatedFile { get; set; }
        public FilePathStruct HotNewsFile { get; set; }
        public FilePathStruct MainNewsFile { get; set; }
       
    }
}
