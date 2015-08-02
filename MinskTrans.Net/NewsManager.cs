using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using MinskTrans.Utilites.Base.IO;
using MinskTrans.Utilites.Base.Net;
using MyLibrary;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace MinskTrans.Net
{
		

	public class ShouldSerializeContractResolver : DefaultContractResolver
 {
     public new static readonly ShouldSerializeContractResolver Instance = new ShouldSerializeContractResolver();
 
     protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
     {
         JsonProperty property = base.CreateProperty(member, memberSerialization);
 
         if (property.DeclaringType == typeof(NewsEntry) && property.PropertyName == "Message")
         {
	         property.ShouldSerialize = (x)=> true;
	         property.PropertyName = "Message";
		 }
		 else if (property.DeclaringType == typeof(NewsEntry) && property.PropertyName == "PostedUtc")
		 {
			 property.ShouldSerialize = (x) => true;
			 property.PropertyName = "Posted";
		 }
		 else if (property.DeclaringType == typeof(NewsEntry) && property.PropertyName == "CollectedUtc")
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
		private string uriNews = @"http://www.minsktrans.by/ru/newsall/news/newscity.html";
		private string uriHotNews = @"http://www.minsktrans.by/ru/newsall/news/operativnaya-informatsiya.html";

		private string fileNameMonths = "months.txt";
		private string fileNameDays = "days.txt";
		public DateTime LastNewsTime { get; set; }
		public DateTime LastHotNewstime { get; set; }
		

		private string pathToSaveData = "";
		private string pathToSaveHotData = "";

		private List<NewsEntry> newDictionary;
		protected List<NewsEntry> hotNewsDictionary;

		private List<NewsEntry> allNews;
		protected List<NewsEntry> allHotNewsDictionary;

		public List<NewsEntry> NewNews
		{
			get
			{
				return allNews.OrderByDescending(key=> key.PostedUtc).ThenByDescending(key=> key.CollectedUtc).ToList();
			}
			set { OnPropertyChanged(); }
		}

		public List<NewsEntry> AllHotNews
		{
			get
			{
				return allHotNewsDictionary.OrderByDescending(key=> key.PostedUtc).ThenByDescending(key=> key.RepairedLineUtc).ToList();
			}
			set { OnPropertyChanged(); }
		}

		//
		public abstract DateTime LastUpdateMainNewsDateTimeUtc { get; set; }

		public abstract DateTime LastUpdateHotNewsDateTimeUtc { get; set; }

		public string FileNameMonths
		{
			get { return fileNameMonths; }
			set { fileNameMonths = value; }
		}

		public string FileNameDays
		{
			get { return fileNameDays; }
			set { fileNameDays = value; }
		}

		public string UriNews
		{
			get { return uriNews; }
			set { uriNews = value; }
		}

		public string UriHotNews
		{
			get { return uriHotNews; }
			set { uriHotNews = value; }
		}

		public string PathToSaveData
		{
			get { return pathToSaveData; }
			set { pathToSaveData = value; }
		}

		public string PathToSaveHotData
		{
			get { return pathToSaveHotData; }
			set { pathToSaveHotData = value; }
		}
		readonly FileHelperBase fileHelper;
		protected readonly InternetHelperBase internetHelper;

		FileHelperBase FileHelper { get { return fileHelper; } }

		public NewsManagerBase(FileHelperBase helper, InternetHelperBase internetHelper)
		{
			if (helper == null)
				throw new ArgumentNullException("helper");
			fileHelper = helper;
			this.internetHelper = internetHelper;
			LastNewsTime = new DateTime();
			newDictionary = new List<NewsEntry>();
			hotNewsDictionary = new List<NewsEntry>();
			allHotNewsDictionary = new List<NewsEntry>();
			allNews = new List<NewsEntry>();
#if DEBUG
			fileNameMonths = "monthsDebug.txt";
			fileNameDays = "daysDebug.txt";
#endif
		}

		public abstract Task<List<NewsEntry>> CheckAsync(string uri, string XpathSelectInfo, string XpathSelectDate);
		public async Task CheckMainNewsAsync()
		{
			newDictionary = (await CheckAsync(UriNews, "div/p", "div/dl/div/table/tr/td[1]")).Where(time => time.PostedUtc > new DateTime()).ToList();
			if (newDictionary.Count <= 0)
				return;
			LastNewsTime = newDictionary.Max(key=> key.PostedUtc);
			foreach (var source in newDictionary)
			{
				if (!allNews.Any(key=> key.PostedUtc == source.PostedUtc))
					allNews.Add(source);
			}
			NewNews = null;
		}

		public abstract Task CheckHotNewsAsync();


		public async Task Load()
		{
			allNews.Clear();
			string allLines = "";

			if (await FileHelper.FileExistAsync(TypeFolder.Local, FileNameMonths))
			{
				allLines = await FileHelper.ReadAllTextAsync(TypeFolder.Local, FileNameMonths);
				allNews.AddRange(JsonConvert.DeserializeObject<List<NewsEntry>>(allLines, new JsonSerializerSettings() { ContractResolver = new ShouldSerializeContractResolver() }));
			}
			NewNews = null;

			AllHotNews.Clear();
			if (await FileHelper.FileExistAsync(TypeFolder.Local, FileNameDays))
			{
				allLines = await FileHelper.ReadAllTextAsync(TypeFolder.Local, FileNameDays);
				allHotNewsDictionary.AddRange(JsonConvert.DeserializeObject<List<NewsEntry>>(allLines, new JsonSerializerSettings() { ContractResolver = new ShouldSerializeContractResolver() }));
			}

			hotNewsDictionary = null;
			AllHotNews = null;
		}

		async Task SaveToFile(string filePath, IEnumerable<NewsEntry> listToWrite)
		{
			if (!listToWrite.Any())
				return;

			string jsonString = JsonConvert.SerializeObject(listToWrite, new JsonSerializerSettings() { ContractResolver = new ShouldSerializeContractResolver() });
			await FileHelper.WriteTextAsync(TypeFolder.Local, filePath, jsonString);
		}

		public async Task SaveToFile()
		{
			DateTime curDateTime = DateTime.UtcNow;
			await SaveToFile(FileNameMonths, newDictionary.Where(x => x.PostedUtc.Month == curDateTime.Month || x.PostedUtc.Month == (curDateTime.Subtract(new TimeSpan(31, 0, 0, 0, 0)).Month)).ToList());
		}

		public async Task SaveToFileHotNews()
		{
			DateTime curDay = DateTime.UtcNow;
			var days = allHotNewsDictionary.Where(key => key.PostedUtc.Day == curDay.Day || (key.PostedUtc.Day == (curDay.Subtract(new TimeSpan(1, 0, 0, 0))).Day)).ToList();
			await SaveToFile(FileNameDays, days);
		}

		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			var handler = PropertyChanged;
			if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));	
		}
	}
}
