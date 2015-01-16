using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinskTrans
{
	public class StopMovelView  :ShedulerModelView
	{
		private List<Stop> filteredStops;
		private string stopNameFilter;
		private IEnumerable<Time> timeSchedule;
		private Stop filteredSelectedStop;

		public StopMovelView()
			: base()
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
				return Stops.Where(x=>Routs.Any(y=>y.Stops.Contains(x)) && x.Name.ToLower().Contains(StopNameFilter.ToLower()));
				return Stops;
			}
			
		}

		public IEnumerable<Schedule> TimeSchedule
		{
			get
			{
				var ss =
					Routs[0].Time.TimesDictionary[0][0].Times.OrderBy(x=>x).Select(x=> new {Routs[0], x})
						.OrderBy(x => x.TimesDictionary[0].OrderBy(y => y.Times));
				ss.ToString();
				return Times.Where(x=>x.Rout.Stops.Contains(FilteredSelectedStop));
			}
		}
	}
}
