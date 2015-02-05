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
using MinskTrans.DesctopClient.Modelview;
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
			ActualStops = cont.ActualStops;
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
			private set
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
				return actualStops;
			}

			private set { actualStops = value; }
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

		public abstract Task DownloadUpdate();

		

		protected List<Rout> newRoutes;
		protected List<Stop> newStops;
		protected List<Schedule> newSchedule;

		public abstract Task<bool> HaveUpdate(string fileStops, string fileRouts, string fileTimes);

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

			ActualStops = new ObservableCollection<Stop>(Stops.AsParallel().Where(x => Routs != null && Routs.AsParallel().Any(d => d.Stops.Contains(x))));

			AllPropertiesChanged();

			OnApplyUpdateEnded();
		}

		public abstract void Save();

		public abstract Task Load();

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

		//async public void Update()
		//{
		//	await DownloadUpdate();
		//	if (await HaveUpdate())
		//		ApplyUpdate();
		//}

		public virtual Task UpdateAsync()
		{
			//TODO
			//throw new NotImplementedException();
			
			return Task.Run(async () =>
			{
				OnUpdateStarted();
				await DownloadUpdate();
				if (await HaveUpdate(list[0].Key + ".new", list[1].Key + ".new", list[2].Key + ".new"))
					ApplyUpdate();
				OnUpdateEnded();
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
		public event EmptyDelegate LoadStarted;
		public event EmptyDelegate LoadEnded;
		public event ErrorLoadingDelegate ErrorLoading;

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

		protected ObservableCollection<int> FavouriteRoutsIds;
		protected ObservableCollection<int> FavouriteStopsIds; 
		/// <summary>
		/// Generates an object from its XML representation.
		/// </summary>
		/// <param name="reader">The <see cref="T:System.Xml.XmlReader"/> stream from which the object is deserialized. </param>
		public void ReadXml(XmlReader reader)
		{
			
			//reader.ReadStartElement("ContextDesctop");
			LastUpdateDataDateTime = Convert.ToDateTime(reader.GetAttribute("LastUpdateTime"));
			//Routs = new ObservableCollection<Rout>();

			reader.ReadStartElement();
			int count = Convert.ToInt32(reader.GetAttribute("Count"));
			FavouriteRoutsIds = new ObservableCollection<int>();
			for (int i = 0; i < count-1; i ++)
			{
				reader.ReadStartElement();
				FavouriteRoutsIds.Add(int.Parse(reader.GetAttribute("id")));
				
				//reader.ReadStartElement("Rout");
				//var rout = new Rout();
				//rout.ReadXml(reader);
				//Routs.Add(rout);
				//reader.ReadEndElement();
				if (!reader.IsEmptyElement)
					reader.ReadEndElement();
				
			}

			count = Convert.ToInt32(reader.GetAttribute("Count"));
			FavouriteStopsIds = new ObservableCollection<int>();
			reader.ReadStartElement();
			for (int i = 0; i < count - 1; i++)
			{
				reader.ReadStartElement();
				FavouriteStopsIds.Add(int.Parse(reader.GetAttribute("id")));

				if (!reader.IsEmptyElement)
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

			writer.WriteStartElement("FavouritRouts");
			writer.WriteAttributeString("Count", FavouriteRouts.Count.ToString());
			foreach (var rout in FavouriteRouts)
			{
				writer.WriteStartElement("Rout");
				writer.WriteAttributeString("id", rout.Rout.RoutId.ToString());
				writer.WriteEndElement();
			}
			writer.WriteEndElement();

			writer.WriteStartElement("FavouriteStops");
			writer.WriteAttributeString("Count", FavouriteStops.Count.ToString());
			foreach (var stop in FavouriteStops)
			{
				writer.WriteStartElement("Stop");
				writer.WriteAttributeString("id", stop.ID.ToString());
				writer.WriteEndElement();
			}
			writer.WriteEndElement();

			writer.WriteStartElement("Groups");
			writer.WriteAttributeString("Count", Groups.Count.ToString());
			foreach (var group in Groups)
			{
				writer.WriteStartElement("Group");
				writer.WriteAttributeString("Name", group.Name);
				foreach (var stop in group.Stops)
				{
					writer.WriteStartElement("Stop");
					writer.WriteAttributeString("id", stop.ID.ToString());
					writer.WriteEndElement();
				}
				group.WriteXml(writer);
				writer.WriteEndElement();
			}
			writer.WriteEndElement();
		}

		#endregion

		#region commands

		private bool updating = false;
		private ObservableCollection<Stop> actualStops;

		public RelayCommand UpdateDataCommand
		{
			get
			{
				return new RelayCommand(async () =>
				{
					updating = true;
					UpdateDataCommand.RaiseCanExecuteChanged();
					await UpdateAsync();
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

		public bool IsFavouriteStop(Stop stop)
		{
			return FavouriteStops.Contains(stop);
		}

		public bool IsFavouriteRout(RoutWithDestinations rout)
		{
			return FavouriteRouts.Contains(rout);
		}

		public RelayCommand<Stop> AddRemoveFavouriteStop
		{
			get { return new RelayCommand<Stop>(x =>
			{
				if (IsFavouriteStop(x))
					RemoveFavouriteSopCommand.Execute(x);
				else
					AddFavouriteSopCommand.Execute(x);

			}
				);}
		}

		public RelayCommand<RoutWithDestinations> AddRemoveFavouriteRout
		{
			get
			{
				return new RelayCommand<RoutWithDestinations>(x =>
				{
					if (IsFavouriteRout(x))
						RemoveFavouriteRoutCommand.Execute(x);
					else
						AddFavouriteRoutCommand.Execute(x);

				}
					);
			}
		} 

		public event Show ShowStop;
		public event Show ShowRoute;
		public delegate void Show(object sender, ShowArgs args);

		public RelayCommand<Stop> ShowStopMap
		{
			get { return new RelayCommand<Stop>((x) => OnShowStop(new ShowArgs() { SelectedStop = x }), (x) => x != null); }
		}

		public RelayCommand<Rout> ShowRouteMap
		{
			get { return new RelayCommand<Rout>((x) => OnShowRoute(new ShowArgs() { SelectedRoute = x }), (x) => x != null); }
		}

		protected virtual void OnShowStop(ShowArgs args)
		{
			var handler = ShowStop;
			if (handler != null) handler(this, args);
		}

		protected virtual void OnShowRoute(ShowArgs args)
		{
			var handler = ShowRoute;
			if (handler != null) handler(this, args);
		}

		protected virtual void OnLoadStarted()
		{
			var handler = LoadStarted;
			if (handler != null) handler(this, EventArgs.Empty);
		}

		protected virtual void OnLoadEnded()
		{
			var handler = LoadEnded;
			if (handler != null) handler(this, EventArgs.Empty);
		}

		protected virtual void OnErrorLoading(ErrorLoadingDelegateArgs args)
		{
			var handler = ErrorLoading;
			if (handler != null) handler(this, args);
		}
	}

	public delegate void ErrorLoadingDelegate(object sender, ErrorLoadingDelegateArgs args);

	public class ErrorLoadingDelegateArgs:EventArgs
	{
		public enum Errors
		{
			NoFileToDeserialize,
			NoSourceFiles
		}

		public Errors Error { get; set; }
	}

	public delegate void LogDelegate(object sender, LogDelegateArgs args);
}