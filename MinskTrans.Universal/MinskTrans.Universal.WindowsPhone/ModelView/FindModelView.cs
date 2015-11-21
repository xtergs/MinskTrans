using MinskTrans.Context;
using MinskTrans.Context.Base;
using MinskTrans.Context.UniversalModelView;
using MinskTrans.DesctopClient;
using MinskTrans.DesctopClient.Modelview;
using MyLibrary;

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

		public FindModelView(IBussnessLogics newContext, ISettingsModelView settingsModelView, IExternalCommands commands) : base(newContext)
		{
			stopModelView = new StopModelView(newContext, settingsModelView, commands, true);
			routsModelview = new RoutsModelView(newContext);
		}
	}
}
