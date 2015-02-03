using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.Threading.Tasks;

#if !WINDOWS_PHONE_APP && !WINDOWS_AP
using System.Runtime.Serialization.Formatters.Binary;
using MinskTrans.DesctopClient.Annotations;
using GalaSoft.MvvmLight.CommandWpf;
#else
using GalaSoft.MvvmLight.Command;
using MinskTrans.Universal.Annotations;
#endif
using MinskTrans.DesctopClient.Model;
using MinskTrans.Universal.Model;


namespace MinskTrans.DesctopClient
{
#if !WINDOWS_PHONE_APP && !WINDOWS_APP
	[Serializable]
#endif
	public abstract class Context : INotifyPropertyChanged , IXmlSerializable, IContext
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
		private ObservableCollection<RoutWithDestinations> favouriteRouts;
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

		public ObservableCollection<RoutWithDestinations> FavouriteRouts
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

		public void Inicialize(Context cont)
		{
			Routs = cont.Routs;
			Stops = cont.Stops;
			Times = cont.Times;
			FavouriteRouts = cont.FavouriteRouts;
			FavouriteStops = cont.FavouriteStops;
			LastUpdateDataDateTime = cont.LastUpdateDataDateTime;
			Groups = cont.Groups;
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
				//ActualStops = new ObservableCollection<Stop>(value.AsParallel().Where(x => Routs != null && Routs.AsParallel().Any(d => d.Stops.Contains(x))));
				OnPropertyChanged();
				OnPropertyChanged("ActualStops");
			}
		}

		public ObservableCollection<Stop> ActualStops
		{
			get {
				if (Stops != null)
					return new ObservableCollection<Stop>(Stops.AsParallel().Where(x => Routs != null && Routs.AsParallel().Any(d => d.Stops.Contains(x))));
				return null;
			}
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

		public bool RoutsHaveStopId(int stopId)
		{
			return Routs.AsParallel().Any(x => x.RouteStops.Contains(stopId));
		}


		public event PropertyChangedEventHandler PropertyChanged;

		public  abstract void Create(bool AutoUpdate = true);

		
		protected abstract bool FileExists(string file);

		protected abstract void FileDelete(string file);

		protected abstract void FileMove(string oldFile, string newFile);

		protected abstract Task<string> FileReadAllText(string file);

		public abstract void DownloadUpdate();

		protected List<Rout> newRoutes;
		protected List<Stop> newStops;
		protected List<Schedule> newSchedule;

		public abstract Task<bool> HaveUpdate();

		public void ApplyUpdate()
		{
			OnApplyUpdateStarted();
			try
			{
				foreach (var keyValuePair in list)
				{
					//if (FileExists(keyValuePair.Key + ".old"))
						FileDelete(keyValuePair.Key + ".old");
					//if (FileExists(keyValuePair.Key))
						FileMove(keyValuePair.Key, keyValuePair.Key + ".old");
					FileMove(keyValuePair.Key + ".new", keyValuePair.Key);
				}

				Stops = new ObservableCollection<Stop>(newStops);
				Routs = new ObservableCollection<Rout>(newRoutes);
				Times = new ObservableCollection<Schedule>(newSchedule);

				Connect(Routs, Stops);

				LastUpdateDataDateTime = DateTime.UtcNow;

				newStops = null;
				newRoutes = null;
				newSchedule = null;
			}
			catch (Exception e)
			{
				OnLogMessage("Apply update: " + e.Message);
				throw new Exception(e.Message, e.InnerException);
			}

			AllPropertiesChanged();

			OnApplyUpdateEnded();
		}

		public abstract void Save();

		public abstract void Load();

		public void AllPropertiesChanged()
		{
			OnPropertyChanged("Stops");
			OnPropertyChanged("Routs");
			OnPropertyChanged("Times");
			OnPropertyChanged("FavouriteRouts");
			OnPropertyChanged("FavouriteStops");
			OnPropertyChanged("Groups");
		}

		
		protected void Connect(IEnumerable<Rout> routs, IEnumerable<Stop> stops)
		{
#if DEBUG
			Stopwatch watch = new Stopwatch();
			watch.Start();
#endif
			Parallel.ForEach(routs, rout =>
			{
				rout.Time = Times.FirstOrDefault(x => x.RoutId == rout.RoutId);
				if (rout.Time != null)
					rout.Time.Rout = rout;

				rout.Stops = new List<Stop>();
				foreach (int st in rout.RouteStops)
				{
					var stop = (stops.First(x => x.ID == st));
					rout.Stops.Add(stop);
					stop.Routs.Add(rout);
				}
			});
#if DEBUG
			watch.Stop();
#endif
		}

		async public void Update()
		{
			DownloadUpdate();
			if (await HaveUpdate())
				ApplyUpdate();
		}

		public virtual Task UpdateAsync()
		{
			//TODO
			//throw new NotImplementedException();
			return Task.Run(async () =>
			{
				DownloadUpdate();
				if (await HaveUpdate())
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
		public event EmptyDelegate ErrorDownloading;
		public event EmptyDelegate UpdateStarted;
		public event EmptyDelegate UpdateEnded;

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
		protected virtual void OnErrorDownloading()
		{
			var handler = ErrorDownloading;
			if (handler != null) handler(this, EventArgs.Empty);
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
			reader.ReadStartElement("ContextDesctop");
			LastUpdateDataDateTime = Convert.ToDateTime(reader.GetAttribute("LastUpdateTime"));
			Routs = new ObservableCollection<Rout>();
			int count = Convert.ToInt32(reader.GetAttribute("Count"));
			reader.ReadStartElement("Routs");
			for (int i = 0; i < count-1; i ++)
			{
				//reader.ReadStartElement("Rout");
				var rout = new Rout();
				rout.ReadXml(reader);
				Routs.Add(rout);
				//reader.ReadEndElement();
				reader.ReadEndElement();
				
			}
			//reader.ReadEndElement();
		}

		/// <summary>
		/// Converts an object into its XML representation.
		/// </summary>
		/// <param name="writer">The <see cref="T:System.Xml.XmlWriter"/> stream to which the object is serialized. </param>
		public void WriteXml(XmlWriter writer)
		{

			writer.WriteAttributeString("LastUpdateTime", LastUpdateDataDateTime.ToString());

			writer.WriteStartElement("Routs");
			writer.WriteAttributeString("Count", Routs.Count.ToString());
			foreach (var rout in Routs)
			{
				writer.WriteStartElement("Rout");
				//writer.WriteValue(rout);
				rout.WriteXml(writer);
				writer.WriteEndElement();
			}
			writer.WriteEndElement();

			writer.WriteStartElement("Stops");
			writer.WriteAttributeString("Count", Stops.Count.ToString());
			foreach (var stop in Stops)
			{
				writer.WriteStartElement("Stop");
				//writer.WriteValue(rout);
				stop.WriteXml(writer);
				writer.WriteEndElement();
			}
			writer.WriteEndElement();

			writer.WriteStartElement("Times");
			writer.WriteAttributeString("Count", Times.Count.ToString());
			foreach (var time in Times)
			{
				writer.WriteStartElement("Time");
				//writer.WriteValue(rout);
				time.WriteXml(writer);
				writer.WriteEndElement();
			}
			writer.WriteEndElement();

			writer.WriteStartElement("FavouriteRouts");
			writer.WriteAttributeString("Count", FavouriteRouts.Count.ToString());
			foreach (var rout in FavouriteRouts)
			{
				writer.WriteStartElement("Rout");
				//writer.WriteValue(rout);
				//TODO
				//rout.WriteXml(writer);
				writer.WriteEndElement();
			}
			writer.WriteEndElement();

			writer.WriteStartElement("FavouriteStops");
			writer.WriteAttributeString("Count", FavouriteStops.Count.ToString());
			foreach (var stop in FavouriteStops)
			{
				writer.WriteStartElement("Stop");
				//writer.WriteValue(rout);
				stop.WriteXml(writer);
				writer.WriteEndElement();
			}
			writer.WriteEndElement();

			writer.WriteStartElement("Groups");
			writer.WriteAttributeString("Count", Groups.Count.ToString());
			foreach (var group in Groups)
			{
				writer.WriteStartElement("Group");
				//writer.WriteValue(rout);
				group.WriteXml(writer);
				writer.WriteEndElement();
			}
			writer.WriteEndElement();
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
					await Task.WhenAll(UpdateAsync()).ConfigureAwait(true);
					updating = false;
					UpdateDataCommand.RaiseCanExecuteChanged();
				}, ()=>!updating);
			}
		}

		public IEnumerable<string> GetDestinations(Rout rout)
		{
			return new List<string>();
		}

		public RelayCommand<RoutWithDestinations> AddFavouriteRoutCommand
		{
			get { return new RelayCommand<RoutWithDestinations>(x => FavouriteRouts.Add(x), p => p != null && !FavouriteRouts.Contains(p)); }
		}

		public RelayCommand<Stop> AddFavouriteSopCommand
		{
			get { return new RelayCommand<Stop>(x => FavouriteStops.Add(x), p => p != null && !FavouriteStops.Contains(p)); }
		}
		public RelayCommand<RoutWithDestinations> RemoveFavouriteRoutCommand
		{
			get { return new RelayCommand<RoutWithDestinations>(x => FavouriteRouts.Remove(x), p => p != null && FavouriteRouts.Contains(p)); }
		}

		public RelayCommand<Stop> RemoveFavouriteSopCommand
		{
			get { return new RelayCommand<Stop>(x => FavouriteStops.Remove(x), p => p != null && FavouriteStops.Contains(p)); }
		}
		#endregion

		protected virtual void OnUpdateStarted()
		{
			var handler = UpdateStarted;
			if (handler != null) handler(this, EventArgs.Empty);
		}

		protected virtual void OnUpdateEnded()
		{
			var handler = UpdateEnded;
			if (handler != null) handler(this, EventArgs.Empty);
		}
	}

	public delegate void LogDelegate(object sender, LogDelegateArgs args);
}