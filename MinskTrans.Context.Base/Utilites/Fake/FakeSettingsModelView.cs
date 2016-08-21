using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MyLibrary
{
	public class FakeSettingsModelView : ISettingsModelView
	{
		#region Implementation of INotifyPropertyChanged

		public event PropertyChangedEventHandler PropertyChanged;

		#endregion

		#region Implementation of ISettingsModelView

		public int[] SimpleArray { get; }
		public int TimeInPast { get; set; }
		public bool Develop { get; set; }
		public bool CurrentDate { get; set; }
		public bool UpdateOnWiFi { get; set; }
		public bool UpdateOnMobileData { get; set; }
		public DateTime LastUpdateDbDateTimeUtc { get; set; }
		public DateTime LastNewsTimeUtc { get; set; }
		public DateTime LastUpdateHotNewsDateTimeUtc { get; set; }
		public TypeOfUpdate LastUpdatedDataInBackground { get; set; }
		public string PrivatyPolicity { get; }
		public string LastUnhandeledException { get; set; }
		public Error TypeError { get; set; }
		public int GPSThreshholdMeters { get; }
		public uint GPSInterval { get; }
		public bool UseGPS { get; set; }
		public bool KeepTracking { get; set; }
		public DateTime LastSeenMainNewsDateTimeUtc { get; set; }
		public DateTime LastSeenHotNewsDateTimeUtc { get; set; }

		public bool HaveConnection()
		{
			throw new NotImplementedException();
		}

		public void UpdateNetworkData()
		{
		}

		public int FontSize { get; set; }
		public string ChangeLogOnce { get; }
		public string ChangeLog { get; }
		public int PrevFavouriteRouts { get; set; }
		public int NextFavouriteRouts { get; set; }
		public bool ConsiderDistanceSortStops { get; set; }
		public bool ConsiderFrequencySortStops { get; set; }
		public bool NotifyAboutNews { get; set; }

		#endregion

		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}