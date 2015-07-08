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
using Task = System.Threading.Tasks.Task;
using MinskTrans.DesctopClient.Utilites.IO;
using MinskTrans.DesctopClient.Update;
using MinskTrans.DesctopClient.Net;
using Autofac;
using PushNotificationServer.CloudStorage.OneDrive;
using PushNotificationServer.CloudStorage;

namespace PushNotificationServer
{
	public class ServerEngine:INotifyPropertyChanged
	{
		private static ServerEngine engine;
		private Context context;
		private string fileNameLastNews = "LastNews.txt";

		private Timer timerNewsAutoUpdate;
		readonly UpdateManagerBase updateManager;

		public static ServerEngine Engine
		{
			get
			{
				if (engine == null)
					engine = new ServerEngine();
				return engine;
			}
		}

		private readonly ICloudStorageController CloudController;

		async public Task InicializeAsync()
		{
			NewsManager.LastNewsTime = Settings.Default.LastUpdatedNews;
			NewsManager.LastHotNewstime = Settings.Default.LastUpdatedHotNews;
			await NewsManager.Load();
			await Context1.Load(LoadType.LoadDB);
			Context1.LastUpdateDataDateTime = Settings.Default.DBUpdateTime;
			OndeDriveController.Inicialize();
			this.StopChecknews += (sender, args) => UploadAllToOneDrive();
#if DEBUG
			fileNameLastNews = "LastNewsDebug.txt";
#endif
		}

		void UploadAllToOneDrive()
		{
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
			OndeDriveController.UploadFileAsync(TypeFolder.Local, NewsManager.FileNameDays);
			OndeDriveController.UploadFileAsync(TypeFolder.Local, NewsManager.FileNameMonths);
			OndeDriveController.UploadFileAsync(TypeFolder.Local, fileNameLastNews);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
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
			//newsManager = new NewsManager(new FileHelper());
			SetAutoUpdateTimer(NewsAutoUpdate);
			//CloudController = cloudStorageController;

			var builder = new ContainerBuilder();

			builder.RegisterType<FileHelperDesktop>().As<FileHelperBase>();
			builder.RegisterType<ContextDesctop>().As<Context>();
			builder.RegisterType<UpdateManagerDesktop>().As<UpdateManagerBase>();
			builder.RegisterType<InternetHelperDesktop>().As<InternetHelperBase>();
			builder.RegisterType<OneDriveController>().As<ICloudStorageController>();
			builder.RegisterType<NewsManager>();

			var container = builder.Build();

			context = container.Resolve<Context>();
			newsManager = container.Resolve<NewsManager>();
			updateManager = container.Resolve<UpdateManagerBase>();
			CloudController = container.Resolve<ICloudStorageController>();
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

		public int DbUpdateMins
		{
			get { return Properties.Settings.Default.DbUpdateInterval; }
			set
			{
				if (value <= 0)
					return;
				Properties.Settings.Default.DbUpdateInterval = value;
				OnPropertyChanged();
			}
		}

		public void SetAutoUpdateTimer(bool turnOn)
		{
			if (turnOn)
			{
				if (timerNewsAutoUpdate == null)
					timerNewsAutoUpdate = new Timer(ChuckNews, null, new TimeSpan(0, 0, 0, 30), new TimeSpan(0, 0, DbUpdateMins, 0));
				else
					timerNewsAutoUpdate.Change(new TimeSpan(0, 0, 0, 30), new TimeSpan(0,0,DbUpdateMins,0));
			}
			else
			{
				timerNewsAutoUpdate.Dispose();
				timerNewsAutoUpdate = null;
			}
		}

		public async void ChuckNews(object obj)
		{
			await CheckNews();
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
				}), Task.Run( async ()=>
				{
					if (await updateManager.DownloadUpdate())
					{
						var timeTable = await updateManager.GetTimeTable();
						if (await context.HaveUpdate(timeTable.Routs, timeTable.Stops, timeTable.Time))
							context.LastUpdateDataDateTime = DateTime.UtcNow;
                        }
				}));

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

		public ICloudStorageController OndeDriveController
		{
			get { return CloudController; }
		}

		public Context Context1
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
