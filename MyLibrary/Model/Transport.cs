using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;

namespace MinskTrans.DesctopClient.Model
{
	class Transport:ObservableObject
	{
		string Num { get; set; }
		string Name { get; set; }
		IList<Rout> Routs { get; set; }
	}
}
