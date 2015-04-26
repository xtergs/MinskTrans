using System;
using System.Collections.Generic;
using System.Linq;

#if !WINDOWS_PHONE_APP && !WINDOWS_AP

using GalaSoft.MvvmLight.CommandWpf;
using MinskTrans.DesctopClient.Properties;

#else

using Windows.Storage;
using Windows.UI.Xaml;
using MinskTrans.Universal;
using GalaSoft.MvvmLight.Command;
#endif

namespace MinskTrans.DesctopClient.Modelview
{
	public class StopModelView : StopModelViewBase
	{
		private ISettingsModelView settingsModelView;
		private bool autoDay;
		private bool autoNowTime;
		private bool bus;
		private int curDay;
		private int nowTimeHour;
		private int nowTimeMin;
		private bool tram;
		private bool trol;
		private string destinationStop;

		//public StopModelView()
		//	: this(null)
		//{
		//}

		public StopModelView(Context newContext, ISettingsModelView settings)
			: base(newContext)
		{
			Bus = Trol = Tram = AutoDay = AutoNowTime = true;
			settingsModelView = settings;

			settingsModelView.PropertyChanged += (sender, args) =>
			{
				if (args.PropertyName == "TimeInPast")
					Refresh();
			};
		}

	public ISettingsModelView SettingsModelView
	{
		get { return settingsModelView; }
	}

		public string DestinationStop
		{
			get
			{
				if (destinationStop == null)
					destinationStop = "";
				return destinationStop;
			}
			set
			{
				destinationStop = value;
				OnPropertyChanged();
				OnPropertyChanged("TimeSchedule");
			}
		}


		public bool AutoDay
		{
			get { return autoDay; }
			set
			{
				//if (value.Equals(autoDay)) return;
				autoDay = value;
				OnPropertyChanged();
				OnPropertyChanged("TimeSchedule");
			}
		}

		public int CurDay
		{
			get
			{
				if (AutoDay)
					if (DateTime.Now.DayOfWeek == DayOfWeek.Sunday)
						return 7;
					else
						return (int) DateTime.Now.DayOfWeek;
				if (curDay <= 0)
					CurDay = 1;
				return curDay;
			}
			set
			{
				if (value == curDay) return;
				if (value <= 0 || value > 7)
					return;
				curDay = value;
				OnPropertyChanged();
				OnPropertyChanged("TimeSchedule");
			}
		}

		public int NowTimeHour
		{
			get { return nowTimeHour; }
			set
			{
				if (value == nowTimeHour) return;
				if (value >= 24 || value < 0)
					value = 0;
				nowTimeHour = value;
				OnPropertyChanged();
				OnPropertyChanged("TimeSchedule");
			}
		}

		public int NowTimeMin
		{
			get { return nowTimeMin; }
			set
			{
				if (value == nowTimeMin) return;

				while (value >= 60)
				{
					NowTimeHour++;
					value -= 60;
				}

				nowTimeMin = value;
				OnPropertyChanged();
				OnPropertyChanged("TimeSchedule");
			}
		}

		public bool AutoNowTime
		{
			get { return autoNowTime; }
			set
			{
				if (value.Equals(autoNowTime)) return;
				autoNowTime = value;
				OnPropertyChanged();
				OnPropertyChanged("TimeSchedule");
			}
		}

		public int CurTime
		{
			get
			{
				if (AutoNowTime)
					return DateTime.Now.Hour*60 + DateTime.Now.Minute;
				return NowTimeHour*60 + NowTimeMin;
			}
		}

		public bool Trol
		{
			get { return trol; }
			set
			{
				if (value.Equals(trol)) return;
				trol = value;
				OnPropertyChanged();
				OnPropertyChanged("TimeSchedule");
			}
		}

		public bool Bus
		{
			get { return bus; }
			set
			{
				if (value.Equals(bus)) return;
				bus = value;
				OnPropertyChanged();
				OnPropertyChanged("TimeSchedule");
			}
		}

		public bool Tram
		{
			get { return tram; }
			set
			{
				if (value.Equals(tram)) return;
				tram = value;
				OnPropertyChanged();
				OnPropertyChanged("TimeSchedule");
			}
		}

		virtual public IEnumerable<KeyValuePair<Rout, int>> TimeSchedule
		{
			get
			{
				if (Context.Times != null && FilteredSelectedStop != null)
				{
					IEnumerable<Schedule> dd = Context.Times.Where(time => time != null && time.Rout != null && (time.Rout.Stops.Contains(FilteredSelectedStop) &&
						time.Rout.Stops.Any(stop=> stop.SearchName.Contains(DestinationStop.Trim().ToLower()))));
					IEnumerable<KeyValuePair<Rout, int>> ss = new List<KeyValuePair<Rout, int>>();
					IEnumerable<KeyValuePair<Rout, int>> tt = new List<KeyValuePair<Rout, int>>();
					//foreach (var schedule in dd)
					//{
					//	var temp =
					//		schedule.GetListTimes(schedule.Rout.Stops.IndexOf(FilteredSelectedStop), CurDay - 1, 24 * 60 + CurTime - settingsModelView.TimeInPast).Where(x =>
					//		{
					//			if (x.Key.Transport == Rout.TransportType.Bus)
					//				return Bus;
					//			if (x.Key.Transport == Rout.TransportType.Trol)
					//				return Trol;
					//			if (x.Key.Transport == Rout.TransportType.Tram)
					//				return Tram;
					//			return true;
					//		});

					//	tt = tt.Concat(temp);
					//}

					//tt.OrderBy(x => x.Value);

					foreach (Schedule sched in dd)
					{
						IEnumerable<KeyValuePair<Rout, int>> temp =
							sched.GetListTimes(sched.Rout.Stops.IndexOf(FilteredSelectedStop), CurDay, CurTime - settingsModelView.TimeInPast).Where(x =>
							{
								if (x.Key.Transport == TransportType.Bus)
									return Bus;
								if (x.Key.Transport == TransportType.Trol)
									return Trol;
								if (x.Key.Transport == TransportType.Tram)
									return Tram;
								return true;
							});

						ss = ss.Concat(temp);

						
					}
					ss = ss.OrderBy(x => x.Value);
					//ss.ToString();
					return tt.Concat(ss);
				}
				return null;
			}
		}

		

		public RelayCommand RefreshTimeSchedule
		{
			get { return new RelayCommand(() => OnPropertyChanged("TimeSchedule")); }
		}

		public RelayCommand<int> SetTimeInPast
		{
			get { return new RelayCommand<int>(x=>settingsModelView.TimeInPast = x);}
		}

		



		

		

		//public ActionCommand ShowRouteMap
		//{
		//	get { return new ActionCommand(x => OnShowRoute(new ShowArgs()), p => RouteSelectedValue != null); }
		//}

		
	}
}