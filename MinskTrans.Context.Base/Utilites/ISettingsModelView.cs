using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using MinskTrans.Utilites.Base.Net;

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

		int FontSize { get; set; }
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

	public class SettingsModelView : ISettingsModelView
	{
		private readonly IApplicationSettingsHelper helper;



		public SettingsModelView(IApplicationSettingsHelper helper)
			: base()
		{
			if (helper == null)
				throw new ArgumentNullException(nameof(helper));
			this.helper = helper;
		}

		static string SettingsToStr([CallerMemberName] string propertyName = null)
		{
			return propertyName;
		}

		public bool CurrentTimeRouts
		{
			get
			{
				return helper.SimpleGet<bool>(false);
			}
			set
			{
				helper.SimpleSet(value);
				OnPropertyChanged();
			}
		}

		public DateTime LastSeenHotNewsDateTimeUtc {
			get { return helper.SimbleGet(); }

			set
			{
				helper.SimpleSet(value);
				OnPropertyChanged();
			}
		}

		public bool HaveConnection()
		{

			return InternetHelperBase.Is_Connected && (InternetHelperBase.Is_InternetAvailable ||
												   InternetHelperBase.Is_Wifi_Connected == UpdateOnWiFi);
		}

		public int FontSize {
			get { return helper.SimpleGet(15); }

			set
			{
				helper.SimpleSet(value);
				OnPropertyChanged();
			}
		}


		public TypeOfUpdate LastUpdatedDataInBackground
		{
			get
			{
				return (TypeOfUpdate)helper.SimpleGet((int)TypeOfUpdate.None);
			   
			}

			set
			{
				helper.SimpleSet((int)value);
				//if (!ApplicationData.Current.LocalSettings.Values.ContainsKey(SettingsToStr()))
				//	ApplicationData.Current.LocalSettings.Values.Add(SettingsToStr(), (int)value);
				//else
				//	ApplicationData.Current.LocalSettings.Values[SettingsToStr()] = (int)value;
				OnPropertyChanged();
			}
		}

		//ApplicationSettingsHelper lastUpdateDBDateTimeback;
		public DateTime LastUpdateDBDatetime
		{

			get
			{
				return helper.SimbleGet();
				//if (lastUpdateDBDateTimeback == null)
				//	lastUpdateDBDateTimeback = new ApplicationSettingsHelper();
				//return lastUpdateDBDateTimeback.DateTimeSettings;
			}

			set
			{
				//if (lastUpdateDBDateTimeback == null)
				//	lastUpdateDBDateTimeback = new ApplicationSettingsHelper();
				//lastUpdateDBDateTimeback.DateTimeSettings = value;
				helper.SimpleSet(value);
				OnPropertyChanged();
			}
		}

		public string LastUnhandeledException
		{

			get
			{
				return helper.SimpleGet("");
			}

			set
			{
				helper.SimpleSet(value);
				OnPropertyChanged();
			}

		}

		public bool ShowTopStopsByCoordinats
		{
			get
			{
				return helper.SimpleGet(true);
			}

			set
			{
				helper.SimpleSet(value);
				OnPropertyChanged();
			}

		}

		public List<int> PreDefMins { get; } = new List<int>() { 5, 10, 15, 20, 30, 60 };

		public int TimeInPast
		{
			get
			{
				return helper.SimpleGet(15);
			}

			set
			{
				helper.SimpleSet(value);
				OnPropertyChanged();
				OnPropertyChanged("TimeSchedule");
			}

		}

		public bool UseGPS
		{

			get
			{
				return helper.SimpleGet(true);
			}

			set
			{
				helper.SimpleSet(value);
				OnPropertyChanged();
			}

		}

		public TimeSpan ReconnectPushServerTimeSpan
		{
			get { return new TimeSpan(0, 0, 1, 0); }
		}

		public bool KeepTracking
		{

			get
			{
				return helper.SimpleGet(true);
			}

			set
			{
				helper.SimpleSet(value);
				OnPropertyChanged();
			}

		}

		public string PrivatyPolicity
		{
			get
			{
				return
					@"Приложение Минский общественный транспорт не хранит пользовательских данных. Приложение использует интернет подключение только для получения последних обновлений расписания транспорта.";
			}
		}
		public TimeSpan InvervalAutoUpdateTimeSpan
		{

			get
			{
				return helper.SimpleGet(new TimeSpan(0, 1, 0, 0, 0));
			}

			set
			{
				helper.SimpleSet(value);
				//if (!ApplicationData.Current.LocalSettings.Values.ContainsKey(SettingsToStr()))
				//	ApplicationData.Current.LocalSettings.Values.Add(SettingsToStr(), value);
				//else
				//	ApplicationData.Current.LocalSettings.Values[SettingsToStr()] = value;
				OnPropertyChanged();
			}
		}

		public Error TypeError { get; set; }

		public int VariantConnect
		{
			get
			{
				return helper.SimpleGet(2);
			   
			}
			set
			{
				if (value < 0)
					return;
				helper.SimpleSet(value);
				//if (!ApplicationData.Current.LocalSettings.Values.ContainsKey(SettingsToStr()))
				//	ApplicationData.Current.LocalSettings.Values.Add(SettingsToStr(), value);
				//else
				//	ApplicationData.Current.LocalSettings.Values[SettingsToStr()] = value;
				OnPropertyChanged();
			}
		}

		public double IntervalAutoUpdate
		{
			get { return InvervalAutoUpdateTimeSpan.TotalMinutes; }
			set
			{
				InvervalAutoUpdateTimeSpan = TimeSpan.FromMinutes(value);
				OnPropertyChanged();
			}
		}

		public int GPSThreshholdMeters
		{
			get { return 5; }
		}

		public uint GPSInterval
		{
			get { return 1000; }
		}

		public bool CurrentDate
		{
			get
			{
				return helper.SimpleGet(true);
			}

			set
			{
				helper.SimpleSet(value);
				OnPropertyChanged();
			}
		}

		public bool UpdateOnWiFi
		{

			get
			{
				return helper.SimpleGet(true);
			}

			set
			{
				helper.SimpleSet(value);
				OnPropertyChanged();
			}
		}

		public bool Develop
		{

			get
			{
#if BETA

                return helper.SimpleGet(true);
#else
			    return false;
#endif
			}

			set
			{
				helper.SimpleSet(value);
				OnPropertyChanged();
			}
		}

		public bool UpdateOnMobileData
		{

			get
			{
				return helper.SimpleGet(true);
			}

			set
			{
				helper.SimpleSet(value);
				OnPropertyChanged();
			}

		}


		public DateTime LastUpdateDbDateTimeUtc
		{
			get
			{
				return helper.SimbleGet();
			}
			set
			{
				helper.SimpleSet(value);
				OnPropertyChanged();
			}
		}

		public DateTime LastNewsTimeUtc
		{
			get { return helper.SimbleGet(); }
			set
			{
				helper.SimpleSet(value);
				OnPropertyChanged();
			}
		}

		public DateTime LastUpdateHotNewsDateTimeUtc
		{
			get { return helper.SimbleGet(); }
			set
			{
				helper.SimpleSet(value);
				OnPropertyChanged();
			}
		}

		public DateTime LastSeenMainNewsDateTimeUtc
		{
			get { return helper.SimbleGet(); }

			set
			{
				helper.SimpleSet(value);
				OnPropertyChanged();
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			var handler = PropertyChanged;
			if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}