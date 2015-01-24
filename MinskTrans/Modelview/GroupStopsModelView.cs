using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using MinskTrans.DesctopClient.Model;
using MinskTrans.Library;

namespace MinskTrans.DesctopClient.Modelview
{
	public class GroupStopsModelView : StopModelView
	{
		private ObservableCollection<GroupStop> groups;
		private GroupStop selectedGroupStop;

		public GroupStopsModelView() : this(null)
		{
		}

		public GroupStopsModelView(Context newContext) : base(newContext)
		{
			Groups = new ObservableCollection<GroupStop>();
			Bus = Trol = Tram = true;
			//Groups.Add(new GroupStop
			//{
			//	Context.Stops.First(x => x.SearchName == "шепичи"),
			//	Context.Stops.First(x => x.SearchName == "плеханова")
			//});
			//Groups[0].Name = "Шепичи";
		}

		public ObservableCollection<GroupStop> Groups
		{
			get { return groups; }
			set
			{
				if (Equals(value, groups)) return;
				groups = value;
				OnPropertyChanged();
			}
		}

		public GroupStop SelectedGroupStop
		{
			get { return selectedGroupStop; }
			set
			{
				if (Equals(value, selectedGroupStop)) return;
				selectedGroupStop = value;
				OnPropertyChanged();
				OnPropertyChanged("GroupTimeSchedule");
			}
		}

		public IEnumerable<KeyValuePair<Rout, int>> GroupTimeSchedule
		{
			get
			{
				if (SelectedGroupStop == null)
					return null;
				if (Groups == null || Groups.Count <= 0)
					return null;
				IEnumerable<KeyValuePair<Rout, int>> temp = new List<KeyValuePair<Rout, int>>();
				foreach (Stop stop in SelectedGroupStop.Stops)
				{
					FilteredSelectedStop = stop;
					temp = temp.Concat(TimeSchedule);
				}
				return temp.OrderBy(x => x.Value);
			}
		}

		public ActionCommand AddGroup
		{
			get { return new ActionCommand(x =>
			{
				Groups.Add((GroupStop) x);
				OnPropertyChanged("Groups");
			}, p=> p!=null && !Groups.Contains((GroupStop)p));}
		}

		public ActionCommand RemoveGorup
		{
			get
			{
				return new ActionCommand(x =>
				{
					Groups.Remove((GroupStop) x);
					OnPropertyChanged("Groups");
				}, p=> p!= null);
			}
		}
	}
}