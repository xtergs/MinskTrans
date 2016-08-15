using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MinskTrans.AutoRouting.AutoRouting;

namespace DesktopUnitTests
{
	[TestClass]
	public class DistanceTests
	{
		[TestMethod]
		public void Test()
		{
			EquirectangularDistance calculator = new EquirectangularDistance();

			var xxx = calculator.CalculateDistance(53.8626242, 27.58738873, 53.87476918, 27.58666302);


		}


	}
}
