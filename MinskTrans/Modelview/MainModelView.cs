using MapControl;
using GalaSoft.MvvmLight.CommandWpf;
using Autofac;
using MinskTrans.Context.Base.BaseModel;
using MinskTrans.Net.Base;
using MinskTrans.Utilites;
using MinskTrans.Utilites.Base.IO;
using MinskTrans.Utilites.Base.Net;
using MinskTrans.Utilites.Desktop;

namespace MinskTrans.DesctopClient.Modelview
{
	public class MainModelView : BaseModelView
	{
		//private readonly IContext context;
		private readonly GroupStopsModelView groupStopsModelView;
		private readonly RoutesModelview routesModelview;
		private readonly SettingsModelView settingsModelView;
		private readonly StopModelView stopMovelView;
		private readonly FavouriteModelView favouriteModelView;
		private readonly FindModelView findModelView;
		private readonly MapModelView mapModelView;
		//private readonly IContext timeTable;
		readonly UpdateManagerBase updateManager;
		static MainModelView model;

		public static MainModelView Get()
		{
			if (model == null)
				model = new MainModelView();
			return model;
		}

		private MainModelView()
		{
			var builder = new ContainerBuilder();
			builder.RegisterType<FileHelperDesktop>().As<FileHelperBase>();
			//builder.RegisterType<SqlEFContext>().As<IContext>().SingleInstance().WithParameter("connectionString", @"Data Source=(localdb)\ProjectsV12;Initial Catalog=Entity3_Test_MinskTrans;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False");
			builder.RegisterType<Context.Context>().As<IContext>().SingleInstance();
			builder.RegisterType<UpdateManagerBase>();
			builder.RegisterType<ShedulerParser>().As<ITimeTableParser>();
			builder.RegisterType<InternetHelperDesktop>().As<InternetHelperBase>();
			//builder.RegisterType<Context>().As<IContext>();
			
			var container = builder.Build();

			context = container.Resolve<IContext>();
			updateManager = container.Resolve<UpdateManagerBase>();
			//timeTable = container.Resolve<IContext>();

			settingsModelView = new SettingsModelView();
			routesModelview = new RoutesModelview(Context, SettingsModelView);
			stopMovelView = new StopModelView(Context, settingsModelView, true);
			groupStopsModelView = new GroupStopsModelView(Context, settingsModelView);
			favouriteModelView = new FavouriteModelView(Context);
			findModelView = new FindModelView(Context, settingsModelView);

			
		}

		public MainModelView(Map map)
			: this()
		{
			mapModelView = new MapModelView(Context, map, settingsModelView);
			model = this;
		}

		public MapModelView MapModelView
		{
			get
			{
				return mapModelView;
			}
		}

		public FindModelView FindModelView
		{
			get { return findModelView; }
		}

		public StopModelView StopMovelView
		{
			get { return stopMovelView; }
		}

		public RoutesModelview RoutesModelview
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

		public UpdateManagerBase UpdateManager
		{
			get
			{
				return updateManager;
			}
		}

		object lockObject = new object();
		bool updateing = false;
		RelayCommand updateDataCommand;
		public RelayCommand UpdateDataCommand
		{
			get
			{
				if (updateDataCommand == null)
					updateDataCommand = new RelayCommand(async () =>
					{
						updateing = true;
						if (await UpdateManager.DownloadUpdate())
						{
							var timeTable = await UpdateManager.GetTimeTable();
							if (await Context.HaveUpdate(timeTable.Routs, timeTable.Stops, timeTable.Time))
								await Context.ApplyUpdate(timeTable.Routs, timeTable.Stops, timeTable.Time);
						}
						updateing = false;
					}, () => { return !updateing; });

				return updateDataCommand;
			}
		}

		public SettingsModelView SettingsModelView
		{
			get
			{
				return settingsModelView;
			}
		}

		//public GalaSoft.MvvmLight.Command.RelayCommand<Stop> ShowStopMap
		//{
		//	get { return new GalaSoft.MvvmLight.Command.RelayCommand<Stop>((x) => OnShowStop(new ShowArgs() { SelectedStop = x }), (x) => x != null); }
		//}

		//public GalaSoft.MvvmLight.Command.RelayCommand<Rout> ShowRouteMap
		//{
		//	get { return new GalaSoft.MvvmLight.Command.RelayCommand<Rout>((x) => OnShowRoute(new ShowArgs() { SelectedRoute = x }), (x) => x != null); }
		//}
		//public event Show ShowStop;
		//public event Show ShowRoute;
		//public delegate void Show(object sender, ShowArgs args);

		//protected virtual void OnShowStop(ShowArgs args)
		//{
		//	var handler = ShowStop;
		//	if (handler != null) handler(this, args);
		//}

		//protected virtual void OnShowRoute(ShowArgs args)
		//{
		//	var handler = ShowRoute;
		//	if (handler != null) handler(this, args);
		//}

		//public IContext TimeTable
		//{
		//	get
		//	{
		//		return timeTable;
		//	}
		//}
	}
}
