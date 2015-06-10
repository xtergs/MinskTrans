using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using MinskTrans.Universal.Annotations;

namespace BackgroundUpdateTask
{
	public class NewsManager :INotifyPropertyChanged
	{

		private Dictionary<DateTime, string> newDictionary;
		private List<KeyValuePair<DateTime, string>> hotNewsDictionary;

		private Dictionary<DateTime, string> allNews;
		private List<KeyValuePair<DateTime, string>> allHotNewsDictionary;

		private string pathToSaveData = "";
		private string pathToSaveHotData = "";

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

		public List<KeyValuePair<DateTime, string>> NewNews
		{
			get
			{
				return allNews.OrderByDescending(key => key.Key).ToList();
			}
			set { OnPropertyChanged(); }
		}

		public List<KeyValuePair<DateTime, string>> AllHotNews
		{
			get
			{
				return allHotNewsDictionary.OrderByDescending(key => key.Key).ToList();
			}
			set { OnPropertyChanged(); }
		}

		public async Task Load()
		{
			allNews.Clear();
			string path = "";
			for (int i = 0; i < 12; i++)
			{
				path = Path.Combine(pathToSaveData, i.ToString() + ".txt");
				StorageFile file = null;
				try
				{
					file = await ApplicationData.Current.LocalFolder.GetFileAsync(path);
				}
				catch (FileNotFoundException e)
				{
					continue;
				}
				var allLines =(await FileIO.ReadLinesAsync(file)).Where(str => str.Length > 0).ToArray();
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

				StorageFile file = null;
				try
				{
					file = await ApplicationData.Current.LocalFolder.GetFileAsync(path);
				}
				catch (FileNotFoundException e)
				{
					continue;
				}
				var allLines = (await FileIO.ReadLinesAsync(file)).Where(str => str.Length > 0).ToArray();
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

		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			var handler = PropertyChanged;
			if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
