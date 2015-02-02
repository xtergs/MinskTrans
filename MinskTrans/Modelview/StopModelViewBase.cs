using System.Collections.Generic;
using System.Linq;
using GalaSoft.MvvmLight.Command;


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
				ShowStopMap.RaiseCanExecuteChanged();
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
					var tempSt = StopNameFilter.ToLower();
					var temp = Context.ActualStops.AsParallel().Where(
							x => x.SearchName.Contains(tempSt));
					return temp.OrderBy(x=>x.SearchName);
				}
				return Context.ActualStops.OrderBy(x => x.SearchName);
			}
		}

		private RelayCommand showStopMap;
		public RelayCommand ShowStopMap
		{
			get
			{
				if (showStopMap == null)
					showStopMap = new RelayCommand(() => OnShowStop(new ShowArgs() { SelectedStop = FilteredSelectedStop }),
						() => FilteredSelectedStop != null);
				return showStopMap;
			}
		}

		public event Show ShowStop;
		public event Show ShowRoute;
		public delegate void Show(object sender, ShowArgs args);

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