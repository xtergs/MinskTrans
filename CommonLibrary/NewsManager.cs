﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using MinskTrans.Universal.Annotations;
using Newtonsoft.Json;
using PushNotificationServer;

namespace BackgroundUpdateTask
{
	public class NewsManager :INotifyPropertyChanged
	{

		private List<NewsEntry> newDictionary;
		private List<NewsEntry> hotNewsDictionary;

		private List<NewsEntry> allNews;
		private List<NewsEntry> allHotNewsDictionary;

		public string pathToSaveData = "";
		public string pathToSaveHotData = "";

		public string fileNameNews = "months.txt";
		public string fileNameHotNews = "days.txt";

		

		public DateTime LastUpdateDataDateTime
		{
#if WINDOWS_PHONE_APP
			get
			{
				if (!ApplicationData.Current.LocalSettings.Values.ContainsKey("LastUpdateDataDateTime"))
					LastUpdateDataDateTime = new DateTime();
				return DateTime.Parse(ApplicationData.Current.LocalSettings.Values["LastUpdateDataDateTime"].ToString());
			}

			set
			{
				if (!ApplicationData.Current.LocalSettings.Values.ContainsKey("LastUpdateDataDateTime"))
					ApplicationData.Current.LocalSettings.Values.Add("LastUpdateDataDateTime", value.ToString());
				else
					ApplicationData.Current.LocalSettings.Values["LastUpdateDataDateTime"] = value;
				OnPropertyChanged();
			}
#else
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
#endif
		}

		public DateTime LastUpdateHotDataDateTime
		{
#if WINDOWS_PHONE_APP
			get
			{
				if (!ApplicationData.Current.LocalSettings.Values.ContainsKey("LastUpdateHotDataDateTime"))
					LastUpdateHotDataDateTime = new DateTime();
				return DateTime.Parse(ApplicationData.Current.LocalSettings.Values["LastUpdateHotDataDateTime"].ToString());
			}

			set
			{
				if (!ApplicationData.Current.LocalSettings.Values.ContainsKey("LastUpdateHotDataDateTime"))
					ApplicationData.Current.LocalSettings.Values.Add("LastUpdateHotDataDateTime", value.ToString());
				else
					ApplicationData.Current.LocalSettings.Values["LastUpdateHotDataDateTime"] = value;
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
				return allNews.OrderByDescending(key => key.Posted).ToList();
			}
			set { OnPropertyChanged(); }
		}

		public List<NewsEntry> AllHotNews
		{
			get
			{
				return allHotNewsDictionary.OrderByDescending(key => key.Posted).ToList();
			}
			set { OnPropertyChanged(); }
		}

		public async Task Load()
		{
			allNews.Clear();
			string path = "";
			path = Path.Combine(pathToSaveData, fileNameNews);
			StorageFile file = null;
			try
			{
				file = await ApplicationData.Current.LocalFolder.GetFileAsync(path);
				var allLines = await FileIO.ReadTextAsync(file);
				allNews.AddRange(JsonConvert.DeserializeObject<List<NewsEntry>>(allLines));
			}
			catch (FileNotFoundException e)
			{

			}
			NewNews = null;

			AllHotNews.Clear();
			path = "";
			path = Path.Combine(pathToSaveHotData, fileNameHotNews);

			file = null;
			try
			{
				file = await ApplicationData.Current.LocalFolder.GetFileAsync(path);
				var allLines = await FileIO.ReadTextAsync(file);
				allHotNewsDictionary.AddRange(JsonConvert.DeserializeObject<List<NewsEntry>>(allLines));
			}
			catch (FileNotFoundException e)
			{

			}


			AllHotNews = null;
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
