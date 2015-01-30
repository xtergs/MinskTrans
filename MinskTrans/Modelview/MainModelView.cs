



using System.Linq;
using GalaSoft.MvvmLight;
using MinskTrans.DesctopClient.ModelView;


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


		public MainModelView(Context newContext)
		{
			context = newContext;
			routesModelview = new RoutesModelview(context);
			stopMovelView = new StopModelView(context);
			settingsModelView = new SettingsModelView(context);
			groupStopsModelView = new GroupStopsModelView(context);
			favouriteModelView = new FavouriteModelView(context);
			findModelView = new FindModelView(context);

			if (IsInDesignMode)
			{
				StopMovelView.FilteredSelectedStop = Context.ActualStops.First(x => x.SearchName.Contains("шепичи"));
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