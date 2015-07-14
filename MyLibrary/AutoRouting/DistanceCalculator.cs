namespace MinskTrans.DesctopClient.AutoRouting
{
	interface IDistanceCalculator
	{
		double CalculateDistance(double lat1, double long1, double lat2, double long2);
	}
}
