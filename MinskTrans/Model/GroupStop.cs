using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;



namespace MinskTrans.DesctopClient.Model
{
#if !WINDOWS_PHONE_APP && !WINDOWS_APP
	[Serializable]
#endif
	public class GroupStop :BaseModel
	{
		private string name;
		private ObservableCollection<Stop> stops;

		public GroupStop()
		{
			Stops = new ObservableCollection<Stop>();
		}

		public GroupStop(GroupStop group)
		{
			Name = group.Name;
			Stops = new ObservableCollection<Stop>();
			foreach (var stop in group.Stops)
			{
				Stops.Add(stop);
			}
		}

		public ObservableCollection<Stop> Stops
		{
			get
			{
				if (stops == null)
					stops = new ObservableCollection<Stop>();
				return stops;
			}
			set
			{
				stops = value;
				//OnPropertyChanged();
			}
		}

		public string Name
		{
			get { return name; }
			set
			{
				name = value;
				//OnPropertyChanged();
			}
		}

		
	}
}