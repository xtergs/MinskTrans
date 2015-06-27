using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

using System.Xml;
using System.Xml.Serialization;
using MinskTrans.DesctopClient.Model;
using MyLibrary;
using Newtonsoft.Json;

namespace MinskTrans.DesctopClient
{
	[Serializable]
	public class ContextDesctop : Context
	{

		public void SaveXml()
		{


			XmlSerializer serializer = new XmlSerializer(GetType());
			using (XmlTextWriter writer = new XmlTextWriter("data.xml", Encoding.UTF8))
			{
				serializer.Serialize(writer, this);
			}
		}

		public void ReadXml()
		{
			XmlSerializer serializer = new XmlSerializer(GetType());
			using (var reader = new XmlTextReader("data.xml"))
			{
				Context obj = (Context) serializer.Deserialize(reader);
				Inicialize(obj);
			}
		}
		#region Overrides of Context

		protected override Task SaveFavourite()
		{
			File.WriteAllText(NameFileCounter, counterViewStops.ToString());
			return new Task(null);
		}

		void SafeMoveFile(string fileName, string newFile)
		{
			File.Delete(newFile + OldExt);
			if (File.Exists(newFile))
				File.Move(newFile, newFile + OldExt);
			File.Move(fileName, newFile);
		}

		public async override Task Save(bool saveAllDb = true)
		{
			try
			{
				using (var stream = File.Open(NameFileFavourite + TempExt, FileMode.Create))
				{
					using (var writer = XmlWriter.Create(stream))
					{
						WriteXml(writer);
						writer.Close();
					}
				}
				
				SafeMoveFile(NameFileFavourite + TempExt, NameFileFavourite);

				var jsonSettings = new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore };

				await Task.WhenAll(Task.Run(async () =>
				{
					string routs = JsonConvert.SerializeObject(Routs, jsonSettings);
					File.WriteAllText(NameFileRouts + TempExt, routs);
					SafeMoveFile(NameFileRouts + TempExt, NameFileRouts);
				}),
					Task.Run(async () =>
					{
						string routs = JsonConvert.SerializeObject(ActualStops, jsonSettings);
						File.WriteAllText(NameFileStops + TempExt, routs);
						SafeMoveFile(NameFileStops + TempExt, NameFileStops);

					}), Task.Run(async () =>
					{
						string routs = JsonConvert.SerializeObject(Times, jsonSettings);
						File.WriteAllText(NameFileTimes + TempExt, routs);
						SafeMoveFile(NameFileTimes + TempExt, NameFileTimes);
					}));
			}
			catch (Exception e)
			{
				throw;
			}
		}

		protected override Task<bool> FileExists(string file)
		{
			return Task<bool>.Run(()=>File.Exists(file));
		}

		protected async override Task FileDelete(string file)
		{
			//if (File.Exists(file))
				File.Delete(file);
		}
		async public override void Create(bool AutoUpdate = true)
		{
			//TODO
			//throw new NotImplementedException();
			//if (File.Exists(NameFileRouts) && File.Exists(NameFileStops) && File.Exists(NameFileTimes))
			//{
			//	await Load();
			//	return;
			//}
			FavouriteRouts = new ObservableCollection<RoutWithDestinations>();
			FavouriteStops = new ObservableCollection<Stop>();
			Groups = new ObservableCollection<GroupStop>();
			//if (AutoUpdate)
			//	await UpdateAsync();
			//DownloadUpdate();
			//HaveUpdate();
			//ApplyUpdate();
		}

		async public override Task<bool> HaveUpdate(string fileStops, string fileRouts, string fileTimes, bool checkUpdate)
		{
			OnLogMessage("Have update started");
			try
			{
				//#if DEBUG

				await Task.WhenAll(Task.Run(async () =>
				{
					newStops = new ObservableCollection<Stop>(ShedulerParser.ParsStops(await FileReadAllText(fileStops)));
				}),
					Task.Run(async () =>
					{
						newRoutes = new ObservableCollection<Rout>(ShedulerParser.ParsRout(await FileReadAllText(fileRouts)));

					}),
					Task.Run(async () =>
					{
						newSchedule = new ObservableCollection<Schedule>(ShedulerParser.ParsTime(await FileReadAllText(fileTimes)));

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
				OnLogMessage(e.Message);
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
		}

		

		protected async override Task FileMove(string oldFile, string newFile)
		{
			if (File.Exists(oldFile) && !File.Exists(newFile))
				File.Move(oldFile, newFile);
		}

		async protected override Task<string> FileReadAllText(string file)
		{
			return await Task.Run(() => File.ReadAllText(file));
		}

		public override async Task<bool> DownloadUpdate()
		{
			//TODO
			//throw new NotImplementedException();
			OnDataBaseDownloadStarted();
			try
			{
				using (var client = new WebClient())
				{
					//Task.WhenAll(
					client.DownloadFile(list[0].Value, list[0].Key + NewExt);
					client.DownloadFile(list[1].Value, list[1].Key + NewExt);
					client.DownloadFile(list[2].Value, list[2].Key + NewExt);
					//);
				}
				OnDataBaseDownloadEnded();

			}
			catch (System.Net.WebException e)
			{
				OnErrorDownloading();
				return false;
			}
			return true;
		}

		public override async Task Load(LoadType type = LoadType.LoadAll)
		{
			OnLoadStarted();

			try
			{
				await Task.WhenAll(
					Task.Run(async () =>
					{
						try
						{
							var routs = File.ReadAllText(NameFileRouts);
							Routs = JsonConvert.DeserializeObject<ObservableCollection<Rout>>(routs);
						}
						catch (FileNotFoundException e)
						{
							throw new TaskCanceledException(e.Message, e);
						}
					}),
					Task.Run(async () =>
					{
						try
						{
							var stops = File.ReadAllText(NameFileStops);
							Stops = JsonConvert.DeserializeObject<ObservableCollection<Stop>>(stops);
						}
						catch (FileNotFoundException e)
						{
							throw new TaskCanceledException(e.Message, e);
						}
					}),
					Task.Run(async () =>
					{
						try
						{

							//var timesFile = await storage.GetFileAsync(NameFileTimes);
							var times = File.ReadAllText(NameFileTimes);

							Times = JsonConvert.DeserializeObject<ObservableCollection<Schedule>>(times);
						}
						catch (FileNotFoundException e)
						{
							throw new TaskCanceledException(e.Message, e);
						}
					})
					);
				await Task.Run(async () =>
				{
					if (await FileExists(NameFileFavourite))
					{
						try
						{

							//var stream = await storage.OpenStreamForReadAsync(NameFileFavourite);
							var stream = File.Open(NameFileFavourite, FileMode.OpenOrCreate);
							using (var reader = XmlReader.Create(stream, new XmlReaderSettings()))
							{
								ReadXml(reader);
							}

							if (FavouriteRoutsIds != null)
								FavouriteRouts = new ObservableCollection<RoutWithDestinations>(FavouriteRoutsIds.Select(x =>
									new RoutWithDestinations(Routs.First(d => d.RoutId == x), this)).ToList());
							else 
								FavouriteRouts = new ObservableCollection<RoutWithDestinations>();
							FavouriteRoutsIds = null;

							if (FavouriteStopsIds != null)
								FavouriteStops = new ObservableCollection<Stop>(FavouriteStopsIds.Select(x => Stops.First(d => d.ID == x)));
							FavouriteStops = new ObservableCollection<Stop>();
							FavouriteStopsIds = null;

							Groups = new ObservableCollection<GroupStop>(GroupsStopIds.Select(x => new GroupStop()
							{
								Name = x.Name,
								Stops = new ObservableCollection<Stop>(Stops.Join(x.StopID, stop => stop.ID, i => i, (stop, i) => stop))
							}));
						}
						catch (FileNotFoundException e)
						{
							Debug.WriteLine("Context.Load.LoadFavourite: " + e.Message);
							return;
						}
						catch (Exception e)
						{
							Debug.WriteLine("Context.Load.LoadFavourite: " + e.Message);
							throw;
						}
					}
					else
					{
						FavouriteRouts = new ObservableCollection<RoutWithDestinations>();
						FavouriteStops = new ObservableCollection<Stop>();
					}
				});
			}
			catch (TaskCanceledException e)
			{
				Routs = null;
				Stops = null;
				Times = null;
				OnErrorLoading(new ErrorLoadingDelegateArgs() {Error = ErrorLoadingDelegateArgs.Errors.NoSourceFiles});
				return;
			}
			catch (Exception e)
			{
				Debug.WriteLine("Context.Load: " + e.Message);
				throw;
			}

			if (Routs == null || Stops == null)
			{

				OnErrorLoading(new ErrorLoadingDelegateArgs() {Error = ErrorLoadingDelegateArgs.Errors.NoFileToDeserialize});
				return;
			}

			Connect(Routs, Stops, Times, VariantLoad);

			OnLoadEnded();
			AllPropertiesChanged();
		}

		public override Task Recover()
		{
			throw new NotImplementedException();
		}

		#endregion
	}
}
