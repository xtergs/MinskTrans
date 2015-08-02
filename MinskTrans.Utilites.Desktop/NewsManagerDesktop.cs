﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HtmlAgilityPack;
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
		public override async Task CheckHotNewsAsync()
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
					var match = Regex.Match(possibleRepairTime.Substring(possibleRepairTime.Length / 2), @"[0-2]?[0-9][-:][0-6][0-9]",
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

			LastHotNewstime = allHotNewsDictionary.Max(key => key.CollectedUtc);
			hotNewsDictionary.Clear();
			AllHotNews = null;
		}

		public override async Task<List<NewsEntry>> CheckAsync(string uri, string XpathSelectInfo, string XpathSelectDate)
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

		public NewsManagerDesktop(FileHelperBase helper, InternetHelperBase internetHelper) : base(helper, internetHelper)
		{
		}
	}
}