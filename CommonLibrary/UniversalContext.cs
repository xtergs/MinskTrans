using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Windows.Storage;
using Windows.System.Threading;
using MinskTrans.DesctopClient;
using MinskTrans.DesctopClient.Model;
using MinskTrans.Universal;
using MyLibrary;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using MinskTrans.DesctopClient.Modelview;

namespace CommonLibrary
{
	public class UniversalContext : Context
	{
		public UniversalContext(FileHelperBase helper) : base(helper)
		{
		}

		ApplicationSettingsHelper lastUpdateDataDateTime;
        public override DateTime LastUpdateDataDateTime
		{
			get
			{
				if (lastUpdateDataDateTime == null)
					lastUpdateDataDateTime = new ApplicationSettingsHelper();
				return lastUpdateDataDateTime.DateTimeSettings;
            }

			set
			{
				if (lastUpdateDataDateTime == null)
					lastUpdateDataDateTime = new ApplicationSettingsHelper();
				lastUpdateDataDateTime.DateTimeSettings = value;
				OnPropertyChanged();
			}

		}

		
		public override async Task<bool> DownloadUpdate()
		{
			//#if DEBUG
			//			OnDataBaseDownloadEnded();
			//			return;
			//#endif
			try
			{
				OnDataBaseDownloadStarted();
				await Task.WhenAll(new List<Task>()
				{
					internetHelper.Download(list[0].Value, list[0].Key + NewExt, TypeFolder.Roaming),
					internetHelper.Download(list[1].Value, list[1].Key + NewExt, TypeFolder.Roaming),
					internetHelper.Download(list[2].Value, list[2].Key + NewExt, TypeFolder.Roaming)
				});
				OnDataBaseDownloadEnded();

			}
			catch (System.Net.WebException e)
			{
				OnErrorDownloading();
				return false;
			}
			return true;
		}

		private void OnDataBaseDownloadEnded(object sender, EventArgs args)
		{

		}

	
	}
}
