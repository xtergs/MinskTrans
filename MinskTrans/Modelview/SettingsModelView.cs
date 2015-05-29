
using System;
using System.Runtime.CompilerServices;
using MinskTrans.Universal;

namespace MinskTrans.DesctopClient.Modelview
{
#if !WINDOWS_PHONE_APP && !WINDOWS_AP
using MinskTrans.DesctopClient.Properties;

#else
	using Windows.Storage;

#endif

	public class SettingsModelView : BaseModelView, ISettingsModelView
	{
		public enum Error
		{
			None = 0,
			Critical = 1,
			Repeated = 2
		}

		public SettingsModelView(Context newContext)
			: base(newContext)
		{

		}

		string SettingsToStr([CallerMemberName] string propertyName = null)
		{
			return propertyName;
		}

		public bool HaveConnection()
		{
			return InternetHelper.Is_Connected && (InternetHelper.Is_InternetAvailable ||
			                                       InternetHelper.Is_Wifi_Connected == UpdateOnWiFi);
		}

		public string LastUnhandeledException
		{
			get
			{
				if (!ApplicationData.Current.LocalSettings.Values.ContainsKey(SettingsToStr()))
					LastUnhandeledException = "";
				return (string)ApplicationData.Current.LocalSettings.Values[SettingsToStr()];
			}

			set
			{
				if (!ApplicationData.Current.LocalSettings.Values.ContainsKey(SettingsToStr()))
					ApplicationData.Current.LocalSettings.Values.Add(SettingsToStr(), value);
				else
					ApplicationData.Current.LocalSettings.Values[SettingsToStr()] = value;
				OnPropertyChanged();
			}
		}

		public bool ShowTopStopsByCoordinats
		{
			get
			{
				if (!ApplicationData.Current.LocalSettings.Values.ContainsKey(SettingsToStr()))
					ShowTopStopsByCoordinats = true;
				return (bool)ApplicationData.Current.LocalSettings.Values[SettingsToStr()];
			}

			set
			{
				if (!ApplicationData.Current.LocalSettings.Values.ContainsKey(SettingsToStr()))
					ApplicationData.Current.LocalSettings.Values.Add(SettingsToStr(), value);
				else
					ApplicationData.Current.LocalSettings.Values[SettingsToStr()] = value;
				OnPropertyChanged();
			}
		}

		public int TimeInPast
		{
			get
			{
#if !WINDOWS_PHONE_APP && !WINDOWS_AP
				return Settings.Default.TimeInPast;
#else
				if (!ApplicationData.Current.RoamingSettings.Values.ContainsKey("TimeInPast"))
					TimeInPast = 15;
				return (int) ApplicationData.Current.RoamingSettings.Values["TimeInPast"];
#endif
			}
			set
			{
#if !WINDOWS_PHONE_APP && !WINDOWS_AP
		
				Settings.Default.TimeInPast = value;
#else
				if (!ApplicationData.Current.RoamingSettings.Values.ContainsKey("TimeInPast"))
					ApplicationData.Current.RoamingSettings.Values.Add("TimeInPast", value);
				else
					ApplicationData.Current.RoamingSettings.Values["TimeInPast"] = value;
#endif
				OnPropertyChanged();
				OnPropertyChanged("TimeSchedule");
			}
		}

		public bool UseGPS
		{
			get
			{
				if (!ApplicationData.Current.LocalSettings.Values.ContainsKey(SettingsToStr()))
					UseGPS = true;
				return (bool)ApplicationData.Current.LocalSettings.Values[SettingsToStr()];
			}

			set
			{
				if (!ApplicationData.Current.LocalSettings.Values.ContainsKey(SettingsToStr()))
					ApplicationData.Current.LocalSettings.Values.Add(SettingsToStr(), value);
				else
					ApplicationData.Current.LocalSettings.Values[SettingsToStr()] = value;
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
				if (!ApplicationData.Current.LocalSettings.Values.ContainsKey(SettingsToStr()))
					KeepTracking = true;
				return (bool)ApplicationData.Current.LocalSettings.Values[SettingsToStr()];
			}

			set
			{
				if (!ApplicationData.Current.LocalSettings.Values.ContainsKey(SettingsToStr()))
					ApplicationData.Current.LocalSettings.Values.Add(SettingsToStr(), value);
				else
					ApplicationData.Current.LocalSettings.Values[SettingsToStr()] = value;
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
				if (!ApplicationData.Current.LocalSettings.Values.ContainsKey(SettingsToStr()))
					InvervalAutoUpdateTimeSpan = new TimeSpan(0, 1, 0, 0, 0);
				return (TimeSpan)ApplicationData.Current.LocalSettings.Values[SettingsToStr()];
			}

			set
			{
				if (!ApplicationData.Current.LocalSettings.Values.ContainsKey(SettingsToStr()))
					ApplicationData.Current.LocalSettings.Values.Add(SettingsToStr(), value);
				else
					ApplicationData.Current.LocalSettings.Values[SettingsToStr()] = value;
				OnPropertyChanged();
			}
		}

		public Error TypeError
		{
			get
			{
				if (!ApplicationData.Current.LocalSettings.Values.ContainsKey(SettingsToStr()))
					TypeError = Error.None;
				return (Error)ApplicationData.Current.LocalSettings.Values[SettingsToStr()];
			}

			set
			{
				if (!ApplicationData.Current.LocalSettings.Values.ContainsKey(SettingsToStr()))
					ApplicationData.Current.LocalSettings.Values.Add(SettingsToStr(), (int)value);
				else
					ApplicationData.Current.LocalSettings.Values[SettingsToStr()] = (int)value;
				OnPropertyChanged();
			}
		}

		public int VariantConnect
		{
			get
			{
				if (!ApplicationData.Current.LocalSettings.Values.ContainsKey(SettingsToStr()))
					VariantConnect = 2;
				return (int)ApplicationData.Current.LocalSettings.Values[SettingsToStr()];
			}
			set
			{
				if (value < 0)
					return;
				if (!ApplicationData.Current.LocalSettings.Values.ContainsKey(SettingsToStr()))
					ApplicationData.Current.LocalSettings.Values.Add(SettingsToStr(), value);
				else
					ApplicationData.Current.LocalSettings.Values[SettingsToStr()] = value;
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

		public bool UpdateOnWiFi
		{
			get
			{
#if !WINDOWS_PHONE_APP && !WINDOWS_AP
				return Settings.Default.UpdateOnWiFi;
#else
				if (!ApplicationData.Current.LocalSettings.Values.ContainsKey("UpdateOnWiFi"))
					UpdateOnWiFi = true;
				return (bool)ApplicationData.Current.LocalSettings.Values["UpdateOnWiFi"];
#endif
			}
			set
			{
#if !WINDOWS_PHONE_APP && !WINDOWS_AP
		
				Settings.Default.UpdateOnWiFi = value;
#else
				if (!ApplicationData.Current.LocalSettings.Values.ContainsKey("UpdateOnWiFi"))
					ApplicationData.Current.LocalSettings.Values.Add("UpdateOnWiFi", value);
				else
					ApplicationData.Current.LocalSettings.Values["UpdateOnWiFi"] = value;
#endif
				OnPropertyChanged();
				//OnPropertyChanged("UpdateOnWiFi");
			}
		}

		public bool UpdateOnMobileData
		{
			get
			{
#if !WINDOWS_PHONE_APP && !WINDOWS_AP
				return Settings.Default.UpdateOnMobileData;
#else
				if (!ApplicationData.Current.LocalSettings.Values.ContainsKey("UpdateOnMobileData"))
					UpdateOnMobileData = true;
				return (bool)ApplicationData.Current.LocalSettings.Values["UpdateOnMobileData"];
#endif
			}
			set
			{
#if !WINDOWS_PHONE_APP && !WINDOWS_AP

				Settings.Default.UpdateOnMobileData = value;
#else
				if (!ApplicationData.Current.LocalSettings.Values.ContainsKey("UpdateOnMobileData"))
					ApplicationData.Current.LocalSettings.Values.Add("UpdateOnMobileData", value);
				else
					ApplicationData.Current.LocalSettings.Values["UpdateOnMobileData"] = value;
#endif
				OnPropertyChanged();
				//OnPropertyChanged("UpdateOnWiFi");
			}
		}
	}
}