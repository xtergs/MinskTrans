using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;
using MinskTrans.DesctopClient.Model;
using MinskTrans.Library;

namespace MinskTrans.DesctopClient.Modelview
{
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

		public ActionCommand AddStop
		{
			get
			{
				return new ActionCommand((x) =>
				{
					Stop.Stops.Add(FilteredSelectedStop);
					OnPropertyChanged("Stop");

				} , (p)=> Stop != null && !Stop.Stops.Contains(FilteredSelectedStop));
			}
		}

		public ActionCommand RemoveStop
		{
			get
			{
				return new ActionCommand(x =>
				{
					Stop.Stops.Remove(SelectedStopGroup);
				}, p=> SelectedStopGroup != null && Stop.Stops.Contains(SelectedStopGroup));
			}
		}

		#endregion
	}
}
