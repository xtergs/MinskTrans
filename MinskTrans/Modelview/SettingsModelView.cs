
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

//using MinskTrans.DesctopClient.Annotations;
//using MinskTrans.DesctopClient.Annotations;
using MyLibrary;

namespace MinskTrans.DesctopClient.Modelview
{
#if !WINDOWS_PHONE_APP && !WINDOWS_AP && !WINDOWS_UAP
using MinskTrans.DesctopClient.Annotations;
using MinskTrans.DesctopClient.Properties;

#else
	using CommonLibrary;
using MinskTrans.Universal;
using MinskTrans.Universal.Annotations;
	using Windows.Storage;

#endif

	public class ApplicationSettingsHelper
	{
#if WINDOWS_PHONE_APP || WINDOWS_UAP

		public ApplicationSettingsHelper([CallerMemberName] string member = null)
		{
			SettingsMember = member;
		}

		private string SettingsMember;
		private DateTime backField;
		public DateTime DateTimeSettings
		{
			get
			{
				if (backField == default(DateTime) && !ApplicationData.Current.LocalSettings.Values.ContainsKey(SettingsMember))
				{
					ApplicationData.Current.LocalSettings.Values.Add(SettingsMember, backField.ToString());
					return backField;
				}
				if (backField != default(DateTime))
					return backField;
				else
				{
					backField = DateTime.Parse(ApplicationData.Current.LocalSettings.Values[SettingsMember].ToString());
					return backField;
				}
			}

			set
			{
				if (backField == value)
					return;
				if (!ApplicationData.Current.LocalSettings.Values.ContainsKey(SettingsMember))
					ApplicationData.Current.LocalSettings.Values.Add(SettingsMember, value.ToString());
				else
					ApplicationData.Current.LocalSettings.Values[SettingsMember] = value.ToString();
				backField = value;
			}

		}

		#region Overrides of Object

		/// <summary>
		/// Returns a string that represents the current object.
		/// </summary>
		/// <returns>
		/// A string that represents the current object.
		/// </returns>
		public override string ToString()
		{
			return SettingsMember + " : " + DateTimeSettings;
		}

		#endregion

		static public void SimpleSet(string value, [CallerMemberName]string key = null)
		{
			if (!ApplicationData.Current.LocalSettings.Values.ContainsKey(key))
				ApplicationData.Current.LocalSettings.Values.Add(key, value);
			else
				ApplicationData.Current.LocalSettings.Values[key] = value;
		}

		static public void SimpleSet(bool value, [CallerMemberName]string key = null)
		{
			if (!ApplicationData.Current.LocalSettings.Values.ContainsKey(key))
				ApplicationData.Current.LocalSettings.Values.Add(key, value);
			else
				ApplicationData.Current.LocalSettings.Values[key] = value;
		}

		static public void SimpleSet(int value, [CallerMemberName]string key = null)
		{
			if (!ApplicationData.Current.LocalSettings.Values.ContainsKey(key))
				ApplicationData.Current.LocalSettings.Values.Add(key, value);
			else
				ApplicationData.Current.LocalSettings.Values[key] = value;
		}

		static public string SimleGet(string defValue = null, [CallerMemberName]string key = null)
		{

			return (string)SimleGet((object)defValue, key);
		}

		static public object SimleGet(object defValue = null, [CallerMemberName]string key = null)
		{
			if (!ApplicationData.Current.LocalSettings.Values.ContainsKey(key))
				ApplicationData.Current.LocalSettings.Values.Add(key, defValue);
			return ApplicationData.Current.LocalSettings.Values[key];
		}

		static public bool SimleGet(bool defValue = true, [CallerMemberName]string key = null)
		{

			return (bool)SimleGet((object)defValue, key);
		}

		static public int SimleGet(int defValue = 0, [CallerMemberName]string key = null)
		{
			return (int)SimleGet((object)defValue, key);
		}
#endif
	}

	public class SettingsModelView : ISettingsModelView
	{


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

		public SettingsModelView()
			: base()
		{

		}

		static string SettingsToStr([CallerMemberName] string propertyName = null)
		{
			return propertyName;
		}

#if WINDOWS_PHONE_APP || WINDOWS_UAP
		public bool HaveConnection()
		{
			
			return InternetHelper.Is_Connected && (InternetHelper.Is_InternetAvailable ||
			                                       InternetHelper.Is_Wifi_Connected == UpdateOnWiFi);
		}
#endif

#if WINDOWS_PHONE_APP || WINDOWS_UAP
		public TypeOfUpdate LastUpdatedDataInBackground
		{
			get
			{
				if (!ApplicationData.Current.LocalSettings.Values.ContainsKey(SettingsToStr()))
					LastUpdatedDataInBackground = TypeOfUpdate.None;
				return (TypeOfUpdate)Enum.Parse(typeof(TypeOfUpdate),ApplicationData.Current.LocalSettings.Values[SettingsToStr()].ToString());
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
#endif

		ApplicationSettingsHelper lastUpdateDBDateTimeback;
		public DateTime LastUpdateDBDatetime
		{
#if WINDOWS_PHONE_APP || WINDOWS_UAP
			get
			{
				if (lastUpdateDBDateTimeback == null)
					lastUpdateDBDateTimeback = new ApplicationSettingsHelper();
				return lastUpdateDBDateTimeback.DateTimeSettings;
            }

			set
			{
				if (lastUpdateDBDateTimeback == null)
					lastUpdateDBDateTimeback = new ApplicationSettingsHelper();
				lastUpdateDBDateTimeback.DateTimeSettings = value;
				OnPropertyChanged();
			}
#else
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
#endif
		}

		public string LastUnhandeledException
		{
#if WINDOWS_PHONE_APP || WINDOWS_UAP
			get
			{
				return ApplicationSettingsHelper.SimleGet("");
			}

			set
			{
				ApplicationSettingsHelper.SimpleSet(value);
				OnPropertyChanged();
			}
#else
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
#endif
		}

		public bool ShowTopStopsByCoordinats
		{
#if WINDOWS_PHONE_APP || WINDOWS_UAP
			get
			{
				return ApplicationSettingsHelper.SimleGet(true);
			}

			set
			{
				ApplicationSettingsHelper.SimpleSet(value);
				OnPropertyChanged();
			}
#else
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
#endif
		}

		public int TimeInPast
		{
			get
			{
				return ApplicationSettingsHelper.SimleGet(15);
			}

			set
			{
				ApplicationSettingsHelper.SimpleSet(value);
				OnPropertyChanged();
				OnPropertyChanged("TimeSchedule");
			}
		}

		public bool UseGPS
		{
#if WINDOWS_PHONE_APP || WINDOWS_UAP
			get
			{
				return ApplicationSettingsHelper.SimleGet(true);
			}

			set
			{
				ApplicationSettingsHelper.SimpleSet(value);
				OnPropertyChanged();
			}
#else
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
#endif
		}

		public TimeSpan ReconnectPushServerTimeSpan
		{
			get { return new TimeSpan(0, 0, 1, 0); }
		}

		public bool KeepTracking
		{
#if WINDOWS_PHONE_APP || WINDOWS_UAP
			get
			{
				return ApplicationSettingsHelper.SimleGet(true);
			}

			set
			{
				ApplicationSettingsHelper.SimpleSet(value);
				OnPropertyChanged();
			}
#else
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
#endif
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
#if WINDOWS_PHONE_APP || WINDOWS_UAP
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
#else
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
#endif
		}

		public Error TypeError
		{
#if WINDOWS_PHONE_APP || WINDOWS_UAP
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
#else
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
#endif
		}

		public int VariantConnect
		{
#if WINDOWS_PHONE_APP || WINDOWS_UAP
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
#else
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
#endif
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
				return ApplicationSettingsHelper.SimleGet(true);
			}

			set
			{
				ApplicationSettingsHelper.SimpleSet(value);
				OnPropertyChanged();
			}
		}

		public bool UpdateOnMobileData
		{
			get
			{
				return ApplicationSettingsHelper.SimleGet(true);
			}

			set
			{
				ApplicationSettingsHelper.SimpleSet(value);
				OnPropertyChanged();
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			var handler = PropertyChanged;
			if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}