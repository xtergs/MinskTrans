using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using HtmlAgilityPack;
using MinskTrans.DesctopClient.Annotations;

namespace PushNotificationServer
{
	public class NewsManager : INotifyPropertyChanged
	{
		private string uriNews = @"http://www.minsktrans.by/ru/newsall/news/newscity.html";
		private string uriHotNews = @"http://www.minsktrans.by/ru/newsall/news/operativnaya-informatsiya.html";
		public DateTime LastNewsTime { get; set; }
		public DateTime LastHotNewstime { get; set; }
		

		private string pathToSaveData = "";
		private string pathToSaveHotData = "";

		private Dictionary<DateTime, string> newDictionary;
		private List<KeyValuePair<DateTime, string>> hotNewsDictionary;

		private Dictionary<DateTime, string> allNews;
		private List<KeyValuePair<DateTime, string>> allHotNewsDictionary;

		public List<KeyValuePair<DateTime, string>> NewNews
		{
			get
			{
				return allNews.OrderByDescending(key=> key.Key).ToList();
			}
			set { OnPropertyChanged(); }
		}

		public List<KeyValuePair<DateTime, string>> AllHotNews
		{
			get
			{
				return allHotNewsDictionary.OrderByDescending(key=> key.Key).ToList();
			}
			set { OnPropertyChanged(); }
		}

		public NewsManager()
		{
			LastNewsTime = new DateTime();
			newDictionary = new Dictionary<DateTime, string>();
			hotNewsDictionary = new List<KeyValuePair<DateTime, string>>();
			allHotNewsDictionary = new List<KeyValuePair<DateTime, string>>(new Dictionary<DateTime, string>());
			allNews = new Dictionary<DateTime, string>();
		}

		public static async Task<Dictionary<DateTime, string>>  CheckAsync(string uri, string XpathSelectInfo, string XpathSelectDate)
		{
			Dictionary<DateTime, string> returnDictionary = new Dictionary<DateTime, string>();
			string text = await Download(uri);
			HtmlAgilityPack.HtmlDocument document = new HtmlDocument();
			document.LoadHtml(text);

			var nodesNews = document.DocumentNode.SelectNodes("//*[@id=\"main\"]/div[2]/div");
			foreach (var nodesNew in nodesNews)
			{
				try
				{
					string dateNews = nodesNew.SelectSingleNode(XpathSelectDate).InnerText;
					dateNews = dateNews.Split('\n')[1].Trim();
					DateTime dateTimeNews = DateTime.Parse(dateNews);
					//if (dateTimeNews > LastDateTime)
					{
						string decodedString =
							HttpUtility.HtmlDecode(nodesNew.SelectSingleNode(XpathSelectInfo).InnerText)
								.Trim(' ', '\n', '\t')
								.Replace('\n', ' ')
								.Replace('\t', ' ')
								.Replace('\r', ' ');
						returnDictionary.Add(dateTimeNews, decodedString);
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
			newDictionary = (await CheckAsync(uriNews, "div/p", "div/dl/div/table/tr/td[1]")).Where(time => time.Key > LastNewsTime)
				.ToDictionary(key => key.Key, key => key.Value);
			if (newDictionary.Count <= 0)
				return;
			LastNewsTime = newDictionary.OrderByDescending(key => key.Key).Select(key=> key.Key).First();
			foreach (var source in newDictionary.Where(key=> !allNews.ContainsKey(key.Key)))
			{
				
				allNews.Add(source.Key, source.Value);
			}
			NewNews = null;
		}

		public async Task CheckHotNewsAsync()
		{
			List<KeyValuePair<DateTime, string>> returnDictionary = new List<KeyValuePair<DateTime, string>>();
			string text = await Download(uriHotNews);
			HtmlAgilityPack.HtmlDocument document = new HtmlDocument();
			document.LoadHtml(text);

			var nodesNews = document.DocumentNode.SelectNodes("//*[@id=\"main\"]/div[2]/div");
			foreach (var nodesNew in nodesNews)
			{
				try
				{
					string dateNews = nodesNew.SelectSingleNode("div/p/strong").InnerText;
					dateNews = dateNews.Trim('.').Split('\n')[0].Trim();
					DateTime dateTimeNews = DateTime.Parse(dateNews);
					//if (dateTimeNews > LastDateTime)
					{
						var firstLine = nodesNew.SelectSingleNode("div/p[2]");
						var secondLine = nodesNew.SelectSingleNode("div/p[3]");
						string allText = "";
						if (firstLine != null)
							allText += firstLine.InnerText;
						if (secondLine != null)
							allText += " " + secondLine.InnerText;
						string decodedString =
							HttpUtility.HtmlDecode(allText).Trim(' ', '\n', '\t').Replace('\n', ' ')
																				 .Replace('\t', ' ')
																				 .Replace('\r', ' ');
						returnDictionary.Add(new KeyValuePair<DateTime, string>(dateTimeNews, decodedString));
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
			foreach (var item in hotNewsDictionary)
			{
				if (allHotNewsDictionary.Any(key => key.Key == item.Key && item.Value.Contains(key.Value)) )
					allHotNewsDictionary.Remove(allHotNewsDictionary.Single(key => key.Key == item.Key && item.Value.Contains(key.Value)));

				allHotNewsDictionary.Add(new KeyValuePair<DateTime, string>(item.Key, item.Value));

			}
			if (hotNewsDictionary.Count <= 0)
				return;
			allHotNewsDictionary = new List<KeyValuePair<DateTime, string>>(allHotNewsDictionary.OrderByDescending(key => key.Key));
			LastHotNewstime = allHotNewsDictionary.Select(key => key.Key).First();
			hotNewsDictionary.Clear();
			AllHotNews = null;
		}

		
		public void Load()
		{
			allNews.Clear();
			string path = "";
			for (int i = 0; i < 12; i++)
			{
				path = Path.Combine(pathToSaveData, i.ToString() + ".txt");
				if (!File.Exists(path))
					continue;
				var allLines = File.ReadAllLines(path).Where(str=> str.Length>0).ToArray();
				for (int j = 0; j < allLines.Length; j += 2)
				{
					try
					{
						allNews.Add(DateTime.Parse(allLines[j]), allLines[j + 1]);
					}
					catch (FormatException e)
					{
						j++;
						allNews.Add(DateTime.Parse(allLines[j]), allLines[j + 1]);
					}
				}
			}
			NewNews = null;

			AllHotNews.Clear();
			path = "";
			for (int i = 0; i < 32; i++)
			{
				path = Path.Combine(pathToSaveHotData, i.ToString() + "_.txt");
				if (!File.Exists(path))
					continue;
				var allLines = File.ReadAllLines(path).Where(str => str.Length > 0).ToArray();
				for (int j = 0; j < allLines.Length; j += 2)
				{
					try
					{
						var keyValuePair = new KeyValuePair<DateTime, string>(DateTime.Parse(allLines[j]), allLines[j + 1]);
						allHotNewsDictionary.Add(keyValuePair);
					}
					catch (FormatException e)
					{
						j++;
						allHotNewsDictionary.Add(new KeyValuePair<DateTime, string>(DateTime.Parse(allLines[j]), allLines[j + 1]));
					}
				}
			}
			hotNewsDictionary = null;
			AllHotNews = null;
		}

		public void SaveToFile()
		{
			if (newDictionary.Count <= 0)
				return;
			var months = newDictionary.GroupBy(key => key.Key.Month);
			string path;
			foreach (var month in months)
			{
				path = Path.Combine(pathToSaveData, month.Key.ToString() + ".txt");
				if (!File.Exists(path))
					File.Create(path).Dispose();
				
				using (var writter = File.AppendText(path))
				{
					foreach (var source in month.OrderBy(key => key.Key))
					{
						string text = source.Value.Trim(' ', '\n', '\t').Replace('\n', ' ').Replace('\t', ' ').Replace('\r', ' ');
						writter.WriteLine(source.Key);
						writter.WriteLine(text);
					}
				}

			}

			newDictionary.Clear();
		}

		public void SaveToFileHotNews()
		{
			if (allHotNewsDictionary.Count <= 0)
				return;
			DateTime curDay = DateTime.Now;

			var days = allHotNewsDictionary.Where(key=> key.Key.Day == curDay.Day || (key.Key.Day == (curDay.Subtract(new TimeSpan(1,0,0,0))).Day) ).GroupBy(key => key.Key.Day);
			string path;
			foreach (var day in days)
			{
				path = Path.Combine(pathToSaveHotData, day.Key.ToString() + "_.txt");
				

				using (var writter = File.CreateText(path))
				{
					foreach (var source in day.OrderBy(key => key.Key))
					{
						string text = source.Value.Trim(' ', '\n', '\t').Replace('\n', ' ').Replace('\t', ' ').Replace('\r', ' ');
						writter.WriteLine(source.Key);
						writter.WriteLine(text);
					}
				}

			}

			//var lastDay = days.OrderByDescending(x => x.Key).FirstOrDefault();
		}

		static private async Task<string> Download(string uri)
		{
			try
			{
				var httpClient = new HttpClient();
				// Increase the max buffer size for the response so we don't get an exception with so many web sites

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
