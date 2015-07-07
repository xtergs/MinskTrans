using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;


namespace MinskTrans.DesctopClient.Model
{

	public class GroupStop :BaseModel
	{
		private string name;
		private IList<Stop> stops;

		public GroupStop()
		{
			Stops = new List<Stop>();
		}

		public GroupStop(GroupStop group)
		{
			Name = group.Name;
			Stops = new List<Stop>();
			foreach (var stop in group.Stops)
			{
				Stops.Add(stop);
			}
		}

		public List<Stop> Stops
		{
			get
			{
				if (stops == null)
					stops = new List<Stop>();
				return stops.ToList();
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