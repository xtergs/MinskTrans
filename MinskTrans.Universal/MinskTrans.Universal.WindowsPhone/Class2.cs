using System.ComponentModel;
using MapControl;

namespace MinskTrans.Universal
{
	public class Base : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		protected void RaisePropertyChanged(string propertyName)
		{
			var propertyChanged = PropertyChanged;
			if (propertyChanged != null)
			{
				propertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}
	}

	public class Point : Base
	{
		private string name;
		public string Name
		{
			get { return name; }
			set
			{
				name = value;
				RaisePropertyChanged("Name");
			}
		}

		private Location location;
		public Location Location
		{
			get { return location; }
			set
			{
				location = value;
				RaisePropertyChanged("Location");
			}
		}
	}
}
