using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.Storage;
using MinskTrans.DesctopClient.Modelview;
using MinskTrans.Universal.Annotations;
using MyLibrary;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace CommonLibrary
{
	public class ShouldSerializeContractResolver : DefaultContractResolver
	{
		public new static readonly ShouldSerializeContractResolver Instance = new ShouldSerializeContractResolver();

		protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
		{
			JsonProperty property = base.CreateProperty(member, memberSerialization);

			if (property.DeclaringType == typeof(NewsEntry) && property.PropertyName == "Message")
			{
				property.ShouldSerialize = (x) => true;
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
			else if (property.DeclaringType == typeof(NewsEntry) && property.PropertyName == "RepairedLineUtc")
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
	public class NewsManager :INotifyPropertyChanged
	{
		[Flags]
		public enum TypeLoad
		{
			LoadAll = LoadHotNews | LoadNews,
			LoadNews = 0x000001,
			LoadHotNews = 0x000002
		}
		private List<NewsEntry> newDictionary;
		private List<NewsEntry> hotNewsDictionary;

		private List<NewsEntry> allNews;
		private List<NewsEntry> allHotNewsDictionary;

		public string pathToSaveData = "";
		public string pathToSaveHotData = "";

		public string fileNameNews = "months.txt";
		public string fileNameHotNews = "days.txt";

		public NewsManager()
		{
			allHotNewsDictionary = new List<NewsEntry>();
			allNews = new List<NewsEntry>();
			newDictionary = new List<NewsEntry>();
			hotNewsDictionary = new List<NewsEntry>();
		}


		private ApplicationSettingsHelper lastUpdateDataDateTimeBack;
		public DateTime LastUpdateMainNewsDateTime
		{
#if WINDOWS_PHONE_APP || WINDOWS_UAP
			get
			{
				if (lastUpdateDataDateTimeBack == null)
					lastUpdateDataDateTimeBack = new ApplicationSettingsHelper();
				return lastUpdateDataDateTimeBack.DateTimeSettings;
			}

			set
			{
				if (lastUpdateDataDateTimeBack == null)
					lastUpdateDataDateTimeBack = new ApplicationSettingsHelper();
				lastUpdateDataDateTimeBack.DateTimeSettings = value;
				OnPropertyChanged();
			}
#else
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
#endif
		}

		private ApplicationSettingsHelper lastUpdateHotDataDateTimeBack;
		public DateTime LastUpdateHotNewsDateTime
		{
#if WINDOWS_PHONE_APP || WINDOWS_UAP
			get
			{
				if (lastUpdateHotDataDateTimeBack == null)
					lastUpdateHotDataDateTimeBack = new ApplicationSettingsHelper();
				return lastUpdateHotDataDateTimeBack.DateTimeSettings;
			}

			set
			{
				if (lastUpdateHotDataDateTimeBack == null)
					lastUpdateHotDataDateTimeBack = new ApplicationSettingsHelper();
				lastUpdateHotDataDateTimeBack.DateTimeSettings = value;
				OnPropertyChanged();
			}
#else
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
#endif
		}

		public List<NewsEntry> NewNews
		{
			get
			{
				return allNews.OrderByDescending(key => key.PostedUtc).ToList();
			}
			set { OnPropertyChanged(); }
		}

		public List<NewsEntry> AllHotNews
		{
			get
			{
				return allHotNewsDictionary.OrderByDescending(key => key.PostedUtc).ToList();
			}
			set { OnPropertyChanged(); }
		}

#pragma warning disable 1998
		public async Task LoadNews(string jsonData)
#pragma warning restore 1998
		{
			allNews.Clear();
			allNews.AddRange(JsonConvert.DeserializeObject<List<NewsEntry>>(jsonData, new JsonSerializerSettings(){ContractResolver = new ShouldSerializeContractResolver()}));
			NewNews = null;
		}

		public async Task Load(TypeLoad type = TypeLoad.LoadAll, string fileMain = null, string fileHot = null)
		{
			//allNews.Clear();
			if (type.HasFlag(TypeLoad.LoadNews))
			{
				if (String.IsNullOrWhiteSpace(fileMain))
					fileMain = fileNameNews;
				var path = Path.Combine(pathToSaveData, fileMain);
				try
				{
					var file = await ApplicationData.Current.LocalFolder.GetFileAsync(path);
					var allLines = await FileIO.ReadTextAsync(file);
					await LoadNews(allLines);
				}
				catch (FileNotFoundException)
				{

				}
			}
			//NewNews = null;

			if (type.HasFlag(TypeLoad.LoadHotNews))
			{
				allHotNewsDictionary.Clear();
				if (string.IsNullOrWhiteSpace(fileHot))
					fileHot = fileNameHotNews;
				var path = Path.Combine(pathToSaveHotData, fileHot);

				try
				{
					var file = await ApplicationData.Current.LocalFolder.GetFileAsync(path);
					var allLines = await FileIO.ReadTextAsync(file);
					allHotNewsDictionary.AddRange(JsonConvert.DeserializeObject<List<NewsEntry>>(allLines, new JsonSerializerSettings(){ContractResolver = new ShouldSerializeContractResolver()}));
				}
				catch (FileNotFoundException)
				{

				}


				AllHotNews = null;
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			var handler = PropertyChanged;
			if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
