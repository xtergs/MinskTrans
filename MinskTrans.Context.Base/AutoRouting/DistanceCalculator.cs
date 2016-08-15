using MinskTrans.Context.Base.BaseModel;

namespace MinskTrans.AutoRouting.AutoRouting
{
	interface IDistanceCalculator
	{
		double CalculateDistance(double lat1, double long1, double lat2, double long2);

		double CalculateDistance(Stop stop1, Stop stop2);
	}
}
