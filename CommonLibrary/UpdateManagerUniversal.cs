using MinskTrans.DesctopClient.Update;
using MyLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibrary
{
	public class UpdateManagerUniversal : UpdateManagerBase
	{
		public UpdateManagerUniversal(FileHelperBase helper, InternetHelperBase internet)
			:base(helper, internet)
		{

		}
		public override async Task<bool> DownloadUpdate()
		{
			OnDataBaseDownloadStarted();
			var folder = TypeFolder.Temp;
			try
			{
				await Task.WhenAll(
				internetHelper.Download(list[0].Value, list[0].Key + FileHelperBase.NewExt, folder),
				internetHelper.Download(list[0].Value, list[0].Key + FileHelperBase.NewExt, folder),
				internetHelper.Download(list[0].Value, list[0].Key + FileHelperBase.NewExt, folder));

				OnDataBaseDownloadEnded();

			}
			catch (System.Net.WebException e)
			{
				OnErrorDownloading();
				await Task.WhenAll(
				fileHelper.DeleteFile(folder, list[0].Key + FileHelperBase.NewExt),
				fileHelper.DeleteFile(folder, list[1].Key + FileHelperBase.NewExt),
				fileHelper.DeleteFile(folder, list[2].Key + FileHelperBase.NewExt));
				return false;
			}
			catch (Exception)
			{
				OnErrorDownloading();
				await Task.WhenAll(
				fileHelper.DeleteFile(folder, list[0].Key + FileHelperBase.NewExt),
				fileHelper.DeleteFile(folder, list[1].Key + FileHelperBase.NewExt),
				fileHelper.DeleteFile(folder, list[2].Key + FileHelperBase.NewExt));
				throw;
			}

			await Task.WhenAll(
			fileHelper.SafeMoveAsync(folder, list[0].Key + FileHelperBase.NewExt, list[0].Key),
			fileHelper.SafeMoveAsync(folder, list[1].Key + FileHelperBase.NewExt, list[1].Key),
			fileHelper.SafeMoveAsync(folder, list[2].Key + FileHelperBase.NewExt, list[2].Key));
			return true;
		}
	}
}
