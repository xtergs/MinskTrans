using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LiteDB;
using MinskTrans.Net;
using MinskTrans.Utilites.Base.IO;

namespace PushNotificationServer.NewsContext
{

	internal class NoSqlNewsContext : INewsContext
	{
		public string[] supportedVersions => new[] { "4" };
		public ListWithDate MainNews { get; private set; }
		public ListWithDate HotNews { get; private set; }

		public Task LoadDataAsync(TypeFolder folder, string file)
		{
			return Task.Run(() =>
			{
				try
				{
					using (var db = new LiteDatabase(@"MyData.db"))
					{
						var customers = db.GetCollection<ListWithDate>(nameof(MainNews));
						if (customers.Count() > 0)
						{
							MainNews = customers.FindOne(Query.All(Query.Descending));
							MainNews.NewsEntries = FixDatestimes(MainNews.NewsEntries);
						}
						else
							MainNews = new ListWithDate();
						customers = db.GetCollection<ListWithDate>(nameof(HotNews));
						if (customers.Count() > 0)
						{
							HotNews = customers.FindOne((Query.All(Query.Descending)));
							HotNews.NewsEntries = FixDatestimes(HotNews.NewsEntries);
						}
						else
							HotNews = new ListWithDate();

					}
				}
				catch (Exception ex)
				{
					HotNews = new ListWithDate();
					MainNews = new ListWithDate();
				}
			});
		}

		private List<NewsEntry> FixDatestimes(List<NewsEntry> entries)
		{
			return entries.Select(x =>
			{
				x.PostedUtc = x.PostedUtc.ToUniversalTime();	 //new DateTime(x.PostedUtc.Ticks, DateTimeKind.Utc);
				x.CollectedUtc = x.CollectedUtc.ToUniversalTime();	 //new DateTime(x.CollectedUtc.Ticks, DateTimeKind.Utc);
				x.RepairedLineUtc = x.RepairedLineUtc.ToUniversalTime(); //new DateTime(x.RepairedLineUtc.Ticks, DateTimeKind.Utc);

				return x;
			}).ToList();
		}

		public async Task LoadDataAsync(ListWithDate mainnews, ListWithDate hotnews)
		{
			MainNews = mainnews;
			HotNews = hotnews;
		}

		public Task Save(TypeFolder folder, string file)
		{
			return Task.Run(() =>
			{
				using (var db = new LiteDatabase(@"MyData.db"))
				{
					using (var trans = db.BeginTrans())
					{
						WriteToDb(db, MainNews, nameof(MainNews));
						WriteToDb(db, HotNews, nameof(HotNews));
						trans.Commit();
					}
				}
			});
		}

		private void FlatterPreviousRecords(LiteCollection<ListWithDate> collection)
		{
			int count = collection.Count();
			if (count > 2)
			{
				var previous = collection.FindOne(x => x.Id == count - 1);
				var secondPrev = collection.FindOne(x => x.Id == count);
				var diff = secondPrev.NewsEntries.Except(previous.NewsEntries).ToList();
				previous.NewsEntries = diff;
				collection.Update(previous);
			}
		}

		private void WriteToDb(LiteDatabase db, ListWithDate news, string  collectionName)
		{
			var customers = db.GetCollection<ListWithDate>(collectionName);
			if (!customers.Exists(x => x.LastUpdateDateTimeUtc == news.LastUpdateDateTimeUtc))
			{
				int count = customers.Count();
				FlatterPreviousRecords(customers);
				news.Id = count + 1;
				customers.Insert(news);
			}
		}

		public async Task Clear(TypeFolder folder)
		{
			HotNews = new ListWithDate();
			MainNews = new ListWithDate();		
				
		}
	}
}
