using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using GalaSoft.MvvmLight.CommandWpf;
using PushNotificationServer.Properties;
using Task = System.Threading.Tasks.Task;
using Autofac;
using MetroLog;
using MinskTrans.Context;
using MinskTrans.Context.Base;
using MinskTrans.Context.Desktop;
using MinskTrans.Context.Fakes;
using MinskTrans.Context.UniversalModelView;
using MinskTrans.Net;
using MinskTrans.Net.Base;
using MinskTrans.Utilites;
using MinskTrans.Utilites.Base.IO;
using MinskTrans.Utilites.Base.Net;
using MinskTrans.Utilites.Desktop;
using MyLibrary;


namespace PushNotificationServer
{
	public class ServerEngine:INotifyPropertyChanged
	{
		private static ServerEngine engine;
		private IBussnessLogics context;
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

	    async public Task InicializeAsync()
		{
			//NewsManager.LastNewsTime = Settings.Default.LastUpdatedNews;
			//NewsManager.LastHotNewstime = Settings.Default.LastUpdatedHotNews;
			await NewsManager.Load();
			await BusnesLogic.LoadDataBase(LoadType.LoadDB);
			BusnesLogic.Settings.LastUpdateDbDateTimeUtc = Settings.Default.DBUpdateTime;
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
				NewsManager.LastUpdateMainNewsDateTimeUtc.ToString(CultureInfo.InvariantCulture) + Environment.NewLine + NewsManager.LastUpdateHotNewsDateTimeUtc.ToString(CultureInfo.InvariantCulture) +
				Environment.NewLine + BusnesLogic.LastUpdateDbDateTimeUtc.ToString(CultureInfo.InvariantCulture));
		}

		private NewsManagerBase newsManager;

		public NewsManagerBase NewsManager { get { return newsManager; } }

		ServerEngine()
		{
			//newsManager = new NewsManager(new FileHelper());
			SetAutoUpdateTimer(NewsAutoUpdate);
			//CloudController = cloudStorageController;

			var builder = new ContainerBuilder();

			builder.RegisterType<FileHelperDesktop>().As<FileHelperBase>().SingleInstance();
           builder.RegisterType<SqlEFContext>().As<IContext>().WithParameter("connectionString", @"default");
           // builder.RegisterType<Context>().As<IContext>().SingleInstance();
			builder.RegisterType<UpdateManagerBase>().SingleInstance();
			builder.RegisterType<InternetHelperDesktop>().As<InternetHelperBase>().SingleInstance();
			builder.RegisterType<OneDriveController>().As<ICloudStorageController>().SingleInstance();
			builder.RegisterType<NewsManagerDesktop>().As<NewsManagerBase>().SingleInstance();
			builder.RegisterType<ShedulerParser>().As<ITimeTableParser>().SingleInstance();
		    builder.RegisterType<BussnessLogic>().As<IBussnessLogics>().SingleInstance();
            builder.RegisterType<FakeGeolocation>().As<IGeolocation>().SingleInstance();
            builder.RegisterType<FakeSettingsModelView>().As<ISettingsModelView>().SingleInstance();
            builder.RegisterType<ExternalCommands>().As<IExternalCommands>().SingleInstance();
            builder.RegisterInstance<ILogManager>(LogManagerFactory.DefaultLogManager).SingleInstance();

            var container = builder.Build();

			context = container.Resolve<IBussnessLogics>();
			newsManager = container.Resolve<NewsManagerBase>();
			updateManager = container.Resolve<UpdateManagerBase>();
			OndeDriveController = container.Resolve<ICloudStorageController>();

			NewsManager.FileNameDays = "daysDebug.txt";
			NewsManager.FileNameMonths = "monthDebug.txt";
			fileNameLastNews = "lastNewsDebug.txt";
		}

	    private class OneDriveController :ICloudStorageController
	    {
	        #region Implementation of ICloudStorageController

	        public void Inicialize()
	        {
	           // throw new NotImplementedException();
	        }

	        public Task UploadFileAsync(TypeFolder pathToFile, string newNameFile)
	        {
	            //throw new NotImplementedException();
	            return Task.Delay(0);
	        }

	        public event EventHandler<EventArgs> NeedAttention;

	        #endregion
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
			    if (timerNewsAutoUpdate == null)
			        return;
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
				}), Task.Run( (async ()=>
				{
				    await context.UpdateTimeTableAsync();
				})));

			}
			catch (TaskCanceledException e)
			{
				Updating = false;
				OnStopChecknews();
				//throw;
			}
			catch (Exception)
			{ 
				throw;
			}
			Settings.Default.LastUpdatedNews = newsManager.LastUpdateMainNewsDateTimeUtc;
			Settings.Default.LastUpdatedHotNews = newsManager.LastUpdateMainNewsDateTimeUtc;
			Settings.Default.DBUpdateTime = context.LastUpdateDbDateTimeUtc;
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
				NewsManager.LastUpdateMainNewsDateTimeUtc = new DateTime();
				NewsManager.LastUpdateHotNewsDateTimeUtc = new DateTime();
				Settings.Default.LastUpdatedHotNews = new DateTime();
				Settings.Default.LastUpdatedNews = new DateTime();
				Settings.Default.Save();
			});}
		}

		public ICloudStorageController OndeDriveController { get; }

	    public IBussnessLogics BusnesLogic
		{
			get { return context; }
		}

		public event PropertyChangedEventHandler PropertyChanged;

		
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
