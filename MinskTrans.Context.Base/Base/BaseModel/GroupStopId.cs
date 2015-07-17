using System.Collections.Generic;

namespace MinskTrans.Context.Base.BaseModel
{
	public class GroupStopId
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
