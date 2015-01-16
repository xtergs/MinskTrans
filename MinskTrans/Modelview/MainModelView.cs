using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinskTrans.Modelview
{
	public class MainModelView
	{
		readonly RoutesModelview routesModelview = new RoutesModelview();
		readonly StopMovelView stopMovelView = new StopMovelView();

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
