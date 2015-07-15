using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MinskTrans.DesctopClient.Model;
using System.Data.Entity;
using MyLibrary.Model;

namespace MinskTrans.DesctopClient
{
	
	public class SettingsDataBase
	{
		public int SettingsDataBaseID{ get; set; }
       public DateTime LastUpdateDateTimeUtc { get; set; }
	}

	public class StopExtentionData
	{
		public int StopID { get; set; }
		public Stop Stop { get; set; }
		public uint Counter { get; set; }
		public bool Favourite { get; set; }
	}
	public class RoutExtentionData
	{
		public int RoutID { get; set; }
		public Rout Rout { get; set; }
		public uint Counter { get; set; }
		public bool Favourite { get; set; }
	}

	public class GroupEx : GroupStop
	{
		public int GroupID { get; set; }
	}

	public class SqlEFContext : DbContext, IContext
	{
		public DbSet<RoutExtentionData> routExtentionData { get; set; }

		public DbSet<Rout> routsEF { get; set; }
		public DbSet<Stop> stopsEF { get; set; }
		public DbSet<Schedule> timesEF { get; set; }

		public DbSet<StopExtentionData> stopsExtentionData { get; set; }
		public DbSet<SettingsDataBase> settingsDataBase { get; set; }

		public DbSet<GroupEx> groupsEF { get; set; }

		public SqlEFContext(string connectionString)
			:base(connectionString)
		{

		}

		protected override void OnModelCreating(DbModelBuilder modelBuilder)
		{
			modelBuilder.Entity<Schedule>().HasKey(x => x.RoutId);
			modelBuilder.Entity<Schedule>().Property(x=> x.InicializeString);

			modelBuilder.Entity<Rout>().HasOptional(key => key.Time).WithOptionalDependent(key => key.Rout);
			modelBuilder.Entity<Rout>().HasMany(key => key.Stops).WithMany(key => key.Routs);
			
			modelBuilder.Entity<Stop>().HasMany(t=> t.Stops).WithMany().Map(x=>
			{
				x.MapLeftKey("Stop_ID");
				x.MapRightKey("Similar_Stop");
				x.ToTable("Stop_Stops");
			});

			modelBuilder.Entity<StopExtentionData>().HasKey(x => x.StopID);
			modelBuilder.Entity<StopExtentionData>().HasRequired(x => x.Stop);

			modelBuilder.Entity<RoutExtentionData>().HasKey(x => x.RoutID);
			modelBuilder.Entity<RoutExtentionData>().HasRequired(x => x.Rout).WithOptional().Map(x => { x.MapKey("aaa"); });

			modelBuilder.Entity<GroupEx>().HasKey(x => x.GroupID);
			modelBuilder.Entity<GroupEx>().HasMany(x => x.Stops);

			
		}

		public IList<Stop> ActualStops
		{
			get
			{
				return stopsEF.ToList<Stop>();
			}
		}

		public IList<Rout> FavouriteRouts
		{
			get
			{
				if (routExtentionData == null)
					return null;
				return routsEF.Where(x=> routExtentionData.Any(d=> d.Rout==x)).ToList().ToList();
			}
		}

		public IList<Stop> FavouriteStops
		{
			get
			{
				return stopsEF.Where(stop=> stopsExtentionData.Any(x=> x.StopID == stop.ID)).ToList();
			}
		}

		public IList<GroupStop> Groups
		{
			get
			{
				if (groupsEF == null)
					return null;
				return groupsEF.ToList<GroupStop>();
			}
		}

		public DateTime LastUpdateDataDateTime
		{
			get
			{
				if (settingsDataBase == null)
					return default(DateTime);
				var tempData = settingsDataBase.FirstOrDefault(d => d.SettingsDataBaseID == settingsDataBase.Max(x => x.SettingsDataBaseID));
				if (tempData == null)
					return default(DateTime);
                return tempData.LastUpdateDateTimeUtc;
			}

			set
			{
				settingsDataBase.Add(new SettingsDataBase() { LastUpdateDateTimeUtc = value });
				SaveChanges();
            }
		}

		public IList<Rout> Routs
		{
			get { return routsEF.ToList(); }
		}

		public IList<Stop> Stops{get{return stopsEF.ToList<Stop>();
			
		}}

		public IList<Schedule> Times { get { return timesEF.ToList();
			
		} }

		public event Context.EmptyDelegate ApplyUpdateEnded;
		public event Context.EmptyDelegate ApplyUpdateStarted;
		public event ErrorLoadingDelegate ErrorLoading;
		public event Context.EmptyDelegate LoadEnded;
		public event Context.EmptyDelegate LoadStarted;
		public event PropertyChangedEventHandler PropertyChanged;
		public event Context.EmptyDelegate UpdateEnded;
		public event Context.EmptyDelegate UpdateStarted;

		public void AllPropertiesChanged()
		{
			throw new NotImplementedException();
		}

		static protected void Connect(/*[NotNull]*/ IList<Rout> routsl,/* [NotNull]*/ IList<Stop> stopsl,
			/*[NotNull]*/ IList<Schedule> timesl)
		{

#if BETA
			Logger.Log("Connect started");
			Debug.WriteLine("Connect Started");
#endif
			//Stopwatch watch = new Stopwatch();
			//watch.Start();

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
			//watch.Stop();
			//var xx = watch.ElapsedMilliseconds;
#if BETA
			string message = "Connect Ended, " + "Milliseconds: " + xx.ToString();
			Debug.WriteLine(message);
			Logger.Log(message);
#endif
		}

		public async Task ApplyUpdate(IList<Rout> newRoutes, IList<Stop> newStops, IList<Schedule> newSchedule)
		{
			//Connect(newRoutes, newStops, newSchedule);
			//routsEF.RemoveRange(Routs);
			//stopsEF.RemoveRange(stopsEF);
			//timesEF.RemoveRange(timesEF);

			routsEF.AddRange(newRoutes);
			stopsEF.AddRange(newStops);
			timesEF.AddRange(newSchedule);

			SaveChanges();
		}

		StopExtentionData GetExtDataFromStop(Stop stop)
		{
			return stopsExtentionData.FirstOrDefault(x => x.Stop.ID == stop.ID);
		}

		RoutExtentionData GetExtDataFromRout(Rout rout)
		{
			return routExtentionData.FirstOrDefault(x => x.Rout.RoutId == rout.RoutId);
		}

		StopExtentionData GetOrCreateExtDataFromStop(Stop stop)
		{
			var stopData = GetExtDataFromStop(stop);
			if (stopData != null)
				return stopData;
			stopData = stopsExtentionData.Add(new StopExtentionData() { Stop = stop, StopID = stop.ID });
			return stopData;
		}

		public void Create(bool AutoUpdate = true)
		{
			//throw new NotImplementedException();
		}

		public uint GetCounter(Stop stop)
		{
			var stopData = GetExtDataFromStop(stop);
			if (stopData == null)
				return 0;
            return stopData.Counter;
		//return stopsEF.First(id => id.ID == stop.ID).Counter;
		}

		public IEnumerable<string> GetDestinations(Rout rout)
		{
			throw new NotImplementedException();
		}

		public async Task<bool> HaveUpdate(IList<Rout> newRoutes, IList<Stop> newStops, IList<Schedule> newSchedule)
		{
			Connect(newRoutes, newStops, newSchedule);
			newStops = new List<Stop>(newStops.Where(stop => stop.Routs.Any()));
			newRoutes = new List<Rout>(newRoutes.Where(rout => rout.Stops.Any()));
			return NeedUpdate(newRoutes, newStops, newSchedule);
		}

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

		public void IncrementCounter(Stop stop)
		{
			var stopData = GetOrCreateExtDataFromStop(stop);
			stopData.Counter++;
			//stopsEF.First(st => st.ID == stop.ID).Counter++;
			SaveChanges();
		}

		public void Inicialize(IContext cont)
		{
			throw new NotImplementedException();
		}

		public async Task Load(LoadType type = LoadType.LoadAll)
		{
			//throw new NotImplementedException();
		}

		public Task Recover()
		{
			throw new NotImplementedException();
		}

		public async Task Save(bool saveAllDb = true)
		{
			await SaveChangesAsync();
			//throw new NotImplementedException();
		}

		public bool IsFavouriteStop(Stop stop)
		{
			return stopsExtentionData.Any(x => x.Stop == stop);
		}

		public async Task AddFavouriteRout(Rout rout)
		{
			var tprout = GetExtDataFromRout(rout);
			if (tprout != null)
				tprout.Favourite = true;
			else
				routExtentionData.Add(new RoutExtentionData() { Rout = rout, Favourite = true });
			await SaveChangesAsync();
		}

		public async Task AddFavouriteStop(Stop stop)
		{
			var tpStop = GetExtDataFromStop(stop);
			if (tpStop != null)
				tpStop.Favourite = true;
			else
				stopsExtentionData.Add(new StopExtentionData() { Stop = stop, Favourite = true });
			await SaveChangesAsync();
		}

		public async Task RemoveFavouriteRout(Rout rout)
		{
			var tprout = GetExtDataFromRout(rout);
			if (tprout != null)
			{
				tprout.Favourite = false;
				await SaveChangesAsync();
			}
        }

		public async Task RemoveFavouriteStop(Stop stop)
		{
			var tpStop = GetExtDataFromStop(stop);
			if (tpStop != null)
			{
				tpStop.Favourite = false;
				await SaveChangesAsync();
			}
        }

		public Task AddGroup(GroupStop group)
		{
			throw new NotImplementedException();
		}

		public Task RemoveGroup(GroupStop group)
		{
			throw new NotImplementedException();
		}

		public bool IsFavouriteRout(Rout rout)
		{
			return routExtentionData.Any(x => x.Rout.RoutId == rout.RoutId);
		}
	}
}
