﻿using MyLibrary;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MinskTrans.DesctopClient.Update
{
	public class UpdateManagerDesktop : UpdateManagerBase
	{
		public UpdateManagerDesktop(FileHelperBase helper)
			:base(helper)
		{

		}
		public override async Task<bool> DownloadUpdate()
		{
			OnDataBaseDownloadStarted();
			try
			{
				using (var client = new WebClient())
				{
					//Task.WhenAll(
					if (!Directory.Exists(fileHelper.GetPath(TypeFolder.Current)))
						Directory.CreateDirectory(fileHelper.GetPath(TypeFolder.Current));
					client.DownloadFile(list[0].Value, Path.Combine(fileHelper.GetPath(TypeFolder.Current), list[0].Key + FileHelperBase.NewExt));
					client.DownloadFile(list[1].Value, Path.Combine(fileHelper.GetPath(TypeFolder.Current), list[1].Key + FileHelperBase.NewExt));
					client.DownloadFile(list[2].Value, Path.Combine(fileHelper.GetPath(TypeFolder.Current), list[2].Key + FileHelperBase.NewExt));
					//);
				}
				OnDataBaseDownloadEnded();

			}
			catch (System.Net.WebException e)
			{
				OnErrorDownloading();
				await Task.WhenAll(
				fileHelper.DeleteFile(TypeFolder.Current, list[0].Key + FileHelperBase.NewExt),
				fileHelper.DeleteFile(TypeFolder.Current, list[1].Key + FileHelperBase.NewExt),
				fileHelper.DeleteFile(TypeFolder.Current, list[2].Key + FileHelperBase.NewExt));
				return false;
			}

			await Task.WhenAll(
			fileHelper.SafeMoveAsync(TypeFolder.Current, list[0].Key + FileHelperBase.NewExt, list[0].Key),
			fileHelper.SafeMoveAsync(TypeFolder.Current, list[1].Key + FileHelperBase.NewExt, list[1].Key),
			fileHelper.SafeMoveAsync(TypeFolder.Current, list[2].Key + FileHelperBase.NewExt, list[2].Key));
			return true;
		}
	}
}
