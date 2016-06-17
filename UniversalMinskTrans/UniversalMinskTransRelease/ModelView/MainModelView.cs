using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO.Compression;
using System.Linq;
using System.Runtime.CompilerServices;
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
using MinskTrans.Context;
using MinskTrans.Context.Base;
using MinskTrans.Context.Geopositioning;
using MinskTrans.Context.UniversalModelView;
using MinskTrans.Net;
using MinskTrans.Net.Base;
using MinskTrans.Utilites;
using MinskTrans.Utilites.Base.IO;
using MinskTrans.Utilites.Base.Net;
using UniversalMinskTransRelease.Annotations;
using UniversalMinskTransRelease.ModelView;
using UniversalMinskTransRelease.Nofity;

namespace MinskTrans.Universal.ModelView
{

	public class MainModelView : INotifyPropertyChanged
	{
		private readonly IContainer container;
		private static MainModelView mainModelView;
		//private readonly IContext context;
/*
		private FindModelView findModelView;
*/
		private ILogger log;

	    public bool IsNeesUpdate
	    {
	        get { return _isNeesUpdate; }
	        set
	        {
	            _isNeesUpdate = value;
	            //if (value)
	            //    IsLoading = false;
	            OnPropertyChanged();
	        }
	    }

	    private int isLoading = 0;
	    public bool IsLoading
	    {
	        get { return isLoading > 0; }
	        set
	        {
	            if (value)
	                Interlocked.Increment(ref isLoading);
	            else
	                Interlocked.Decrement(ref isLoading);
	            OnPropertyChanged();
	        }
	    }

	    public Task LoadAllData()
	    {
	        return Task.WhenAll(
	            Context.LoadDataBase(),
	            NewsManager.Load());
	    }

	    public IBussnessLogics Context => container.Resolve<IBussnessLogics>();


	    public static MainModelView MainModelViewGet => 
			mainModelView ?? (mainModelView = new MainModelView());

		private MainModelView()
		{
#if DEBUG
            Stopwatch watch = new Stopwatch();
            watch.Start();
#endif
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
			builder.RegisterType<FileHelper>().As<FileHelperBase>();
			//builder.RegisterType<SqliteContext>().As<IContext>().SingleInstance();
			builder.RegisterType<Context.Context>().As<IContext>().SingleInstance();
			builder.RegisterType<UpdateManagerBase>();
			builder.RegisterType<ShedulerParser>().As<ITimeTableParser>();
			builder.RegisterType<InternetHelperUniversal>().As<InternetHelperBase>();
			builder.RegisterType<NewsManager>().As<NewsManagerBase>().SingleInstance();
			builder.RegisterType<BussnessLogic>().As<IBussnessLogics>().SingleInstance();
			builder.RegisterType<UniversalApplicationSettingsHelper>().As<IApplicationSettingsHelper>();
			builder.RegisterInstance<ILogManager>(LogManagerFactory.DefaultLogManager).SingleInstance();
			builder.RegisterType<FilePathsSettings>().SingleInstance();

            #region Register ModelView

		    builder.RegisterType<SingFilterForAllDataModelView>().AsSelf();
			builder.RegisterType<SettingsModelViewUIDispatcher>().As<ISettingsModelView>().SingleInstance();
            builder.RegisterType<UniversalGeolocator>().As<IGeolocation>().SingleInstance();
		    builder.RegisterType<MapModelViewUIDispatcher>().As<MapModelView>();
			builder.RegisterType<GroupStopsModelView>().SingleInstance();
		    builder.RegisterType<StopModelViewUIDispatcher>().As<StopModelView>();
		    builder.RegisterType<RoutsModelViewUIDispatcher>().As<RoutsModelView>();
			builder.RegisterType<NewsModelViewUIDispatcher>().As<NewsModelView>();
			builder.RegisterType<FindModelView>().SingleInstance().WithParameter("UseGPS", true);
			builder.RegisterType<ExternalCommands>().As<IExternalCommands>().SingleInstance();
			builder.RegisterType<NotifyHelperUniversal>().As<INotifyHelper>();
		    builder.RegisterType<WebSeacher>().AsSelf();
            #endregion
            container = builder.Build();

			log = container.Resolve<ILogManager>().GetLogger<MainModelView>();

			ExternalCommands = container.Resolve<IExternalCommands>();
#if DEBUG
            watch.Stop();
            Debug.WriteLine($"\n\nMainViewModel!: {watch.ElapsedMilliseconds}\n\n");
#endif
        }

			//public IGeolocation Geolocation => container.Resolve<IGeolocation>();

		public NewsManagerBase NewsManager => container.Resolve<NewsManagerBase>();

        public MapModelView MapModelView { get; set; }

	    public SingFilterForAllDataModelView SingFilterForAllDataModelView
	        => container.Resolve<SingFilterForAllDataModelView>();
        public MapModelView.MapModelViewFactory MapModelViewFactory => container.Resolve<MapModelView.MapModelViewFactory>();
		public IExternalCommands ExternalCommands { get; }

		public ISettingsModelView SettingsModelView => container.Resolve<ISettingsModelView>();

		public FindModelView FindModelView => container.Resolve<FindModelView>();
	    public GroupStopsModelView GroupStopsModelView => container.Resolve<GroupStopsModelView>();

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
					Updating = true;
				    bool oldNeedUpdate = IsNeesUpdate;
				    IsNeesUpdate = false;
					try
					{
						UpdateDataCommand.RaiseCanExecuteChanged();
						using (cancelSource = new CancellationTokenSource())
						{
						    var token = cancelSource.Token;
						    await Task.WhenAll(Context.UpdateTimeTableAsync(token), Task.Run(
						        async () =>
						        {
						            bool result = await Context.UpdateNewsTableAsync(token);
						            if (!result)
						                return;

						            if (token.IsCancellationRequested)
						                return;
						            await NewsManager.Load();
						        }, token));

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
						Updating = false;
					}
					UpdateDataCommand.RaiseCanExecuteChanged();
					log?.Info("UpdateDataCommand, OK");
				}, () => !Updating));
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

		public INotifyHelper NotifyHelper => container.Resolve<INotifyHelper>();
		public FileHelperBase FileHelper => container.Resolve<FileHelperBase>();

	    public bool Updating
	    {
	        get { return updating; }
	        private set
	        {
	            if (updating == value)
	                return;
	            updating = value;
	            OnPropertyChanged();
	        }
	    }

	    public event PropertyChangedEventHandler PropertyChanged;

	    [NotifyPropertyChangedInvocator]
	    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
	    {
	        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	    }
	}
}