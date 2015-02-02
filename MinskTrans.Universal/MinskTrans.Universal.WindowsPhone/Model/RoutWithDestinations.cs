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
