﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.System.Threading;
using MinskTrans.DesctopClient;
using Windows.Web.Http;
using System.IO.Compression;
using System.Linq;
using MinskTrans.DesctopClient.Model;
using Buffer = Windows.Storage.Streams.Buffer;
using UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding;


namespace MinskTrans.Universal
{
	public  class UniversalContext : Context
	{
		#region Overrides of Context

		protected async Task<bool> FileExistss(string file)
		{
			try
			{
				var fl = await ApplicationData.Current.LocalFolder.GetFileAsync(file);
				OnLogMessage("file " + file + " exist");
				return true;
			}
			catch (FileNotFoundException ex)
			{
				OnLogMessage("file " + file + "not exist");
				return false;
			}
		}

		public override void Create(bool AutoUpdate = true)
		{
			//TODO
			//throw new NotImplementedException();
			if (FileExists("data.dat"))
			{
				Load();
				//return;
			}
			FavouriteRouts = new ObservableCollection<Rout>();
			FavouriteStops = new ObservableCollection<Stop>();
			Groups = new ObservableCollection<GroupStop>();
			//if (AutoUpdate)
			//	await UpdateAsync();
			DownloadUpdate();
			//HaveUpdate();
			//ApplyUpdate();
		}



		protected override bool FileExists(string file)
		{
			return FileExistss(file).Result;
		}

		protected override async void FileDelete(string file)
		{
			try
			{
				var fl = await ApplicationData.Current.LocalFolder.GetFileAsync(file);
				fl.DeleteAsync();
			}
			catch(FileNotFoundException fileNotFound)
			{ }
			
		}

		protected override async void FileMove(string oldFile, string newFile)
		{
			try
			{
				var fl = await ApplicationData.Current.LocalFolder.GetFileAsync(oldFile);
				fl.RenameAsync(newFile);
			}
			catch(FileNotFoundException fileNOtFound)
			{ }
		}

		protected Task<string> FileReadAllTextt(string file)
		{
			return Task.Run(async () =>
			{
				var fl = await ApplicationData.Current.LocalFolder.GetFileAsync(file);
				var xx = (await FileIO.ReadBufferAsync(fl));
				//var tt =await FileIO.ReadLinesAsync(fl);
				var resultText = await FileIO.ReadTextAsync(fl);
				return resultText;
			});
		}

		async protected override Task<string> FileReadAllText(string file)
		{
			var fl = await ApplicationData.Current.LocalFolder.GetFileAsync(file);
			var xx = (await FileIO.ReadBufferAsync(fl));
			//var tt =await FileIO.ReadLinesAsync(fl);
			var resultText = await FileIO.ReadTextAsync(fl);
			return resultText;
			
		}

		async public override void DownloadUpdate()
		{
			OnDataBaseDownloadStarted();
			try
			{
				await Task.WhenAll(new List<Task>(){
					DoSincroFit(list[0].Value, list[0].Key + ".new"),
					DoSincroFit(list[1].Value, list[1].Key + ".new"),
					DoSincroFit(list[2].Value, list[2].Key + ".new")});
				OnDataBaseDownloadEnded();

			}
			catch (System.Net.WebException e)
			{
				OnErrorDownloading();
			}
		}

		public override Task DownloadUpdateAsync()
		{
			return Task.Run(()=>DownloadUpdate());
		}

		async Task<string> ReadAllFile(StorageFile file)
		{
			StringBuilder builder = new StringBuilder();
			using (var stream = await file.OpenStreamForReadAsync())
			{
				TextReader reader = new StreamReader(stream);
				
				builder.Append(reader.ReadToEnd());
			}
			return builder.ToString();
		}

		async public override Task<bool> HaveUpdate()
		{
			//return await Task.Run(async () =>
			//{
				OnLogMessage("Have update started");
				try
				{
#if DEBUG
					await Task.WhenAll( Task.Run(async () =>
					{
					ShedulerParser.LogMessage += (sender, args) => OnLogMessage(args.Message);

						StorageFile file = await ApplicationData.Current.LocalFolder.GetFileAsync(list[0].Key + ".new");
						OnLogMessage("get file" + list[0].Key);
						newStops = ShedulerParser.ParsStops(await ReadAllFile(file));
						OnLogMessage("parsed file" + list[0].Key);
					}),
						Task.Run(async () =>
						{
					ShedulerParser.LogMessage += (sender, args) => OnLogMessage(args.Message);

							StorageFile file = await ApplicationData.Current.LocalFolder.GetFileAsync(list[1].Key + ".new");
							OnLogMessage("get file" + list[1].Key);

							newRoutes = ShedulerParser.ParsRout(await ReadAllFile(file));
							OnLogMessage("parsed file" + list[1].Key);

						}),
						Task.Run(async () =>
						{
					ShedulerParser.LogMessage += (sender, args) => OnLogMessage(args.Message);

							StorageFile file = await ApplicationData.Current.LocalFolder.GetFileAsync(list[2].Key + ".new");
							OnLogMessage("get file" + list[2].Key);

							newSchedule = ShedulerParser.ParsTime(await ReadAllFile(file));
							OnLogMessage("parsed file" + list[2].Key);

						}));
#else
					ShedulerParser.LogMessage += (sender, args) => OnLogMessage(args.Message);
					StorageFile file = await ApplicationData.Current.LocalFolder.GetFileAsync(list[0].Key + ".new");
					OnLogMessage("get file " + list[0].Key);
					newStops = ShedulerParser.ParsStops(await ReadAllFile(file));
					OnLogMessage("parsed file " + list[0].Key);

					file = await ApplicationData.Current.LocalFolder.GetFileAsync(list[1].Key + ".new");
					OnLogMessage("get fil e" + list[1].Key);

					newRoutes = ShedulerParser.ParsRout(await ReadAllFile(file));
					OnLogMessage("parsed file " + list[1].Key);

					file = await ApplicationData.Current.LocalFolder.GetFileAsync(list[2].Key + ".new");
					OnLogMessage("get file " + list[2].Key);

					newSchedule = ShedulerParser.ParsTime(await ReadAllFile(file));
					OnLogMessage("parsed file " + list[2].Key);
#endif
					OnLogMessage("All threads ended");
				}
				catch (FileNotFoundException e)
				{
					OnLogMessage("error file not found");
					return false;
				}
				catch (Exception e)
				{
					OnLogMessage(e.Message);
					throw new Exception(e.Message);
				}

				if (Stops == null || Routs == null || Times == null)
					return true;

				if (newStops.Count == Stops.Count && newRoutes.Count == Routs.Count && newSchedule.Count == Times.Count)
					return false;

				foreach (var newRoute in newRoutes)
				{
					if (Routs.AsParallel().All(x => x.RoutId == newRoute.RoutId && x.Datestart == newRoute.Datestart))
						return false;
				}


				OnLogMessage("have update true");
				return true;
			//});

		}

		private async Task DoSincroFit(string uri, string file)
		{
			HttpWebRequest request = HttpWebRequest.CreateHttp(uri);
			//this.file = file;
			//Add headers to request
			request.Headers["Type"] = "sincrofit";
			request.Headers["Device"] = "1";
			request.Headers["Version"] = "0.000";
			request.Headers["Os"] = "WindowsPhone";
			request.Headers["Cache-Control"] = "no-cache";
			request.Headers["Pragma"] = "no-cache";
			var x = await request.GetResponseAsync();
			await playResponseAsync(x, file);
				//var x = request.BeginGetResponse(playResponseAsync, request);
			


		}

		//private string file;

		struct Mystr
		{
			public IAsyncResult Result;
			public string File;
		}

		public  Task playResponseAsync(WebResponse response, string file)
		{
			//Declaration of variables
			//HttpWebRequest webRequest =response;
			return Task.Run(async () =>
			{
				try
				{
					string fileName = file;

					//using (HttpWebResponse webResponse = response.res)
					{
						byte[] buffer = new byte[1024];
						Buffer bufferb = new Buffer(1024);

						var newZipFile =
							await ApplicationData.Current.LocalFolder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);

						using (var writeStream = await newZipFile.OpenAsync(FileAccessMode.ReadWrite))
						{
							using (var outputStream = writeStream.GetOutputStreamAt(0))
							{
								using (var dataWriter = new DataWriter(outputStream))
								{
									using (Stream input = response.GetResponseStream())
									{
										int size = 0;
										var totalSize = 0;
										for (size = input.Read(buffer, 0, buffer.Length);
											size == buffer.Length;
											size = input.Read(buffer, 0, buffer.Length))
										{
											dataWriter.WriteBytes(buffer);
											totalSize += size; //get the progress of download
										}
										for (int i = 0; i < size; i++)
											dataWriter.WriteByte(buffer[i]);
										await dataWriter.StoreAsync();
										await outputStream.FlushAsync();
										dataWriter.DetachStream();
									}
								}
							}
						}

					}
				}
				catch (Exception e)
				{

				}
				OnLogMessage("file downloaded: " + file);
			});
		}



		public override async void Save()
		{
			await IsolatedStorageOperations.Save(this, "data.dat");
		}

		public override async void Load()
		{
			string str =await ReadAllFile(await ApplicationData.Current.LocalFolder.GetFileAsync("data.dat"));
			//var tempThis = await IsolatedStorageOperations.Load<UniversalContext>("data.dat");
			//Stops = tempThis.Stops;
			//Routs = tempThis.Routs;
			//Times = tempThis.Times;
		}

		#endregion
	}
}
