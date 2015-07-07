



using System.Linq;
using GalaSoft.MvvmLight;
using MapControl;
using MinskTrans.DesctopClient.Update;
using GalaSoft.MvvmLight.CommandWpf;

namespace MinskTrans.DesctopClient.Modelview
{
	public class MainModelView : ViewModelBase
	{
		private readonly Context context;
		private readonly GroupStopsModelView groupStopsModelView;
		private readonly RoutesModelview routesModelview;
		private readonly SettingsModelView settingsModelView;
		private readonly StopModelView stopMovelView;
		private readonly FavouriteModelView favouriteModelView;
		private readonly FindModelView findModelView;
		private readonly MapModelView mapModelView;
		private readonly TimeTableRepositoryBase TimeTable;
		readonly UpdateManagerBase updateManager;

		public MainModelView(Context newContext, TimeTableRepositoryBase timeTable, UpdateManagerBase updateManagerBase)
		{
			if (newContext == null)
				throw new System.ArgumentNullException("newContext");
			if (timeTable == null)
				throw new System.ArgumentNullException("timeTable");
			if (updateManagerBase == null)
				throw new System.ArgumentNullException("updateManagerBase");
			context = newContext;
			settingsModelView = new SettingsModelView();
			routesModelview = new RoutesModelview(context);
			stopMovelView = new StopModelView(context, settingsModelView, true);
			groupStopsModelView = new GroupStopsModelView(context, settingsModelView);
			favouriteModelView = new FavouriteModelView(context);
			findModelView = new FindModelView(context, settingsModelView);

			if (IsInDesignMode)
			{
				StopMovelView.FilteredSelectedStop = Context.ActualStops.First(x => x.SearchName.Contains("шепичи"));
			}
		}

		public MainModelView(Context newContext, Map map, TimeTableRepositoryBase timeTable, UpdateManagerBase updateManager)
			: this(newContext, timeTable, updateManager)
		{
			mapModelView = new MapModelView(newContext, map);
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

		public Context Context { get { return context; } }

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
	}
}
