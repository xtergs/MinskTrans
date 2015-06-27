



using System.Linq;
using GalaSoft.MvvmLight;
using MapControl;


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


		public MainModelView(Context newContext)
		{
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

		public MainModelView(Context newContext, Map map)
			:this(newContext)
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
			get { return findModelView;}
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

		
	}
}