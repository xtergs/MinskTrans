using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MyLibrary
{
	public interface IApplicationSettingsHelper
	{
		// DateTime DateTimeSettings { get; set; }
		void SimpleSet<T>(T value, [CallerMemberName] string key = null);
		T SimpleGet<T>(T defValue = default(T), [CallerMemberName] string key = null);
		Error SimpleGet(Error defValue = default(Error), [CallerMemberName] string key = null);
		void SimpleSet<T>(Error value, [CallerMemberName] string key = null);
		//void SimpleSet(string value, [CallerMemberName] string key = null);
		//void SimpleSet(bool value, [CallerMemberName] string key = null);
		//void SimpleSet(int value, [CallerMemberName] string key = null);
		//string SimleGet(string defValue = null, [CallerMemberName] string key = null);
		//object SimleGet(object defValue = null, [CallerMemberName] string key = null);
		//bool SimleGet(bool defValue = true, [CallerMemberName] string key = null);
		//int SimleGet(int defValue = 0, [CallerMemberName] string key = null);
		DateTime SimbleGet(DateTime value = default(DateTime), [CallerMemberName] string key = null);
		void SimpleSet(DateTime value, [CallerMemberName] string key = null);
	}
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
		DateTime LastSeenMainNewsDateTimeUtc { get; set; }
		DateTime LastSeenHotNewsDateTimeUtc { get; set; }
		bool HaveConnection();
	    void UpdateNetworkData();

		int FontSize { get; set; }

        string ChangeLogOnce { get; }
        string ChangeLog { get; }
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