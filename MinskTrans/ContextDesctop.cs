using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

using System.Xml;
using System.Xml.Serialization;
using MinskTrans.DesctopClient.Model;
using MyLibrary;
using Newtonsoft.Json;

namespace MinskTrans.DesctopClient
{
	//[Serializable]
	public class ContextDesctop : Context
	{


		public ContextDesctop(FileHelperBase helper, InternetHelperBase internetHelper) 
			: base(helper, internetHelper)
		{
		}
	}
}
