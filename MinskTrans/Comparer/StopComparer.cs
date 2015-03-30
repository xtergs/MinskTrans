using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinskTrans.DesctopClient.Comparer
{
	class StopComparer:IEqualityComparer<Stop>
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
