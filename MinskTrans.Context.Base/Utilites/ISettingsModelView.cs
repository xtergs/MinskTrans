using System;
using System.ComponentModel;

namespace MyLibrary
{
	public interface ISettingsModelView:INotifyPropertyChanged
	{
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
	    bool HaveConnection();
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