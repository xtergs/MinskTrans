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
using MinskTrans.DesctopClient;
using Autofac;
using CommonLibrary.IO;
using MinskTrans.Context;
using MinskTrans.Context.Base;
using MinskTrans.Context.Base.BaseModel;
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
		private static MainModelView mainModelView;
		//private readonly IContext context;
	    private readonly NewsModelView newsModelView;
		private readonly GroupStopsModelView groupStopsModelView;
		private readonly RoutsModelView routesModelview;
		private readonly SettingsModelView settingsModelView;
		private readonly StopModelView stopMovelView;
		private readonly FavouriteModelView favouriteModelView;
		private FindModelView findModelView;
		private MapModelView mapMOdelView;
		private readonly NewsManagerBase newsManager;
		readonly UpdateManagerBase updateManager;
		

		public UpdateManagerBase UpdateManager { get { return updateManager; } }		

		

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
            var builder = new ContainerBuilder();
            builder.RegisterType<FileHelper>().As<FileHelperBase>();
            //builder.RegisterType<SqliteContext>().As<IContext>().SingleInstance();
            builder.RegisterType<Context.Context>().As<IContext>().SingleInstance();
            builder.RegisterType<UpdateManagerBase>();
            builder.RegisterType<ShedulerParser>().As<ITimeTableParser>();
            builder.RegisterType<InternetHelperUniversal>().As<InternetHelperBase>();
            builder.RegisterType<NewsManager>().As<NewsManagerBase>();
            //builder.RegisterType<Context>().As<IContext>();
		    builder.RegisterType<BussnessLogic>().As<IBussnessLogics>();
		    builder.RegisterType<UniversalGeolocator>().As<IGeolocation>();
		    builder.RegisterType<SettingsModelView>().As<ISettingsModelView>();

            var container = builder.Build();

            context = container.Resolve<IBussnessLogics>();
            newsManager = container.Resolve<NewsManagerBase>();
            updateManager = container.Resolve<UpdateManagerBase>();

            //var fileHelper = new FileHelper();
            //var internetHelper = new InternetHelperUniversal(fileHelper);
            //context = new UniversalContext(fileHelper, internetHelper);
            //updateManager = new UpdateManagerUniversal(fileHelper, internetHelper);

            settingsModelView = new SettingsModelView();
			//routesModelview = new RoutsModelView(context);
			//stopMovelView = new StopModelView(context, settingsModelView, true);
			groupStopsModelView = new GroupStopsModelView(context, settingsModelView);
			favouriteModelView = new FavouriteModelView(context, settingsModelView);
		    newsModelView = new NewsModelView(NewsManager);
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
			get { return newsManager;}
		}

		public MapModelView MapModelView
		{
			get { return mapMOdelView;}
			set { mapMOdelView = value; }
		}

		public SettingsModelView SettingsModelView
		{
			get { return settingsModelView; }
		}

		public FindModelView FindModelView
		{
			get
			{
				if (findModelView == null)
					findModelView = new FindModelView(context, settingsModelView, true);
				return findModelView;
			}
		}

		public StopModelView StopMovelView
		{
			get { return stopMovelView; }
		}

		public RoutsModelView RoutsModelView
		{
			get { return routesModelview; }
		}

		public GroupStopsModelView GroupStopsModelView
		{
			get { return groupStopsModelView; }
		}

		public FavouriteModelView FavouriteModelView
		{
			get
			{
				return favouriteModelView;
			}
		}

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
						updating = false;
						UpdateDataCommand.RaiseCanExecuteChanged();
					}, () => !updating);
				return updateDataCommand;
			}
		}

		public RelayCommand<Stop> ShowStopMap
		{
			get { return new RelayCommand<Stop>((x) => OnShowStop(new ShowArgs() { SelectedStop = x }), (x) => x != null); }
		}

		public RelayCommand<Rout> ShowRouteMap
		{
			get { return new RelayCommand<Rout>((x) => OnShowRoute(new ShowArgs() { SelectedRoute = x }), (x) => x != null); }
		}

	    public NewsModelView NewsModelView
        {
	        get { return newsModelView; }
	    }

	    public event Show ShowStop;
		public event Show ShowRoute;
		public delegate void Show(object sender, ShowArgs args);

		protected virtual void OnShowStop(ShowArgs args)
		{
			var handler = ShowStop;
			if (handler != null) handler(this, args);
		}

		protected virtual void OnShowRoute(ShowArgs args)
		{
			var handler = ShowRoute;
			if (handler != null) handler(this, args);
		}
	}
}