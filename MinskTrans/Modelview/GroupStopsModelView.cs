

using MinskTrans.Context.Base.BaseModel;

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

		public GroupStopsModelView(IContext newContext, SettingsModelView settingsModelView)
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

		override public IEnumerable<KeyValuePair<Rout, int>> TimeSchedule
		{
			get
			{
				if (SelectedGroupStop == null)
					return null;
				if (Context.Groups == null || Context.Groups.Count <= 0)
					return null;
				IEnumerable<KeyValuePair<Rout, int>> temp = new List<KeyValuePair<Rout, int>>();
				foreach (Stop stop in SelectedGroupStop.Stops)
				{
					FilteredSelectedStop = stop;
					temp = temp.Concat(base.TimeSchedule);
				}
				return temp.OrderBy(x => x.Value);
			}
		}

		public RelayCommand<GroupStop> AddGroup
		{
			get
			{
				return new RelayCommand<GroupStop>(x =>
			{
				Context.Groups.Add(x);
				//OnPropertyChanged("Groups");
			}, p => p != null && !Context.Groups.Contains(p));
			}
		}

		public RelayCommand<GroupStop> RemoveGorup
		{
			get
			{
				return new RelayCommand<GroupStop>(x =>
				{
					Context.Groups.Remove(x);
					//OnPropertyChanged("Groups");
				}, p=> p!= null);
			}
		}
	}
}