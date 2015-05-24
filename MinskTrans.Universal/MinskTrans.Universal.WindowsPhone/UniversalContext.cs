using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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
using MinskTrans.DesctopClient.Modelview;
using MinskTrans.Universal.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
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
				var fl = await ApplicationData.Current.RoamingFolder.GetFileAsync(file);
				OnLogMessage("file " + file + " exist");
				return true;
			}
			catch (FileNotFoundException ex)
			{
				OnLogMessage("file " + file + "not exist");
#if BETA
				Logger.Log("FileExistss").WriteLine(ex.Message).WriteLine(ex.FileName);
#endif
				return false;
			}
		}

		public override void Create(bool AutoUpdate = true)
		{
			FavouriteRouts = new ObservableCollection<RoutWithDestinations>();
			FavouriteStops = new ObservableCollection<Stop>();
			Groups = new ObservableCollection<GroupStop>();
		}



		protected override async Task<bool> FileExists(string file)
		{
			return await FileExistss(file);
		}

		protected override async Task FileDelete(string file)
		{
			try
			{
				var fl = await ApplicationData.Current.RoamingFolder.GetFileAsync(file);
				await fl.DeleteAsync();
			}
			catch (FileNotFoundException fileNotFound)
			{
				return;
			}

		}

		protected override async Task FileMove(string oldFile, string newFile)
		{
			try
			{
				var fl = await ApplicationData.Current.RoamingFolder.GetFileAsync(oldFile);
				await fl.RenameAsync(newFile);
			}
			catch (FileNotFoundException fileNOtFound)
			{
			}
		}

		protected Task<string> FileReadAllTextt(string file)
		{
			return Task.Run(async () =>
			{
				var fl = await ApplicationData.Current.RoamingFolder.GetFileAsync(file);
				//var xx = (await FileIO.ReadBufferAsync(fl));
				//var tt =await FileIO.ReadLinesAsync(fl);
				var resultText = await FileIO.ReadTextAsync(fl);
				return resultText;
			});
		}

		protected override async Task<string> FileReadAllText(string file)
		{
			var fl = await ApplicationData.Current.RoamingFolder.GetFileAsync(file);
			//var xx = (await FileIO.ReadBufferAsync(fl));
			//var tt =await FileIO.ReadLinesAsync(fl);
			var resultText = await FileIO.ReadTextAsync(fl);
			return resultText;

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
					Download(list[0].Value, list[0].Key + NewExt),
					Download(list[1].Value, list[1].Key + NewExt),
					Download(list[2].Value, list[2].Key + NewExt)
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

		public override async Task<bool> HaveUpdate(string fileStops, string fileRouts, string fileTimes, bool checkUpdate)
		{
			//return  Task.Run(async () =>
			//{
			OnLogMessage("Have update started");
			try
			{
//#if DEBUG

				await Task.WhenAll(Task.Run(async () =>
				{
					StorageFile file = await ApplicationData.Current.RoamingFolder.GetFileAsync(fileStops);
					newStops = new ObservableCollection<Stop>(ShedulerParser.ParsStops(await ReadAllFile(file)));
				}),
					Task.Run(async () =>
					{
						StorageFile file = await ApplicationData.Current.RoamingFolder.GetFileAsync(fileRouts);
						newRoutes = new ObservableCollection<Rout>(ShedulerParser.ParsRout(await ReadAllFile(file)));

					}),
					Task.Run(async () =>
					{
						StorageFile file = await ApplicationData.Current.RoamingFolder.GetFileAsync(fileTimes);
						newSchedule = new ObservableCollection<Schedule>(ShedulerParser.ParsTime(await ReadAllFile(file)));

					}));
				Debug.WriteLine("All threads ended");
				//OnLogMessage("All threads ended");
			}
			catch (FileNotFoundException e)
			{
				OnLogMessage(e.Message);
				return false;
			}
			catch (Exception e)
			{
#if BETA
				Logger.Log("HaveUpdate").WriteLineTime(e.Message).WriteLine(e.StackTrace);
#endif
				return false;
			}
			if (checkUpdate)
			{
				if (Stops == null || Routs == null || Times == null || !Stops.Any() || !Routs.Any() || !Times.Any())
					return true;

				if (newStops.Count != Stops.Count || newRoutes.Count != Routs.Count || newSchedule.Count != Times.Count)
					return true;

				foreach (var newRoute in newRoutes)
				{
					if (Routs.AsParallel().All(x => x.RoutId == newRoute.RoutId && x.Datestart != newRoute.Datestart))
						return true;
				}
			}


			OnLogMessage("don't have update true");
			return false;
			//});

		}

		private async Task Download(string uri, string file)
		{
			try
			{
				var httpClient = new HttpClient();
				// Increase the max buffer size for the response so we don't get an exception with so many web sites

				httpClient.DefaultRequestHeaders.Add("user-agent",
					"Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2; WOW64; Trident/6.0)");

				HttpResponseMessage response = await httpClient.GetAsync(new Uri(uri));
				response.EnsureSuccessStatusCode();

				//string str= response.StatusCode + " " + response.ReasonPhrase + Environment.NewLine;
				var fileGet =
					await ApplicationData.Current.RoamingFolder.CreateFileAsync(file, CreationCollisionOption.ReplaceExisting);
				using (var writeStream = await fileGet.OpenAsync(FileAccessMode.ReadWrite))
				{
					using (var outputStream = writeStream.GetOutputStreamAt(0))
					{
						var responseBodyAsText = await response.Content.WriteToStreamAsync(outputStream);
					}
				}
			}
			catch (Exception e)
			{
#if BETA
				Logger.Log().WriteLineTime("Can't download " + uri).WriteLine(e.Message).WriteLine(e.StackTrace);
#endif
				throw new TaskCanceledException(e.Message, e);
			}
		}

		protected override async Task SaveFavourite()
		{
			await SaveFavourite(ApplicationData.Current.RoamingFolder);
		}
		async  Task SaveFavourite(StorageFolder storage)
		{
			StorageFile stream = await storage.CreateFileAsync(NameFileFavourite + TempExt, CreationCollisionOption.ReplaceExisting);

			using (var writer = XmlWriter.Create(await stream.OpenStreamForWriteAsync()))
			{
				WriteXml(writer);
			}

			await stream.RenameAsync(NameFileFavourite, NameCollisionOption.ReplaceExisting);
		}

		async Task SaveStatistics(JsonSerializerSettings jsonSettings, StorageFolder storage)
		{
			string counter = JsonConvert.SerializeObject(counterViewStops, jsonSettings);
			var counterFile = await storage.CreateFileAsync(NameFileCounter + TempExt, CreationCollisionOption.ReplaceExisting);
			await FileIO.WriteTextAsync(counterFile, counter);
			counterFile.RenameAsync(NameFileCounter, NameCollisionOption.ReplaceExisting);

		}
		
		public override async Task Save()
		{

			//await IsolatedStorageOperations.Save(this, "data.dat");

			var storage = ApplicationData.Current.RoamingFolder;
			StorageFile stream = null;

			try
			{
				
				await SaveFavourite(storage);

				var jsonSettings = new JsonSerializerSettings() {ReferenceLoopHandling = ReferenceLoopHandling.Ignore};
				SaveStatistics(jsonSettings, storage);
				
				await Task.WhenAll(Task.Run(async () =>
				{
					string routs = JsonConvert.SerializeObject(Routs, jsonSettings);
					var routsFile = await storage.CreateFileAsync(NameFileRouts + TempExt, CreationCollisionOption.ReplaceExisting);
					await FileIO.WriteTextAsync(routsFile, routs);
					routsFile.RenameAsync(NameFileRouts, NameCollisionOption.ReplaceExisting);

				}),
					Task.Run(async () =>
					{
						string routs = JsonConvert.SerializeObject(ActualStops, jsonSettings);
						var stopsFile = await storage.CreateFileAsync(NameFileStops + TempExt, CreationCollisionOption.ReplaceExisting);
						await FileIO.WriteTextAsync(stopsFile, routs);
						stopsFile.RenameAsync(NameFileStops, NameCollisionOption.ReplaceExisting);

					}), Task.Run(async () =>
					{
						string routs = JsonConvert.SerializeObject(Times, jsonSettings);
						var timesFile = await storage.CreateFileAsync(NameFileTimes + TempExt, CreationCollisionOption.ReplaceExisting);
						await FileIO.WriteTextAsync(timesFile, routs);
						timesFile.RenameAsync(NameFileTimes, NameCollisionOption.ReplaceExisting);

					}));
			}
			catch (Exception e)
			{
#if BETA
				Logger.Log("Save exception").WriteLineTime(e.Message).WriteLine(e.StackTrace);
				Logger.Log().SaveToFile();
#endif
				throw;
			}
		}

		private Timer saveTimer;
		

		public override async Task Load()
		{
			Debug.WriteLine("UniversalContext.Load started");
			Debug.WriteLine("UniversalContext LoadSourceData started");
#if BETA
			Logger.Log().WriteLineTime("Load started");
#endif
			OnLoadStarted();

			saveTimer = new Timer((x) =>
			{
				var jsonSettings = new JsonSerializerSettings() {ReferenceLoopHandling = ReferenceLoopHandling.Ignore};
				var storagee = ApplicationData.Current.RoamingFolder;
				//SaveStatistics( jsonSettings, storagee);
			},null, new TimeSpan(0,0, 10,0,0), new TimeSpan(0,0,0, 30,0) );

			var storage = ApplicationData.Current.RoamingFolder;
			ObservableCollection<Rout> tpRouts = null;
			ObservableCollection<Stop> tpStops = null;
			ObservableCollection<Schedule> tpTimes = null;
			ObservableCollection<Stop> tpFavouriteStops = null;
			ObservableCollection<RoutWithDestinations> tpFavouriteRouts = null;
			ObservableCollection<GroupStop> tpGroups = null;

			try
			{
				try
				{
					var routsFile = await storage.GetFileAsync(NameFileCounter);
					var routs = await FileIO.ReadTextAsync(routsFile);
					counterViewStops = JsonConvert.DeserializeObject<Dictionary<int, uint>>(routs);
				}

				catch (FileNotFoundException e)
				{
					counterViewStops = new Dictionary<int, uint>();
				}
				await Task.WhenAll(
					Task.Run(async () =>
					{
						try
						{
							var routsFile = await storage.GetFileAsync(NameFileRouts);
							var routs = await FileIO.ReadTextAsync(routsFile);
							tpRouts = JsonConvert.DeserializeObject<ObservableCollection<Rout>>(routs);
						}

						catch (FileNotFoundException e)
						{
							throw new TaskCanceledException(e.Message, e);
						}
					}), Task.Run(async () =>
					{
						try
						{
							var stopsFile = await storage.GetFileAsync(NameFileStops);
							var stops = await FileIO.ReadTextAsync(stopsFile);
							tpStops = JsonConvert.DeserializeObject<ObservableCollection<Stop>>(stops);
						}
						catch (FileNotFoundException e)
						{
							throw new TaskCanceledException(e.Message, e);
						}
					}), Task.Run(async () =>
					{
						try
						{

							var timesFile = await storage.GetFileAsync(NameFileTimes);
							var times = await FileIO.ReadTextAsync(timesFile);

							tpTimes = JsonConvert.DeserializeObject<ObservableCollection<Schedule>>(times);
						}
						catch (FileNotFoundException e)
						{
							throw new TaskCanceledException(e.Message, e);
						}
					}));



				//await Task.WhenAll(
				//   Task.Run(async () =>
				//   {
				//	   try
				//	   {
				//		   var routsFile = await storage.GetFileAsync(NameFileRouts);
				//		   var routs = await FileIO.ReadTextAsync(routsFile);
				//		   Routs = JsonConvert.DeserializeObject<ObservableCollection<Rout>>(routs);
				//	   }
				//	   catch (FileNotFoundException e)
				//	   {
				//		   throw new TaskCanceledException(e.Message, e);
				//	   }
				//   }),
				//	Task.Run(async () =>
				//   {
				//	   try
				//	   {
				//		   var stopsFile = await storage.GetFileAsync(NameFileStops);
				//		   var stops = await FileIO.ReadTextAsync(stopsFile);
				//		   Stops =  JsonConvert.DeserializeObject<ObservableCollection<Stop>>(stops);
				//	   }
				//	   catch (FileNotFoundException e)
				//	   {
				//		   throw new TaskCanceledException(e.Message, e);
				//	   }
				//   }),
				//   Task.Run(async () =>
				//   {
				//	   try
				//	   {

				//		   var timesFile = await storage.GetFileAsync(NameFileTimes);
				//		   var times = await FileIO.ReadTextAsync(timesFile);

				//		   Times =  JsonConvert.DeserializeObject<ObservableCollection<Schedule>>(times);
				//	   }
				//	   catch (FileNotFoundException e)
				//	   {
				//		   throw new TaskCanceledException(e.Message, e);
				//	   }
				//   })
				//   );
				//await Task.Run(async () =>
				//{
				//	if (await FileExistss(NameFileFavourite))
				//	{
				//		try
				//		{

				//			var stream = await storage.OpenStreamForReadAsync(NameFileFavourite);

				//			using (var reader = XmlReader.Create(stream, new XmlReaderSettings()))
				//			{
				//				ReadXml(reader);
				//			}

				//			if (FavouriteRoutsIds != null)
				//			{
				//				tpFavouriteRouts = new ObservableCollection<RoutWithDestinations>(FavouriteRoutsIds.Select(x =>
				//					new RoutWithDestinations(Routs.First(d => d.RoutId == x), this)).ToList());
				//				FavouriteRoutsIds = null;
				//			}

				//			if (FavouriteStopsIds != null)
				//			{
				//				tpFavouriteStops = new ObservableCollection<Stop>(FavouriteStopsIds.Select(x => Stops.First(d => d.ID == x)));
				//				FavouriteStopsIds = null;
				//			}
				//			if (GroupsStopIds != null)
				//			{
				//				tpGroups = new ObservableCollection<GroupStop>(GroupsStopIds.Select(x => new GroupStop()
				//				{
				//					Name = x.Name,
				//					Stops = new ObservableCollection<Stop>(Stops.Join(x.StopID, stop => stop.ID, i => i, (stop, i) => stop))
				//				}));
				//			}
				//		}
				//		catch (FileNotFoundException e)
				//		{
				//			Debug.WriteLine("Context.Load.LoadFavourite: " + e.Message);
				//			return;
				//		}
				//		catch (Exception e)
				//		{
				//			Debug.WriteLine("Context.Load.LoadFavourite: " + e.Message);
				//			throw new Exception(e.Message, e);
				//		}
				//	}
				//	else
				//	{
				//		FavouriteRouts = new ObservableCollection<RoutWithDestinations>();
				//		FavouriteStops = new ObservableCollection<Stop>();
				//	}
				//});
			}
			catch (TaskCanceledException e)
			{
				//CleanTp();
				OnErrorLoading(new ErrorLoadingDelegateArgs() {Error = ErrorLoadingDelegateArgs.Errors.NoSourceFiles});
#if BETA
				Logger.Log("Load taskcanceledException").WriteLineTime(e.Message).WriteLine(e.StackTrace);
				Logger.Log().SaveToFile();
#endif
				return;
			}
			catch (Exception e)
			{
				Debug.WriteLine("Context.Load: " + e.Message );
#if BETA
				Logger.Log("Load exception").WriteLineTime(e.Message).WriteLine(e.StackTrace);
				Logger.Log().SaveToFile();
#endif
				throw;
			}
			
			if (tpRouts == null || tpStops == null)
			{
				//CleanTp();
				OnErrorLoading(new ErrorLoadingDelegateArgs() { Error = ErrorLoadingDelegateArgs.Errors.NoSourceFiles });
				return;
			}

			Debug.WriteLine("UniversalContext LoadSourceData ended");
			Connect(tpRouts, tpStops, tpTimes, VariantLoad);

			//lock (o)
			//{
			Routs = tpRouts;
			Stops = tpStops;
			Times = tpTimes;
			Debug.WriteLine("UniversalContext loadfavourite started");
			if (await FileExistss(NameFileFavourite))
			{
				try
				{

					var stream = await storage.OpenStreamForReadAsync(NameFileFavourite);

					using (var reader = XmlReader.Create(stream, new XmlReaderSettings()))
					{
						ReadXml(reader);
					}

					if (FavouriteRoutsIds != null)
					{
						var temp1 = FavouriteRoutsIds.Select(x =>
							new RoutWithDestinations(tpRouts.First(d => d.RoutId == x), this)).ToList();
						tpFavouriteRouts = new ObservableCollection<RoutWithDestinations>(temp1);
						FavouriteRoutsIds = null;
					}

					if (FavouriteStopsIds != null)
					{
						tpFavouriteStops = new ObservableCollection<Stop>(FavouriteStopsIds.Select(x => tpStops.First(d => d.ID == x)));
						FavouriteStopsIds = null;
					}
					if (GroupsStopIds != null)
					{
						tpGroups = new ObservableCollection<GroupStop>(GroupsStopIds.Select(x => new GroupStop()
						{
							Name = x.Name,
							Stops = new ObservableCollection<Stop>(tpStops.Join(x.StopID, stop => stop.ID, i => i, (stop, i) => stop))
						}));
					}
				}
				catch (FileNotFoundException e)
				{
					Debug.WriteLine("Context.Load.LoadFavourite: " + e.Message);
#if BETA
					Logger.Log().WriteLineTime("Load favourite filenotFound");
#endif
					return;
				}
				catch (Exception e)
				{
					Debug.WriteLine("Context.Load.LoadFavourite: " + e.Message);
#if BETA
					Logger.Log("Load favourite exception").WriteLineTime(e.Message).WriteLine(e.StackTrace);
					Logger.Log().SaveToFile();
#endif
					throw;
				}
			}
			else
			{
				tpFavouriteRouts = new ObservableCollection<RoutWithDestinations>();
				tpFavouriteStops = new ObservableCollection<Stop>();
				tpGroups = new ObservableCollection<GroupStop>();
			}
				
			//}
			FavouriteRouts = tpFavouriteRouts;
			FavouriteStops = tpFavouriteStops;
			Groups = tpGroups;
			Debug.WriteLine("UniversalContext loadfavourite ended");

			//CleanTp();
			AllPropertiesChanged();
			OnLoadEnded();
			Debug.WriteLine("UniversalContext.Load ended");
#if BETA
			Logger.Log().WriteLineTime("Load ended");
#endif
		}

		public async override Task Recover()
		{
#if BETA
			Logger.Log().WriteLineTime("Recover started");
#endif
			await FileDelete(NameFileRouts);
			await FileDelete(NameFileStops);
			await FileDelete(NameFileTimes);
#if BETA
			Logger.Log().WriteLineTime("Recover ended");
#endif
		}

		private static object o = new Object();

		#endregion
	}
}
