using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using MinskTrans.Utilites.Base.Net;

namespace MyLibrary
{
    public class SettingsModelView : ISettingsModelView
    {
        protected readonly IApplicationSettingsHelper helper;
        private InternetHelperBase internetHelper;


        public SettingsModelView(IApplicationSettingsHelper helper, InternetHelperBase internetHelper)
            : base()
        {
            if (internetHelper == null)
                throw new ArgumentNullException(nameof(internetHelper));
            if (helper == null)
                throw new ArgumentNullException(nameof(helper));
            this.helper = helper;
            this.internetHelper = internetHelper;
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

        public void UpdateNetworkData()
        {
            internetHelper.UpdateNetworkInformation();
        }

        public int FontSize {
            get { return helper.SimpleGet(15); }

            set
            {
                helper.SimpleSet(value);
                OnPropertyChanged();
            }
        }

        public virtual string ChangeLogOnce => "";
        public virtual string ChangeLog => "";


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

        public TimeSpan ReconnectPushServerTimeSpan => new TimeSpan(0, 0, 1, 0);

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

        public string PrivatyPolicity => @"ѕриложение ћинский общественный транспорт не хранит пользовательских данных. ѕриложение использует интернет подключение только дл€ получени€ последних обновлений расписани€ транспорта.";

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

        public int GPSThreshholdMeters => 5;

        public uint GPSInterval => 1000;

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
#if DEBUG

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
                OnPropertyChanged(nameof(LastUpdateDbDateTime));
            }
        }

        public DateTime LastUpdateDbDateTime => LastUpdateDbDateTimeUtc.ToLocalTime();

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
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}