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
using System.Xml;
using System.Xml.Serialization;
using MinskTrans.DesctopClient.Model;
using MinskTrans.Universal.Model;
using Buffer = Windows.Storage.Streams.Buffer;
using UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding;


namespace MinskTrans.Universal
{
	public class UniversalContext : Context
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
			//if (FileExists("data.dat"))
			//{
			//	Load();
			//	//return;
			//}
			FavouriteRouts = new ObservableCollection<RoutWithDestinations>();
			FavouriteStops = new ObservableCollection<Stop>();
			Groups = new ObservableCollection<GroupStop>();
			//DataBaseDownloadEnded += async (sender, args) => 
			//{


			//};
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
			catch (FileNotFoundException fileNotFound)
			{
			}

		}

		protected override async void FileMove(string oldFile, string newFile)
		{
			try
			{
				var fl = await ApplicationData.Current.LocalFolder.GetFileAsync(oldFile);
				fl.RenameAsync(newFile);
			}
			catch (FileNotFoundException fileNOtFound)
			{
			}
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

		protected override async Task<string> FileReadAllText(string file)
		{
			var fl = await ApplicationData.Current.LocalFolder.GetFileAsync(file);
			var xx = (await FileIO.ReadBufferAsync(fl));
			//var tt =await FileIO.ReadLinesAsync(fl);
			var resultText = await FileIO.ReadTextAsync(fl);
			return resultText;

		}

		public override async Task DownloadUpdate()
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
					Download(list[0].Value, list[0].Key + ".new"),
					Download(list[1].Value, list[1].Key + ".new"),
					Download(list[2].Value, list[2].Key + ".new")
				});
				OnDataBaseDownloadEnded();

			}
			catch (System.Net.WebException e)
			{
				OnErrorDownloading();
			}
		}

		public override async Task UpdateAsync()
		{
			//TODO
			//throw new NotImplementedException();
			//return Task.Run(async () =>
			//{
			OnUpdateStarted();
			await DownloadUpdate();
			if (await HaveUpdate(list[0].Key + ".new", list[1].Key + ".new", list[2].Key + ".new"))
				ApplyUpdate();
			OnUpdateEnded();

			//});
		}

		private async void OnDataBaseDownloadEnded(object sender, EventArgs args)
		{

		}


		private async Task<string> ReadAllFile(StorageFile file)
		{
			StringBuilder builder = new StringBuilder();
			using (var stream = await file.OpenStreamForReadAsync())
			{
				TextReader reader = new StreamReader(stream);

				builder.Append(reader.ReadToEnd());
			}
			return builder.ToString();
		}

		public override async Task<bool> HaveUpdate(string fileStops, string fileRouts, string fileTimes)
		{
			//return  Task.Run(async () =>
			//{
			OnLogMessage("Have update started");
			try
			{
//#if DEBUG

				await Task.WhenAll(Task.Run(async () =>
				{
					ShedulerParser.LogMessage += (sender, args) => OnLogMessage(args.Message);

					StorageFile file = await ApplicationData.Current.LocalFolder.GetFileAsync(fileStops);
					OnLogMessage("get file" + list[0].Key);
					newStops = ShedulerParser.ParsStops(await ReadAllFile(file));
					OnLogMessage("parsed file" + list[0].Key);
				}),
					Task.Run(async () =>
					{
						ShedulerParser.LogMessage += (sender, args) => OnLogMessage(args.Message);

						StorageFile file = await ApplicationData.Current.LocalFolder.GetFileAsync(fileRouts);
						OnLogMessage("get file" + list[1].Key);

						newRoutes = ShedulerParser.ParsRout(await ReadAllFile(file));
						OnLogMessage("parsed file" + list[1].Key);

					}),
					Task.Run(async () =>
					{
						ShedulerParser.LogMessage += (sender, args) => OnLogMessage(args.Message);

						StorageFile file = await ApplicationData.Current.LocalFolder.GetFileAsync(fileTimes);
						OnLogMessage("get file" + list[2].Key);

						newSchedule = ShedulerParser.ParsTime(await ReadAllFile(file));
						OnLogMessage("parsed file" + list[2].Key);

					}));
//#else
//					ShedulerParser.LogMessage += (sender, args) => OnLogMessage(args.Message);
//					StorageFile file = await ApplicationData.Current.LocalFolder.GetFileAsync(list[0].Key + ".new");
//					OnLogMessage("get file " + list[0].Key);
//					newStops = ShedulerParser.ParsStops(await ReadAllFile(file));
//					OnLogMessage("parsed file " + list[0].Key);

//					file = await ApplicationData.Current.LocalFolder.GetFileAsync(list[1].Key + ".new");
//					OnLogMessage("get fil e" + list[1].Key);

//					newRoutes = ShedulerParser.ParsRout(await ReadAllFile(file));
//					OnLogMessage("parsed file " + list[1].Key);

//					file = await ApplicationData.Current.LocalFolder.GetFileAsync(list[2].Key + ".new");
//					OnLogMessage("get file " + list[2].Key);

//					newSchedule = ShedulerParser.ParsTime(await ReadAllFile(file));
//					OnLogMessage("parsed file " + list[2].Key);
//#endif
				OnLogMessage("All threads ended");
			}
			catch (FileNotFoundException e)
			{
				OnLogMessage(e.Message);
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

		private async Task Download(string uri, string file)
		{
			var httpClient = new HttpClient();
			// Increase the max buffer size for the response so we don't get an exception with so many web sites

			httpClient.DefaultRequestHeaders.Add("user-agent",
				"Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2; WOW64; Trident/6.0)");

			HttpResponseMessage response = await httpClient.GetAsync(new Uri(uri));
			response.EnsureSuccessStatusCode();

			//string str= response.StatusCode + " " + response.ReasonPhrase + Environment.NewLine;
			var fileGet =
				await ApplicationData.Current.LocalFolder.CreateFileAsync(file, CreationCollisionOption.ReplaceExisting);
			using (var writeStream = await fileGet.OpenAsync(FileAccessMode.ReadWrite))
			{
				using (var outputStream = writeStream.GetOutputStreamAt(0))
				{
					var responseBodyAsText = await response.Content.WriteToStreamAsync(outputStream);
				}
			}

			//var responseBodyAsText1 = responseBodyAsText.Replace("<br>", Environment.NewLine); // Insert new lines

			//HttpWebRequest request = HttpWebRequest.CreateHttp(uri);
			////this.file = file;
			////Add headers to request
			//request.Headers["Type"] = "sincrofit";
			//request.Headers["Device"] = "1";
			//request.Headers["Version"] = "0.000";
			//request.Headers["Os"] = "WindowsPhone";
			//request.Headers["Cache-Control"] = "no-cache";
			//request.Headers["Pragma"] = "no-cache";
			//request.Headers[HttpRequestHeader.UserAgent] =
			//	@"Mozilla/5.0 (Windows NT 6.1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/41.0.2228.0 Safari/537.36";
			//request.ContinueTimeout = 100000;
			//OnLogMessage("Starting reqest");
			//var x = await request.GetResponseAsync();
			//OnLogMessage("End Request");
			//await ResponseAsync(x, file);
			//	//var x = request.BeginGetResponse(playResponseAsync, request);



		}

		public Task ResponseAsync(WebResponse response, string file)
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
#if DEBUG
				var File = await ApplicationData.Current.LocalFolder.GetFileAsync(file);
				string str = await FileIO.ReadTextAsync(File);
#endif
				OnLogMessage("file downloaded: " + file);
			});
		}



		public override async void Save()
		{

			await IsolatedStorageOperations.Save(this, "data.dat");
		}

		public override async Task Load()
		{
			OnLoadStarted();
			foreach (var keyValuePair in list)
			{

				if (!await FileExistss(keyValuePair.Key))
				{
					OnErrorLoading(new ErrorLoadingDelegateArgs() {Error = ErrorLoadingDelegateArgs.Errors.NoSourceFiles});
					return;
				}
			}

			await HaveUpdate(list[0].Key, list[1].Key, list[2].Key);
			ApplyUpdate();
			//string str =await ReadAllFile(await ApplicationData.Current.LocalFolder.GetFileAsync("data.dat"));
			string nameFile = "data.dat";
			if (await FileExistss(nameFile))
			{

				if (Routs == null || Stops == null)
				{

					OnErrorLoading(new ErrorLoadingDelegateArgs() {Error = ErrorLoadingDelegateArgs.Errors.NoFileToDeserialize});
					return;
				}
				//var tempThis = await IsolatedStorageOperations.Load<UniversalContext>(nameFile);
				var storage = ApplicationData.Current.LocalFolder;
				

				//if (storage.FileExists(file))
						var stream = await storage.OpenStreamForReadAsync(nameFile);
						XmlSerializer serializer = new XmlSerializer(typeof(UniversalContext));
				var abj = (UniversalContext)serializer.Deserialize(stream);
				
				//XmlReader reader = XmlReader.Create(stream);
				//		//serializer.Deserialize()
				//ReadXml(reader);
				{
					//IsolatedStorageFileStream stream = null;
					try
					{
					}
					catch (Exception e)
					{
					}
					finally
					{

						//if (stream != null)
						//{
						//	stream.Close();
						//	stream.Dispose();
						//}
					}
					//return obj;
				}
				//await obj.Save(file);
				//return obj;
					abj.FavouriteRouts = new ObservableCollection<RoutWithDestinations>();
				foreach (var favouriteRoutsId in abj.FavouriteRoutsIds)
				{
					abj.FavouriteRouts.Add(new RoutWithDestinations(Routs.First(x=>x.RoutId == favouriteRoutsId), this));
				}
				abj.FavouriteStops = new ObservableCollection<Stop>();
				foreach (var favouriteStopsId in abj.FavouriteStopsIds)
				{
					abj.FavouriteStops.Add(Stops.First(x=>x.ID == favouriteStopsId));
				}
					FavouriteRouts = abj.FavouriteRouts;
					FavouriteStops = abj.FavouriteStops;
				
				

			}
			else
			{
				FavouriteRouts = new ObservableCollection<RoutWithDestinations>();
				FavouriteStops = new ObservableCollection<Stop>();
			}

			//Stops = tempThis.Stops;
			//Routs = tempThis.Routs;
			//Times = tempThis.Times;
			AllPropertiesChanged();
			OnLoadEnded();
		}

		#endregion
	}
}
