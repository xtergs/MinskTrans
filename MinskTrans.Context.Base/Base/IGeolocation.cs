namespace MinskTrans.Context.Base
{
	public interface IGeolocation
	{
		int MovementThreshold { get; set; }
		object PositionChanged { get; set; }
		uint ReportInterval { get; set; }
		event StatusChangedEventArgs StatusChanged;
	}
}
