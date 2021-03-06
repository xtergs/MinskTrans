﻿

namespace MinskTrans.DesctopClient.Modelview
{
using System.Collections.Generic;
using System.Linq;
#if WINDOWS_PHONE_APP
using GalaSoft.MvvmLight.Command;
#else
using GalaSoft.MvvmLight.CommandWpf;
#endif
using MinskTrans.DesctopClient.Model;
	public class GroupEditModelView : StopModelViewBase
	{
		private GroupStop stop;
		private Stop selectedStopGroup;

		public GroupEditModelView(Context newContext) : base(newContext)
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
				return Context.Routs.Where(x => x.Stops.Contains(FilteredSelectedStop)).Distinct();
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
