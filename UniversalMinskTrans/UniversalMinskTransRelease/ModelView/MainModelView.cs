using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommonLibrary;
using GalaSoft.MvvmLight.Command;
using MinskTrans.DesctopClient.Modelview;
using MyLibrary;
using Autofac;
using CommonLibrary.IO;
using CommonLibrary.Notify;
using MetroLog;
using MetroLog.Targets;
using MinskTrans.Context;
using MinskTrans.Context.Base;
using MinskTrans.Context.UniversalModelView;
using MinskTrans.Net;
using MinskTrans.Net.Base;
using MinskTrans.Utilites;
using MinskTrans.Utilites.Base.IO;
using MinskTrans.Utilites.Base.Net;
using UniversalMinskTransRelease.ModelView;
using UniversalMinskTransRelease.Nofity;

namespace MinskTrans.Universal.ModelView
{

	public class MainModelView : BaseModelView, INotifyPropertyChanged
	{
		private readonly IContainer container;
		private static MainModelView mainModelView;
		//private readonly IContext context;
/*
		private FindModelView findModelView;
*/
		private readonly NewsManagerBase newsManager;
		private readonly INotifyHelper notifyHelper;
		private ILogger log;

	    public bool IsNeesUpdate
	    {
	        get { return _isNeesUpdate; }
	        set
	        {
	            _isNeesUpdate = value;
	            if (value)
	                IsLoading = false;
	            OnPropertyChanged();
	        }
	    }

	    public bool IsLoading
	    {
	        get { return _isLoading; }
	        set
	        {
	            _isLoading = value;
	            OnPropertyChanged();
	        }
	    }

	    public UpdateManagerBase UpdateManager { get; }


		public static MainModelView MainModelViewGet => 
			mainModelView ?? (mainModelView = new MainModelView());

		private MainModelView()
		{
            //			var configuration = new LoggingConfiguration();
            //#if DEBUG
            //			configuration.AddTarget(LogLevel.Trace, LogLevel.Fatal, new DebugTarget());
            //#endif
            //			configuration.AddTarget(LogLevel.Trace, LogLevel.Fatal, new FileStreamingTarget());
            //			configuration.IsEnabled = true;

            //			LogManagerFactory.DefaultConfiguration = configuration;
		    //LogManagerFactory.DefaultConfiguration.AddTarget(LogLevel.Trace, LogLevel.Fatal, new FileStreamingTarget());

            //LogManagerFactory.DefaultConfiguration.IsEnabled = true;
			var builder = new ContainerBuilder();
			builder.RegisterType<FileHelper>().As<FileHelperBase>().SingleInstance();
			//builder.RegisterType<SqliteContext>().As<IContext>().SingleInstance();
			builder.RegisterType<Context.Context>().As<IContext>().SingleInstance();
			builder.RegisterType<UpdateManagerBase>();
			builder.RegisterType<ShedulerParser>().As<ITimeTableParser>();
			builder.RegisterType<InternetHelperUniversal>().As<InternetHelperBase>();
			builder.RegisterType<NewsManager>().As<NewsManagerBase>().SingleInstance();
			builder.RegisterType<BussnessLogic>().As<IBussnessLogics>().SingleInstance();
			builder.RegisterType<UniversalGeolocator>().As<IGeolocation>().SingleInstance();
			builder.RegisterType<SettingsModelView>().As<ISettingsModelView>().SingleInstance();
			builder.RegisterType<UniversalApplicationSettingsHelper>().As<IApplicationSettingsHelper>();
			builder.RegisterType<GroupStopsModelView>().SingleInstance();
			builder.RegisterType<FavouriteModelView>().SingleInstance();
			builder.RegisterType<NewsModelView>().SingleInstance();
			builder.RegisterType<FindModelView>().SingleInstance().WithParameter("UseGPS", true);
			builder.RegisterType<ExternalCommands>().As<IExternalCommands>().SingleInstance();
			builder.RegisterInstance<ILogManager>(LogManagerFactory.DefaultLogManager).SingleInstance();
			builder.RegisterType<NotifyHelperUniversal>().As<INotifyHelper>();
			builder.RegisterType<FilePathsSettings>().SingleInstance();

			container = builder.Build();

			context = container.Resolve<IBussnessLogics>();
			newsManager = container.Resolve<NewsManagerBase>();
			UpdateManager = container.Resolve<UpdateManagerBase>();
			notifyHelper = container.Resolve<INotifyHelper>();
			log = container.Resolve<ILogManager>().GetLogger<MainModelView>();

			ExternalCommands = container.Resolve<IExternalCommands>();

		}

			public IGeolocation Geolocation => container.Resolve<IGeolocation>();

		public NewsManagerBase NewsManager => container.Resolve<NewsManagerBase>();

		public MapModelView MapModelView { get; set; }
		public IExternalCommands ExternalCommands { get; }

		public ISettingsModelView SettingsModelView => container.Resolve<ISettingsModelView>();

		public FindModelView FindModelView => container.Resolve<FindModelView>();

		public StopModelView StopMovelView { get; }

		public RoutsModelView RoutsModelView { get; }

		public GroupStopsModelView GroupStopsModelView => container.Resolve<GroupStopsModelView>();

		public FavouriteModelView FavouriteModelView => container.Resolve<FavouriteModelView>();

		//public IContext Context { get { return context; } }


		public List<NewsEntry> AllNews
		{
			get
			{
#if DEBUG
				var xxx = NewsManager?.AllHotNews.Concat(newsManager.NewNews).ToList();
#endif
				return NewsManager?.AllHotNews.Concat(newsManager.NewNews).OrderByDescending(key => key.PostedUtc).ThenByDescending(key=> key.RepairedLineUtc).ToList();
			}
			set
			{

				OnPropertyChanged();
			}
		}

		readonly ObservableCollection<string> resultString = new ObservableCollection<string>();

		public ObservableCollection<string> AllLogs => resultString;

		bool updating = false;
		private CancellationTokenSource cancelSource;
		RelayCommand updateDataCommand;
		public RelayCommand UpdateDataCommand
		{
			get
			{
				return updateDataCommand ?? (updateDataCommand = new RelayCommand(async () =>
				{
					log?.Trace("UpdateDataCommand activated");
					updating = true;
				    bool oldNeedUpdate = IsNeesUpdate;
				    IsNeesUpdate = false;
					try
					{
						UpdateDataCommand.RaiseCanExecuteChanged();
						using (cancelSource = new CancellationTokenSource())
						{
							await Context.UpdateTimeTableAsync(cancelSource.Token);
							await Context.UpdateNewsTableAsync(cancelSource.Token);
							await NewsManager.Load();
						}
					}
					catch (Exception e)
					{
						NotifyHelper.ReportErrorAsync("Во время обновления произошла ошибка попробуйте позже");
						log?.Error($"UpdateDataCommand {e.Message}", e);
					    if (oldNeedUpdate)
					        IsNeesUpdate = true;
					}
					finally
					{
						updating = false;
					}
					UpdateDataCommand.RaiseCanExecuteChanged();
					log?.Info("UpdateDataCommand, OK");
				}, () => !updating));
			}
		}

		public async Task< List<string>> GetLogsAsync()
		{
			string folder = "metroLogs";
			var fileHelper = container.Resolve<FileHelperBase>();
			var xxx =await LogManagerFactory.DefaultLogManager.GetCompressedLogs();
			await fileHelper.DeleteFile(TypeFolder.Temp, "lll.log");
			await fileHelper.WriteTextAsync(TypeFolder.Temp, "lll.log", xxx);
			 xxx.Dispose();
			//var xxxxxxx = (await MetroLog.WinRT.Logger.GetCompressedLogFile());

			ZipFile.ExtractToDirectory(fileHelper.GetPath(TypeFolder.Temp) + "\\" + "lll.log", fileHelper.GetPath(TypeFolder.Temp) + "\\" + folder);

			var fileNames = await fileHelper.GetNamesFiles(TypeFolder.Temp, folder);
			List<string> resultList = new List<string>();
			foreach (var fileName in fileNames)
			{
#if DEBUG

#endif
				resultList.Add(await fileHelper.ReadAllTextAsync(TypeFolder.Temp, fileName, folder));
				await fileHelper.DeleteFile(TypeFolder.Temp, folder + "\\" + fileName);

			}
			return resultList;
		}

		private bool logsWork = false;
	    private bool _isNeesUpdate = false;
	    private bool _isLoading = true;

	    public RelayCommand RefreshLogsCommand
		{
			get
			{
				return new RelayCommand(async () =>
				{
					if (logsWork)
						return;
					try
					{
						logsWork = true;
						resultString.Clear();
						var entries = await GetLogsAsync();
						entries.All(x =>
						{
							resultString.Add(x);
							return true;
						});
						entries.Aggregate((x, y) =>
						{
							resultString.Add(x);
							return "";
						});
						// OnPropertyChanged("AllLogs");
						OnPropertyChanged("AllLogs");
					}
					catch (Exception e)
					{
						log.Error(e.Message, e);
					}
					finally
					{
						logsWork = false;
					}
				}, ()=> !logsWork);


			}
		}

		public event Show ShowStop
		{
			add { ExternalCommands.ShowStop += value; }
			remove { ExternalCommands.ShowStop -= value; }
		}

		public event Show ShowRoute {
			add { ExternalCommands.ShowRoute += value; }
			remove { ExternalCommands.ShowRoute -= value; }
		}



		public NewsModelView NewsModelView => container.Resolve<NewsModelView>();

		public INotifyHelper NotifyHelper => notifyHelper;
		public FileHelperBase FileHelper { get { return container.Resolve<FileHelperBase>(); } }
	}
}