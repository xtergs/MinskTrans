using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using MinskTrans.DesctopClient.Model;
using MinskTrans.Library;

namespace MinskTrans.DesctopClient
{
	[Serializable]
	public class ContextDesctop : Context
	{
		#region Overrides of Context

		public override void Save()
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

		protected override bool FileExists(string file)
		{
			return File.Exists(file);
		}

		protected override void FileDelete(string file)
		{
			File.Delete(file);
		}
		async public override void Create(bool AutoUpdate = true)
		{
			//TODO
			//throw new NotImplementedException();
			if (FileExists("data.dat"))
			{
				Load();
				return;
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

		public override bool HaveUpdate()
		{
			if (list.Any(keyValuePair => !FileExists(keyValuePair.Key + ".new")))
			{
				return false;
			}

			newStops = ShedulerParser.ParsStops(FileReadAllText(list[0].Key + ".new").Result);
			newRoutes = ShedulerParser.ParsRout(FileReadAllText(list[1].Key + ".new").Result);
			newSchedule = ShedulerParser.ParsTime(FileReadAllText(list[2].Key + ".new").Result);

			if (Stops == null || Routs == null || Times == null)
				return true;

			if (newStops.Count == Stops.Count && newRoutes.Count == Routs.Count && newSchedule.Count == Times.Count)
				return false;

			foreach (var newRoute in newRoutes)
			{
				if (Routs.AsParallel().All(x => x.RoutId == newRoute.RoutId && x.Datestart == newRoute.Datestart))
					return false;
			}



			return true;
		}
		protected override void FileMove(string oldFile, string newFile)
		{
			File.Move(oldFile, newFile);
		}

		async protected override Task<string> FileReadAllText(string file)
		{
			return await Task.Run(() => File.ReadAllText(file));
		}

		public override void DownloadUpdate()
		{
			//TODO
			//throw new NotImplementedException();
			OnDataBaseDownloadStarted();
			try
			{
				using (var client = new WebClient())
				{
					client.DownloadFile(list[0].Value, list[0].Key + ".new");
					client.DownloadFile(list[1].Value, list[1].Key + ".new");
					client.DownloadFile(list[2].Value, list[2].Key + ".new");
				}
				OnDataBaseDownloadEnded();

			}
			catch (System.Net.WebException e)
			{
				OnErrorDownloading();
			}
		}

		public override void Load()
		{
			//throw new NotImplementedException();
			BinaryFormatter serializer = new BinaryFormatter();
			var streamWriter = new FileStream("data.dat", FileMode.Open, FileAccess.Read);
			try
			{
				var tempDateTime = (DateTime)serializer.Deserialize(streamWriter);
				var tempStops = (ObservableCollection<Stop>)serializer.Deserialize(streamWriter);
				var tempRouts = (ObservableCollection<Rout>)serializer.Deserialize(streamWriter);
				var tempTimes = (ObservableCollection<Schedule>)serializer.Deserialize(streamWriter);
				var tempGroup = (List<GroupStopId>)serializer.Deserialize(streamWriter);
				FavouriteRouts = (ObservableCollection<Rout>)serializer.Deserialize(streamWriter);
				FavouriteStops = (ObservableCollection<Stop>)serializer.Deserialize(streamWriter);


				LastUpdateDataDateTime = tempDateTime;
				Stops = tempStops;
				Routs = tempRouts;
				Times = tempTimes;

				Connect(Routs, Stops);

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
		}

		

		#endregion
	}
}
