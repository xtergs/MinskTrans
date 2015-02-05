using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MinskTrans.DesctopClient;

namespace MinskTrans.Universal.Model
{
	public class RoutWithDestinations
	{

		public RoutWithDestinations(Rout newRout, IEnumerable<string> listDestinations)
		{
			rout = newRout;
			destinations = listDestinations.Distinct();
		}

		public RoutWithDestinations(Rout newRout, Context context)
		{
			rout = newRout;
			destinations =
				context.Routs.Where(
					x =>
						x.Stops.Count > 0 && x.RouteNum == newRout.RouteNum && x.Transport == newRout.Transport &&
						(x.RoutType.Contains("A>B") || x.RoutType.Contains("B>A")))
					.Select(x => x.StartStop.Name + " - " + x.DestinationStop.Name);
		}

		private readonly Rout rout;
		private readonly IEnumerable<string> destinations; 

		public Rout Rout
		{
			get { return rout; }
		}

		public IEnumerable<string> Destinations
		{
			get
			{
				return destinations;
			}
		} 

	}
}
