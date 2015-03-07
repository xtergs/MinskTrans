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

		
		public override async Task Save()
		{

			//await IsolatedStorageOperations.Save(this, "data.dat");

			var storage = ApplicationData.Current.LocalFolder;
			StorageFile stream = null;

			try
			{
				stream = await storage.CreateFileAsync("data.dat" + ".temp", CreationCollisionOption.ReplaceExisting);

				using (var writer = XmlWriter.Create(await stream.OpenStreamForWriteAsync()))
				{
					WriteXml(writer);
				}

				await stream.RenameAsync("data.dat", NameCollisionOption.ReplaceExisting);
			}
			catch (Exception e)
			{
				throw new Exception(e.Message, e.InnerException);
			}
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

			await HaveUpdate(list[0].Key, list[1].Key, list[2].Key, false);
			await ApplyUpdate();
			//string str =await ReadAllFile(await ApplicationData.Current.LocalFolder.GetFileAsync("data.dat"));
			string nameFile = "data.dat";
			if (await FileExistss(nameFile))
			{

				if (Routs == null || Stops == null)
				{

					OnErrorLoading(new ErrorLoadingDelegateArgs() {Error = ErrorLoadingDelegateArgs.Errors.NoFileToDeserialize});
					return;
				}
				var storage = ApplicationData.Current.LocalFolder;
				

				//if (storage.FileExists(file))
						var stream = await storage.OpenStreamForReadAsync(nameFile);
				//var type = GetType();
				//		XmlSerializer serializer = new XmlSerializer(GetType());
				//		var abj = (Context)serializer.Deserialize(stream);
				using (var reader = XmlReader.Create(stream, new XmlReaderSettings()))
				{
					ReadXml(reader);
				}
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
				
				
					FavouriteRouts = new ObservableCollection<RoutWithDestinations>(FavouriteRoutsIds.Select(x => 
					new RoutWithDestinations(Routs.First(d => d.RoutId == x), this)));
					FavouriteRoutsIds = null;

					FavouriteStops = new ObservableCollection<Stop>(FavouriteStopsIds.Select(x=> Stops.First(d=>d.ID == x)));

				//abj.FavouriteStops = new ObservableCollection<Stop>();
				//foreach (var favouriteStopsId in abj.FavouriteStopsIds)
				//{
				//	abj.FavouriteStops.Add(Stops.First(x=>x.ID == favouriteStopsId));
				//}
				//abj.Groups = new ObservableCollection<GroupStop>();
				//foreach (var groupsStopId in abj.GroupsStopIds)
				//{
				//	var group = new GroupStop();
				//	group.Name = groupsStopId.Name;
				//	group.Stops = new ObservableCollection<Stop>();
				//	foreach (var i in groupsStopId.StopID)
				//	{
				//		group.Stops.Add(Stops.First(x=>x.ID == i));
				//	}
				//	abj.Groups.Add(group);
				//}
				//	FavouriteRouts = abj.FavouriteRouts;
				//	FavouriteStops = abj.FavouriteStops;
				//Groups = abj.Groups;
				//abj.FavouriteRoutsIds = null;
				//abj.FavouriteStopsIds = null;
				//abj.GroupsStopIds = null;
				//abj = null;



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
