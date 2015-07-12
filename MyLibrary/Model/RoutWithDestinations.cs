using System.Collections.Generic;
using System.Linq;

namespace MinskTrans.DesctopClient.Model
{
	public class RoutWithDestinations : Rout
	{

		public RoutWithDestinations(Rout newRout, IContext context)
			:base(newRout)
		{

			//rout = newRout;
			destinations =
				context.Routs.Where(
					x =>
						x.Stops.Count > 0 && x.RouteNum == newRout.RouteNum && x.Transport == newRout.Transport &&
						(x.RoutType.Contains("A>B") || x.RoutType.Contains("B>A")))
					.Select(x => Trim(x.StartStop.Name) + " - " + Trim(x.DestinationStop.Name)).Distinct();
		}

		public RoutWithDestinations()
			:base()
		{
			
		}
		string Trim(string str)
		{
			return str.Replace("~(посадки-высадки нет)", "");
		}

		//private readonly Rout rout;
		private readonly IEnumerable<string> destinations; 

		public Rout Rout
		{
			get { return this; }
		}

		public IEnumerable<string> Destinations => destinations;
	}
}
