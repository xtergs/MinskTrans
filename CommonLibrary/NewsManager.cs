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

        private ApplicationSettingsHelper lastUpdateDataDateTimeBack;
        public override DateTime LastUpdateMainNewsDateTimeUtc
        {
#if WINDOWS_PHONE_APP || WINDOWS_UAP
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
#else
			get { return allNews.Max(x => x.PostedUtc); }
#endif
        }

        private ApplicationSettingsHelper lastUpdateHotDataDateTimeBack;
        public override DateTime LastUpdateHotNewsDateTimeUtc
        {
#if WINDOWS_PHONE_APP || WINDOWS_UAP
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
#else
            get { return allHotNewsDictionary.Max(x => x.CollectedUtc); }

#endif
        }

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
