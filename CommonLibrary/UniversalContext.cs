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
		

		#region Overrides of Context

//		async Task<bool> FileExistss(IStorageFolder folder, string file)
//		{
//			try
//			{
//				var fl = await folder.GetFileAsync(file);
//				OnLogMessage("file " + file + " exist");
//				return true;
//			}
//			catch (FileNotFoundException ex)
//			{
//				OnLogMessage("file " + file + "not exist");
//#if BETA
//				Logger.Log("FileExistss").WriteLine(ex.Message).WriteLine(ex.FileName);
//#endif
//				return false;
//			}
//		}

		//protected async Task<bool> FileExistss(string file)
		//{
		//	return await FileExistss(ApplicationData.Current.RoamingFolder, file);
		//}

		
		ApplicationSettingsHelper lastUpdateDataDateTime;
        public override DateTime LastUpdateDataDateTime
		{
#if WINDOWS_PHONE_APP || WINDOWS_UAP
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
#else
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
#endif
		}

		



		//protected override async Task<bool> FileExists(string file)
		//{
		//	return await FileExistss(file);
		//}

		//protected override async Task FileDelete(string file)
		//{
		//	try
		//	{
		//		var fl = await ApplicationData.Current.RoamingFolder.GetFileAsync(file);
		//		await fl.DeleteAsync();
		//	}
		//	catch (FileNotFoundException fileNotFound)
		//	{
		//		return;
		//	}

		//}

		//protected override async Task FileMove(string oldFile, string newFile)
		//{
		//	try
		//	{
		//		var fl = await ApplicationData.Current.RoamingFolder.GetFileAsync(oldFile);
		//		await fl.RenameAsync(newFile, NameCollisionOption.ReplaceExisting);
		//	}
		//	catch (FileNotFoundException fileNOtFound)
		//	{
		//	}
		//}

		//async Task FileMove(IStorageFolder folder, string oldFile, string newFile)
		//{
		//	try
		//	{
		//		var fl = await folder.GetFileAsync(oldFile);
		//		await fl.RenameAsync(newFile, NameCollisionOption.ReplaceExisting);
		//	}
		//	catch (FileNotFoundException fileNOtFound)
		//	{
		//	}
		//}

		//protected Task<string> FileReadAllTextt(string file)
		//{
		//	return Task.Run(async () =>
		//	{
		//		var fl = await ApplicationData.Current.RoamingFolder.GetFileAsync(file);
		//		//var xx = (await FileIO.ReadBufferAsync(fl));
		//		//var tt =await FileIO.ReadLinesAsync(fl);
		//		var resultText = await FileIO.ReadTextAsync(fl);
		//		return resultText;
		//	});
		//}

		//protected override async Task<string> FileReadAllText(string file)
		//{
		//	var fl = await ApplicationData.Current.RoamingFolder.GetFileAsync(file);
		//	//var xx = (await FileIO.ReadBufferAsync(fl));
		//	//var tt =await FileIO.ReadLinesAsync(fl);
		//	var resultText = await FileIO.ReadTextAsync(fl);
		//	return resultText;

		//}

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
					InternetHelper.Download(list[0].Value, list[0].Key + NewExt),
					InternetHelper.Download(list[1].Value, list[1].Key + NewExt),
					InternetHelper.Download(list[2].Value, list[2].Key + NewExt)
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

		private async void OnDataBaseDownloadEnded(object sender, EventArgs args)
		{

		}


		//private async Task<string> ReadAllFile(StorageFile file)
		//{
		//	StringBuilder builder = new StringBuilder();
		//	using (var stream = await file.OpenStreamForReadAsync())
		//	{
		//		TextReader reader = new StreamReader(stream);

		//		builder.Append(reader.ReadToEnd());
		//	}
		//	return builder.ToString();
		//}

		

		private static object o = new Object();

		#endregion

		public UniversalContext(FileHelperBase helper) : base(helper)
		{
		}
	}
}
