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
using Newtonsoft.Json;
using HtmlAgilityPack;
using MinskTrans.DesctopClient.Annotations;

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
			HtmlAgilityPack.HtmlDocument document = new HtmlDocument();
			document.LoadHtml(text);

			var nodesNews = document.DocumentNode.SelectNodes("//*[@id=\"main\"]/div[2]/div");
			foreach (var nodesNew in nodesNews)
			{
				try
				{
					string dateNews = nodesNew.SelectSingleNode(XpathSelectDate).InnerText.DecodeHtml();
					dateNews = dateNews.Split('\n')[1].Trim();
					DateTime dateTimeNews = DateTime.Parse(dateNews);
					//if (dateTimeNews > LastDateTime)
					{
						StringBuilder builder = new StringBuilder();
						string decodedString;
						foreach (var selectNode in nodesNew.SelectNodes(XpathSelectInfo))
						{
							builder.Append(selectNode.InnerText.DecodeHtml().Trim());
						}
						decodedString = builder.ToString().Trim();
							
						returnDictionary.Add(new NewsEntry()
						{
							Posted = dateTimeNews,
							Message = decodedString,
							Collected = DateTime.Now
						});
						//allNews.Add(dateTimeNews, decodedString);
					}

				}
				catch (NullReferenceException e)
				{

				}
			}
			return returnDictionary;
		}
		public async Task CheckMainNewsAsync()
		{
			newDictionary = (await CheckAsync(uriNews, "div/p", "div/dl/div/table/tr/td[1]")).Where(time => time.Posted > new DateTime()).ToList();
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
			string text = await Download(uriHotNews);
			HtmlAgilityPack.HtmlDocument document = new HtmlDocument();
			document.LoadHtml(text);

			var nodesNews = document.DocumentNode.SelectNodes("//*[@id=\"main\"]/div[2]/div");
			foreach (var nodesNew in nodesNews)
			{
				try
				{
					string dateNews = nodesNew.SelectSingleNode("div/p/strong").InnerText.DecodeHtml();
					dateNews = dateNews.Trim('.').Split('\n')[0].Trim();
					DateTime dateTimeNews = DateTime.Parse(dateNews);
					//if (dateTimeNews > LastDateTime)
					{
						var firstLine = nodesNew.SelectNodes("div/p").Skip(1).ToList();
						//var secondLine = nodesNew.SelectSingleNode("div/p[3]");
						StringBuilder builder = new StringBuilder();
						string allText;
						foreach (var htmlNode in firstLine)
						{
							builder.Append(htmlNode.InnerText.DecodeHtml());
						}
						allText = builder.ToString();
						var possibleRepairTime = firstLine.Last().InnerText.DecodeHtml().Replace('.',' ').Trim();
						var match = Regex.Match(possibleRepairTime.Substring(possibleRepairTime.Length/2), @"[0-2]?[0-9][-:][0-6][0-9]",
							RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);
						DateTime possibleDateTime;
						if (DateTime.TryParse(match.Value, out possibleDateTime))
							;
						else
						{
							possibleDateTime = default (DateTime);
						}
						//if (firstLine != null)
						//	allText += firstLine.InnerText;
						//if (secondLine != null)
						//	allText += " " + secondLine.InnerText;
						string decodedString = allText.Trim();
						returnDictionary.Add(new NewsEntry(dateTimeNews, decodedString,dateTimeNews.Add(possibleDateTime.TimeOfDay)));
						//allNews.Add(dateTimeNews, decodedString);
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
							(newsEntry.RepairedLIne == key.RepairedLIne || newsEntry.Message.Contains(key.Message)));
				if (tempNode.Collected != default(DateTime))
				{
					allHotNewsDictionary.Remove(tempNode);
				}
				allHotNewsDictionary.Add(newsEntry);
			}
			//foreach (var item in hotNewsDictionary)
			//{
			//	if (allHotNewsDictionary.Any(key => key.Key == item.Key && item.Value.Contains(key.Value)) )
			//		allHotNewsDictionary.Remove(allHotNewsDictionary.Single(key => key.Key == item.Key && item.Value.Contains(key.Value)));

			//	allHotNewsDictionary.Add(new KeyValuePair<DateTime, string>(item.Key, item.Value));

			//}
			//if (hotNewsDictionary.Count <= 0)
			//	return;
			//allHotNewsDictionary = new List<KeyValuePair<DateTime, string>>(allHotNewsDictionary.OrderByDescending(key => key.Key));
			//LastHotNewstime = allHotNewsDictionary.Select(key => key.Key).First();
			hotNewsDictionary.Clear();
			AllHotNews = null;
		}


		public void Load()
		{
			allNews.Clear();
			string path = "";

			path = Path.Combine(pathToSaveData, fileNameMonths);
			string allLines = "";
			if (File.Exists(path))
			{
				allLines = File.ReadAllText(path);
				NewNews.AddRange(JsonConvert.DeserializeObject<List<NewsEntry>>(allLines));
			}

			NewNews = null;

			AllHotNews.Clear();
			path = "";

			path = Path.Combine(pathToSaveHotData, fileNameDays);
			if (File.Exists(path))

			{
				allLines = File.ReadAllText(path);
				allHotNewsDictionary.AddRange(JsonConvert.DeserializeObject<List<NewsEntry>>(allLines));
			}

			hotNewsDictionary = null;
			AllHotNews = null;
		}

		public void SaveToFile()
		{
			if (newDictionary.Count <= 0)
				return;
			DateTime curDateTime = DateTime.Now;
			var months = newDictionary.Where(x=> x.Posted.Month == curDateTime.Month || x.Posted.Month == (curDateTime.Subtract(new TimeSpan(31,0,0,0,0)).Month )).ToList();
			string path;
			path = Path.Combine(pathToSaveData, fileNameMonths);
			string jsonString = JsonConvert.SerializeObject(months);
			File.WriteAllText(path, jsonString);

			newDictionary.Clear();
		}

		public void SaveToFileHotNews()
		{
			if (allHotNewsDictionary.Count <= 0)
				return;
			DateTime curDay = DateTime.Now;

			var days = allHotNewsDictionary.Where(key=> key.Posted.Day == curDay.Day || (key.Posted.Day == (curDay.Subtract(new TimeSpan(1,0,0,0))).Day) ).ToList();
			string path;

			path = Path.Combine(pathToSaveHotData, fileNameDays);
			string jsonString = JsonConvert.SerializeObject(days);

			File.WriteAllText(path, jsonString);
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

				//string str= response.StatusCode + " " + response.ReasonPhrase + Environment.NewLine;

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
