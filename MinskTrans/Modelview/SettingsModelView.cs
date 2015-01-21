using System.Collections.Generic;
using System.Web.Routing;
using MinskTrans.DesctopClient.Model;

namespace MinskTrans.DesctopClient.Modelview
{
	public class SettingsModelView
	{
		private readonly Context context;

		public SettingsModelView(Context newContext)
		{
			context = newContext;
		}

		public List<Route> FavouritRoutes { get; set; }
		public List<Stop> FavouritStops { get; set; }

		public List<GroupStop> GroupStops { get; set; }
	}
}