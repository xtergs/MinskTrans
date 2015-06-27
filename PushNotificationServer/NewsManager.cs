using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using HtmlAgilityPack;
using MinskTrans.DesctopClient.Annotations;
using MyLibrary;
using Newtonsoft.Json;

namespace PushNotificationServer
{
		static class HtmlDecoder
		{
			public static string DecodeHtml(this string str)
			{
				return HttpUtility.HtmlDecode(str);
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
				return allNews.OrderByDescending(key=> key.Posted).ThenByDescending(key=> key.Collected).ToList();
			}
			set { OnPropertyChanged(); }
		}

		public List<NewsEntry> AllHotNews
		{
			get
			{
				return allHotNewsDictionary.OrderByDescending(key=> key.Posted).ThenByDescending(key=> key.Collected).ToList();
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

		public NewsManager()
		{
			LastNewsTime = new DateTime();
			newDictionary = new List<NewsEntry>();
			hotNewsDictionary = new List<NewsEntry>();
			allHotNewsDictionary = new List<NewsEntry>();
			allNews = new List<NewsEntry>();
		}

		public static async Task<List<NewsEntry>>  CheckAsync(string uri, string XpathSelectInfo, string XpathSelectDate)
		{
			List<NewsEntry> returnDictionary = new List<NewsEntry>();
			string text = await Download(uri);
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
					var decodedString = builder.Replace("  ", " ").ToString().Trim();

					returnDictionary.Add(new NewsEntry()
					{
						Posted = dateTimeNews,
						Message = decodedString,
						Collected = DateTime.UtcNow
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
			newDictionary = (await CheckAsync(UriNews, "div/p", "div/dl/div/table/tr/td[1]")).Where(time => time.Posted > new DateTime()).ToList();
			if (newDictionary.Count <= 0)
				return;
			LastNewsTime = newDictionary.Max(key=> key.Posted);
			foreach (var source in newDictionary)
			{
				if (!allNews.Any(key=> key.Posted == source.Posted))
					allNews.Add(source);
			}
			NewNews = null;
		}

		public async Task CheckHotNewsAsync()
		{
			List<NewsEntry> returnDictionary = new List<NewsEntry>();
			string text = await Download(UriHotNews);
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
					DateTime dateTimeNews = DateTime.Parse(dateNews);
					var firstLine = nodesNew.SelectNodes("div/p").Skip(1).ToList();
					if (firstLine == null)
						continue;
					//var secondLine = nodesNew.SelectSingleNode("div/p[3]");
					StringBuilder builder = new StringBuilder();
					foreach (var htmlNode in firstLine)
					{
						builder.Append(htmlNode.InnerText.DecodeHtml());
					}
					var allText = builder.Replace("  ", " ").ToString();
					var possibleRepairTime = firstLine.Last().InnerText.DecodeHtml().Replace('.', ' ').Trim();
					var match = Regex.Match(possibleRepairTime.Substring(possibleRepairTime.Length/2), @"[0-2]?[0-9][-:][0-6][0-9]",
					RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);
					DateTime possibleDateTime;
					string decodedString = allText.Trim();
					if (DateTime.TryParse(match.Value, out possibleDateTime))
						returnDictionary.Add(new NewsEntry(dateTimeNews, decodedString, dateTimeNews.Add(possibleDateTime.ToUniversalTime().TimeOfDay)));
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
				var tempNode =
					AllHotNews.FirstOrDefault(
						key =>
							key.Posted == newsEntry.Posted &&
							(newsEntry.RepairedLIne == key.RepairedLIne && newsEntry.Message.Contains(key.Message)));
				if (tempNode.Collected != default(DateTime))
				{
					allHotNewsDictionary.Remove(tempNode);
				}
				allHotNewsDictionary.Add(newsEntry);
			}

			LastHotNewstime = allHotNewsDictionary.Max(key =>key.Collected);
			hotNewsDictionary.Clear();
			AllHotNews = null;
		}


		public void Load()
		{
			allNews.Clear();
			string path = Path.Combine(PathToSaveData, FileNameMonths);
			string allLines = "";
			if (File.Exists(path))
			{
				allLines = File.ReadAllText(path);
				allNews.AddRange(JsonConvert.DeserializeObject<List<NewsEntry>>(allLines));
			}

			NewNews = null;

			AllHotNews.Clear();
			path = Path.Combine(PathToSaveHotData, FileNameDays);
			if (File.Exists(path))
			{
				allLines = File.ReadAllText(path);
				allHotNewsDictionary.AddRange(JsonConvert.DeserializeObject<List<NewsEntry>>(allLines));
			}

			hotNewsDictionary = null;
			AllHotNews = null;
		}

		void SaveToFile(string filePath, IEnumerable<NewsEntry> listToWrite)
		{
			if (!listToWrite.Any())
				return;
			
			string jsonString = JsonConvert.SerializeObject(listToWrite);
			File.WriteAllText(filePath, jsonString);
		}

		public void SaveToFile()
		{
			DateTime curDateTime = DateTime.UtcNow;
			SaveToFile(Path.Combine(PathToSaveData, FileNameMonths), newDictionary.Where(x => x.Posted.Month == curDateTime.Month || x.Posted.Month == (curDateTime.Subtract(new TimeSpan(31, 0, 0, 0, 0)).Month)).ToList());
		}

		public void SaveToFileHotNews()
		{
			DateTime curDay = DateTime.UtcNow;
			var days = allHotNewsDictionary.Where(key=> key.Posted.Day == curDay.Day || (key.Posted.Day == (curDay.Subtract(new TimeSpan(1,0,0,0))).Day) ).ToList();
			var path = Path.Combine(PathToSaveHotData, FileNameDays);
			SaveToFile(path, days);
		}

		static private async Task<string> Download(string uri)
		{
			try
			{
				var httpClient = new HttpClient();
				// Increase the max buffer size for the response so we don't get an exception with so many web sites

				httpClient.Timeout = new TimeSpan(0, 0, 10, 0);
				httpClient.MaxResponseContentBufferSize = 256000;
				httpClient.DefaultRequestHeaders.Add("user-agent",
					"Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2; WOW64; Trident/6.0)");

				HttpResponseMessage response = await httpClient.GetAsync(new Uri(uri));
				response.EnsureSuccessStatusCode();

				return await response.Content.ReadAsStringAsync();
			}
			catch (Exception e)
			{
#if BETA
				Logger.Log().WriteLineTime("Can't download " + uri).WriteLine(e.Message).WriteLine(e.StackTrace);
#endif
				throw new TaskCanceledException(e.Message, e);
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
