using System;
using System.Collections.Generic;
using System.Linq;
using MinskTrans.Library;

namespace MinskTrans.DesctopClient.Modelview
{
	public class StopModelView : StopModelViewBase
	{
		private bool autoDay;
		private bool autoNowTime;
		private bool bus;
		private int curDay;
		private int nowTimeHour;
		private int nowTimeMin;
		private bool tram;
		private bool trol;

		public StopModelView()
			: this(null)
		{
		}

		public StopModelView(Context newContext)
			: base(newContext)
		{
			Bus = Trol = Tram = AutoDay = AutoNowTime = true;
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

		public IEnumerable<KeyValuePair<Rout, int>> TimeSchedule
		{
			get
			{
				IEnumerable<Schedule> dd = Context.Times.Where(x => (x.Rout.Stops.Contains(FilteredSelectedStop)));
				IEnumerable<KeyValuePair<Rout, int>> ss = new List<KeyValuePair<Rout, int>>();
				foreach (Schedule sched in dd)
				{
					IEnumerable<KeyValuePair<Rout, int>> temp =
						sched.GetListTimes(sched.Rout.Stops.IndexOf(FilteredSelectedStop), CurDay, CurTime).Where(x =>
						{
							if (x.Key.Transport == "bus")
								return Bus;
							if (x.Key.Transport == "trol")
								return Trol;
							if (x.Key.Transport == "tram")
								return Tram;
							return false;
						});

					ss = ss.Concat(temp);
				}
				ss = ss.OrderBy(x => x.Value);
				//ss.ToString();
				return ss;
			}
		}

		public ActionCommand RefreshTimeSchedule
		{
			get { return new ActionCommand(x => OnPropertyChanged("TimeSchedule")); }
		}

		public event Show ShowStop;
		public event Show ShowRoute;
		public delegate void Show(object sender, ShowArgs args);

		public ActionCommand ShowStopMap
		{
			get { return new ActionCommand(x => OnShowStop(new ShowArgs(){SelectedStop = FilteredSelectedStop}), p => FilteredSelectedStop != null); }
		}

		//public ActionCommand ShowRouteMap
		//{
		//	get { return new ActionCommand(x => OnShowRoute(new ShowArgs()), p => RouteSelectedValue != null); }
		//}

		protected virtual void OnShowStop(ShowArgs args)
		{
			var handler = ShowStop;
			if (handler != null) handler(this, args);
		}

		protected virtual void OnShowRoute(ShowArgs args)
		{
			var handler = ShowRoute;
			if (handler != null) handler(this, args);
		}
	}
}