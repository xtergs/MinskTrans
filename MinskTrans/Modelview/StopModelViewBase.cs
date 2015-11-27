using System;
using System.Collections.Generic;
using GalaSoft.MvvmLight.Command;
using MinskTrans.Context;
using MinskTrans.Context.Base.BaseModel;
using MyLibrary;


namespace MinskTrans.DesctopClient.Modelview
{
	public class StopModelViewBase : BaseModelView
	{
		private Stop filteredSelectedStop;
		private string stopNameFilter;
		
		private ISettingsModelView settings;

		public ISettingsModelView Settings
		{
			get { return settings; }
		}

		private Location lastLocation;

		public StopModelViewBase(IBussnessLogics newContext, ISettingsModelView newSettings) : base(newContext)
		{
			settings = newSettings;
			newContext.Context.PropertyChanged+= (sender, args) =>
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
			try {
				OnPropertyChanged("StopNameFilter");
				OnPropertyChanged("FilteredStops");
			}
			catch
			{ }
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
			get { return Context.Context.IsFavouriteStop(FilteredSelectedStop); }
			set { OnPropertyChanged();}
		}

        public bool ShowDetailViewStop
        {
            get { return showDetailViewStop; }
            set
            {
                showDetailViewStop = value;
                OnPropertyChanged();
            }
        }

        public virtual Stop FilteredSelectedStop
		{
			get { return filteredSelectedStop; }
			set
			{
			    if (value == null)
			    {
			        ShowDetailViewStop = true;
			        return;
			    }
			    else
			        ShowDetailViewStop = true;
                //if (Equals(value, filteredSelectedStop)) return;
                //ShowStopMap.RaiseCanExecuteChanged();
                filteredSelectedStop = value;
				Context.Context.IncrementCounter(filteredSelectedStop);
				OnPropertyChanged();
				OnPropertyChanged("TimeSchedule");
				OnPropertyChanged("IsStopFavourite");

				OnPropertyChanged("DirectionsStop");
			}
		}


		public virtual IEnumerable<Stop> FilteredStops
		{
		    get { throw new NotImplementedException(); }
		    set { throw new NotImplementedException(); }
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
	    private bool showDetailViewStop;

	    //public new RelayCommand ShowStopMap
		//{
		//	get
		//	{
		//		if (showStopMap == null)
		//			showStopMap = new RelayCommand(() => OnShowStop(new ShowArgs() { SelectedStop = FilteredSelectedStop }),
		//				() => FilteredSelectedStop != null);
		//		return showStopMap;
		//	}
		//}


		public RelayCommand AddStopToGroup
		{
			get { return new RelayCommand(() =>
			{
			    SelectedGroup.Stops.Add(FilteredSelectedStop);
			} );}
		}

		//public event Show ShowStop;
		//public event Show ShowRoute;
		//public delegate void Show(object sender, ShowArgs args);

		//protected virtual void OnShowStop(ShowArgs args)
		//{
		//	var handler = ShowStop;
		//	if (handler != null) handler(this, args);
		//}

		//protected virtual void OnShowRoute(ShowArgs args)
		//{
		//	var handler = ShowRoute;
		//	if (handler != null) handler(this, args);
		//}
	}
}