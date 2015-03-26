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

using MinskTrans.Universal.Model;

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

		public async override Task Save()
		{
			//throw new NotImplementedException();
			var groupsId = Groups.Select(groupStop => new GroupStopId(groupStop)).ToList();
			BinaryFormatter serializer = new BinaryFormatter();
			var streamWriter = new FileStream("data.dat", FileMode.Create, FileAccess.Write);
			try
			{
				serializer.Serialize(streamWriter, LastUpdateDataDateTime);
				serializer.Serialize(streamWriter, Stops);
				serializer.Serialize(streamWriter, Routs);
				serializer.Serialize(streamWriter, Times);
				serializer.Serialize(streamWriter, groupsId);
				serializer.Serialize(streamWriter, FavouriteRouts);
				serializer.Serialize(streamWriter, FavouriteStops);

			}
			finally
			{
				streamWriter.Close();
			}
		}

		protected override Task<bool> FileExists(string file)
		{
			return Task<bool>.Run(()=>File.Exists(file));
		}

		protected async override Task FileDelete(string file)
		{
			if (File.Exists(file))
				File.Delete(file);
		}
		async public override void Create(bool AutoUpdate = true)
		{
			//TODO
			//throw new NotImplementedException();
			if (File.Exists("data.dat"))
			{
				//Load();
				return;
			}
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
			return await Task.Run(async () =>
			{
				if (!File.Exists(fileStops) || !File.Exists(fileRouts) || !File.Exists(fileTimes))
					return false;
#if DEBUG
				Stopwatch watch = new Stopwatch();
				watch.Start();
#endif
				newStops = ShedulerParser.ParsStops(await FileReadAllText(fileStops));
				newRoutes = ShedulerParser.ParsRout(await FileReadAllText(fileRouts));
				newSchedule = ShedulerParser.ParsTime(await FileReadAllText(fileTimes));
#if DEBUG
				watch.Stop();
#endif
				if (checkUpdate)
				{
					if (Stops == null || Routs == null || Times == null)
						return true;

					if (newStops.Count == Stops.Count && newRoutes.Count == Routs.Count && newSchedule.Count == Times.Count)
						return false;

					foreach (var newRoute in newRoutes)
					{
						if (Routs.AsParallel().All(x => x.RoutId == newRoute.RoutId && x.Datestart == newRoute.Datestart))
							return false;
					}

				}

				return true;
			});
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

		public override async Task DownloadUpdate()
		{
			//TODO
			//throw new NotImplementedException();
			OnDataBaseDownloadStarted();
			try
			{
				using (var client = new WebClient())
				{
					//Task.WhenAll(
					client.DownloadFile(list[0].Value, list[0].Key + ".new");
					client.DownloadFile(list[1].Value, list[1].Key + ".new");
					client.DownloadFile(list[2].Value, list[2].Key + ".new");
					//);
				}
				OnDataBaseDownloadEnded();

			}
			catch (System.Net.WebException e)
			{
				OnErrorDownloading();
			}
		}

		public override Task Load()
		{
			//throw new NotImplementedException();
			return Task.Run(() =>
			{
				BinaryFormatter serializer = new BinaryFormatter();
				var streamWriter = new FileStream("data.dat", FileMode.Open, FileAccess.Read);
				try
				{
					var tempDateTime = (DateTime) serializer.Deserialize(streamWriter);
					var tempStops = (ObservableCollection<Stop>) serializer.Deserialize(streamWriter);
					var tempRouts = (ObservableCollection<Rout>) serializer.Deserialize(streamWriter);
					var tempTimes = (ObservableCollection<Schedule>) serializer.Deserialize(streamWriter);
					var tempGroup = (List<GroupStopId>) serializer.Deserialize(streamWriter);
					FavouriteRouts = (ObservableCollection<RoutWithDestinations>) serializer.Deserialize(streamWriter);
					FavouriteStops = (ObservableCollection<Stop>) serializer.Deserialize(streamWriter);


					LastUpdateDataDateTime = tempDateTime;
					Stops = tempStops;
					Routs = tempRouts;
					Times = tempTimes;

					tempStops = null;
					tempRouts = null;
					tempTimes = null;

					//Connect(Routs, Stops);

					Groups = new ObservableCollection<GroupStop>();
					foreach (var groupStopId in tempGroup)
					{
						var newGroupStop = new GroupStop();
						newGroupStop.Name = groupStopId.Name;
						newGroupStop.Stops = new ObservableCollection<Stop>();
						foreach (var i in groupStopId.StopID)
						{
							newGroupStop.Stops.Add(Stops.First(x => x.ID == i));
						}
					}
					//Connect(FavouriteRouts, FavouriteStops);
				}
				finally
				{
					streamWriter.Close();
				}
			});
		}

		

		#endregion
	}
}
