using System;
using MinskTrans.Context.Base.BaseModel;

namespace MinskTrans.AutoRouting.AutoRouting
{
	public class EquirectangularDistance : IDistanceCalculator
	{
		public double CalculateDistance(double lat1, double long1, double lat2, double long2)
		{
			//var x = (long2 - long1)*Math.Cos((lat1 + lat2)/2);
			//var y = lat2 - lat1;
			//double R = 6371000; //Earch radius
			//var d = Math.Sqrt(x*x + y*y)*R;
			//return d;
			var R = 6371000; //m

			var dLat = (lat2 - lat1) * (Math.PI / 180);
			var dLon = (long2 - long1) * (Math.PI / 180);

			lat1 *= (Math.PI/180);
			lat2 *= (Math.PI/180);

			var a = Math.Sin(dLat/2)*Math.Sin(dLat/2) +
			        Math.Sin((dLon/2))*Math.Sin(dLon/2)*
			        Math.Cos(lat1)*Math.Cos(lat2);

			var c = 2*Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

			return R*c;
		}

		public double CalculateDistance(Stop stop1, Stop stop2)
		{
			return CalculateDistance(stop1.Lat, stop1.Lng, stop2.Lat, stop2.Lng);
		}
	}
}
