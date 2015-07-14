using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MinskTrans.DesctopClient.Model;

namespace MinskTrans.DesctopClient.Comparer
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
