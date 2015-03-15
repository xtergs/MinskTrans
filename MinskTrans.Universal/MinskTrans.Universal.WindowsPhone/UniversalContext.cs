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
				var fl = await ApplicationData.Current.LocalFolder.GetFileAsync(file);
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
				var fl = await ApplicationData.Current.LocalFolder.GetFileAsync(oldFile);
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
				var fl = await ApplicationData.Current.LocalFolder.GetFileAsync(file);
				//var xx = (await FileIO.ReadBufferAsync(fl));
				//var tt =await FileIO.ReadLinesAsync(fl);
				var resultText = await FileIO.ReadTextAsync(fl);
				return resultText;
			});
		}

		protected override async Task<string> FileReadAllText(string file)
		{
			var fl = await ApplicationData.Current.LocalFolder.GetFileAsync(file);
			//var xx = (await FileIO.ReadBufferAsync(fl));
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
					Download(list[0].Value, list[0].Key + NewExt),
					Download(list[1].Value, list[1].Key + NewExt),
					Download(list[2].Value, list[2].Key + NewExt)
				});
				OnDataBaseDownloadEnded();

			}
			catch (System.Net.WebException e)
			{
				OnErrorDownloading();
			}
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
					StorageFile file = await ApplicationData.Current.LocalFolder.GetFileAsync(fileStops);
					newStops = new ObservableCollection<Stop>(ShedulerParser.ParsStops(await ReadAllFile(file)));
				}),
					Task.Run(async () =>
					{
						StorageFile file = await ApplicationData.Current.LocalFolder.GetFileAsync(fileRouts);
						newRoutes = new ObservableCollection<Rout>(ShedulerParser.ParsRout(await ReadAllFile(file)));

					}),
					Task.Run(async () =>
					{
						StorageFile file = await ApplicationData.Current.LocalFolder.GetFileAsync(fileTimes);
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
				OnLogMessage(e.Message);
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
					await ApplicationData.Current.LocalFolder.CreateFileAsync(file, CreationCollisionOption.ReplaceExisting);
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
				throw new TaskCanceledException(e.Message, e);
			}
		}

		
		public override async Task Save()
		{

			//await IsolatedStorageOperations.Save(this, "data.dat");

			var storage = ApplicationData.Current.LocalFolder;
			StorageFile stream = null;

			try
			{
				stream = await storage.CreateFileAsync(NameFileFavourite + TempExt, CreationCollisionOption.ReplaceExisting);

				using (var writer = XmlWriter.Create(await stream.OpenStreamForWriteAsync()))
				{
					WriteXml(writer);
				}

				await stream.RenameAsync(NameFileFavourite, NameCollisionOption.ReplaceExisting);

				var jsonSettings = new JsonSerializerSettings() {ReferenceLoopHandling = ReferenceLoopHandling.Ignore};

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
				throw new Exception(e.Message, e);
			}
		}

		public override async Task Load()
		{
			OnLoadStarted();
			
			var storage = ApplicationData.Current.LocalFolder;

			try
			{
				await Task.WhenAll(
					Task.Run(async () =>
					{
						try
						{
							var routsFile = await storage.GetFileAsync(NameFileRouts);
							var routs = await FileIO.ReadTextAsync(routsFile);
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
							var stopsFile = await storage.GetFileAsync(NameFileStops);
							var stops = await FileIO.ReadTextAsync(stopsFile);
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

							var timesFile = await storage.GetFileAsync(NameFileTimes);
							var times = await FileIO.ReadTextAsync(timesFile);

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
					if (await FileExistss(NameFileFavourite))
					{
						try
						{

							var stream = await storage.OpenStreamForReadAsync(NameFileFavourite);

							using (var reader = XmlReader.Create(stream, new XmlReaderSettings()))
							{
								ReadXml(reader);
							}

							FavouriteRouts = new ObservableCollection<RoutWithDestinations>(FavouriteRoutsIds.Select(x =>
								new RoutWithDestinations(Routs.First(d => d.RoutId == x), this)).ToList());
							FavouriteRoutsIds = null;

							FavouriteStops = new ObservableCollection<Stop>(FavouriteStopsIds.Select(x => Stops.First(d => d.ID == x)));
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
							throw new Exception(e.Message, e);
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
				Debug.WriteLine("Context.Load: " + e.Message );
				throw new Exception(e.Message, e);
			}
			
			if (Routs == null || Stops == null)
			{

				OnErrorLoading(new ErrorLoadingDelegateArgs() { Error = ErrorLoadingDelegateArgs.Errors.NoFileToDeserialize });
				return;
			}

			Connect(Routs, Stops, Times);
			
			AllPropertiesChanged();
			OnLoadEnded();
		}

		#endregion
	}
}
