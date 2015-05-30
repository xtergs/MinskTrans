using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
#if WINDOWS_PHONE_APP
using Windows.Devices.Geolocation;
using Windows.UI.Core
#endif
using GalaSoft.MvvmLight.Command;
using MapControl;
using MinskTrans.DesctopClient.Model;


namespace MinskTrans.DesctopClient.Modelview
{
	public class StopModelViewBase : BaseModelView
	{
		private Stop filteredSelectedStop;
		private string stopNameFilter;
		
		private SettingsModelView settings;

		public SettingsModelView Settings
		{
			get { return settings; }
		}

		private Location lastLocation;

		public StopModelViewBase(Context newContext, SettingsModelView newSettings) : base(newContext)
		{
			settings = newSettings;
			newContext.PropertyChanged+= (sender, args) =>
			{
				if (args.PropertyName == "ActualStops")
				Refresh();
			};
			settings.PropertyChanged += (sender, args) =>
			{
				//if (args.PropertyName == "UseGPS")
				//	SetGPS();
			};
			//SetGPS();
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
				Context.IncrementCounter(filteredSelectedStop);
				OnPropertyChanged();
				OnPropertyChanged("TimeSchedule");
				OnPropertyChanged("IsStopFavourite");

				OnPropertyChanged("DirectionsStop");
			}
		}


		

		public virtual IEnumerable<Stop> FilteredStops
		{
			get
			{
				if (StopNameFilter != null && Context.ActualStops != null)
				{
					var tempSt = StopNameFilter.ToLower();
					var temp = Context.ActualStops.AsParallel().Where(
							x => x.SearchName.Contains(tempSt));
					if (lastLocation != null)
						return temp.OrderByDescending(x=>Context.GetCounter(x)).ThenBy(Distance);
					else
						return temp.OrderByDescending(x => Context.GetCounter(x)).ThenBy(x => x.SearchName);
				}
				if (Context.ActualStops != null)
					return lastLocation == null ? Context.ActualStops.OrderByDescending(x => Context.GetCounter(x)).ThenBy(x => x.SearchName) :
												  Context.ActualStops.OrderByDescending(x => Context.GetCounter(x)).ThenBy(Distance);
				return null;
			}
		}

		private double Distance(Stop x)
		{
			return Math.Abs(Math.Sqrt(Math.Pow(lastLocation.Longitude - x.Lng, 2) + Math.Pow(lastLocation.Latitude - x.Lat, 2)));
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