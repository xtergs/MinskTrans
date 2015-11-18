

using MinskTrans.Context;
using MinskTrans.Context.Base;
using MinskTrans.Context.Base.BaseModel;
using MyLibrary;

namespace MinskTrans.DesctopClient.Modelview
{
using System.Collections.Generic;
using System.Linq;
#if WINDOWS_PHONE_APP || WINDOWS_UAP
using GalaSoft.MvvmLight.Command;
#else
	using GalaSoft.MvvmLight.CommandWpf;
#endif
using MinskTrans.DesctopClient.Model;
	public class GroupEditModelView : StopModelViewBase
	{
		private GroupStop stop;
		private Stop selectedStopGroup;

		public GroupEditModelView(IBussnessLogics newContext, ISettingsModelView settings) : base(newContext, settings)
		{
		}

		public GroupStop Stop
		{
			get
			{
				if (stop == null)
					stop = new GroupStop();
				return stop;
			}
			set
			{
				if (Equals(value, stop)) return;
				stop = value;
				OnPropertyChanged();
				OnPropertyChanged("GroupName");
			}
		}

		public string GroupName
		{
			get
			{
				if (Stop == null)
				{
					Stop = new GroupStop();
				}
				return Stop.Name;
			}
			set
			{
				if (Stop == null)
					return;
				Stop.Name = value;
				OnPropertyChanged();
			}
		}

		public Stop SelectedStopGroup
		{
			get { return selectedStopGroup; }
			set
			{
				selectedStopGroup = value; 
				OnPropertyChanged();
				OnPropertyChanged("DirectionsStop");
			}
		}

		public IEnumerable<Rout> DirectionsStop
		{
			get
			{
				return Context.GetDirectionsStop(FilteredSelectedStop);
			}
		} 

		#region Commands

		public RelayCommand AddStop
		{
			get
			{
				return new RelayCommand(() =>
				{
					Stop.Stops.Add(FilteredSelectedStop);
					OnPropertyChanged("Stop");

				} , ()=> Stop != null && !Stop.Stops.Contains(FilteredSelectedStop));
			}
		}

		public RelayCommand RemoveStop
		{
			get
			{
				return new RelayCommand(() =>
				{
					Stop.Stops.Remove(SelectedStopGroup);
				}, ()=> SelectedStopGroup != null && Stop.Stops.Contains(SelectedStopGroup));
			}
		}

		#endregion
	}
}
