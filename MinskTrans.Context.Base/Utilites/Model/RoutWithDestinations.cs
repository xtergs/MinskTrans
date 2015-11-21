using System.Collections.Generic;
using System.Linq;
using MinskTrans.Context;
using MinskTrans.Context.Base;
using MinskTrans.Context.Base.BaseModel;

namespace MinskTrans.DesctopClient.Model
{
	public class RoutWithDestinations : Rout
	{

		public RoutWithDestinations(Rout newRout, IBussnessLogics context)
			:base(newRout)
		{

			//rout = newRout;
			destinations =
				context.Routs.Where(
					x =>
						x.Stops.Count > 0 && x.RouteNum == newRout.RouteNum && x.Transport == newRout.Transport &&
						(x.RoutType.Contains("A>B") || x.RoutType.Contains("B>A")))
					.Select(x => Trim(x.StartStop.Name) + " - " + Trim(x.DestinationStop.Name)).Distinct();
		    this.Stops = newRout.Stops;
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

		public IEnumerable<string> Destinations
		{
			get { return destinations; }
		}
	}
}
