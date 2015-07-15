using System;
using System.Net;
using System.Threading.Tasks;
using MinskTrans.DesctopClient.Update;
using MyLibrary;

namespace CommonLibrary
{
	public class UpdateManagerUniversal : UpdateManagerBase
	{
		public UpdateManagerUniversal(FileHelperBase helper, InternetHelperBase internet)
			:base(helper, internet)
		{

		}
		
	}
}
