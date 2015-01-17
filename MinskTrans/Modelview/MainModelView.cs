namespace MinskTrans.Modelview
{
	public class MainModelView
	{
		private readonly Context context;
		private readonly RoutesModelview routesModelview;
		private readonly StopMovelView stopMovelView;
		private readonly SettingsModelView settingsModelView;

		public MainModelView()
		{
			context = new Context();
			routesModelview = new RoutesModelview(context);
			stopMovelView = new StopMovelView(context);
			settingsModelView = new SettingsModelView();
		}

		public StopMovelView StopMovelView
		{
			get { return stopMovelView; }
		}

		public RoutesModelview RoutesModelview
		{
			get { return routesModelview; }
		}
	}
}