using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HtmlAgilityPack;
using MetroLog;
using MinskTrans.Context;
using MinskTrans.Net;
using MinskTrans.Utilites.Base.IO;
using MinskTrans.Utilites.Base.Net;
using MyLibrary;

namespace MinskTrans.Utilites.Desktop
{
	public static class HtmlDecoder
	{
		public static string DecodeHtml(this string str)
		{
			return WebUtility.HtmlDecode(str);
		}
	}

	public class NewsManagerDesktop : NewsManagerBase
	{
		private ILogger log;

	    private async Task<List<NewsEntry>> GetHotNewsEntries(string link)
	    {
            List<NewsEntry> returnDictionary = new List<NewsEntry>();
            string text = await internetHelper.Download(link);
            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(text);
            log.Debug($"html downloaded and loaded");
            var nodesNews = document.DocumentNode.SelectNodes("//*[@id=\"main\"]/div[2]/div");
            if (nodesNews == null)
            {
                log.Error("CheckHotNewsAsync: Don't have nodes with news");
                return null;
            }
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
                    log.Debug($"CheckHotNewsAsync: Get news text: {allText}");
                    string possibleRepairTime =
                        firstLine.Select(x => x.InnerText.DecodeHtml().Trim())
                            .Last(x => !String.IsNullOrWhiteSpace(x))
                            .Replace('.', ' ')
                            .Replace(" ", "")
                            .Trim();
                    var match = Regex.Match(possibleRepairTime.Substring(possibleRepairTime.Length / 2), @"[0-2]?[0-9][-:][0-6][0-9]",
                    RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);
                    DateTime possibleDateTime;
                    string decodedString = allText.Replace("  ", " ").Trim();
                    if (DateTime.TryParse(match.Value.Replace('-', ':'), out possibleDateTime))
                    {
                        log.Debug($"Posible repair time: {possibleDateTime.TimeOfDay}");
                        returnDictionary.Add(new NewsEntry(dateTimeNews, decodedString, dateTimeNews.Add(possibleDateTime.TimeOfDay)));
                    }
                    //dateTimeNews = dateTimeNews.Add(possibleDateTime.TimeOfDay);
                    else
                    {

                        returnDictionary.Add(new NewsEntry(dateTimeNews, decodedString));
                    }

                }
                catch (NullReferenceException e)
                {
                    log.Error("CheckHotNewsAsync", e);
                    return null;
                }
                catch (Exception e)
                {
                    log.Fatal("CheckHotNewsAsync", e);
                    throw;
                }
            }
	        return returnDictionary;
	    } 

		public override async Task<bool> CheckHotNewsAsync()
		{
			log.Debug("CheckHotNewsAsync started");
		    var returnDictionary = await GetHotNewsEntries(Files.HotNewsFile.OriginalLink);
           
            bool flagResult = false;

		    List<NewsEntry> newsToAdd = new List<NewsEntry>();
			foreach (var newsEntry in returnDictionary)
			{
				if (
                    allHotNewsDictionary.NewsEntries.Any(
						key =>
							key.PostedUtc == newsEntry.PostedUtc && key.RepairedLineUtc == newsEntry.RepairedLineUtc &&
							key.Message.Length == newsEntry.Message.Length && key.Message == newsEntry.Message))
				{
					continue;
				}
				var tempNode =
                    allHotNewsDictionary.NewsEntries.FirstOrDefault(
						key =>
							key.PostedUtc == newsEntry.PostedUtc && key.RepairedLineUtc != newsEntry.RepairedLineUtc &&
							(newsEntry.Message.ToLowerInvariant().Contains(key.Message.ToLowerInvariant()) && key.Message.Length != newsEntry.Message.Length));

				if (tempNode.Message != null)
				{
					allHotNewsDictionary.NewsEntries.Remove(tempNode);
				}
                newsToAdd.Add(newsEntry);
			    flagResult = true;
			}
		    if (newsToAdd.Count == 0)
		        return false;
		    allHotNewsDictionary.NewsEntries.AddRange(newsToAdd);
            allHotNewsDictionary.LastUpdateDateTimeUtc = newsToAdd.Max(key => key.CollectedUtc);
			log.Info($"{nameof(allHotNewsDictionary.LastUpdateDateTimeUtc)} is {allHotNewsDictionary.LastUpdateDateTimeUtc}");
			//hotNewsDictionary.Clear();
			AllHotNews = null;
			log.Trace("CheckHotNews ended");
		    return flagResult;
		}

        public override async Task<List<NewsEntry>> CheckAsync(string uri, string XpathSelectInfo, string XpathSelectDate)
		{
			log.Trace("CheckAsync started");
			List<NewsEntry> returnDictionary = new List<NewsEntry>();
			string text = await internetHelper.Download(uri);
			HtmlDocument document = new HtmlDocument();
			document.LoadHtml(text);
			log.Debug($"html downloaded and loaded");
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
					DateTime dateTimeNews = DateTime.Parse(dateNews).ToUniversalTime();
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
					log.Debug($"main news decodedString: {decodedString}");
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
					log.Error("CheckNews", e);
				}
			}
			log.Trace("CheckNews ended");
			return returnDictionary;
		}

		public NewsManagerDesktop(FileHelperBase helper, InternetHelperBase internetHelper, ILogManager logManager, FilePathsSettings files, INewsContext context) : base(helper, internetHelper, logManager, files, context)
		{
			log = logManager.GetLogger<NewsManagerDesktop>();
		}
	}
}
