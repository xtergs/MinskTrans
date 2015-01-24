using System.Collections.Generic;
using System.Linq;
using MinskTrans.Library;

namespace MinskTrans.DesctopClient.Modelview
{
	public class StopModelViewBase : BaseModelView
	{
		private Stop filteredSelectedStop;
		private string stopNameFilter;

		public StopModelViewBase(Context newContext) : base(newContext)
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
				OnPropertyChanged("DirectionsStop");
			}
		}

		public IEnumerable<Stop> FilteredStops
		{
			get
			{
				if (StopNameFilter != null)
				{
					var temp = Context.ActualStops.AsParallel().Where(
							x => x.SearchName.Contains(StopNameFilter.ToLower()));
					return temp;
				}
				return Context.Stops;
			}
		}
	}
}