using System.Collections.Generic;
using MinskTrans.Context.Base.BaseModel;

namespace MinskTrans.AutoRouting.Comparer
{
	public class StopComparer:IEqualityComparer<Stop>
	{
		public bool Equals(Stop x, Stop y)
		{
			return (x.ID == y.ID);

		}

		public int GetHashCode(Stop obj)
		{
			return obj.ID;
		}
	}
}
