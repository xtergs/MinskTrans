﻿using MinskTrans.Context;
using MinskTrans.Context.Base;
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

		public FindModelView(IBussnessLogics newContext, ISettingsModelView settingsModelView) : base(newContext)
		{
			stopModelView = new StopModelView(newContext, settingsModelView);
			routesModelview = new RoutesModelview(newContext, settingsModelView);
		}
	}
}
