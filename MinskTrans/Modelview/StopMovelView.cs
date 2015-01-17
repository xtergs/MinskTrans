using System;
using System.Collections.Generic;
using System.Linq;
using MinskTrans.Model;

namespace MinskTrans.Modelview
{
	public class StopMovelView : BaseModelView
	{
		private bool autoDay;
		private bool autoNowTime;
		private int curDay;
		private int curTime;
		private Stop filteredSelectedStop;
		private List<Stop> filteredStops;
		private int nowTimeHour;
		private int nowTimeMin;
		private string stopNameFilter;
		private IEnumerable<Time> timeSchedule;

		public StopMovelView()
		{
		}

		public StopMovelView(Context newContext)
			: base(newContext)
		{
		}

		public string StopNameFilter
		{
			get { return stopNameFilter; }
			set
			{
				if (value == stopNameFilter) return;
				stopNameFilter = value;
				OnPropertyChanged();
				OnPropertyChanged("FilteredStops");
			}
		}

		public Stop FilteredSelectedStop
		{
			get { return filteredSelectedStop; }
			set
			{
				if (Equals(value, filteredSelectedStop)) return;
				filteredSelectedStop = value;
				OnPropertyChanged();
				OnPropertyChanged("TimeSchedule");
			}
		}

		public IEnumerable<Stop> FilteredStops
		{
			get
			{
				if (StopNameFilter != null)
					return
						Context.Stops.Where(
							x => Context.Routs.Any(y => y.Stops.Contains(x)) && x.SearchName.Contains(StopNameFilter.ToLower()));
				return Context.Stops;
			}
		}

		public bool AutoDay
		{
			get { return autoDay; }
			set
			{
				if (value.Equals(autoDay)) return;
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

		public IEnumerable<KeyValuePair<Rout, int>> TimeSchedule
		{
			get
			{
				IEnumerable<Schedule> dd = Context.Times.Where(x => x.Rout.Stops.Contains(FilteredSelectedStop));
				IEnumerable<KeyValuePair<Rout, int>> ss = new List<KeyValuePair<Rout, int>>();
				foreach (Schedule sched in dd)
				{
					List2<Rout, int> temp = sched.GetListTimes(sched.Rout.Stops.IndexOf(FilteredSelectedStop), CurDay, CurTime);
					ss = ss.Concat(temp);
				}
				ss = ss.OrderBy(x => x.Value);
				//ss.ToString();
				return ss;
			}
		}
	}
}