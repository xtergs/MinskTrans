



using System.Linq;
using GalaSoft.MvvmLight;
using MapControl;
using MinskTrans.DesctopClient.Update;
using GalaSoft.MvvmLight.CommandWpf;
using Autofac;
using MinskTrans.DesctopClient.Utilites.IO;
using MinskTrans.DesctopClient.Net;
using MyLibrary;

namespace MinskTrans.DesctopClient.Modelview
{
	public class MainModelView : ViewModelBase
	{
		private readonly IContext context;
		private readonly GroupStopsModelView groupStopsModelView;
		private readonly RoutesModelview routesModelview;
		private readonly SettingsModelView settingsModelView;
		private readonly StopModelView stopMovelView;
		private readonly FavouriteModelView favouriteModelView;
		private readonly FindModelView findModelView;
		private readonly MapModelView mapModelView;
		private readonly TimeTableRepositoryBase timeTable;
		readonly UpdateManagerBase updateManager;

		public MainModelView()
		{
			var builder = new ContainerBuilder();
			builder.RegisterType<FileHelperDesktop>().As<FileHelperBase>();
			builder.RegisterType<ContextDesctop>().As<IContext>().SingleInstance();
			builder.RegisterType<UpdateManagerDesktop>().As<UpdateManagerBase>();
			builder.RegisterType<InternetHelperDesktop>().As<InternetHelperBase>();
			builder.RegisterType<TimeTableRepository>().As<TimeTableRepositoryBase>();
			
			var container = builder.Build();

			context = container.Resolve<IContext>();
			updateManager = container.Resolve<UpdateManagerBase>();
			timeTable = container.Resolve<TimeTableRepositoryBase>();

			settingsModelView = new SettingsModelView();
			routesModelview = new RoutesModelview(TimeTable);
			stopMovelView = new StopModelView(TimeTable, settingsModelView, true);
			groupStopsModelView = new GroupStopsModelView(TimeTable, settingsModelView);
			favouriteModelView = new FavouriteModelView(TimeTable);
			findModelView = new FindModelView(TimeTable, settingsModelView);

			if (IsInDesignMode)
			{
				StopMovelView.FilteredSelectedStop = Context.ActualStops.First(x => x.SearchName.Contains("шепичи"));
			}
		}

		public MainModelView(Map map)
			: this()
		{
			mapModelView = new MapModelView(TimeTable, map, settingsModelView);
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

		public IContext Context { get { return context; } }

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

		public TimeTableRepositoryBase TimeTable
		{
			get
			{
				return timeTable;
			}
		}
	}
}
