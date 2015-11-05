using System;

namespace MinskTrans.AutoRouting.AutoRouting
{
	public class EquirectangularDistance : IDistanceCalculator
	{
		public double CalculateDistance(double lat1, double long1, double lat2, double long2)
		{
			var x = (long2 - long1)*Math.Cos((lat1 + lat2)/2);
			var y = lat2 - lat1;
			double R = 6371000; //Earch radius
			var d = Math.Sqrt(x*x + y*y)*R;
			return d;
		}
	}
}
