using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.Storage;
using MinskTrans.DesctopClient.Modelview;
using MinskTrans.Net;
using MinskTrans.Universal.Annotations;
using MinskTrans.Utilites.Base.IO;
using MinskTrans.Utilites.Base.Net;
using MyLibrary;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace CommonLibrary
{
	
	public class NewsManager : NewsManagerBase
	{
		

		public NewsManager(FileHelperBase fileHelper, InternetHelperBase internet)
			:base(fileHelper, internet)
		{
			
		}


		#region Overrides of NewsManagerBase

		public override Task<List<NewsEntry>> CheckAsync(string uri, string XpathSelectInfo, string XpathSelectDate)
		{
			throw new NotImplementedException();
		}

		public override Task CheckHotNewsAsync()
		{
			throw new NotImplementedException();
		}

		#endregion
	}
}
