using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using GalaSoft.MvvmLight.CommandWpf;
using MinskTrans.DesctopClient;
using MinskTrans.DesctopClient.Annotations;
using MyLibrary;
using PushNotificationServer.Properties;

namespace PushNotificationServer
{
	public class ServerEngine:INotifyPropertyChanged
	{
		private static ServerEngine engine;
		private ContextDesctop context;
		private string fileNameLastNews = "LastNews.txt";

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

		private OneDriveController ondeDriveController;

		async public Task InicializeAsync()
		{
			NewsManager.LastNewsTime = Settings.Default.LastUpdatedNews;
			NewsManager.LastHotNewstime = Settings.Default.LastUpdatedHotNews;
			NewsManager.Load();
			await Context1.Load(LoadType.LoadDB);
			Context1.LastUpdateDataDateTime = Settings.Default.DBUpdateTime;
			OndeDriveController.Inicialize();
			this.StopChecknews += (sender, args) => UploadAllToOneDrive();
		}

		void UploadAllToOneDrive()
		{
			OndeDriveController.UploadFileAsync(NewsManager.FileNameDays, NewsManager.FileNameDays);
			OndeDriveController.UploadFileAsync(NewsManager.FileNameMonths, NewsManager.FileNameMonths);
			OndeDriveController.UploadFileAsync(fileNameLastNews, fileNameLastNews);
		}

		void SaveTime()
		{
			File.WriteAllText(fileNameLastNews,
				NewsManager.LastNewsTime + Environment.NewLine + NewsManager.LastHotNewstime +
				Environment.NewLine + Context1.LastUpdateDataDateTime);
		}

		private NewsManager newsManager;

		public NewsManager NewsManager { get { return newsManager;} }

		ServerEngine()
		{
			newsManager = new NewsManager();
			SetAutoUpdateTimer(NewsAutoUpdate);
			ondeDriveController = new OneDriveController();
			context = new ContextDesctop();
		}

		public bool NewsAutoUpdate
		{
			get { return Settings.Default.NewsAutoUpdate; }
			set
			{
				Settings.Default.NewsAutoUpdate = value;
				Settings.Default.Save();
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

		private static object o = new object();
		private static object a = new object();

		public async Task CheckNews()
		{
			if (Updating)
				return;
			Debug.WriteLine("Check news started");
			Updating = true;
			OnStartCheckNews();
			try
			{
				await Task.WhenAll(Task.Run(async () =>
				{
					try
					{
						await newsManager.CheckMainNewsAsync();
						lock (a)
						{
							newsManager.SaveToFile();
						}
						//OndeDriveController.UploadFile(NewsManager.FileNameMonths, NewsManager.FileNameMonths);
					}
					catch (Exception e)
					{

						throw;
					}
				}), Task.Run(async () =>
				{
					try
					{
						await newsManager.CheckHotNewsAsync();

						lock (o)
						{
							newsManager.SaveToFileHotNews();
						}
					}
					catch
					{
						throw;
					}
					//OndeDriveController.UploadFile(NewsManager.FileNameDays, NewsManager.FileNameDays);
				}),
					Context1.UpdateAsync());

			}
			catch (TaskCanceledException e)
			{
				Updating = false;
				OnStopChecknews();
				throw;
			}
			catch (Exception)
			{
				throw;
			}
			Settings.Default.LastUpdatedNews = newsManager.LastNewsTime;
			Settings.Default.LastUpdatedHotNews = newsManager.LastHotNewstime;
			Settings.Default.DBUpdateTime = context.LastUpdateDataDateTime;
			Settings.Default.Save();		
			SaveTime();
			Updating = false;
			OnStopChecknews();
			Debug.WriteLine("Check news ended");
		}

		public RelayCommand ResetLastUpdatetime
		{
			get { return new RelayCommand(() =>
			{
				NewsManager.LastHotNewstime = new DateTime();
				NewsManager.LastHotNewstime = new DateTime();
				Settings.Default.LastUpdatedHotNews = new DateTime();
				Settings.Default.LastUpdatedNews = new DateTime();
				Settings.Default.Save();
			});}
		}

		public OneDriveController OndeDriveController
		{
			get { return ondeDriveController; }
		}

		public ContextDesctop Context1
		{
			get { return context; }
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
