using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MinskTrans.DesctopClient.Modelview;
using MinskTrans.Net;
using MinskTrans.Utilites.Base.IO;
using MinskTrans.Utilites.Base.Net;
using MyLibrary;

namespace CommonLibrary
{
	
	public class NewsManager : NewsManagerBase
	{
		

		public NewsManager(FileHelperBase fileHelper, InternetHelperBase internet)
			:base(fileHelper, internet)
		{
			
		}

        private ApplicationSettingsHelper lastUpdateDataDateTimeBack;
        public override DateTime LastUpdateMainNewsDateTimeUtc
        {
            get
            {
                if (lastUpdateDataDateTimeBack == null)
                    lastUpdateDataDateTimeBack = new ApplicationSettingsHelper();
                return lastUpdateDataDateTimeBack.DateTimeSettings;
            }

            set
            {
                if (lastUpdateDataDateTimeBack == null)
                    lastUpdateDataDateTimeBack = new ApplicationSettingsHelper();
                lastUpdateDataDateTimeBack.DateTimeSettings = value;
                OnPropertyChanged();
            }
        }

        private ApplicationSettingsHelper lastUpdateHotDataDateTimeBack;
        public override DateTime LastUpdateHotNewsDateTimeUtc
        {

			get
			{
				if (lastUpdateHotDataDateTimeBack == null)
					lastUpdateHotDataDateTimeBack = new ApplicationSettingsHelper();
				return lastUpdateHotDataDateTimeBack.DateTimeSettings;
			}

			set
			{
				if (lastUpdateHotDataDateTimeBack == null)
					lastUpdateHotDataDateTimeBack = new ApplicationSettingsHelper();
				lastUpdateHotDataDateTimeBack.DateTimeSettings = value;
				OnPropertyChanged();
			}
        }

        public override Task<List<NewsEntry>> CheckAsync(string uri, string XpathSelectInfo, string XpathSelectDate)
		{
			throw new NotImplementedException();
		}

		public override Task CheckHotNewsAsync()
		{
			throw new NotImplementedException();
		}
	}
}
