using System.Collections.Generic;

namespace MinskTrans.DesctopClient.Model
{
	class Transport
	{
		string Num { get; set; }
		string Name { get; set; }
		IList<Rout> Routs { get; set; }
	}
}
