using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MinskTrans.Context.Base.BaseModel;
using MinskTrans.DesctopClient;

namespace MinskTrans.Net.Base
{
	public interface ITimeTableParser
	{
		IEnumerable<Stop> ParsStops(string stops, IContext context = null);

		IEnumerable<Rout> ParsRout(string routs, IContext context = null);

		IEnumerable<Schedule> ParsTime(string times);
	}
}
