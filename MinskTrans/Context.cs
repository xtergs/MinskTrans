using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using MinskTrans.DesctopClient.Annotations;
using System.Threading.Tasks;
using MinskTrans.Library;
using GalaSoft.MvvmLight.CommandWpf;
using MinskTrans.DesctopClient.Model;


namespace MinskTrans.DesctopClient
{
	[Serializable]
	public class Context : INotifyPropertyChanged , IXmlSerializable, IContext
	{
		protected readonly List<KeyValuePair<string, string>> list = new List<KeyValuePair<string, string>>
		{
			new KeyValuePair<string, string>("stops.txt", @"http://www.minsktrans.by/city/minsk/stops.txt"),
			new KeyValuePair<string, string>("routes.txt", @"http://www.minsktrans.by/city/minsk/routes.txt"),
			new KeyValuePair<string, string>("times.txt", @"http://www.minsktrans.by/city/minsk/times.txt")
		};

		private ObservableCollection<Rout> routs;
		private ObservableCollection<Stop> stops;
		private ObservableCollection<Schedule> times;
		private ObservableCollection<GroupStop> groups;
		private ObservableCollection<Rout> favouriteRouts;
		private ObservableCollection<Stop> favouriteStops;
		private DateTime lastUpdateDataDateTime;

		public DateTime LastUpdateDataDateTime
		{
			get { return lastUpdateDataDateTime; }
			set
			{
				lastUpdateDataDateTime = value;
				OnPropertyChanged();
			}
		}

		public ObservableCollection<Rout> FavouriteRouts
		{
			get { return favouriteRouts; }
			set
			{
				favouriteRouts = value;
				OnPropertyChanged();
			}
		}

		public ObservableCollection<Stop> FavouriteStops
		{
			get { return favouriteStops; }
			set
			{
				favouriteStops = value;
				OnPropertyChanged();
			}
		}
		public ObservableCollection<GroupStop> Groups
		{
			get { return groups; }
			set
			{
				groups = value; 
				OnPropertyChanged();
			}
		}


		public Context()
		{
			Create();
		}


		public ObservableCollection<Schedule> Times
		{
			get { return times; }
			set
			{
				times = value;
				OnPropertyChanged();
			}
		}

		public ObservableCollection<Stop> Stops
		{
			get { return stops; }
			set
			{
				if (Equals(value, stops)) return;
				stops = value;
				OnPropertyChanged();
				OnPropertyChanged("ActualStops");
			}
		}

		public ObservableCollection<Stop> ActualStops
		{
			get { return new ObservableCollection<Stop>(Stops.Where(x => Routs.Any(d => d.Stops.Contains(x)))); }
		}

		public ObservableCollection<Rout> Routs
		{
			get { return routs; }
			set
			{
				if (Equals(value, routs))
					return;
				routs = value;
				OnPropertyChanged();
			}
		}


		public event PropertyChangedEventHandler PropertyChanged;

		public virtual void Create()
		{
			//TODO
			//throw new NotImplementedException();
			if (File.Exists("data.dat"))
			{
				Load();
				return;
			}
			FavouriteRouts = new ObservableCollection<Rout>();
			FavouriteStops = new ObservableCollection<Stop>();
			Groups = new ObservableCollection<GroupStop>();
			DownloadUpdate();
			HaveUpdate();
			ApplyUpdate();
		}

		public virtual void DownloadUpdate()
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
				OnLogMessage("Error donwloading");
			}
		}

		private List<Rout> newRoutes;
		private List<Stop> newStops;
		private List<Schedule> newSchedule; 

		public bool HaveUpdate()
		{
			if (list.Any(keyValuePair => !File.Exists(keyValuePair.Key + ".new")))
			{
				return false;
			}

			newStops = ShedulerParser.ParsStops(File.ReadAllText(list[0].Key + ".new"));
			newRoutes = ShedulerParser.ParsRout(File.ReadAllText(list[1].Key + ".new"));
			newSchedule = ShedulerParser.ParsTime(File.ReadAllText(list[2].Key + ".new"));

			if (Stops == null || Routs == null || Times == null)
				return true;

			if (newStops.Count == Stops.Count && newRoutes.Count == Routs.Count && newSchedule.Count == Times.Count)
				return false;

			foreach (var newRoute in newRoutes)
			{
				if (Routs.AsParallel().All(x=>x.RoutId == newRoute.RoutId && x.Datestart == newRoute.Datestart))
					return false;
			}

			

			return true;
		}

		public void ApplyUpdate()
		{
			OnApplyUpdateStarted();

			foreach (var keyValuePair in list)
			{
				if (File.Exists(keyValuePair.Key + ".old"))
					File.Delete(keyValuePair.Key + ".old");
				if (File.Exists(keyValuePair.Key))
					File.Move(keyValuePair.Key, keyValuePair.Key + ".old");
				File.Move(keyValuePair.Key + ".new", keyValuePair.Key);
			}

			Stops = new ObservableCollection<Stop>(newStops);
			Routs = new ObservableCollection<Rout>(newRoutes);
			Times = new ObservableCollection<Schedule>(newSchedule);

			foreach (Rout rout in Routs)
			{
				rout.Time = Times.FirstOrDefault(x => x.RoutId == rout.RoutId);
				if (rout.Time != null)
					rout.Time.Rout = rout;

				rout.Stops = new List<Stop>();
				foreach (int st in rout.RouteStops)
				{
					rout.Stops.Add(Stops.First(x => x.ID == st));
				}
			}

			LastUpdateDataDateTime = DateTime.UtcNow;

			newStops = null;
			newRoutes = null;
			newSchedule = null;

			OnApplyUpdateEnded();
		}

		/// <summary>
		/// Save to data.dat
		/// </summary>
		public virtual void Save()
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

		public virtual void Load()
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
						newGroupStop.Stops.Add(Stops.First(x=>x.ID == i));
					}
				}
				//Connect(FavouriteRouts, FavouriteStops);
			}
			finally
			{
				streamWriter.Close();
			}
		}

		void Connect(IEnumerable<Rout> routs, IEnumerable<Stop> stops)
		{
			foreach (Rout rout in routs)
			{
				rout.Time = Times.FirstOrDefault(x => x.RoutId == rout.RoutId);
				if (rout.Time != null)
					rout.Time.Rout = rout;

				rout.Stops = new List<Stop>();
				foreach (int st in rout.RouteStops)
				{
					rout.Stops.Add(stops.First(x => x.ID == st));
				}
			}
		}

		public virtual Task UpdateAsync()
		{
			//TODO
			//throw new NotImplementedException();
			return Task.Run(() =>
			{
				DownloadUpdate();
				if (HaveUpdate())
					ApplyUpdate();
			});
		}

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChangedEventHandler handler = PropertyChanged;
			if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
		}

		#region Event
		public delegate void EmptyDelegate(object sender, EventArgs args);

		public event EmptyDelegate DataBaseDownloadStarted;
		public event EmptyDelegate DataBaseDownloadEnded;
		public event EmptyDelegate ApplyUpdateStarted;
		public event EmptyDelegate ApplyUpdateEnded;
		public event LogDelegate LogMessage;


		#region Invokators
		protected virtual void OnDataBaseDownloadStarted()
		{
			var handler = DataBaseDownloadStarted;
			if (handler != null) handler(this, EventArgs.Empty);
		}
		protected virtual void OnDataBaseDownloadEnded()
		{
			var handler = DataBaseDownloadEnded;
			if (handler != null) handler(this, EventArgs.Empty);
		}
		protected virtual void OnApplyUpdateStarted()
		{
			var handler = ApplyUpdateStarted;
			if (handler != null) handler(this, EventArgs.Empty);
		}
		protected virtual void OnApplyUpdateEnded()
		{
			var handler = ApplyUpdateEnded;
			if (handler != null) handler(this, EventArgs.Empty);
		}
		protected virtual void OnLogMessage(string message)
		{
			var handler = LogMessage;
			if (handler != null) handler(this, new LogDelegateArgs(){Message = message});
		}
		#endregion

		#endregion

		#region Implementation of IXmlSerializable

		/// <summary>
		/// This method is reserved and should not be used. When implementing the IXmlSerializable interface, you should return null (Nothing in Visual Basic) from this method, and instead, if specifying a custom schema is required, apply the <see cref="T:System.Xml.Serialization.XmlSchemaProviderAttribute"/> to the class.
		/// </summary>
		/// <returns>
		/// An <see cref="T:System.Xml.Schema.XmlSchema"/> that describes the XML representation of the object that is produced by the <see cref="M:System.Xml.Serialization.IXmlSerializable.WriteXml(System.Xml.XmlWriter)"/> method and consumed by the <see cref="M:System.Xml.Serialization.IXmlSerializable.ReadXml(System.Xml.XmlReader)"/> method.
		/// </returns>
		public XmlSchema GetSchema()
		{
			return null;
		}

		/// <summary>
		/// Generates an object from its XML representation.
		/// </summary>
		/// <param name="reader">The <see cref="T:System.Xml.XmlReader"/> stream from which the object is deserialized. </param>
		public void ReadXml(XmlReader reader)
		{
			//reader.MoveToAttribute("LastUpdateTime");
			//LastUpdateDataDateTime = reader.ReadContentAsDateTime();
		}

		/// <summary>
		/// Converts an object into its XML representation.
		/// </summary>
		/// <param name="writer">The <see cref="T:System.Xml.XmlWriter"/> stream to which the object is serialized. </param>
		public void WriteXml(XmlWriter writer)
		{
			writer.WriteAttributeString("LastUpdateTime", LastUpdateDataDateTime.ToString());
			writer.WriteStartAttribute("Routs");
			foreach (var rout in Routs)
			{
				rout.WriteXml(writer);
			}
			writer.WriteEndAttribute();
		}

		#endregion

		#region commands

		private bool updating = false;
		
		public RelayCommand UpdateDataCommand
		{
			get
			{
				return new RelayCommand(async () =>
				{
					updating = true;
					await UpdateAsync();
					updating = false;
					UpdateDataCommand.RaiseCanExecuteChanged();
				}, ()=>!updating);
			}
		}

		public RelayCommand<Rout> AddFavouriteRoutCommand
		{
			get { return new RelayCommand<Rout>(x=>FavouriteRouts.Add(x), p=> p!= null && !FavouriteRouts.Contains(p));}
		}

		public RelayCommand<Stop> AddFavouriteSopCommand
		{
			get { return new RelayCommand<Stop>(x => FavouriteStops.Add(x), p => p != null && !FavouriteStops.Contains(p)); }
		}
		public RelayCommand<Rout> RemoveFavouriteRoutCommand
		{
			get { return new RelayCommand<Rout>(x => FavouriteRouts.Remove(x), p => p != null && FavouriteRouts.Contains(p)); }
		}

		public RelayCommand<Stop> RemoveFavouriteSopCommand
		{
			get { return new RelayCommand<Stop>(x => FavouriteStops.Remove(x), p => p != null && FavouriteStops.Contains(p)); }
		}
		#endregion

		
	}

	public delegate void LogDelegate(object sender, LogDelegateArgs args);
}