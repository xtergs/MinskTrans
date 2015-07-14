namespace MinskTrans.Universal
{
	public struct LocationX
	{
		public LocationX(double latitude, double longitude)
			: this()
		{
			Latitude = latitude;
			Longitude = longitude;
		}

		public double Latitude { get; set; }
		public double Longitude { get; set; }
	}
}
