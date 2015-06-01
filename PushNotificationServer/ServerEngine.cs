using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Animation;
using GalaSoft.MvvmLight.CommandWpf;
using Microsoft.Build.Utilities;
using MinskTrans.DesctopClient.Annotations;
using Task = System.Threading.Tasks.Task;

namespace PushNotificationServer
{
	public class ServerEngine:INotifyPropertyChanged
	{
		private static ServerEngine engine;

		private Timer timerNewsAutoUpdate;

		public static ServerEngine Engine
		{
			get
			{
				if (engine == null)
					engine = new ServerEngine();
				return engine;
			}
		}

		public void Inicialize()
		{
			NewsManager.LastNewsTime = Properties.Settings.Default.LastUpdatedNews;
			NewsManager.Load();
		}

		private NewsManager newsManager;

		public NewsManager NewsManager { get { return newsManager;} }

		ServerEngine()
		{
			newsManager = new NewsManager();
			SetAutoUpdateTimer(NewsAutoUpdate);
		}

		public bool NewsAutoUpdate
		{
			get { return Properties.Settings.Default.NewsAutoUpdate; }
			set
			{
				Properties.Settings.Default.NewsAutoUpdate = value;
				Properties.Settings.Default.Save();
				SetAutoUpdateTimer(NewsAutoUpdate);
				OnPropertyChanged();
			}
		}

		public void SetAutoUpdateTimer(bool turnOn)
		{
			if (turnOn)
			{
				if (timerNewsAutoUpdate == null)
					timerNewsAutoUpdate = new Timer(ChuckNews, null, new TimeSpan(0, 0, 0, 30), new TimeSpan(0, 1, 0, 0));
				else
					timerNewsAutoUpdate.Change(new TimeSpan(0, 0, 0, 30), new TimeSpan(0, 1, 0, 0));
			}
			else
			{
				timerNewsAutoUpdate.Dispose();
				timerNewsAutoUpdate = null;
			}
		}

		public async void ChuckNews(object obj)
		{
			CheckNews();
		}

		private bool Updating = false;

		public event StartCheckNewsDelegate StartCheckNews;
		public event StartCheckNewsDelegate StopChecknews;

		public async Task CheckNews()
		{
			if (Updating)
				return;
			Updating = true;
			OnStartCheckNews();
			try
			{
				await Task.WhenAll(Task.Run(async () =>
				{
					try
					{
						await newsManager.CheckMainNewsAsync();
						newsManager.SaveToFile();
					}
					catch (Exception e)
					{
						
						throw;
					}
				}), Task.Run(async () =>
				{
					await newsManager.CheckHotNewsAsync();
					newsManager.SaveToFileHotNews();
				}));

			}
			catch (TaskCanceledException e)
			{

				throw;
			}
			Properties.Settings.Default.LastUpdatedNews = newsManager.LastNewsTime;
			Properties.Settings.Default.LastUpdatedHotNews = newsManager.LastHotNewstime;
			Properties.Settings.Default.Save();
			Updating = false;
			OnStopChecknews();
		}

		public RelayCommand ResetLastUpdatetime
		{
			get { return new RelayCommand(() =>
			{
				NewsManager.LastHotNewstime = new DateTime();
				NewsManager.LastHotNewstime = new DateTime();
				Properties.Settings.Default.LastUpdatedHotNews = new DateTime();
				Properties.Settings.Default.LastUpdatedNews = new DateTime();
				Properties.Settings.Default.Save();
			});}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			var handler = PropertyChanged;
			if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
		}

		protected virtual void OnStartCheckNews()
		{
			var handler = StartCheckNews;
			if (handler != null) handler(this, EventArgs.Empty);
		}

		protected virtual void OnStopChecknews()
		{
			var handler = StopChecknews;
			if (handler != null) handler(this, EventArgs.Empty);
		}
	}

	public delegate void StartCheckNewsDelegate(object sender, EventArgs args);
}
