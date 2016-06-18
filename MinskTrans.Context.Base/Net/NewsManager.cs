using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using MetroLog;
using MetroLog.Layouts;
using MinskTrans.Context;
using MinskTrans.Utilites.Base.IO;
using MinskTrans.Utilites.Base.Net;
using MyLibrary;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace MinskTrans.Net
{
    public class ShouldSerializeContractResolver : DefaultContractResolver
    {
        public static readonly ShouldSerializeContractResolver Instance = new ShouldSerializeContractResolver();

        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            JsonProperty property = base.CreateProperty(member, memberSerialization);

            if (property.DeclaringType == typeof (NewsEntry) && property.PropertyName == "Message")
            {
                property.ShouldSerialize = (x) => true;
                property.PropertyName = "Message";
            }
            else if (property.DeclaringType == typeof (NewsEntry) && property.PropertyName == "PostedUtc")
            {
                property.ShouldSerialize = (x) => true;
                property.PropertyName = "Posted";
            }
            else if (property.DeclaringType == typeof (NewsEntry) && property.PropertyName == "CollectedUtc")
            {
                property.ShouldSerialize = (x) => true;
                property.PropertyName = "Collected";
            }
            else if (property.DeclaringType == typeof (NewsEntry) && property.PropertyName == "RepairedLineUtc")
            {
                property.ShouldSerialize = (x) => true;
                property.PropertyName = "RepairedLIne";
            }
            else
            {
                property.Ignored = true;
                property.ShouldSerialize = (x) => false;
            }
            return property;
        }
    }


    public abstract class NewsManagerBase : INotifyPropertyChanged
    {
        private ILogger log;

        private List<NewsEntry> newDictionary;
        protected List<NewsEntry> hotNewsDictionary;

        private List<NewsEntry> allNews;
        protected List<NewsEntry> allHotNewsDictionary;

        public List<NewsEntry> NewNews
        {
            get
            {
                return allNews.OrderByDescending(key => key.PostedUtc).ThenByDescending(key => key.CollectedUtc).ToList();
            }
            set { OnPropertyChanged(); }
        }

        public List<NewsEntry> AllHotNews
        {
            get
            {
                return
                    allHotNewsDictionary.OrderByDescending(key => key.PostedUtc)
                        .ThenByDescending(key => key.RepairedLineUtc)
                        .ToList();
            }
            set { OnPropertyChanged(); }
        }


        protected readonly InternetHelperBase internetHelper;
        protected readonly ISettingsModelView settings;

        private FileHelperBase FileHelper { get; }

        public NewsManagerBase(FileHelperBase helper, InternetHelperBase internetHelper, ISettingsModelView settings,
            ILogManager logManager, FilePathsSettings files)
        {
            if (helper == null)
                throw new ArgumentNullException(nameof(helper));
            FileHelper = helper;
            if (internetHelper == null)
                throw new ArgumentNullException(nameof(internetHelper));
            this.internetHelper = internetHelper;
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));
            if (logManager == null)
                throw new ArgumentNullException(nameof(logManager));
            log = logManager.GetLogger<NewsManagerBase>();
            this.settings = settings;
            this.Files = files;
            //LastNewsTime = new DateTime();
            newDictionary = new List<NewsEntry>();
            hotNewsDictionary = new List<NewsEntry>();
            allHotNewsDictionary = new List<NewsEntry>();
            allNews = new List<NewsEntry>();
            log.Trace($"{nameof(NewsManagerBase)} is created");
        }

        public abstract Task<List<NewsEntry>> CheckAsync(string uri, string XpathSelectInfo, string XpathSelectDate);

        public async Task CheckMainNewsAsync()
        {
            log.Trace("CheckMainNewsAsync is started");
            newDictionary =
                (await CheckAsync(Files.MainNewsFile.OriginalLink, "div/p", "div/dl/div/table/tr/td[1]")).Where(
                    time => time.PostedUtc > new DateTime()).ToList();
            if (newDictionary.Count <= 0)
            {
                log.Trace("CheckMainNewsAsync:new Main news have not been found");
                return;
            }
            settings.LastNewsTimeUtc = newDictionary.Max(key => key.PostedUtc);
            log.Trace($"CheckMainNewsAsync: {nameof(settings.LastNewsTimeUtc)} is {settings.LastNewsTimeUtc}");
            foreach (var source in newDictionary)
            {
                if (!allNews.Any(key => key.PostedUtc == source.PostedUtc))
                {
                    allNews.Add(source);
                    log.Debug($"CheckMainNewsAsync: main news has been found: {source.CollectedUtc} {source.Message}");
                }
            }
            NewNews = null;
        }

        public abstract Task CheckHotNewsAsync();


        public async Task Load()
        {
            allNews.Clear();
            log.Trace(
                $"Load: read file for months {FileHelper.GetPath(Files.MainNewsFile.Folder)} + {Files.MainNewsFile.FileName}");
            JsonSerializer serializer = new JsonSerializer();
            try
            {
                using (
                    var stream =
                        new StreamReader(
                            await FileHelper.OpenStream(Files.MainNewsFile.Folder, Files.MainNewsFile.FileName)))
                using (var textReader = new JsonTextReader(stream))
                {
                    var news = serializer.Deserialize<List<NewsEntry>>(textReader);
                    allNews.AddRange(news);
                }
                log.Trace($"Load: has been readed {allNews.Count} news");
            }
            catch (FileNotFoundException e)
            {
                Debug.WriteLine(e.Message);
            }

            NewNews = null;

            AllHotNews.Clear();
            log.Trace($"read hot news");
            try
            {
                using (var stream =new StreamReader(
                            await FileHelper.OpenStream(Files.MainNewsFile.Folder, Files.MainNewsFile.FileName)))
                using (var textReader = new JsonTextReader(stream))
                {
                    serializer.ContractResolver = new ShouldSerializeContractResolver();
                    var news = serializer.Deserialize<List<NewsEntry>>(textReader);
                    allHotNewsDictionary.AddRange(news);
                }
                log.Trace($"Load: has been readed {allHotNewsDictionary.Count} hot news");
            }
            catch (FileNotFoundException e)
            {
                Debug.WriteLine(e.Message);
            }

            hotNewsDictionary = null;
            AllHotNews = null;
        }

        private async Task SaveToFile(string filePath, TypeFolder folder, IEnumerable<NewsEntry> listToWrite)
        {
            if (!listToWrite.Any())
                return;

            string jsonString = JsonConvert.SerializeObject(listToWrite,
                new JsonSerializerSettings() {ContractResolver = new ShouldSerializeContractResolver()});
            log.Trace($"SaveToFile {filePath}, JSON: {jsonString}");
            await FileHelper.WriteTextAsync(folder, filePath, jsonString);
            log.Trace($"SaveToFile successfully done");
        }

        public async Task SaveToFile()
        {
            DateTime curDateTime = DateTime.UtcNow;
            await
                SaveToFile(Files.MainNewsFile.FileName, Files.MainNewsFile.Folder,
                    newDictionary.Where(
                        x =>
                            x.PostedUtc.Month == curDateTime.Month ||
                            x.PostedUtc.Month == (curDateTime.Subtract(new TimeSpan(31, 0, 0, 0, 0)).Month)).ToList());
        }

        public async Task SaveToFileHotNews()
        {
            DateTime curDay = DateTime.UtcNow;
            var days =
                allHotNewsDictionary.Where(
                    key =>
                        key.PostedUtc.Day == curDay.Day ||
                        (key.PostedUtc.Day == (curDay.Subtract(new TimeSpan(1, 0, 0, 0))).Day)).ToList();
            await SaveToFile(Files.HotNewsFile.FileName, Files.HotNewsFile.Folder, days);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public DateTime LastUpdateMainNewsDateTimeUtc
        {
            get { return settings.LastNewsTimeUtc; }
            set
            {
                settings.LastNewsTimeUtc = value;
                log.Debug($"{nameof(LastUpdateMainNewsDateTimeUtc)} has seted to {settings.LastNewsTimeUtc}");
            }
        }

        public DateTime LastUpdateHotNewsDateTimeUtc
        {
            get { return settings.LastUpdateHotNewsDateTimeUtc; }
            set
            {
                settings.LastUpdateHotNewsDateTimeUtc = value;
                log.Debug($"{nameof(LastUpdateHotNewsDateTimeUtc)} has seted to {settings.LastUpdateHotNewsDateTimeUtc}");
            }
        }

        public FilePathsSettings Files { get; }
    }
}