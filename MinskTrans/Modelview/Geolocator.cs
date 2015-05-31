namespace MinskTrans.DesctopClient.Modelview
{
	public class Geolocator
	{
		public int MovementThreshold { get; set; }
		public uint ReportInterval { get; set; }
		public object StatusChanged { get; set; }
		public object PositionChanged { get; set; }
	}
}