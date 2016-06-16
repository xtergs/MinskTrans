using MinskTrans.Context;
using MinskTrans.Context.Base;
using MinskTrans.Context.UniversalModelView;
using MyLibrary;

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

		public FindModelView(StopModelView stopModelView, RoutesModelview routesModelView, IBussnessLogics newContext) : base(newContext)
		{
			this.stopModelView = stopModelView;
			this.routesModelview = routesModelView;
		}
	}
}
