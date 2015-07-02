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
		enum TypeSaveData
		{
			DB,
			Favourite,
			Statisticks
		}
		Dictionary<TypeSaveData, IStorageFolder> folders = new Dictionary<TypeSaveData, IStorageFolder>()
		 {{TypeSaveData.DB, ApplicationData.Current.LocalFolder},
			 {TypeSaveData.Favourite, ApplicationData.Current.RoamingFolder},
			 {TypeSaveData.Statisticks, ApplicationData.Current.RoamingFolder}
			
		}; 

		#region Overrides of Context

		async Task<bool> FileExistss(IStorageFolder folder, string file)
		{
			try
			{
				var fl = await folder.GetFileAsync(file);
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

		protected async Task<bool> FileExistss(string file)
		{
			return await FileExistss(ApplicationData.Current.RoamingFolder, file);
		}

		
		ApplicationSettingsHelper lastUpdateDataDateTime;
        public override DateTime LastUpdateDataDateTime
		{
#if WINDOWS_PHONE_APP
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
			Connect(newRoutes, newStops, newSchedule, VariantLoad);

			newStops = new ObservableCollection<Stop>(newStops.Where(stop => stop.Routs.Any()));
			newRoutes = new ObservableCollection<Rout>(newRoutes.Where(rout => rout.Stops.Any()));
			if (checkUpdate)
			{
				return NeedUpdate();
			}


			OnLogMessage("don't have update true");
			return false;
			//});

		}
		
		

		protected override async Task SaveFavourite()
		{
			await SaveFavourite(ApplicationData.Current.RoamingFolder);
		}
		async  Task SaveFavourite(IStorageFolder storage)
		{
			StorageFile stream = await storage.CreateFileAsync(NameFileFavourite + TempExt, CreationCollisionOption.ReplaceExisting);

			//using (var writer = XmlWriter.Create(await stream.OpenStreamForWriteAsync()))
			//{
			//	WriteXml(writer);
			//}
			//await stream.RenameAsync(NameFileFavourite, NameCollisionOption.ReplaceExisting);

			var favouriteString = JsonConvert.SerializeObject(new
			{
				Routs = FavouriteRouts.Select(x => x.Rout.RoutId).ToList(),
				Stops = FavouriteStops.Select(x => x.ID).ToList(),
				Groups = Groups.Select(x => new
				{
					Name = x.Name,
					IDs = x.Stops.Select(stop => stop.ID)
				})
			});
			await FileIO.WriteTextAsync(stream, favouriteString);
			await stream.MoveAsync(storage, NameFileFavourite, NameCollisionOption.ReplaceExisting);
		}

		async Task SaveStatistics(JsonSerializerSettings jsonSettings, IStorageFolder storage)
		{
			string counter = JsonConvert.SerializeObject(counterViewStops, jsonSettings);
			var counterFile = await storage.CreateFileAsync(NameFileCounter + TempExt, CreationCollisionOption.ReplaceExisting);
			await FileIO.WriteTextAsync(counterFile, counter);
			await counterFile.RenameAsync(NameFileCounter, NameCollisionOption.ReplaceExisting);

		}
		
		public override async Task Save(bool saveAllDb = true)
		{

			//await IsolatedStorageOperations.Save(this, "data.dat");

			//var storage = ApplicationData.Current.RoamingFolder;
			//StorageFile stream = null;

			try
			{

				await SaveFavourite(folders[TypeSaveData.Favourite]);

				var jsonSettings = new JsonSerializerSettings() {ReferenceLoopHandling = ReferenceLoopHandling.Ignore};
				
				await SaveStatistics(jsonSettings, folders[TypeSaveData.Statisticks]);
				if (saveAllDb)
					await Task.WhenAll(Task.Run(async () =>
					{
						string routs = JsonConvert.SerializeObject(Routs, jsonSettings);
						var routsFile = await folders[TypeSaveData.DB].CreateFileAsync(NameFileRouts + TempExt, CreationCollisionOption.ReplaceExisting);
						await FileIO.WriteTextAsync(routsFile, routs);
						await routsFile.RenameAsync(NameFileRouts, NameCollisionOption.ReplaceExisting);

					}),
						Task.Run(async () =>
						{
							string stopsString = JsonConvert.SerializeObject(ActualStops, jsonSettings);
							var stopsFile = await folders[TypeSaveData.DB].CreateFileAsync(NameFileStops + TempExt, CreationCollisionOption.ReplaceExisting);
							await FileIO.WriteTextAsync(stopsFile, stopsString);
							await stopsFile.RenameAsync(NameFileStops, NameCollisionOption.ReplaceExisting);

						}), Task.Run(async () =>
						{
							string routs = JsonConvert.SerializeObject(Times, jsonSettings);
							var timesFile = await folders[TypeSaveData.DB].CreateFileAsync(NameFileTimes + TempExt, CreationCollisionOption.ReplaceExisting);
							await FileIO.WriteTextAsync(timesFile, routs);
							await timesFile.RenameAsync(NameFileTimes, NameCollisionOption.ReplaceExisting);

						}));
			}
			catch (Exception e)
			{
				Debug.WriteLine("Exception in UniversalContext.Save");
#if BETA
				Logger.Log("Save exception").WriteLineTime(e.Message).WriteLine(e.StackTrace);
				Logger.Log().SaveToFile();
#endif
				throw;
			}
		}

		private Timer saveTimer;

		public override async Task Load(LoadType type=LoadType.LoadAll)
		{
			Debug.WriteLine("UniversalContext.Load started");
			Debug.WriteLine("UniversalContext LoadSourceData started");
#if BETA
			Logger.Log().WriteLineTime("Load started");
#endif
			OnLoadStarted();

			//await Task.Delay(new TimeSpan(0, 0, 0, 10));

			ObservableCollection<Rout> tpRouts = null;
			ObservableCollection<Stop> tpStops = null;
			ObservableCollection<Schedule> tpTimes = null;
			ObservableCollection<Stop> tpFavouriteStops = null;
			ObservableCollection<RoutWithDestinations> tpFavouriteRouts = null;
			ObservableCollection<GroupStop> tpGroups = null;

			try
			{
				if (type.HasFlag(LoadType.LoadFavourite))
					try
					{
						//await Task.Delay(new TimeSpan(0, 0, 0, 10));

						var routsFile = await folders[TypeSaveData.Statisticks].GetFileAsync(NameFileCounter);
						var routs = await FileIO.ReadTextAsync(routsFile);
						counterViewStops = JsonConvert.DeserializeObject<Dictionary<int, uint>>(routs);
					}

					catch (FileNotFoundException e)
					{
						counterViewStops = new Dictionary<int, uint>();
					}
				//await Task.Delay(new TimeSpan(0, 0, 0, 10));
				if (type.HasFlag(LoadType.LoadDB))
					await Task.WhenAll(
						Task.Run(async () =>
						{
							try
							{
								var routsFile = await folders[TypeSaveData.DB].GetFileAsync(NameFileRouts);
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
								StorageFile stopsFile = await folders[TypeSaveData.DB].GetFileAsync(NameFileStops);
								string stops = await FileIO.ReadTextAsync(stopsFile);
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

								var timesFile = await folders[TypeSaveData.DB].GetFileAsync(NameFileTimes);
								var times = await FileIO.ReadTextAsync(timesFile);

								tpTimes = JsonConvert.DeserializeObject<ObservableCollection<Schedule>>(times);
							}
							catch (FileNotFoundException e)
							{
								throw new TaskCanceledException(e.Message, e);
							}
						}));
				else
				{
					tpRouts = Routs;
					tpStops = Stops;
					tpTimes = Times;
				}
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

			//await Task.Delay(new TimeSpan(0, 0, 0, 10));
			Debug.WriteLine("UniversalContext LoadSourceData ended");
			if (type.HasFlag(LoadType.LoadDB))
			{
				//await Task.Delay(new TimeSpan(0, 0, 0, 10));
				Connect(tpRouts, tpStops, tpTimes, VariantLoad);

				//lock (o)
				//{
				Routs = tpRouts;
				Stops = tpStops;
				Times = tpTimes;
			}
			//await Task.Delay(new TimeSpan(0, 0, 0, 10));
			Debug.WriteLine("UniversalContext loadfavourite started");
			if (type.HasFlag(LoadType.LoadFavourite) && await FileExistss(folders[TypeSaveData.Favourite], NameFileFavourite))
			{
				try
				{

					var stream = await folders[TypeSaveData.Favourite].GetFileAsync(NameFileFavourite);

					var textFavourite = await FileIO.ReadTextAsync(stream);
					try
					{
						var desFavourite = JsonConvert.DeserializeAnonymousType(textFavourite, new
						{
							Routs = FavouriteRouts.Select(x => x.Rout.RoutId).ToList(),
							Stops = FavouriteStops.Select(x => x.ID).ToList(),
							Groups = Groups.Select(x => new
							{
								Name = x.Name,
								IDs = x.Stops.Select(stop => stop.ID)
							})
						});




						//using (var reader = XmlReader.Create(stream, new XmlReaderSettings()))
						//{
						//	ReadXml(reader);
						//}

						if (desFavourite.Routs != null)
						{
							var temp1 = desFavourite.Routs.Select(x =>
								new RoutWithDestinations(tpRouts.First(d => d.RoutId == x), this)).ToList();
							tpFavouriteRouts = new ObservableCollection<RoutWithDestinations>(temp1);
							//desFavourite.Routs = null;
						}

						if (desFavourite.Stops != null)
						{
							tpFavouriteStops = new ObservableCollection<Stop>(desFavourite.Stops.Select(x => tpStops.First(d => d.ID == x)));
							//FavouriteStopsIds = null;
						}
						if (desFavourite.Groups != null)
						{
							tpGroups = new ObservableCollection<GroupStop>(desFavourite.Groups.Select(x => new GroupStop()
							{
								Name = x.Name,
								Stops = new ObservableCollection<Stop>(tpStops.Join(x.IDs, stop => stop.ID, i => i, (stop, i) => stop))
							}));
						}
					}
					catch (JsonReaderException ex)
					{

							ReadXml(textFavourite);
						//using (var reader = XmlReader.Create(textFavourite, new XmlReaderSettings()))
						//{
						//}

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
							GroupsStopIds = null;
						}
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
			try
			{
				FavouriteRouts = tpFavouriteRouts;
				FavouriteStops = tpFavouriteStops;
				Groups = tpGroups;
				Debug.WriteLine("UniversalContext loadfavourite ended");

				//CleanTp();
				//AllPropertiesChanged();
				//await Task.Delay(new TimeSpan(0, 0, 0, 10));
				OnLoadEnded();
			}
			catch (Exception e)
			{
				Debug.WriteLine("Load exception, set favourite " + e.ToString() );
#if BETA
			Logger.Log().WriteLineTime("Load exception");
#endif
				throw;
			}
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
