using MinskTrans.DesctopClient.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using PropertyChanged;

namespace MinskTrans.DesctopClient
{

//	[ImplementPropertyChanged]
	public class TimeTableRepository : TimeTableRepositoryBase
	{
		readonly Context context;
		public TimeTableRepository(Context context)
			: base(context)
		{

		}
		
		
		public override bool RoutsHaveStopId(int stopId)
		{
			return Routs.AsParallel().Any(x => x.RouteStops.Contains(stopId));
		}

		public override string TransportToString(Stop stop, TransportType type)
		{
			switch (type)
			{
				case TransportType.Bus:
					{
						var temp = stop.Routs.Where(rout => rout.Transport == TransportType.Bus).Select(rout => rout.RouteNum).Distinct().ToList();
						if (temp.Count == 0)
							return "";
						StringBuilder builder = new StringBuilder("Авт: ");
						foreach (var rout in temp)
						{
							builder.Append(rout).Append(", ");
						}
						//builder.Append(temp.Select(x => x.RouteNum + ", ").ToList());
						builder.Remove(builder.Length - 2, 2);
						return builder.ToString();
					}
				case TransportType.Tram:
					{
						var temp = stop.Routs.Where(rout => rout.Transport == TransportType.Tram).Select(rout => rout.RouteNum).Distinct().ToList();
						if (temp.Count == 0)
							return "";
						StringBuilder builder = new StringBuilder("Трам: ");
						foreach (var rout in temp)
						{
							builder.Append(rout).Append(", ");
						}
						//builder.Append(temp.Select(x => x.RouteNum + ", "));
						builder.Remove(builder.Length - 2, 2);

						return builder.ToString();
					}
				case TransportType.Metro:
					{
						var temp = stop.Routs.Where(rout => rout.Transport == TransportType.Metro).Select(rout => rout.RouteNum).Distinct().ToList();
						if (temp.Count == 0)
							return "";
						StringBuilder builder = new StringBuilder("Метро: ");
						foreach (var rout in temp)
						{
							builder.Append(rout).Append(", ");
						}
						//builder.Append(temp.Select(x => x.RouteNum + ", "));
						builder.Remove(builder.Length - 2, 2);
						return builder.ToString();
					}
				case TransportType.Trol:
					{
						var temp = stop.Routs.Where(rout => rout.Transport == TransportType.Trol).Select(rout => rout.RouteNum).Distinct().ToList();
						if (temp.Count == 0)
							return "";
						StringBuilder builder = new StringBuilder("трол: ");
						foreach (var rout in temp)
						{
							builder.Append(rout).Append(", ");
						}
						//builder.Append(temp.Select(x => x.RouteNum + ", "));
						builder.Remove(builder.Length - 2, 2);
						return builder.ToString();
					}
			}
			return "";
		}

	}
}
