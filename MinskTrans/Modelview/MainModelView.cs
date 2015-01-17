namespace MinskTrans.Modelview
{
	public class MainModelView
	{
		private readonly Context context;
		private readonly RoutesModelview routesModelview;
		private readonly StopMovelView stopMovelView;
		private readonly SettingsModelView settingsModelView;
		private readonly GroupStopsModelView groupStopsModelView;

		public MainModelView()
		{
			context = new Context();
			routesModelview = new RoutesModelview(context);
			stopMovelView = new StopMovelView(context);
			settingsModelView = new SettingsModelView(context);
			groupStopsModelView = new GroupStopsModelView(context);
		}

		public StopMovelView StopMovelView
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
	}
}