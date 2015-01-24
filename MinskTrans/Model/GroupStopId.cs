using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinskTrans.DesctopClient.Model
{
	[Serializable]
	class GroupStopId
	{
		public GroupStopId() { }

		public GroupStopId(GroupStop group)
		{
			Name = group.Name;
			StopID = new List<int>();
			foreach (var stop in group.Stops)
			{
				StopID.Add(stop.ID);
			}
		}
		public string Name { get; set; }
		public List<int> StopID { get; set; }
	}
}
