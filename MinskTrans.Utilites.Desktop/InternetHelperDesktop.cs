using System;
using MinskTrans.Utilites.Base.IO;
using MinskTrans.Utilites.Base.Net;

namespace MinskTrans.Utilites.Desktop
{
	public class InternetHelperDesktop:InternetHelperBase
	{
		public InternetHelperDesktop(FileHelperBase fileHelper)
			:base(fileHelper)
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
