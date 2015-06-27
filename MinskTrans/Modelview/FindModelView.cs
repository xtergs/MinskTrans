namespace MinskTrans.DesctopClient.Modelview
{
	public class FindModelView : BaseModelView
	{
		private readonly StopModelView stopModelView;
		private readonly RoutesModelview routesModelview;

		public StopModelView StopModelView
		{
			get
			{
				return stopModelView;
			}
		}

		public RoutesModelview RoutesModelview
		{
			get { return routesModelview;
			}
		}

		public FindModelView(Context newContext, SettingsModelView settingsModelView) : base(newContext)
		{
			stopModelView = new StopModelView(newContext, settingsModelView);
			routesModelview = new RoutesModelview(newContext);
		}
	}
}
