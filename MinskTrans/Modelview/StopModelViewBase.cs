using System.Collections.Generic;
using System.Linq;

using GalaSoft.MvvmLight.Command;
using MinskTrans.DesctopClient.Model;


namespace MinskTrans.DesctopClient.Modelview
{
	public class StopModelViewBase : BaseModelView
	{
		private Stop filteredSelectedStop;
		private string stopNameFilter;

		public StopModelViewBase(Context newContext) : base(newContext)
		{
			newContext.PropertyChanged+= (sender, args) =>
			{
				if (args.PropertyName == "ActualStops")
				Refresh();
			};
		}

		#region Overrides of BaseModelView

		public override void Refresh()
		{
			OnPropertyChanged("StopNameFilter");
			OnPropertyChanged("FilteredStops");
		}

		#endregion

		public string StopNameFilter
		{
			get { return stopNameFilter; }
			set
			{
				//if (value == stopNameFilter) return;
				stopNameFilter = value;
				OnPropertyChanged();
				OnPropertyChanged("FilteredStops");
			}
		}

		public bool IsStopFavourite
		{
			get { return Context.IsFavouriteStop(FilteredSelectedStop); }
			set { OnPropertyChanged();}
		}

		public virtual Stop FilteredSelectedStop
		{
			get { return filteredSelectedStop; }
			set
			{
				if (value == null)
					return;
				//if (Equals(value, filteredSelectedStop)) return;
				ShowStopMap.RaiseCanExecuteChanged();
				filteredSelectedStop = value;
				OnPropertyChanged();
				OnPropertyChanged("TimeSchedule");
				OnPropertyChanged("IsStopFavourite");

				OnPropertyChanged("DirectionsStop");
			}
		}


		public IEnumerable<Stop> FilteredStops
		{
			get
			{
				if (StopNameFilter != null && Context.ActualStops != null)
				{
					var tempSt = StopNameFilter.ToLower();
					var temp = Context.ActualStops.AsParallel().Where(
							x => x.SearchName.Contains(tempSt));
					return temp.OrderBy(x=>x.SearchName);
				}
				if (Context.ActualStops != null) 
					return Context.ActualStops.OrderBy(x => x.SearchName);
				return null;
			}
		}

		public GroupStop SelectedGroup
		{
			get { return selectedGroup; }
			set
			{
				selectedGroup = value;
				OnPropertyChanged();
			}
		}

		private RelayCommand showStopMap;
		private GroupStop selectedGroup;

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


		public RelayCommand AddStopToGroup
		{
			get { return new RelayCommand(()=>SelectedGroup.Stops.Add(FilteredSelectedStop) );}
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