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

            if ( property.PropertyName == nameof(ListWithDate.LastUpdateDateTimeUtc))
            {
                property.ShouldSerialize = (x) => true;
                property.PropertyName = nameof(ListWithDate.LastUpdateDateTimeUtc);
            } else
            if (property.PropertyName == nameof(ListWithDate.NewsEntries))
            {
                property.ShouldSerialize = (x) => true;
                property.PropertyName = nameof(ListWithDate.NewsEntries);
            } else
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

    public class ListWithDate
    {
        public DateTime LastUpdateDateTimeUtc { get; set; } = new DateTime();
        public List<NewsEntry> NewsEntries { get; set; } = new List<NewsEntry>(10);
    }

	public interface INewsContext
	{
		ListWithDate MainNews { get; }
		ListWithDate HotNews { get; }
		void LoadData();
		void Save();
	}


	public abstract class NewsManagerBase : INotifyPropertyChanged
    {
        private ILogger log;
	    private INewsContext context;

		protected ListWithDate allNews => context.MainNews;
		protected ListWithDate allHotNewsDictionary => context.HotNews;

        public DateTime NewNewsDateTimeUtc => allNews.LastUpdateDateTimeUtc;
        public List<NewsEntry> NewNews
        {
            get
            {
                return allNews.NewsEntries.OrderByDescending(key => key.PostedUtc).ThenByDescending(key => key.CollectedUtc).ToList();
            }
            set { OnPropertyChanged(); }
        }

        public DateTime HotNewsDateTimeUtc => allHotNewsDictionary.LastUpdateDateTimeUtc;
        public List<NewsEntry> AllHotNews
        {
            get
            {
                return
                    allHotNewsDictionary.NewsEntries.OrderByDescending(key => key.PostedUtc)
                        .ThenByDescending(key => key.RepairedLineUtc)
                        .ToList();
            }
            set { OnPropertyChanged(); }
        }


        protected readonly InternetHelperBase internetHelper;
        //protected readonly ISettingsModelView settings;

        private FileHelperBase FileHelper { get; }

        public NewsManagerBase(FileHelperBase helper, InternetHelperBase internetHelper,
            ILogManager logManager, FilePathsSettings files, INewsContext context)
        {
	        if (context == null)
		        throw new ArgumentNullException(nameof(context));
            if (helper == null)
                throw new ArgumentNullException(nameof(helper));
            FileHelper = helper;
            if (internetHelper == null)
                throw new ArgumentNullException(nameof(internetHelper));
            this.internetHelper = internetHelper;
            if (logManager == null)
                throw new ArgumentNullException(nameof(logManager));
            log = logManager.GetLogger<NewsManagerBase>();
            //this.settings = settings;
            this.Files = files;
	        this.context = context;
            //LastNewsTime = new DateTime();
            //newDictionary = new ListWithDate();
            //hotNewsDictionary = new ListWithDate();
            //allHotNewsDictionary = new ListWithDate();
            //allNews = new ListWithDate();
            log.Trace($"{nameof(NewsManagerBase)} is created");
        }

        public abstract Task<List<NewsEntry>> CheckAsync(string uri, string XpathSelectInfo, string XpathSelectDate);
        

        public async Task<bool> CheckMainNewsAsync()
        {
            log.Trace("CheckMainNewsAsync is started");
            var newDictionary =
                (await CheckAsync(Files.MainNewsFile.OriginalLink, "div/p", "div/dl/div/table/tr/td[1]")).Where(
                    time => time.PostedUtc > new DateTime()).ToList();
            if (newDictionary.Count <= 0)
            {
                log.Trace("CheckMainNewsAsync:new Main news have not been found");
                return false;
            }
            var currentUpdateTimeUtc = newDictionary.Max(key => key.PostedUtc);
            if (currentUpdateTimeUtc <= allNews.LastUpdateDateTimeUtc)
                return false;
            log.Trace($"CheckMainNewsAsync: {nameof(allNews.LastUpdateDateTimeUtc)} is {allNews.LastUpdateDateTimeUtc}");
            bool flagResult = false;
            List<NewsEntry> newsToAdd = new List<NewsEntry>();
            foreach (var source in newDictionary)
            {
                if (allNews.NewsEntries.All(key => key.PostedUtc != source.PostedUtc))
                {
                    newsToAdd.Add(source);
                    flagResult = true;
                }
            }
            allNews.NewsEntries.AddRange(newsToAdd);
            allNews.LastUpdateDateTimeUtc = currentUpdateTimeUtc;
            NewNews = null;
            return flagResult;
        }

        public abstract Task<bool> CheckHotNewsAsync();


        public async Task Load()
        {
            //allNews.NewsEntries.Clear();
            
            log.Trace(
                $"Load: read file for months {FileHelper.GetPath(Files.MainNewsFile.Folder)} + {Files.MainNewsFile.FileName}");
				context.LoadData();
    //        JsonSerializer serializer = new JsonSerializer();
    //        try
    //        {
    //            //using (
    //            //    var stream =
    //            //        new StreamReader(
    //            //            await FileHelper.OpenStream(Files.MainNewsFile.Folder, Files.MainNewsFile.FileName)))
    //            //using (var textReader = new JsonTextReader(stream))
    //            //{
    //            //    var news = serializer.Deserialize<ListWithDate>(textReader);
    //            //    allNews = news;
    //            //    //allNews.NewsEntries.AddRange(news);
    //            //}
    //            log.Trace($"Load: has been readed {allNews.NewsEntries.Count} news");
    //        }
    //        catch (FileNotFoundException e)
    //        {
    //            Debug.WriteLine(e.Message);
    //        }
    //        catch (JsonSerializationException e)
    //        {
    //            log?.Trace(e.Message);
    //        }


    //        //AllHotNews.Clear();
    //        log.Trace($"read hot news");
    //        try
    //        {
				//context.LoadData();
    //            //using (var stream = new StreamReader(
    //            //    await FileHelper.OpenStream(Files.MainNewsFile.Folder, Files.MainNewsFile.FileName)))
    //            //using (var textReader = new JsonTextReader(stream))
    //            //{
    //            //    serializer.ContractResolver = new ShouldSerializeContractResolver();
    //            //    var news = serializer.Deserialize<ListWithDate>(textReader);
    //            //    allHotNewsDictionary = news;
    //            //}
    //            log.Trace($"Load: has been readed {allHotNewsDictionary.NewsEntries.Count} hot news");
    //        }
    //        catch (FileNotFoundException e)
    //        {
    //            Debug.WriteLine(e.Message);
    //        }
    //        catch (JsonSerializationException e)
    //        {
    //            log?.Trace(e.Message);
    //        }

            //hotNewsDictionary = null;
            NewNews = null;
            AllHotNews = null;
        }

        private async Task SaveToFile(string filePath, TypeFolder folder, ListWithDate listToWrite)
        {
			context.Save();
            //string jsonString = JsonConvert.SerializeObject(listToWrite,
            //    new JsonSerializerSettings() {ContractResolver = new ShouldSerializeContractResolver()});
            //return FileHelper.WriteTextAsync(folder, filePath, jsonString);
        }
        
        public Task SaveToFile()
        {
            return SaveToFile(Files.MainNewsFile.FileName, Files.MainNewsFile.Folder,
                    allNews);
        }

        public Task SaveToFileHotNews()
        {
            return SaveToFile(Files.HotNewsFile.FileName, Files.HotNewsFile.Folder, allHotNewsDictionary);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public FilePathsSettings Files { get; }
    }
}