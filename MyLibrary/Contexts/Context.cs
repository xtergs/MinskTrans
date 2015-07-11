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
using System.Text;
using System.Threading;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.Threading.Tasks;
using System.Xml.Linq;


using MinskTrans.DesctopClient.Model;

using MyLibrary;
using Newtonsoft.Json;
using TransportType = MinskTrans.DesctopClient.Model.TransportType;
using GalaSoft.MvvmLight.Command;

namespace MinskTrans.DesctopClient
{
public class Context : INotifyPropertyChanged,  IContext
	{
		

		protected Dictionary<int, uint> counterViewStops =new Dictionary<int,uint>();

		enum TypeSaveData
		{
			DB,
			Favourite,
			Statisticks
		}
		Dictionary<TypeSaveData, TypeFolder> folders = new Dictionary<TypeSaveData, TypeFolder>()
		 {{TypeSaveData.DB, TypeFolder.Local},
			 {TypeSaveData.Favourite, TypeFolder.Roaming},
			 {TypeSaveData.Statisticks, TypeFolder.Roaming}
			
		}; 

		public void IncrementCounter(Stop stop)
		{
			if (!counterViewStops.Keys.Contains(stop.ID))
			{
				counterViewStops.Add(stop.ID, 1);
			}
			else
			{
				counterViewStops[stop.ID]++;
			}
		}

		public uint GetCounter(Stop stop)
		{
			if (counterViewStops != null)
				return counterViewStops.Keys.Contains(stop.ID) ? counterViewStops[stop.ID] : 0;
			return 0;
		}

		
		public string TempExt
		{
			get { return FileHelperBase.TempExt; }
		}

		public string OldExt { get { return FileHelperBase.OldExt; } }
		public string NewExt { get { return FileHelperBase.NewExt; } }

		public string GetTempFileName(string filename)
		{
			return filename + TempExt;
		}

		public string NameFileFavourite
		{
			get
			{
				if (String.IsNullOrWhiteSpace(nameFileFavourite))
					nameFileFavourite = "data.dat";
				return nameFileFavourite;
			}
			set { nameFileFavourite = value; }
		}

		public string NameFileRouts
		{
			get
			{
				if (string.IsNullOrWhiteSpace(nameFileRouts))
					nameFileRouts = "dataRouts.dat";
				return nameFileRouts;
			}
			set { nameFileRouts = value; }
		}

		public string NameFileStops
		{
			get
			{
				if (string.IsNullOrWhiteSpace(nameFileStops))
					nameFileStops = "dataStops.dat";
				return nameFileStops;
			}
			set { nameFileStops = value; }
		}

		public string NameFileTimes
		{
			get
			{
				if (string.IsNullOrWhiteSpace(nameFileTimes))
					nameFileTimes = "dataTimes.dat";
				return nameFileTimes;
			}
			set { nameFileTimes = value; }
		}

		public string NameFileCounter
		{
			get
			{
				if (string.IsNullOrWhiteSpace(nameFileTimes))
					nameFileCounter = "counters.dat";
				return nameFileCounter;
			}
			set { nameFileCounter = value; }
		}

		//private ObservableCollection<Rout> routs;
		private int variantLoad;
		//private ObservableCollection<Stop> stops;
		//private ObservableCollection<Schedule> times;
		//private ObservableCollection<GroupStop> groups;
		//private ObservableCollection<RoutWithDestinations> favouriteRouts;
		//private ObservableCollection<Stop> favouriteStops;
		private DateTime lastUpdateDataDateTime;

		public int VariantLoad
		{
			get { return variantLoad; }
			set { variantLoad = value; }
		}
		public virtual DateTime LastUpdateDataDateTime
		{
			get { return lastUpdateDataDateTime; }
			set
			{
				lastUpdateDataDateTime = value;
				OnPropertyChanged();
			}
		}

		

	private readonly FileHelperBase fileHelper;
		protected readonly InternetHelperBase internetHelper;
		
		public Context(FileHelperBase helper, InternetHelperBase internetHelper)
		{
			fileHelper = helper;
			this.internetHelper = internetHelper;
			Create();
		}

		public void Inicialize(Context cont)
		{
			Routs = cont.Routs;
			Stops = cont.Stops;
			//ActualStops = cont.ActualStops;
			Times = cont.Times;
			FavouriteRouts = cont.FavouriteRouts;
			favouriteStops = cont.favouriteStops;
			LastUpdateDataDateTime = cont.LastUpdateDataDateTime;
			Groups = cont.Groups;
		}

		

		public virtual void Create(bool AutoUpdate = true)
		{
			FavouriteRouts = new ObservableCollection<Rout>();
			favouriteStops = new ObservableCollection<int>();
			Groups = new ObservableCollection<GroupStop>();
		}
		
		public async Task ApplyUpdate(IList<Rout> newRoutes, IList<Stop> newStops, IList<Schedule> newSchedule)
		{
			OnApplyUpdateStarted();
			try
			{
				Stops = newStops;
				Routs = newRoutes;
				Times = newSchedule;
				//ActualStops = Stops;

				LastUpdateDataDateTime = DateTime.UtcNow;

				newStops = null;
				newRoutes = null;
				newSchedule = null;
			}
			catch (Exception e)
			{
				Debug.WriteLine("Apply update: " + e.Message);
#if BETA
				Logger.Log("ApplyUpdate exception").WriteLine(e.Message).WriteLine(e.StackTrace).SaveToFile();
#endif
				throw;
			}
			OnApplyUpdateEnded();
		}

		public virtual async Task<bool> HaveUpdate(IList<Rout> newRoutes, IList<Stop> newStops, IList<Schedule> newSchedule)
		{
			Connect(newRoutes, newStops, newSchedule, VariantLoad);
			foreach (var toRemoveStop in newStops.Where(stop => !stop.Routs.Any()).ToList())
            {
				newStops.Remove(toRemoveStop);
			}
			foreach (var toRemoveRout in newRoutes.Where(rout => !rout.Stops.Any()).ToList())
			{
				newRoutes.Remove(toRemoveRout);
			}
			return NeedUpdate(newRoutes, newStops, newSchedule);		
		}
		
		protected virtual async Task SaveFavourite(TypeFolder storage)
		{
			var favouriteString = JsonConvert.SerializeObject(new
			{
				Routs = FavouriteRouts.Select(x => x.RoutId).ToList(),
				Stops = favouriteStops,
				Groups = Groups.Select(x => new
				{
					Name = x.Name,
					IDs = x.Stops.Select(stop => stop.ID)
				})
			});
			//await FileIO.WriteTextAsync(stream, favouriteString);
			//await stream.MoveAsync(storage, NameFileFavourite, NameCollisionOption.ReplaceExisting);

			await FileHelper.WriteTextAsync(storage, NameFileFavourite + TempExt, favouriteString);
			await FileHelper.SafeMoveAsync(storage, NameFileFavourite + TempExt, NameFileFavourite);
		}

		async Task SaveStatistics(JsonSerializerSettings jsonSettings, TypeFolder storage)
		{
			string counter = JsonConvert.SerializeObject(counterViewStops, jsonSettings);
			//var counterFile = await storage.CreateFileAsync(NameFileCounter + TempExt, CreationCollisionOption.ReplaceExisting);
			//await FileIO.WriteTextAsync(counterFile, counter);
			//await counterFile.RenameAsync(NameFileCounter, NameCollisionOption.ReplaceExisting);

			await FileHelper.WriteTextAsync(storage, NameFileCounter + TempExt, counter);
			await FileHelper.SafeMoveAsync(storage, NameFileCounter + TempExt, NameFileCounter);
		}

		public virtual async Task Save(bool saveAllDb = true)
		{
			try
			{

				await SaveFavourite(folders[TypeSaveData.Favourite]);

				var jsonSettings = new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore };

				await SaveStatistics(jsonSettings, folders[TypeSaveData.Statisticks]);
				if (saveAllDb)
					await Task.WhenAll(Task.Run(async () =>
					{
						string routs = JsonConvert.SerializeObject(Routs, jsonSettings);
						await FileHelper.WriteTextAsync(folders[TypeSaveData.DB], NameFileRouts + TempExt, routs);
						await FileHelper.SafeMoveAsync(folders[TypeSaveData.DB], NameFileRouts + TempExt, NameFileRouts);

					}),
						Task.Run(async () =>
						{
							string stopsString = JsonConvert.SerializeObject(ActualStops, jsonSettings);

							await FileHelper.WriteTextAsync(folders[TypeSaveData.DB], NameFileStops + TempExt, stopsString);
							await FileHelper.SafeMoveAsync(folders[TypeSaveData.DB], NameFileStops + TempExt, NameFileStops);

						}), Task.Run(async () =>
						{
							string routs = JsonConvert.SerializeObject(Times, jsonSettings);

							await FileHelper.WriteTextAsync(folders[TypeSaveData.DB], NameFileTimes + TempExt, routs);
							await FileHelper.SafeMoveAsync(folders[TypeSaveData.DB], NameFileTimes + TempExt, NameFileTimes);

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

		public virtual async Task Load(LoadType type = LoadType.LoadAll)
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
			IList<int> tpFavouriteStops = null;
			ObservableCollection<Rout> tpFavouriteRouts = null;
			ObservableCollection<GroupStop> tpGroups = null;

			try
			{
				if (type.HasFlag(LoadType.LoadFavourite))
					try
					{
						//await Task.Delay(new TimeSpan(0, 0, 0, 10));

						//var routsFile = await folders[TypeSaveData.Statisticks].GetFileAsync(NameFileCounter);
						//var routs = await FileIO.ReadTextAsync(routsFile);
						string routs = await FileHelper.ReadAllTextAsync(folders[TypeSaveData.Statisticks], NameFileCounter);
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
								var routs = await FileHelper.ReadAllTextAsync(folders[TypeSaveData.DB], NameFileRouts);
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
								string stops = await FileHelper.ReadAllTextAsync(folders[TypeSaveData.DB], NameFileStops);
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
								string times = await FileHelper.ReadAllTextAsync(folders[TypeSaveData.DB], NameFileTimes);

								tpTimes = JsonConvert.DeserializeObject<ObservableCollection<Schedule>>(times);
							}
							catch (FileNotFoundException e)
							{
								throw new TaskCanceledException(e.Message, e);
							}
						}));
				else
				{
					tpRouts = new ObservableCollection<Rout>(Routs);
					tpStops = new ObservableCollection<Stop>(Stops);
					tpTimes = new ObservableCollection<Schedule>(Times);
				}
			}
			catch (TaskCanceledException e)
			{
				//CleanTp();
				OnErrorLoading(new ErrorLoadingDelegateArgs() { Error = ErrorLoadingDelegateArgs.Errors.NoSourceFiles });
#if BETA
				Logger.Log("Load taskcanceledException").WriteLineTime(e.Message).WriteLine(e.StackTrace);
				Logger.Log().SaveToFile();
#endif
				return;
			}
			catch (Exception e)
			{
				Debug.WriteLine("Context.Load: " + e.Message);
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
				//ActualStops = Stops;
			}
			//await Task.Delay(new TimeSpan(0, 0, 0, 10));
			Debug.WriteLine("UniversalContext loadfavourite started");
			if (type.HasFlag(LoadType.LoadFavourite) && await FileHelper.FileExistAsync(folders[TypeSaveData.Favourite], NameFileFavourite))
			{
				try
				{
					var textFavourite = await FileHelper.ReadAllTextAsync(folders[TypeSaveData.Favourite], NameFileFavourite);
					try
					{
						var desFavourite = JsonConvert.DeserializeAnonymousType(textFavourite, new
						{
							Routs = FavouriteRouts.Select(x => x.RoutId).ToList(),
							Stops = favouriteStops,
							Groups = Groups.Select(x => new
							{
								Name = x.Name,
								IDs = x.Stops.Select(stop => stop.ID)
							})
						});

						if (desFavourite.Routs != null)
						{
							var temp1 = desFavourite.Routs.Select(x =>
								new RoutWithDestinations(tpRouts.First(d => d.RoutId == x), this)).ToList();
							tpFavouriteRouts = new ObservableCollection<Rout>(temp1);
							//desFavourite.Routs = null;
						}

						if (desFavourite.Stops != null)
						{
							tpFavouriteStops = desFavourite.Stops;
							//FavouriteStopsIds = null;
						}
						if (desFavourite.Groups != null)
						{
							tpGroups = new ObservableCollection<GroupStop>(desFavourite.Groups.Select(x => new GroupStop()
							{
								Name = x.Name,
								Stops = new List<Stop>(tpStops.Join(x.IDs, stop => stop.ID, i => i, (stop, i) => stop))
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
							tpFavouriteRouts = new ObservableCollection<Rout>(temp1);
							FavouriteRoutsIds = null;
						}

						if (FavouriteStopsIds != null)
						{
							tpFavouriteStops = FavouriteStopsIds;
							FavouriteStopsIds = null;
						}
						if (GroupsStopIds != null)
						{
							tpGroups = new ObservableCollection<GroupStop>(GroupsStopIds.Select(x => new GroupStop()
							{
								Name = x.Name,
								Stops = new List<Stop>(tpStops.Join(x.StopID, stop => stop.ID, i => i, (stop, i) => stop))
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
				tpFavouriteRouts = new ObservableCollection<Rout>();
				tpFavouriteStops = new ObservableCollection<int>();
				tpGroups = new ObservableCollection<GroupStop>();
			}

			//}
			try
			{
				FavouriteRouts = tpFavouriteRouts;
				favouriteStops = tpFavouriteStops;
				Groups = tpGroups;
				Debug.WriteLine("UniversalContext loadfavourite ended");

				//CleanTp();
				//AllPropertiesChanged();
				//await Task.Delay(new TimeSpan(0, 0, 0, 10));
				OnLoadEnded();
			}
			catch (Exception e)
			{
				Debug.WriteLine("Load exception, set favourite " + e.ToString());
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

		public async virtual Task Recover()
		{
#if BETA
			Logger.Log().WriteLineTime("Recover started");
#endif
			await FileHelper.DeleteFile(TypeFolder.Roaming, NameFileRouts);
			await FileHelper.DeleteFile(TypeFolder.Roaming, NameFileStops);
			await FileHelper.DeleteFile(TypeFolder.Roaming, NameFileTimes);
#if BETA
			Logger.Log().WriteLineTime("Recover ended");
#endif
		}


		

		public void AllPropertiesChanged()
		{
			OnPropertyChanged("ActualStops");
			OnPropertyChanged("Stops");
			OnPropertyChanged("Routs");
			OnPropertyChanged("Times");
			OnPropertyChanged("FavouriteRouts");
			OnPropertyChanged("FavouriteStops");
			OnPropertyChanged("Groups");
		}


		static protected void Connect(/*[NotNull]*/ IList<Rout> routsl,/* [NotNull]*/ IList<Stop> stopsl,
			/*[NotNull]*/ IList<Schedule> timesl, int variantLoad)
		{
			
#if BETA
			Logger.Log("Connect started");
			Debug.WriteLine("Connect Started");
#endif
			Stopwatch watch = new Stopwatch();
			watch.Start();
			
			if (routsl == null) throw new ArgumentNullException("routsl");
			if (stopsl == null) throw new ArgumentNullException("stopsl");
			if (timesl == null) throw new ArgumentNullException("timesl");

			//Parallel.ForEach(routsl, (rout) =>

			//foreach (var stop in stopsl)
			//{
			//	stop.Routs = new List<Rout>(5);
			//}

			//Parallel.ForEach(routsl, (rout) =>
				foreach (var rout in routsl)
			{
				var rout1 = rout;
				Schedule first = timesl.Where(x =>
				{
					if (x == null)
						return false;
					return x.RoutId == rout1.RoutId;
				}).FirstOrDefault();
				rout.Time = first;
				if (rout.Time != null)
					rout.Time.Rout = rout;


				rout1.Stops = rout1.RouteStops.Join(stopsl, i => i, stop => stop.ID, (i, stop) =>
				{
					if (stop.Routs == null)
						stop.Routs = new List<Rout>();
					stop.Routs.Add(rout1);
					return stop;
				}).ToList();
			}
			watch.Stop();
			var xx = watch.ElapsedMilliseconds;
#if BETA
			string message = "Connect Ended, " + "Milliseconds: " + xx.ToString();
			Debug.WriteLine(message);
			Logger.Log(message);
#endif
		}

		//async public void Update()
		//{
		//	await DownloadUpdate();
		//	if (await HaveUpdate())
		//		ApplyUpdate();
		//}

		//public virtual async Task UpdateAsync(bool SaveAllDB = true)
		//{
		//	//TODO
		//	//throw new NotImplementedException();

		//	OnUpdateStarted();
		//	try
		//	{
		//		try
		//		{
		//			if (!await DownloadUpdate())
		//				return;
		//		}
		//		catch (TaskCanceledException)
		//		{
		//			await Task.WhenAll(FileHelper.DeleteFile(TypeFolder.Roaming, list[0].Key + NewExt),

		//									FileHelper.DeleteFile(TypeFolder.Roaming, list[1].Key + NewExt),
		//									FileHelper.DeleteFile(TypeFolder.Roaming, list[2].Key + NewExt)).ContinueWith((x) => OnErrorDownloading());
		//			return;
		//		}
		//		if (await HaveUpdate(list[0].Key + NewExt, list[1].Key + NewExt, list[2].Key + NewExt, checkUpdate: true))
		//		{
		//			await ApplyUpdate();
		//			await Save(SaveAllDB);
		//		}
		//		await Task.WhenAll(
		//			FileHelper.DeleteFile(TypeFolder.Roaming, list[0].Key + NewExt),
		//			FileHelper.DeleteFile(TypeFolder.Roaming, list[1].Key + NewExt),
		//			FileHelper.DeleteFile(TypeFolder.Roaming, list[2].Key + NewExt));

		//	}
		//	catch (Exception)
		//	{
		//		throw;
		//	}
		//	OnUpdateEnded();
		//}

		protected bool NeedUpdate(IList<Rout> newRoutes, IList<Stop> newStops, IList<Schedule> newSchedule)
		{
			if (Stops == null || Routs == null || Times == null || !Stops.Any() || !Routs.Any() || !Times.Any())
				return true;
#if DEBUG
			var xx = newSchedule.Except(Times).ToList();
#endif
			if (newStops.Count != Stops.Count || newRoutes.Count != Routs.Count || newSchedule.Count != Times.Count)
				return true;

			foreach (var newRoute in newRoutes)
			{
				if (Routs.AsParallel().All(x => x.RoutId == newRoute.RoutId && x.Datestart != newRoute.Datestart))
					return true;
			}
			return false;
		}

		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChangedEventHandler handler = PropertyChanged;
			if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
		}

		#region Event
		public delegate void EmptyDelegate(object sender, EventArgs args);

		//public event EmptyDelegate DataBaseDownloadStarted;
		//public event EmptyDelegate DataBaseDownloadEnded;
		public event EmptyDelegate ApplyUpdateStarted;
		public event EmptyDelegate ApplyUpdateEnded;
		public event LogDelegate LogMessage;
		
		public event EmptyDelegate UpdateStarted;
		public event EmptyDelegate UpdateEnded;
		public event EmptyDelegate LoadStarted;
		public event EmptyDelegate LoadEnded;
		public event ErrorLoadingDelegate ErrorLoading;

		#region Invokators
		
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

		#region Legacy code, Using xml for store settings

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

		protected internal ObservableCollection<int> FavouriteRoutsIds;
		protected internal ObservableCollection<int> FavouriteStopsIds;
		protected internal ObservableCollection<GroupStopId> GroupsStopIds;
		/// <summary>
		/// Generates an object from its XML representation.
		/// </summary>
		/// <param name="reader">The <see cref="T:System.Xml.XmlReader"/> stream from which the object is deserialized. </param>
		public void ReadXml(string reader)
		{
			try
			{

			var node = XDocument.Parse(reader);
			var document = (XElement)node.Root;

			//document.Root.Attribute("LastUpdateTime").Value;

				//LastUpdateDataDateTime = (DateTime) document.Attribute("LastUpdateTime");

				var temp1 = document.Elements("FavouriteStops").Elements("ID");
				FavouriteStopsIds = new ObservableCollection<int>(temp1.Select(x => (int)(x)).ToList());

				var temp = document.Elements("FavouritRouts").Elements("ID").ToList();
				if (temp.Count() <= 0)
					;
				else
					FavouriteRoutsIds = new ObservableCollection<int>(temp.Select(x =>
					{

						return (int) x;
					}));

				var xGroups = document.Element("Groups");
				if (xGroups != null)
					GroupsStopIds = new ObservableCollection<GroupStopId>(xGroups.Elements("Group").Select(XGroup =>
					{
						var groupid = new GroupStopId();
						groupid.Name = (string) XGroup.Attribute("Name");
						groupid.StopID = XGroup.Elements("ID").Select(id => (int) id).ToList();
						return groupid;
					}));
				else
					GroupsStopIds = new ObservableCollection<GroupStopId>();
			}
			catch (Exception e)
			{
				Debug.WriteLine("Context.ReadXml: " + e.Message);
				return;
			}
		}

		

		public void ReadXml(XmlReader reader)
		{
			try
			{
				var node = XDocument.Load(reader);
				var document = (XElement)node.Root;

				//document.Root.Attribute("LastUpdateTime").Value;

				//LastUpdateDataDateTime = (DateTime) document.Attribute("LastUpdateTime");

				var temp1 = document.Elements("FavouriteStops").Elements("ID");
				FavouriteStopsIds = new ObservableCollection<int>(temp1.Select(x => (int)(x)).ToList());

				var temp = document.Elements("FavouritRouts").Elements("ID").ToList();
				if (temp.Count() <= 0)
					;
				else
					FavouriteRoutsIds = new ObservableCollection<int>(temp.Select(x =>
					{

						return (int)x;
					}));

				var xGroups = document.Element("Groups");
				if (xGroups != null)
					GroupsStopIds = new ObservableCollection<GroupStopId>(xGroups.Elements("Group").Select(XGroup =>
					{
						var groupid = new GroupStopId();
						groupid.Name = (string)XGroup.Attribute("Name");
						groupid.StopID = XGroup.Elements("ID").Select(id => (int)id).ToList();
						return groupid;
					}));
				else
					GroupsStopIds = new ObservableCollection<GroupStopId>();
			}
			catch (Exception e)
			{
				Debug.WriteLine("Context.ReadXml: " + e.Message);
				return;
			}
		}

		/// <summary>
		/// Converts an object into its XML representation.
		/// </summary>
		/// <param name="writer">The <see cref="T:System.Xml.XmlWriter"/> stream to which the object is serialized. </param>
		public void WriteXml(XmlWriter writer)
		{
			XElement element = new XElement("Context");
			XDocument document = new XDocument(new XDeclaration("1.0", "utf-8", "yes"), element);
			//document.Declaration = new XDeclaration("1.0", "UTF", "yes");

			//element.SetAttributeValue("LastUpdateTime", LastUpdateDataDateTime);

			

			element.SetAttributeValue("V", 1);


			XElement el = new XElement("FavouritRouts", FavouriteRouts.Select(x => new XElement("ID", x.RoutId)));

			element.Add(el);

			el = new XElement("FavouriteStops", FavouriteStops.Select(x => new XElement("ID", x.ID)));

			element.Add(el);

			el = new XElement("Groups", Groups.Select(x =>
			{
				var elem = new XElement("Group", x.Stops.Select(y => new XElement("ID", y.ID)));
				elem.SetAttributeValue("Name", x.Name);
				return elem;
			}));
			

			element.Add(el);

			document.Save(writer);
		}
		
		#endregion

		#region commands

		private bool updating = false;
		private string nameFileFavourite;
		private string nameFileRouts;
		private string nameFileStops;
		private string nameFileTimes;

//		public RelayCommand UpdateDataCommand
//		{
//			get
//			{
//				return new RelayCommand(async () =>
//				{
//#if WINDOWS_PHONE_APP
//					InternetHelperBase.UpdateNetworkInformation();
//					if (!InternetHelperBase.Is_Connected)
//						return;
//#endif
//					updating = true;
//					UpdateDataCommand.RaiseCanExecuteChanged();
//					//InternetHelper.UpdateNetworkInformation();
					
//					await UpdateAsync();
//					updating = false;
//					UpdateDataCommand.RaiseCanExecuteChanged();
//				}, ()=>!updating);
//			}
//		}

		public IEnumerable<string> GetDestinations(Rout rout)
		{
			return new List<string>();
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
			return favouriteStops.Contains(stop.ID);
		}

		public bool IsFavouriteRout(Rout rout)
		{
			return FavouriteRouts.Contains(rout);
		}

		public RelayCommand<Rout> AddFavouriteRoutCommand
		{
			get
			{
				return new RelayCommand<Rout>(async x =>
				{
					await AddFavouriteRout(x);
				}, p => p != null && !FavouriteRouts.Contains(p));
			}
		}

		public RelayCommand<Stop> AddFavouriteSopCommand
		{
			get
			{
				return new RelayCommand<Stop>(async x =>
				{
					await AddFavouriteStop(x);
				}

			  , p => p != null && FavouriteStops != null && !FavouriteStops.Contains(p));
			}
		}
		public RelayCommand<RoutWithDestinations> RemoveFavouriteRoutCommand
		{
			get
			{
				return new RelayCommand<RoutWithDestinations>(async x =>
				{
					await RemoveFavouriteRout(x);
				}, p => p != null && FavouriteRouts.Contains(p));
			}
		}

		public RelayCommand<Stop> RemoveFavouriteSopCommand
		{
			get
			{
				return new RelayCommand<Stop>(async x =>
				{
					await RemoveFavouriteStop(x);
				}, p => p != null && FavouriteStops.Contains(p));
			}
		}

		public RelayCommand<Stop> AddRemoveFavouriteStop
		{
			get
			{
				return new RelayCommand<Stop>(async x =>
				{
					if (IsFavouriteStop(x))
						await RemoveFavouriteStop(x);
					else
						await AddFavouriteStop(x);

				}
			  );
			}
		}

		public RelayCommand<RoutWithDestinations> AddRemoveFavouriteRout
		{
			get
			{
				return new RelayCommand<RoutWithDestinations>(async x =>
				{
					if (IsFavouriteRout(x))
						await RemoveFavouriteRout(x);
					else
						await AddFavouriteRout(x);

				}
					);
			}
		}

		public RelayCommand<string> CreateGroup
		{
			get
			{
				return new RelayCommand<string>(async x =>
				{
					await AddGroup(new GroupStop() { Name = x });
				}, p => !string.IsNullOrWhiteSpace(p));
			}
		}

		public RelayCommand<GroupStop> DeleteGroups
		{
			get
			{
				return new RelayCommand<GroupStop>(async x =>
				{
					if (x != null)
					{
						await RemoveGroup(x);
					}
				});
			}
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

		public async Task AddFavouriteRout(Rout rout)
		{
			FavouriteRouts.Add(new RoutWithDestinations(rout, this));
			await SaveFavourite(TypeFolder.Roaming);
		}

		public async Task AddFavouriteStop(Stop stop)
		{
			if (IsFavouriteStop(stop))
				return;
			favouriteStops.Add(stop.ID);
			await SaveFavourite(TypeFolder.Roaming);
		}

		public async Task RemoveFavouriteRout(Rout rout)
		{
			FavouriteRouts.Remove(rout);
			await SaveFavourite(TypeFolder.Roaming);
		}

		public async Task RemoveFavouriteStop(Stop stop)
		{
			if (!IsFavouriteStop(stop))
				return;
			FavouriteStops.Remove(stop);
			await SaveFavourite(TypeFolder.Roaming);
		}

		public async Task AddGroup(GroupStop group)
		{
			Groups.Add(group);
			await SaveFavourite(TypeFolder.Roaming);
		}

		public async Task RemoveGroup(GroupStop group)
		{
			Groups.Remove(group);
			await SaveFavourite(TypeFolder.Roaming);
		}

		public string nameFileCounter { get; set; }

	public FileHelperBase FileHelper
	{
		get { return fileHelper; }
	}

		public IList<Rout> Routs { get; private set; }
		public IList<Schedule> Times { get; private set; }
		public IList<Stop> ActualStops { get { return Stops; } }
		public IList<Rout> FavouriteRouts { get; private set; }
		public IList<GroupStop> Groups { get; private set; }
		IList<int> favouriteStops { get; set; }
		public IList<Stop> FavouriteStops
		{
			get
			{
				if (ActualStops == null)
					return null;
				return ActualStops.Where(st => favouriteStops.Contains(st.ID)).ToList();
			}
		}
		public IList<Stop> Stops { get; private set; }
		#region Implementation of INotifyPropertyChanged

		public event PropertyChangedEventHandler PropertyChanged;

		#endregion
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