using MinskTrans.Context.Base;
using MinskTrans.DesctopClient;
using MinskTrans.DesctopClient.Modelview;

namespace MinskTrans.Universal.ModelView
{
	public class FindModelView : BaseModelView
	{
		private readonly StopModelView stopModelView;
		private readonly RoutsModelView routsModelview;

		public StopModelView StopModelView
		{
			get
			{
				return stopModelView;
			}
		}

		public RoutsModelView RoutsModelView
		{
			get { return routsModelview;
			}
		}

		public FindModelView(IContext newContext, SettingsModelView settingsModelView) : base(newContext)
		{
			stopModelView = new StopModelView(newContext, settingsModelView, true);
			routsModelview = new RoutsModelView(newContext);
		}
	}
}
