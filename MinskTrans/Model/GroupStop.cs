using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using MinskTrans.DesctopClient.Annotations;


namespace MinskTrans.DesctopClient.Model
{
	[Serializable]
	public class GroupStop :BaseModel, INotifyPropertyChanged
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
				OnPropertyChanged();
			}
		}

		public string Name
		{
			get { return name; }
			set
			{
				name = value;
				OnPropertyChanged();
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			var handler = PropertyChanged;
			if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}