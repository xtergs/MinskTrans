using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using MetroLog;
using MinskTrans.Context;
using MinskTrans.Utilites.Base.IO;
using MinskTrans.Utilites.Base.Net;
using Newtonsoft.Json;

namespace MinskTrans.Net
{
    public class ListWithDate
    {
        public DateTime LastUpdateDateTimeUtc { get; set; } = new DateTime();
        public List<NewsEntry> NewsEntries { get; set; } = new List<NewsEntry>(10);
    }

	public interface INewsContext
	{
		ListWithDate MainNews { get; }
		ListWithDate HotNews { get; }
		Task LoadDataAsync(TypeFolder folder, string file);
		Task Save(TypeFolder folder, string file);
	    Task Clear(TypeFolder Folder);
	}

	public class BaseNewsContext : INewsContext
	{
		private ILogger log;
		private FileHelperBase fileHelerp;
		public BaseNewsContext(FileHelperBase fileHelper, ILogManager logManager)
		{
			this.fileHelerp = fileHelper;
			this.log = logManager.GetLogger<BaseNewsContext>();
		}
		public ListWithDate MainNews { get; private set; }
		public ListWithDate HotNews { get; private set; }

		readonly object saveLocker = new object();
		public async Task LoadDataAsync(TypeFolder folder, string file)
		{
			if (!await fileHelerp.FileExistAsync(folder, file).ConfigureAwait(false))
			{
				MainNews = new ListWithDate();
				HotNews = new ListWithDate();
				return;
			}
			var data = await fileHelerp.ReadAllTextAsync(folder, file).ConfigureAwait(false);
			if (string.IsNullOrWhiteSpace(data))
			{
				MainNews = new ListWithDate();
				HotNews = new ListWithDate();
				return;
			}
			try
			{
				var result = JsonConvert.DeserializeObject<ListWithDate[]>(data);
				lock (saveLocker)
				{
					MainNews = result[0];
					HotNews = result[1];
				}
			}
			catch (JsonReaderException e)
			{
				MainNews = new ListWithDate();
				HotNews = new ListWithDate();
				Debug.WriteLine("NewsContext: not corrent json format data");
			}
		}

		public Task Save(TypeFolder folder, string file)
		{
			try
			{
				string str = "";
				lock(saveLocker)
					str = JsonConvert.SerializeObject(new[] {MainNews, HotNews});
				return fileHelerp.WriteTextAsync(folder, file, str);
			}
			catch (JsonSerializationException e)
			{
				log.Error($"News context: can't serialize data:{e.Message} \n {MainNews}{HotNews}");
			}
			return null;
		}

	    public Task Clear(TypeFolder Folder)
	    {
            HotNews = new ListWithDate();
            MainNews = new ListWithDate();
	        return fileHelerp.ClearFolder(Folder);
	    }
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
                return allNews?.NewsEntries.OrderByDescending(key => key.PostedUtc).ThenByDescending(key => key.CollectedUtc).ToList();
            }
            set { OnPropertyChanged(); }
        }

        public DateTime HotNewsDateTimeUtc => allHotNewsDictionary.LastUpdateDateTimeUtc;
        public List<NewsEntry> AllHotNews
        {
            get
            {
                return
                    allHotNewsDictionary?.NewsEntries.OrderByDescending(key => key.PostedUtc)
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
				await context.LoadDataAsync(Files.AllNewsFileV3.Folder, Files.AllNewsFileV3.FileName);
            NewNews = null;
            AllHotNews = null;
        }

        private async Task SaveToFile(string filePath, TypeFolder folder, ListWithDate listToWrite)
        {
			await context.Save(folder, filePath);
        }
        
        public Task SaveToFile()
        {
            return SaveToFile(Files.AllNewsFileV3.FileName, Files.AllNewsFileV3.Folder,
                    allNews);
        }

        public Task SaveToFileHotNews()
        {
            return SaveToFile(Files.AllNewsFileV3.FileName, Files.AllNewsFileV3.Folder, allHotNewsDictionary);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public FilePathsSettings Files { get; }

	    public void ResetState()
	    {
	        context.Clear(Files.AllNewsFileV3.Folder);
	        NewNews = null;
	    }
    }
}