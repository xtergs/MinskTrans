using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Windows.UI.Xaml;
using CommonLibrary;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using MinskTrans.DesctopClient;
using MinskTrans.DesctopClient.Modelview;
using MyLibrary;
using Autofac;
using CommonLibrary.IO;
using MetroLog;
using MetroLog.Targets;
using MinskTrans.Context;
using MinskTrans.Context.Base;
using MinskTrans.Context.Base.BaseModel;
using MinskTrans.Context.UniversalModelView;
using MinskTrans.DesctopClient.Model;
using MinskTrans.Net;
using MinskTrans.Net.Base;
using MinskTrans.Utilites;
using MinskTrans.Utilites.Base.IO;
using MinskTrans.Utilites.Base.Net;
using UniversalMinskTransRelease.Context;
using UniversalMinskTransRelease.ModelView;

namespace MinskTrans.Universal.ModelView
{

	public class MainModelView : BaseModelView, INotifyPropertyChanged
	{
	    private IContainer container;
        private static MainModelView mainModelView;
		//private readonly IContext context;
	    private FindModelView findModelView;
	    private readonly NewsManagerBase newsManager;


	    public UpdateManagerBase UpdateManager { get; }


	    public static MainModelView MainModelViewGet
		{
			get
			{
				if (mainModelView == null)
					mainModelView = new MainModelView();
				return mainModelView;}
		}

		private MainModelView()
		{
            var configuration = new LoggingConfiguration();

            configuration.AddTarget(LogLevel.Trace, LogLevel.Fatal, new DebugTarget());

            configuration.AddTarget(LogLevel.Trace, LogLevel.Fatal, new FileStreamingTarget());
            configuration.IsEnabled = true;

            LogManagerFactory.DefaultConfiguration = configuration;
            var builder = new ContainerBuilder();
            builder.RegisterType<FileHelper>().As<FileHelperBase>().SingleInstance();
            //builder.RegisterType<SqliteContext>().As<IContext>().SingleInstance();
            builder.RegisterType<Context.Context>().As<IContext>().SingleInstance();
            builder.RegisterType<UpdateManagerBase>();
            builder.RegisterType<ShedulerParser>().As<ITimeTableParser>();
            builder.RegisterType<InternetHelperUniversal>().As<InternetHelperBase>();
            builder.RegisterType<NewsManager>().As<NewsManagerBase>();
		    builder.RegisterType<BussnessLogic>().As<IBussnessLogics>().SingleInstance();
		    builder.RegisterType<UniversalGeolocator>().As<IGeolocation>().SingleInstance();
		    builder.RegisterType<SettingsModelView>().As<ISettingsModelView>().SingleInstance();
		    builder.RegisterType<UniversalApplicationSettingsHelper>().As<IApplicationSettingsHelper>();
		    builder.RegisterType<GroupStopsModelView>().SingleInstance();
            builder.RegisterType<FavouriteModelView>().SingleInstance();
            builder.RegisterType<NewsModelView>().SingleInstance();
		    builder.RegisterType<FindModelView>().SingleInstance().WithParameter("UseGPS", true);
		    builder.RegisterType<ExternalCommands>().As<IExternalCommands>().SingleInstance();
		    builder.RegisterInstance<ILogger>(LogManagerFactory.DefaultLogManager.GetLogger("Log")).SingleInstance();

            container = builder.Build();

            context = container.Resolve<IBussnessLogics>();
            newsManager = container.Resolve<NewsManagerBase>();
            UpdateManager = container.Resolve<UpdateManagerBase>();

		    ExternalCommands = container.Resolve<IExternalCommands>();
		    


		    //var fileHelper = new FileHelper();
		    //var internetHelper = new InternetHelperUniversal(fileHelper);
		    //context = new UniversalContext(fileHelper, internetHelper);
		    //updateManager = new UpdateManagerUniversal(fileHelper, internetHelper);

		    //SettingsModelView = container.Resolve<ISettingsModelView>();
		    //routesModelview = new RoutsModelView(context);
		    //stopMovelView = new StopModelView(context, settingsModelView, true);

		    //NewsModelView = new NewsModelView(NewsManager);
		}

		//private MainModelView(Context newContext)
		//{
		//	context = newContext;
		//	settingsModelView = new SettingsModelView();
		//	//routesModelview = new RoutsModelView(context);
		//	//stopMovelView = new StopModelView(context, settingsModelView, true);
		//	groupStopsModelView = new GroupStopsModelView(context, settingsModelView);
		//	favouriteModelView = new FavouriteModelView(context, settingsModelView);
			
		//	newsManager = new NewsManager();
		//	//Context.VariantLoad = SettingsModelView.VariantConnect;
			
		//}

		public NewsManagerBase NewsManager
		{
			get { return container.Resolve<NewsManagerBase>();}
		}

		public MapModelView MapModelView { get; set; }
        public IExternalCommands ExternalCommands { get; }

	    public ISettingsModelView SettingsModelView { get { return container.Resolve<ISettingsModelView>(); } }

	    public FindModelView FindModelView
		{
			get { return container.Resolve<FindModelView>(); }
		}

		public StopModelView StopMovelView { get; }

	    public RoutsModelView RoutsModelView { get; }

	    public GroupStopsModelView GroupStopsModelView { get { return container.Resolve<GroupStopsModelView>(); } }

	    public FavouriteModelView FavouriteModelView { get { return container.Resolve<FavouriteModelView>(); } }

	    //public IContext Context { get { return context; } }


		public List<NewsEntry> AllNews
		{
			get
			{
				if (NewsManager != null)
				{
#if DEBUG
					var xxx = NewsManager.AllHotNews.Concat(newsManager.NewNews).ToList();
#endif
					return NewsManager.AllHotNews.Concat(newsManager.NewNews).OrderByDescending(key => key.PostedUtc).ThenByDescending(key=> key.RepairedLineUtc).ToList();
				}
				return null;
			}
			set
			{

				OnPropertyChanged("AllNews");
			}
		}


	    public string AllLogs
	    {
	        get
	        {
	            var xx = new FileStreamingTarget();
                
                var configuration = new LoggingConfiguration();
                configuration.AddTarget(LogLevel.Trace, LogLevel.Fatal, new DebugTarget());
                configuration.AddTarget(LogLevel.Trace, LogLevel.Fatal, xx);
                configuration.IsEnabled = true;

	            var fileHelper = container.Resolve<FileHelperBase>();
                //fileHelper.ReadAllTextAsync(TypeFolder.Local, )
	            return "";
	        }
	    }

		bool updating = false;
		RelayCommand updateDataCommand;
		public RelayCommand UpdateDataCommand
		{
			get
			{
				if (updateDataCommand == null)
					updateDataCommand = new RelayCommand(async () =>
					{
						updating = true;
						UpdateDataCommand.RaiseCanExecuteChanged();

						await Context.UpdateTimeTableAsync();
					    await Context.UpdateNewsTableAsync();
						updating = false;
						UpdateDataCommand.RaiseCanExecuteChanged();
					}, () => !updating);
				return updateDataCommand;
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



        public NewsModelView NewsModelView { get { return container.Resolve<NewsModelView>(); } }
	}
}