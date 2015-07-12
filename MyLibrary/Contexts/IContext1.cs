using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using MinskTrans.DesctopClient.Model;


namespace MinskTrans.DesctopClient
{
	[Flags]
	public enum LoadType
	{
		LoadDB = 0x00000001,
		LoadFavourite = 0x00000002,
		LoadAll = LoadDB | LoadFavourite
	}

	public interface IContext
	{
		IList<Stop> ActualStops { get; }
		IList<Rout> FavouriteRouts { get; }
		IList<Stop> FavouriteStops { get; }
		IList<GroupStop> Groups { get; }
		DateTime LastUpdateDataDateTime { get; set; }
		IList<Rout> Routs { get; }
		IList<Stop> Stops { get; }
		IList<Schedule> Times { get; }

		event Context.EmptyDelegate ApplyUpdateEnded;
		event Context.EmptyDelegate ApplyUpdateStarted;
		event ErrorLoadingDelegate ErrorLoading;
		event Context.EmptyDelegate LoadEnded;
		event Context.EmptyDelegate LoadStarted;
		event PropertyChangedEventHandler PropertyChanged;
		event Context.EmptyDelegate UpdateEnded;
		event Context.EmptyDelegate UpdateStarted;

		bool IsFavouriteStop(Stop stop);
        void AllPropertiesChanged();
		Task ApplyUpdate(IList<Rout> newRoutes, IList<Stop> newStops, IList<Schedule> newSchedule);
		void Create(bool AutoUpdate = true);
		uint GetCounter(Stop stop);
		IEnumerable<string> GetDestinations(Rout rout);
		Task<bool> HaveUpdate(IList<Rout> newRoutes, IList<Stop> newStops, IList<Schedule> newSchedule);
		void IncrementCounter(Stop stop);
		void Inicialize(IContext cont);
		Task Load(LoadType type = LoadType.LoadAll);
		Task Recover();
		Task Save(bool saveAllDb = true);
		Task AddFavouriteRout(Rout rout);
		Task AddFavouriteStop(Stop stop);
		Task RemoveFavouriteRout(Rout rout);
		Task RemoveFavouriteStop(Stop stop);
		Task AddGroup(GroupStop group);
		Task RemoveGroup(GroupStop group);
		bool IsFavouriteRout(Rout rout);
    }
}