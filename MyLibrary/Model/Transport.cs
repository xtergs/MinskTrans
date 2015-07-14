using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace MinskTrans.DesctopClient.Model
{
	class Transport
	{
		string Num { get; set; }
		string Name { get; set; }
		IList<Rout> Routs { get; set; }
	}
}
