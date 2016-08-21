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
            StopsFile = new FilePathStruct("stops.dat", TypeFolder.Local, "http://www.minsktrans.by/city/minsk/stops.txt", "https://onedrive.live.com/download?cid=27EDF63E3C801B19&resid=27EDF63E3C801B19%2116855&authkey=AN0rfhVuMuxHT68");
			RouteFile = new FilePathStruct("route.dat", TypeFolder.Local, "http://www.minsktrans.by/city/minsk/routes.txt", "https://onedrive.live.com/download?cid=27EDF63E3C801B19&resid=27EDF63E3C801B19%2116854&authkey=ABcKVzT0ApakBiY");
			TimeFile = new FilePathStruct("time.dat", TypeFolder.Local, "http://www.minsktrans.by/city/minsk/times.txt", "https://onedrive.live.com/download?cid=27EDF63E3C801B19&resid=27EDF63E3C801B19%2116856&authkey=AOKR4i0KOtmFo58");
			FavouriteFile = new FilePathStruct("favourite.dat", TypeFolder.Roaming);
            StatisticFile = new FilePathStruct("statistics.dat", TypeFolder.Roaming);
            LastUpdatedFile = new FilePathStruct("LastNews.txt", TypeFolder.Local, "", "https://onedrive.live.com/download.aspx?cid=27EDF63E3C801B19&resid=27edf63e3c801b19%2111529&authkey=%21ADs9KNHO9TDPE3Q&canary=3P%2F1MinRbysxZGv9ZvRDurX7Th84GvFR4kV1zdateI8%3D4");
            HotNewsFile = new FilePathStruct("days.txt", TypeFolder.Local, @"http://www.minsktrans.by/ru/newsall/news/operativnaya-informatsiya.html", "https://onedrive.live.com/download.aspx?cid=27EDF63E3C801B19&resid=27edf63e3c801b19%2111531&authkey=%21AIJo-8Q4661GpiI&canary=3P%2F1MinRbysxZGv9ZvRDurX7Th84GvFR4kV1zdateI8%3D2");
            MainNewsFile = new FilePathStruct("months.txt", TypeFolder.Local, @"http://www.minsktrans.by/ru/newsall/news/newscity.html", "https://onedrive.live.com/download.aspx?cid=27EDF63E3C801B19&resid=27edf63e3c801b19%2111532&authkey=%21AAQED1sY1RWFib8&canary=3P%2F1MinRbysxZGv9ZvRDurX7Th84GvFR4kV1zdateI8%3D8");

			TimeTableAllFile = new FilePathStruct("timetableall.json.v", TypeFolder.Local, "", "https://onedrive.live.com/download?cid=27EDF63E3C801B19&resid=27EDF63E3C801B19%2116977&authkey=ACy2zEe1xRXErhM");
			MainNewsFileV2 = new FilePathStruct("months.v2.txt", TypeFolder.Local, "");
            HotNewsFileV2 = new FilePathStruct("days.v2.txt", TypeFolder.Local, "");

			AllNewsFileV3 = new FilePathStruct("news.json_v3", TypeFolder.Local, "", "https://onedrive.live.com/download?cid=27EDF63E3C801B19&resid=27EDF63E3C801B19%2117176&authkey=AIeOI5DJwJDd2n0");

		}
        public FilePathStruct TimeFile { get; private set; }
        public FilePathStruct RouteFile { get; private set; }
        public FilePathStruct StopsFile { get; private set; }

        public FilePathStruct FavouriteFile { get; private set; }
        public FilePathStruct StatisticFile { get; private set; }
      

        public FilePathStruct LastUpdatedFile { get; private set; }
        public FilePathStruct HotNewsFile { get; private set; }
        public FilePathStruct MainNewsFile { get; private set; }
        public FilePathStruct MainNewsFileV2 { get; private set; }
        public FilePathStruct HotNewsFileV2 { get; private set; }
		public FilePathStruct AllNewsFileV3 { get; private set; }

        public FilePathStruct TimeTableAllFile { get; private set; }

    }
}
