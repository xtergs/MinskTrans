using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Windows.UI.Xaml;
using CommonLibrary;
using GalaSoft.MvvmLight.Command;
using MinskTrans.DesctopClient;
using MinskTrans.DesctopClient.Modelview;
using Autofac;
using CommonLibrary.IO;
using MinskTrans.Context.Base;
using MinskTrans.Context.Base.BaseModel;
using MinskTrans.Net;
using MinskTrans.Net.Base;
using MinskTrans.Utilites.Base.IO;
using MinskTrans.Utilites.Base.Net;

namespace MinskTrans.Universal.ModelView
{

	public class MainModelView : BaseModelView, INotifyPropertyChanged
	{
		private static MainModelView mainModelView;
		//private readonly IContext context;
		private readonly GroupStopsModelView groupStopsModelView;
		private readonly RoutsModelView routesModelview;
		private readonly SettingsModelView settingsModelView;
		private readonly StopModelView stopMovelView;
		private readonly FavouriteModelView favouriteModelView;
		private FindModelView findModelView;
		private MapModelView mapMOdelView;
		private readonly NewsManager newsManager;
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
			//builder.RegisterType<SqlEFContext>().As<IContext>().SingleInstance().WithParameter("connectionString", @"Data Source=(localdb)\ProjectsV12;Initial Catalog=Entity3_Test_MinskTrans;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False");
			builder.RegisterType<Context>().As<IContext>().SingleInstance();
			builder.RegisterType<UpdateManagerUniversal>().As<UpdateManagerBase>();
			builder.RegisterType<InternetHelperUniversal>().As<InternetHelperBase>();
			//builder.RegisterType<Context>().As<IContext>();

			var container = builder.Build();

			context = container.Resolve<IContext>();
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

			newsManager = new NewsManager();
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

		public NewsManager NewsManager
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

						if (await updateManager.DownloadUpdate())
						{
							var timeTable = await updateManager.GetTimeTable();
							if (await Context.HaveUpdate(timeTable.Routs, timeTable.Stops, timeTable.Time))
							{
								await Context.ApplyUpdate(timeTable.Routs, timeTable.Stops, timeTable.Time);
								Context.AllPropertiesChanged();
								await Context.Save(true);
							}
						}
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