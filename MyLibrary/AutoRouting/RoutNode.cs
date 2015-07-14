using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MinskTrans.DesctopClient.Model;

namespace MinskTrans.DesctopClient.AutoRouting
{
	public struct RoutNode
	{
		public Stop Start;
		public Stop End;
		public double Distance;
	}
}
