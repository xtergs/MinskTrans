using System;
using MetroLog;
using MinskTrans.Utilites.Base.IO;
using MinskTrans.Utilites.Base.Net;

namespace MinskTrans.Utilites.Desktop
{
	public class InternetHelperDesktop:InternetHelperBase
	{
		public InternetHelperDesktop(FileHelperBase fileHelper, ILogger logger)
			:base(fileHelper, logger)
		{

		}

		#region Overrides of InternetHelperBase

		public override void UpdateNetworkInformation()
		{
			throw new NotImplementedException();
		}

		#endregion
	}
}
