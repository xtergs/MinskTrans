using System;
using System.ComponentModel;
using MinskTrans.Context.Base.BaseModel;

namespace MinskTrans.Context.Utilites
{
    public interface ISettingsModelView:INotifyPropertyChanged
	{
		int[] SimpleArray { get; }
		int TimeInPast { get; set; }
		bool Develop { get; set; }
		bool CurrentDate { get; set; }
	  //  bool DayOfWeek { get; set; }
		bool UpdateOnWiFi { get; set; }
		bool UpdateOnMobileData { get; set; }
		DateTime LastUpdateDbDateTimeUtc { get; set; }
		DateTime LastNewsTimeUtc { get; set; }
		DateTime LastUpdateHotNewsDateTimeUtc { get; set; }
		TypeOfUpdate LastUpdatedDataInBackground { get; set; }
		string PrivatyPolicity { get; }
		string LastUnhandeledException { get; set; }
		Error TypeError { get; set; }
		int GPSThreshholdMeters { get;}
		uint GPSInterval { get; }
		bool UseGPS { get; set; }
		bool KeepTracking { get; set; }
		DateTime LastSeenMainNewsDateTimeUtc { get; set; }
		DateTime LastSeenHotNewsDateTimeUtc { get; set; }
		bool HaveConnection();
	    void UpdateNetworkData();

		int FontSize { get; set; }

        string ChangeLogOnce { get; }
        string ChangeLog { get; }

		int PrevFavouriteRouts { get; set; }
		int NextFavouriteRouts { get; set; }

		bool ConsiderDistanceSortStops { get; set; }
		bool ConsiderFrequencySortStops { get; set; }
		bool NotifyAboutNews { get; set; }
        bool UseWebSeacher { get; set; }
	    TimeSpan CachedCalculatorKeepAliveInterval { get; set; }

        bool IsShowStops { get; set; }
        bool IsShowFavouriteStops { get; set; }
        bool IsShowFavouriteRoutes { get; set; }
	    TransportType RoutsSelectedTransportType { get; set; }
        bool ShowButtonLabels { get; set; }
    }

	public enum Error
	{
		None = 0,
		Critical = 1,
		Repeated = 2
	}

	[Flags]
	public enum TypeOfUpdate
	{
		None = 0,
		Db = 0x00000001,
		News = 0x00000002
	}
}