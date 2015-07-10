using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HtmlAgilityPack;
using System.Net;
using MyLibrary;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace PushNotificationServer
{
		static class HtmlDecoder
		{
			public static string DecodeHtml(this string str)
			{
				return WebUtility.HtmlDecode(str);
			}
		}

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

	public class NewsManager : INotifyPropertyChanged
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
		private List<NewsEntry> hotNewsDictionary;

		private List<NewsEntry> allNews;
		private List<NewsEntry> allHotNewsDictionary;

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
		readonly InternetHelperBase internetHelper;

		FileHelperBase FileHelper { get { return fileHelper; } }

		public NewsManager(FileHelperBase helper, InternetHelperBase internetHelper)
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

		public async Task<List<NewsEntry>>  CheckAsync(string uri, string XpathSelectInfo, string XpathSelectDate)
		{
			List<NewsEntry> returnDictionary = new List<NewsEntry>();
			string text = await internetHelper.Download(uri);
			HtmlDocument document = new HtmlDocument();
			document.LoadHtml(text);

			var nodesNews = document.DocumentNode.SelectNodes("//*[@id=\"main\"]/div[2]/div");
			foreach (var nodesNew in nodesNews)
			{
				try
				{
					var node = nodesNew.SelectSingleNode(XpathSelectDate);
					if (node == null)
						continue;
					string dateNews = node.InnerText.DecodeHtml();
					dateNews = dateNews.Split('\n')[1].Trim();
					DateTime dateTimeNews = DateTime.Parse(dateNews);
					//if (dateTimeNews > LastDateTime)

					var nodes = nodesNew.SelectNodes(XpathSelectInfo);
					if (nodes == null)
						continue;
					StringBuilder builder = new StringBuilder();
					foreach (var selectNode in nodes)
					{
						builder.Append(selectNode.InnerText.DecodeHtml().Trim());
					}
					var decodedString = builder.Replace("  ", " ").Replace("  ", " ").ToString().Trim();

					returnDictionary.Add(new NewsEntry()
					{
						PostedUtc = dateTimeNews,
						Message = decodedString,
						CollectedUtc = DateTime.UtcNow
					});
					//allNews.Add(dateTimeNews, decodedString);
				}
				catch (NullReferenceException e)
				{

				}
			}
			return returnDictionary;
		}
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

		public async Task CheckHotNewsAsync()
		{
			List<NewsEntry> returnDictionary = new List<NewsEntry>();
			string text = await internetHelper.Download(UriHotNews);
			HtmlDocument document = new HtmlDocument();
			document.LoadHtml(text);

			var nodesNews = document.DocumentNode.SelectNodes("//*[@id=\"main\"]/div[2]/div");
			if (nodesNews == null)
				return;
			foreach (var nodesNew in nodesNews)
			{
				try
				{
					var node = nodesNew.SelectSingleNode("div/p/strong");
					if (node == null)
						continue;
					string dateNews = node.InnerText.DecodeHtml();
					dateNews = dateNews.Trim('.').Split('\n')[0].Trim();
					DateTime dateTimeNews = DateTime.Parse(dateNews).ToUniversalTime();
					var firstLine = nodesNew.SelectNodes("div/p").Skip(1).ToList();
					if (firstLine == null)
						continue;
					//var secondLine = nodesNew.SelectSingleNode("div/p[3]");
					StringBuilder builder = new StringBuilder();
					foreach (var htmlNode in firstLine)
					{
						builder.Append(htmlNode.InnerText.DecodeHtml());
					}
					var allText = builder.Replace("  ", " ").Replace("  ", " ").ToString().Trim();
					string possibleRepairTime =
						firstLine.Select(x => x.InnerText.DecodeHtml().Trim())
							.Last(x => !String.IsNullOrWhiteSpace(x))
							.Replace('.', ' ')
							.Replace(" ", "")
							.Trim();
					var match = Regex.Match(possibleRepairTime.Substring(possibleRepairTime.Length/2), @"[0-2]?[0-9][-:][0-6][0-9]",
					RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);
					DateTime possibleDateTime;
					string decodedString = allText.Replace("  ", " ").Trim();
					if (DateTime.TryParse(match.Value.Replace('-', ':'), out possibleDateTime))
						returnDictionary.Add(new NewsEntry(dateTimeNews, decodedString, dateTimeNews.Add(possibleDateTime.TimeOfDay)));
						//dateTimeNews = dateTimeNews.Add(possibleDateTime.TimeOfDay);
					else
					{
						
						returnDictionary.Add(new NewsEntry(dateTimeNews, decodedString));
					}
					
				}
				catch (NullReferenceException e)
				{

				}
				catch (Exception e)
				{

					throw;
				}
			}
			hotNewsDictionary = returnDictionary;
			foreach (var newsEntry in hotNewsDictionary)
			{
				if (
					AllHotNews.Any(
						key =>
							key.PostedUtc == newsEntry.PostedUtc && key.RepairedLineUtc == newsEntry.RepairedLineUtc &&
							key.Message.Length == newsEntry.Message.Length && key.Message == newsEntry.Message))
					continue;
				var tempNode =
					AllHotNews.FirstOrDefault(
						key =>
							key.PostedUtc == newsEntry.PostedUtc && key.RepairedLineUtc != newsEntry.RepairedLineUtc &&
							(newsEntry.Message.ToLowerInvariant().Contains(key.Message.ToLowerInvariant()) && key.Message.Length != newsEntry.Message.Length));

				if (tempNode.Message != null)
				{
					allHotNewsDictionary.Remove(tempNode);
					allHotNewsDictionary.Add(newsEntry);
				}
				else

				//if (tempNode.RepairedLIne == default(DateTime) && newsEntry.RepairedLIne != default(DateTime))
				//{
				//}
					allHotNewsDictionary.Add(newsEntry);
			}

			LastHotNewstime = allHotNewsDictionary.Max(key =>key.CollectedUtc);
			hotNewsDictionary.Clear();
			AllHotNews = null;
		}


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
