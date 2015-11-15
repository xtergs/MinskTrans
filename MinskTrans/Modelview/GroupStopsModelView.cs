

using MinskTrans.Context;
using MinskTrans.Context.Base;
using MinskTrans.Context.Base.BaseModel;
using MyLibrary;

namespace MinskTrans.DesctopClient.Modelview
{
using System.Collections.Generic;

using System.Linq;
#if !WINDOWS_PHONE_APP && WINDOWS_AP
using GalaSoft.MvvmLight.CommandWpf;
#else
using GalaSoft.MvvmLight.Command;
#endif
using MinskTrans.DesctopClient.Model;
	public class GroupStopsModelView : StopModelView
	{
		
		private GroupStop selectedGroupStop;
		private IList<GroupStop> selectedGroups;

		//public GroupStopsModelView() : this(null)
		//{
		//}

		public GroupStopsModelView(IBussnessLogics newContext, ISettingsModelView settingsModelView)
			: base(newContext, settingsModelView)
		{
			Bus = Trol = Tram=Metro = true;
		}

		//public ObservableCollection<GroupStop> Groups
		//{
		//	get { return groups; }
		//	set
		//	{
		//		if (Equals(value, groups)) return;
		//		groups = value;
		//		OnPropertyChanged();
		//	}
		//}

		public GroupStop SelectedGroupStop
		{
			get { return selectedGroupStop; }
			set
			{
				if (Equals(value, selectedGroupStop)) return;
				selectedGroupStop = value;
				OnPropertyChanged();
				OnPropertyChanged("TimeSchedule");
			}
		}

		public IList<GroupStop> SelectedGroups
		{
			get { return selectedGroups; }
			set
			{		
				selectedGroups = value;
				OnPropertyChanged();
			}
		}

		override public IEnumerable<TimeLineModel> TimeSchedule
		{
			get
			{
				if (SelectedGroupStop == null)
					return null;
				if (Context.Context.Groups == null || Context.Context.Groups.Count <= 0)
					return null;
				IEnumerable<TimeLineModel> temp = new List<TimeLineModel>();
				foreach (Stop stop in SelectedGroupStop.Stops)
				{
					FilteredSelectedStop = stop;
					temp = temp.Concat(Context.GetStopTimeLine(stop, CurDay, CurTime - SettingsModelView.TimeInPast));
				}
				return temp.OrderBy(x => x.Time);
				
			}
		}

		public RelayCommand<GroupStop> AddGroup
		{
			get
			{
				return new RelayCommand<GroupStop>(x =>
			{
				Context.Context.Groups.Add(x);
				//OnPropertyChanged("Groups");
			}, p => p != null && !Context.Context.Groups.Contains(p));
			}
		}

		public RelayCommand<GroupStop> RemoveGorup
		{
			get
			{
				return new RelayCommand<GroupStop>(x =>
				{
					Context.Context.Groups.Remove(x);
					//OnPropertyChanged("Groups");
				}, p=> p!= null);
			}
		}
	}
}