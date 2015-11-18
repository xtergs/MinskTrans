
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using MinskTrans.Utilites.Base.Net;

//using MinskTrans.DesctopClient.Annotations;
//using MinskTrans.DesctopClient.Annotations;
using MyLibrary;

namespace MinskTrans.DesctopClient.Modelview
{
#if !WINDOWS_PHONE_APP && !WINDOWS_AP && !WINDOWS_UAP
using MinskTrans.DesctopClient.Annotations;
using System.Configuration;
using MinskTrans.DesctopClient.Properties;

#else

using MinskTrans.Universal.Annotations;
	using Windows.Storage;

#endif



    public interface IApplicationSettingsHelper
    {
        // DateTime DateTimeSettings { get; set; }
        void SimpleSet<T>(T value, [CallerMemberName] string key = null);
        T SimpleGet<T>(T defValue = default(T), [CallerMemberName] string key = null);
	    Error SimpleGet(Error defValue = default(Error), [CallerMemberName] string key = null);
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


    public class UniversalApplicationSettingsHelper : IApplicationSettingsHelper
    {
        readonly Dictionary<string, DateTime> dateTimeDictionary = new Dictionary<string, DateTime>(); 
        
        public DateTime SimbleGet(DateTime value = default(DateTime), [CallerMemberName] string key = null)
        {
            if (dateTimeDictionary.ContainsKey(key) )
                if (dateTimeDictionary[key] != default(DateTime))
                    return dateTimeDictionary[key];
                else
                {
                    if (ApplicationData.Current.LocalSettings.Values.ContainsKey(key))
                    {
                        var backField =
                            DateTime.Parse(ApplicationData.Current.LocalSettings.Values[key].ToString());
                        dateTimeDictionary.Add(key, backField);
                        return backField;
                    }

                }
            return value;
        }

        public void SimpleSet<T>(T value, [CallerMemberName] string key = null)
        {
            if (!ApplicationData.Current.LocalSettings.Values.ContainsKey(key))
                ApplicationData.Current.LocalSettings.Values.Add(key, value);
            else
                ApplicationData.Current.LocalSettings.Values[key] = value;
        }

        public T SimpleGet<T>(T defValue = default(T), [CallerMemberName] string key = null)
        {
            if (!ApplicationData.Current.LocalSettings.Values.ContainsKey(key))
                ApplicationData.Current.LocalSettings.Values.Add(key, defValue);
            return (T)ApplicationData.Current.LocalSettings.Values[key];
        }

		public Error SimpleGet(Error defValue = default(Error), [CallerMemberName] string key = null)
		{
			if (!ApplicationData.Current.LocalSettings.Values.ContainsKey(key))
				ApplicationData.Current.LocalSettings.Values.Add(key, (int)defValue);
			return (Error)ApplicationData.Current.LocalSettings.Values[key];
		}

		public void SimpleSet(DateTime value, [CallerMemberName] string key = null)
        {
            if (dateTimeDictionary.ContainsKey(key) && dateTimeDictionary[key] == value)
                return;
            if (!ApplicationData.Current.LocalSettings.Values.ContainsKey(key))
            {
                ApplicationData.Current.LocalSettings.Values.Add(key, value.ToString());
                dateTimeDictionary.Add(key, value);
            }
            else
            {
                ApplicationData.Current.LocalSettings.Values[key] = value.ToString();
                dateTimeDictionary[key] = value;
            }
        }

       //public void SimpleSet(string value, [CallerMemberName]string key = null)
       // {
       //     if (!ApplicationData.Current.LocalSettings.Values.ContainsKey(key))
       //         ApplicationData.Current.LocalSettings.Values.Add(key, value);
       //     else
       //         ApplicationData.Current.LocalSettings.Values[key] = value;
       // }

       // public void SimpleSet(bool value, [CallerMemberName]string key = null)
       // {
       //     if (!ApplicationData.Current.LocalSettings.Values.ContainsKey(key))
       //         ApplicationData.Current.LocalSettings.Values.Add(key, value);
       //     else
       //         ApplicationData.Current.LocalSettings.Values[key] = value;
       // }

       // public void SimpleSet(int value, [CallerMemberName]string key = null)
       // {
       //     if (!ApplicationData.Current.LocalSettings.Values.ContainsKey(key))
       //         ApplicationData.Current.LocalSettings.Values.Add(key, value);
       //     else
       //         ApplicationData.Current.LocalSettings.Values[key] = value;
       // }

       // public string SimleGet(string defValue = null, [CallerMemberName]string key = null)
       // {

       //     return (string)SimleGet((object)defValue, key);
       // }

       // public object SimleGet(object defValue = null, [CallerMemberName]string key = null)
       // {
       //     if (!ApplicationData.Current.LocalSettings.Values.ContainsKey(key))
       //         ApplicationData.Current.LocalSettings.Values.Add(key, defValue);
       //     return ApplicationData.Current.LocalSettings.Values[key];
       // }

       // public bool SimleGet(bool defValue = true, [CallerMemberName]string key = null)
       // {

       //     return (bool)SimleGet((object)defValue, key);
       // }

       // public int SimleGet(int defValue = 0, [CallerMemberName]string key = null)
       // {
       //     return (int)SimleGet((object)defValue, key);
       // }

    }

    public class SettingsModelView : ISettingsModelView
    {
        private readonly IApplicationSettingsHelper helper;

		

		public SettingsModelView(IApplicationSettingsHelper helper)
			: base()
		{
            if (helper == null)
                throw new ArgumentNullException("helper");
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
		public bool HaveConnection()
		{
			
			return InternetHelperBase.Is_Connected && (InternetHelperBase.Is_InternetAvailable ||
												   InternetHelperBase.Is_Wifi_Connected == UpdateOnWiFi);
		}


		public TypeOfUpdate LastUpdatedDataInBackground
		{
			get
			{
			    return (TypeOfUpdate)helper.SimpleGet(TypeOfUpdate.None);
				if (!ApplicationData.Current.LocalSettings.Values.ContainsKey(SettingsToStr()))
					LastUpdatedDataInBackground = TypeOfUpdate.None;
				return (TypeOfUpdate)Enum.Parse(typeof(TypeOfUpdate),ApplicationData.Current.LocalSettings.Values[SettingsToStr()].ToString());
			}

			set
			{
			    helper.SimpleSet((int) value);
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

        public List<int> PreDefMins { get; } = new List<int>() {5,10,15,20,30,60};

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
				if (!ApplicationData.Current.LocalSettings.Values.ContainsKey(SettingsToStr()))
					InvervalAutoUpdateTimeSpan = new TimeSpan(0, 1, 0, 0, 0);
				return (TimeSpan)ApplicationData.Current.LocalSettings.Values[SettingsToStr()];
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

		public Error TypeError
		{

			get
			{
			    return helper.SimpleGet(Error.None);
				if (!ApplicationData.Current.LocalSettings.Values.ContainsKey(SettingsToStr()))
					TypeError = Error.None;
				return (Error)ApplicationData.Current.LocalSettings.Values[SettingsToStr()];
			}

			set
			{
			    helper.SimpleSet(value);
				//if (!ApplicationData.Current.LocalSettings.Values.ContainsKey(SettingsToStr()))
				//	ApplicationData.Current.LocalSettings.Values.Add(SettingsToStr(), (int)value);
				//else
				//	ApplicationData.Current.LocalSettings.Values[SettingsToStr()] = (int)value;
				OnPropertyChanged();
			}
		}

		public int VariantConnect
		{
			get
			{
			    return helper.SimpleGet(2);
				if (!ApplicationData.Current.LocalSettings.Values.ContainsKey(SettingsToStr()))
					VariantConnect = 2;
				return (int)ApplicationData.Current.LocalSettings.Values[SettingsToStr()];
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

        public bool CurrentDate {
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
                return helper.SimpleGet(true);
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

        public DateTime LastUpdateHotNewsDateTimeUtc {
            get { return helper.SimbleGet(); }
            set
            {
                helper.SimpleSet(value);
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