using System.Collections.Generic;

using MinskTrans.DesctopClient.Model;
using MinskTrans.Library;

namespace MinskTrans.DesctopClient.Modelview
{
	public class SettingsModelView
	{
		private readonly Context context;

		public SettingsModelView(Context newContext)
		{
			context = newContext;
		}

		public List<Rout> FavouritRoutes { get; set; }
		public List<Stop> FavouritStops { get; set; }

		public List<GroupStop> GroupStops { get; set; }
	}
}