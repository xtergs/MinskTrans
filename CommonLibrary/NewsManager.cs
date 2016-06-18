﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MetroLog;
using MinskTrans.Context;
using MinskTrans.DesctopClient.Modelview;
using MinskTrans.Net;
using MinskTrans.Utilites.Base.IO;
using MinskTrans.Utilites.Base.Net;
using MyLibrary;

namespace CommonLibrary
{
	
	public class NewsManager : NewsManagerBase
	{
		

		public NewsManager(FileHelperBase fileHelper, InternetHelperBase internet, ISettingsModelView settings, ILogManager logManager, FilePathsSettings files)
			:base(fileHelper, internet, logManager, files)
		{
			
		}

/*
        private ISettingsModelView lastUpdateDataDateTimeBack;
*/
        

       

        public override Task<List<NewsEntry>> CheckAsync(string uri, string XpathSelectInfo, string XpathSelectDate)
		{
			throw new NotImplementedException();
		}

		public override Task<bool> CheckHotNewsAsync()
		{
			throw new NotImplementedException();
		}
	}
}
