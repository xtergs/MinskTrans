

using MinskTrans.Library;

namespace MinskTrans.DesctopClient.Modelview
{
	public class MainModelView
	{
		private readonly Context context;
		private readonly GroupStopsModelView groupStopsModelView;
		private readonly RoutesModelview routesModelview;
		private readonly SettingsModelView settingsModelView;
		private readonly StopModelView stopMovelView;

		public MainModelView(Context newContext)
		{
			context = newContext;
			routesModelview = new RoutesModelview(context);
			stopMovelView = new StopModelView(context);
			settingsModelView = new SettingsModelView(context);
			groupStopsModelView = new GroupStopsModelView(context);
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

		public Context Context { get { return context; } }

		
	}
}