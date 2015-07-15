using System.Collections.Generic;

namespace MinskTrans.Context.Base.BaseModel
{
	class Transport
	{
		string Num { get; set; }
		string Name { get; set; }
		IList<Rout> Routs { get; set; }
	}
}
