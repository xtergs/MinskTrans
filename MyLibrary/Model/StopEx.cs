using System.Collections.Generic;
using System.Linq;

namespace MinskTrans.DesctopClient.Model
{
	public class StopEx:Stop
	{
		private IContext context;
		public StopEx(IContext context,string str, Stop stop) : base(str, stop)
		{
			this.context = context;
		}

		public StopEx()
			:base()
		{ }

		public StopEx(IContext context,string str) : base(str)
		{
			this.context = context;
		}

		#region Overrides of Stop

		public override List<Rout> Routs
		{
			get
			{
				if (routs == null)
					routs = context.Routs.Where(x => x.RouteStops.Contains(ID)).ToList();
				return routs;
			}
			set { base.Routs = value; }
		}

		#endregion
	}
}
