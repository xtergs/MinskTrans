using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using MinskTrans.Context.Base;
using MinskTrans.Context.Base.BaseModel;
using Microsoft.Data.Entity;
using Microsoft.Data.Entity.Metadata;
using MinskTrans.Net;

namespace MinskTrans.Context.Desktop
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

	public enum TypeNews
	{
		HotNews, MainNews
	}

	public class NewsEntryEx:NewsEntry
	{
		[Key]
		public int NewsEntryId { get; set; }

		public TypeNews Type { get; set; }

		public NewsEntryEx Fill(NewsEntry entry, TypeNews type)
		{
			this.Message = entry.Message;
			this.PostedUtc = entry.PostedUtc;
			this.CollectedUtc = entry.CollectedUtc;
			this.RepairedLineUtc = entry.RepairedLineUtc;
			Type = type;
			return this;
		}
	}

	public class NewsSqlContext : DbContext, INewsContext
	{
		public  DbSet<NewsEntryEx> News { get; set; }

		public NewsSqlContext()
		{
			
		}

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			//optionsBuilder.UseSqlite("Filename=Sensors.db");
		}

		public ListWithDate MainNews { get; private set; }
		public ListWithDate HotNews { get; private set; }
		public void LoadData()
		{
			var main = News.Where(x => x.Type == TypeNews.MainNews).Select(x => (NewsEntry) x).ToList();
			MainNews = new ListWithDate() {NewsEntries = main, LastUpdateDateTimeUtc = DateTime.MinValue};
			main = News.Where(x => x.Type == TypeNews.HotNews).Select(x => (NewsEntry)x).ToList();
			HotNews = new ListWithDate() { NewsEntries = main, LastUpdateDateTimeUtc = DateTime.MinValue };
		}

		public void Save()
		{
			for (int i = 0; i < MainNews.NewsEntries.Count; i++)
			{
				var n = MainNews.NewsEntries[i];
				var mes = News.FirstOrDefault(x => x.Message == n.Message);
				if (mes != null)
				{
					mes.Fill(n, TypeNews.MainNews);
					News.Update(mes);
				}
				else
				{
					News.Add(new NewsEntryEx().Fill(n, TypeNews.MainNews));
				}

			}
			for (int i = 0; i < HotNews.NewsEntries.Count; i++)
			{
				var n = HotNews.NewsEntries[i];
				var mes = News.FirstOrDefault(x => x.Message == n.Message);
				if (mes != null)
				{
					mes.Fill(n, TypeNews.HotNews);
					News.Update(mes);
				}
				else
				{
					News.Add(new NewsEntryEx().Fill(n, TypeNews.HotNews));
				}

			}

			SaveChanges();
		}
	}

//	public class SqlEFContext : DbContext, IContext, IDisposable
//	{
//		public DbSet<RoutExtentionData> routExtentionData { get; set; }

//		public DbSet<Rout> routsEF { get; set; }
//		public DbSet<Stop> stopsEF { get; set; }
//		public DbSet<Schedule> timesEF { get; set; }

//		public DbSet<StopExtentionData> stopsExtentionData { get; set; }
//		public DbSet<SettingsDataBase> settingsDataBase { get; set; }

//		public DbSet<GroupEx> groupsEF { get; set; }

//		public SqlEFContext(string connectionString)
//			:base(connectionString)
//		{
//		    this.Configuration.LazyLoadingEnabled = false;

           

//        }

//	    private IEnumerable<Stop> GetStopDirectionDelegatee(int stopId, int count)
//	    {
//	        using (SqlEFContext context = new SqlEFContext(this.Database.Connection.ConnectionString))
//	        {
//                SqlParameter param = new SqlParameter("@StopID", stopId);
//                SqlParameter param1 = new SqlParameter("@Count", stopId);
//                return context.Database.SqlQuery<Stop>("dbo.GetStopDirections @StopID, @Count", param, param1).ToList();
//	        }
//	    }

//		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
//		{
//			optionsBuilder.UseSqlite("Filename=Sensors.db");
//		}

//		protected override void OnModelCreating(ModelBuilder modelBuilder)
//		{
//            //GetStopDirectionDelegate = (stopId, count) => GetStopDirectionDelegatee(stopId, count);

            

//            modelBuilder.Entity<Schedule>().HasKey(x => x.RoutId);
//		    modelBuilder.Entity<Schedule>().Property(x => x.RoutId).HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);
//			modelBuilder.Entity<Schedule>().Property(x=> x.InicializeString);
            

//			modelBuilder.Entity<Rout>().HasOptional(key => key.Time).WithOptionalDependent(key => key.Rout).WillCascadeOnDelete();
//			modelBuilder.Entity<Rout>().HasMany(key => key.Stops).WithMany(key => key.Routs).MapToStoredProcedures();
//            modelBuilder.Entity<Rout>().Property(x => x.RoutId).HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

//            modelBuilder.Entity<Stop>().HasMany(t=> t.Stops).WithMany().MapToStoredProcedures();
//            modelBuilder.Entity<Stop>().Property(x => x.ID).HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

//            modelBuilder.Entity<StopExtentionData>().HasKey(x => x.StopID);
//			modelBuilder.Entity<StopExtentionData>().HasRequired(x => x.Stop);

//			modelBuilder.Entity<RoutExtentionData>().HasKey(x => x.RoutID);
//			modelBuilder.Entity<RoutExtentionData>().HasRequired(x => x.Rout).WithOptional().Map(x => { x.MapKey("aaa"); });

//			modelBuilder.Entity<GroupEx>().HasKey(x => x.GroupID);
//			modelBuilder.Entity<GroupEx>().HasMany(x => x.Stops);

			
//		}

//		public IEnumerable<Stop> ActualStops
//		{
//			get
//			{
//				return Stops;
//			}
//		}

//		public IList<Rout> FavouriteRouts
//		{
//			get
//			{
//				if (routExtentionData == null)
//					return null;
//				return routsEF.Where(x=> routExtentionData.Any(d=> d.Rout==x)).ToList().ToList();
//			}
//		}

//		public IList<Stop> FavouriteStops
//		{
//			get
//			{
//				return stopsEF.Where(stop=> stopsExtentionData.Any(x=> x.StopID == stop.ID)).ToList();
//			}
//		}

//		public IList<GroupStop> Groups
//		{
//			get
//			{
//				if (groupsEF == null)
//					return null;
//				return groupsEF.ToList<GroupStop>();
//			}
//		}

//		public DateTime LastUpdateDataDateTime
//		{
//			get
//			{
//				if (settingsDataBase == null)
//					return default(DateTime);
//				var tempData = settingsDataBase.FirstOrDefault(d => d.SettingsDataBaseID == settingsDataBase.Max(x => x.SettingsDataBaseID));
//				if (tempData == null)
//					return default(DateTime);
//				return tempData.LastUpdateDateTimeUtc;
//			}

//			set
//			{
//				settingsDataBase.Add(new SettingsDataBase() { LastUpdateDateTimeUtc = value });
//				SaveChanges();
//			}
//		}

//		public Rout[] Routs
//		{
//		    get
//		    {
//		        if (routsEF.Local.Count == 0)
//		            routsEF.Load();
//		        return routsEF.Local.ToArray();
//		    }
//		}
        
//		public Stop[] Stops{get
//		{
//		    if (stopsEF.Count() == 0)
//		    {
//		        stopsEF.Include(x => x.Routs).Load();
//		        timesEF.Load();
//		    }
//		    return stopsEF.ToArray();

//		}
//        }

//		public Schedule[] Times { get
//		{
//		    if (timesEF.Count() == 0)
//		        timesEF.Load();
//            return timesEF.ToArray();
			
//		} }

//		public event EventHandler<EventArgs> ApplyUpdateEnded;
//		public event EventHandler<EventArgs> ApplyUpdateStarted;
//		public event ErrorLoadingDelegate ErrorLoading;
//		public event EventHandler<EventArgs> LoadEnded;
//		public event EventHandler<EventArgs> LoadStarted;


//		public event PropertyChangedEventHandler PropertyChanged;
//		public event EventHandler<EventArgs> UpdateEnded;
//		public event EventHandler<EventArgs> UpdateStarted;


//		public void AllPropertiesChanged()
//		{
//			//throw new NotImplementedException("AllPropertiesChanged");
//		}

//		static protected void Connect(/*[NotNull]*/ IList<Rout> routsl,/* [NotNull]*/ IList<Stop> stopsl,
//			/*[NotNull]*/ IList<Schedule> timesl)
//		{

//#if BETA
//			Logger.Log("Connect started");
//			Debug.WriteLine("Connect Started");
//#endif
//			//Stopwatch watch = new Stopwatch();
//			//watch.Start();

//			if (routsl == null) throw new ArgumentNullException("routsl");
//			if (stopsl == null) throw new ArgumentNullException("stopsl");
//			if (timesl == null) throw new ArgumentNullException("timesl");

//			//Parallel.ForEach(routsl, (rout) =>

//			//foreach (var stop in stopsl)
//			//{
//			//	stop.Routs = new List<Rout>(5);
//			//}

//			//Parallel.ForEach(routsl, (rout) =>
//			foreach (var rout in routsl)
//			{
//				var rout1 = rout;
//				//Schedule first = timesl.Where(x =>
//				//{
//				//	if (x == null)
//				//		return false;
//				//	return x.RoutId == rout1.RoutId;
//				//}).FirstOrDefault();
//				//rout.Time = first;
//				//if (rout.Time != null)
//				//	rout.Time.Rout = rout;


//				rout1.Stops = rout1.RouteStops.Join(stopsl, i => i, stop => stop.ID, (i, stop) =>
//				{
//					if (stop.Routs == null)
//						stop.Routs = new List<Rout>();
//					stop.Routs.Add(rout1);
//					return stop;
//				}).ToList();
//			}
//			//watch.Stop();
//			//var xx = watch.ElapsedMilliseconds;
//#if BETA
//			string message = "Connect Ended, " + "Milliseconds: " + xx.ToString();
//			Debug.WriteLine(message);
//			Logger.Log(message);
//#endif
//		}

//		public async Task ApplyUpdate(IEnumerable<Rout> newRoutes, IList<Stop> newStops, IList<Schedule> newSchedule)
//		{
//            //Connect(newRoutes, newStops, newSchedule);
//            //routsEF.RemoveRange(Routs);
//            //stopsEF.RemoveRange(stopsEF);
//            //timesEF.RemoveRange(timesEF);

//		    using (var trans = this.Database.BeginTransaction())
//		    {

//		        timesEF.RemoveRange(timesEF.ToList());
//		        routsEF.RemoveRange(routsEF.ToList());
//		        stopsEF.RemoveRange(stopsEF.ToList());
//		    //await SaveChangesAsync();
//            routsEF.AddRange(newRoutes.AsParallel());
//			stopsEF.AddRange(newStops.AsParallel());
//			timesEF.AddRange(newSchedule.AsParallel());
//			//SaveChanges();
//		        SaveChanges();
//                trans.Commit();
//		    }
//		}

//		StopExtentionData GetExtDataFromStop(Stop stop)
//		{
//			return stopsExtentionData.FirstOrDefault(x => x.Stop.ID == stop.ID);
//		}

//		RoutExtentionData GetExtDataFromRout(Rout rout)
//		{
//			return routExtentionData.FirstOrDefault(x => x.Rout.RoutId == rout.RoutId);
//		}

//		StopExtentionData GetOrCreateExtDataFromStop(Stop stop)
//		{
//			var stopData = GetExtDataFromStop(stop);
//			if (stopData != null)
//				return stopData;
//			stopData = stopsExtentionData.Add(new StopExtentionData() { Stop = stop, StopID = stop.ID });
//			return stopData;
//		}

//		public void Create(bool AutoUpdate = true)
//		{
//			//throw new NotImplementedException();
//		}

//		public uint GetCounter(Stop stop)
//		{
//			var stopData = GetExtDataFromStop(stop);
//			if (stopData == null)
//				return 0;
//			return stopData.Counter;
//		//return stopsEF.First(id => id.ID == stop.ID).Counter;
//		}

//		public IEnumerable<string> GetDestinations(Rout rout)
//		{
//			throw new NotImplementedException("GetDestinations");
//		}

//		public async Task<bool> HaveUpdate(IList<Rout> newRoutes, IList<Stop> newStops, IList<Schedule> newSchedule)
//		{
//			Connect(newRoutes, newStops, newSchedule);
//			newStops = new List<Stop>(newStops.Where(stop => stop.Routs.Any()));
//			newRoutes = new List<Rout>(newRoutes.Where(rout => rout.Stops.Any()));
//			return NeedUpdate(newRoutes, newStops, newSchedule);
//		}

//		protected bool NeedUpdate(IList<Rout> newRoutes, IList<Stop> newStops, IList<Schedule> newSchedule)
//		{
//			if (Stops == null || Routs == null || Times == null || !Stops.Any() || !Routs.Any() || !Times.Any())
//				return true;
//#if DEBUG
//			var xx = newSchedule.Except(Times).ToList();
//#endif
//			if (newStops.Count != Stops.Count() || newRoutes.Count != Routs.Length || newSchedule.Count != Times.Length)
//				return true;

//			foreach (var newRoute in newRoutes)
//			{
//				if (Routs.AsParallel().All(x => x.RoutId == newRoute.RoutId && x.Datestart != newRoute.Datestart))
//					return true;
//			}
//			return false;
//		}

//		public void IncrementCounter(Stop stop)
//		{
//			var stopData = GetOrCreateExtDataFromStop(stop);
//			stopData.Counter++;
//			//stopsEF.First(st => st.ID == stop.ID).Counter++;
//			SaveChanges();
//		}

//		public void Inicialize(IContext cont)
//		{
//			throw new NotImplementedException("Inicialize");
//		}

//		public async Task Load(LoadType type = LoadType.LoadAll)
//		{
//			//throw new NotImplementedException();
//		}

//		public Task Recover()
//		{
//			throw new NotImplementedException("Recover");
//		}

//		public async Task Save(bool saveAllDb = true)
//		{
//			await SaveChangesAsync();
//			//throw new NotImplementedException();
//		}

//		public bool IsFavouriteStop(Stop stop)
//		{
//			return stopsExtentionData.Any(x => x.Stop == stop);
//		}

//		public async Task AddFavouriteRout(Rout rout)
//		{
//			var tprout = GetExtDataFromRout(rout);
//			if (tprout != null)
//				tprout.Favourite = true;
//			else
//				routExtentionData.Add(new RoutExtentionData() { Rout = rout, Favourite = true });
//			await SaveChangesAsync();
//		}

//		public async Task AddFavouriteStop(Stop stop)
//		{
//			var tpStop = GetExtDataFromStop(stop);
//			if (tpStop != null)
//				tpStop.Favourite = true;
//			else
//				stopsExtentionData.Add(new StopExtentionData() { Stop = stop, Favourite = true });
//			await SaveChangesAsync();
//		}

//		public async Task RemoveFavouriteRout(Rout rout)
//		{
//			var tprout = GetExtDataFromRout(rout);
//			if (tprout != null)
//			{
//				tprout.Favourite = false;
//				await SaveChangesAsync();
//			}
//		}

//		public async Task RemoveFavouriteStop(Stop stop)
//		{
//			var tpStop = GetExtDataFromStop(stop);
//			if (tpStop != null)
//			{
//				tpStop.Favourite = false;
//				await SaveChangesAsync();
//			}
//		}

//		public Task AddGroup(GroupStop group)
//		{
//			throw new NotImplementedException("AddGroup");
//		}

//		public Task RemoveGroup(GroupStop group)
//		{
//			throw new NotImplementedException("RemoveGroup");
//		}

//		public bool IsFavouriteRout(Rout rout)
//		{
//			return routExtentionData.Any(x => x.Rout.RoutId == rout.RoutId);
//		}

	   
//	    public getDirection GetStopDirectionDelegate { get; protected set; }

//	    public Stop GetStop(int id)
//	    {
//	        return ActualStops.FirstOrDefault(x => x.ID == id);
//	    }

//	    public getStop GetStopDelegate { get; protected set; }

	    
//	}
}
