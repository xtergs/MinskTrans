using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MinskTrans.DesctopClient;
using MinskTrans.DesctopClient.Modelview;

namespace MinskTrans.DesctopClient.ModelView
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

		public FindModelView(Context newContext, ISettingsModelView settingsModelView) : base(newContext)
		{
			stopModelView = new StopModelView(newContext, settingsModelView);
			routesModelview = new RoutesModelview(newContext);
		}
	}
}
