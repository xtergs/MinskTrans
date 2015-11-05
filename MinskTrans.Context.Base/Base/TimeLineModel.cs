using MinskTrans.Context.Base.BaseModel;
using System;

namespace MinskTrans.Context.Base
{
	public class TimeLineModel
	{
		public TimeLineModel(Rout newRout, TimeSpan time)
		{
			Rout = newRout;
			Time = time;
		}
		public Rout Rout { get; set; }
		public TimeSpan Time { get; set; }
	}
}
