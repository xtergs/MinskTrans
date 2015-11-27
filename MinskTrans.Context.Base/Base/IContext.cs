﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using MinskTrans.Context.Base.BaseModel;

namespace MinskTrans.Context.Base
{
	[Flags]
	public enum LoadType
	{
		LoadDB = 0x00000001,
		LoadFavourite = 0x00000002,
		LoadAll = LoadDB | LoadFavourite
	}

	public interface IContext : INotifyPropertyChanged
	{
		IEnumerable<Stop> ActualStops { get; }
		IList<Rout> FavouriteRouts { get; }
		IList<Stop> FavouriteStops { get; }
		IList<GroupStop> Groups { get; }
		DateTime LastUpdateDataDateTime { get; set; }
		IList<Rout> Routs { get; }
		IList<Stop> Stops { get; }
		IList<Schedule> Times { get; }

		event EventHandler<EventArgs> ApplyUpdateEnded;
		event EventHandler<EventArgs> ApplyUpdateStarted;
        event EventHandler<EventArgs> UpdateStarted;
        event EventHandler<EventArgs> UpdateEnded;
        event EventHandler<EventArgs> LoadStarted;
        event EventHandler<EventArgs> LoadEnded;
        event ErrorLoadingDelegate ErrorLoading;



        bool IsFavouriteStop(Stop stop);
		void AllPropertiesChanged();
		Task ApplyUpdate(IEnumerable<Rout> newRoutes, IList<Stop> newStops, IList<Schedule> newSchedule);
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
		getStop GetStopDelegate { get; }
        getDirection GetStopDirectionDelegate { get; }
	   
	}

    public delegate void ErrorLoadingDelegate(object sender, ErrorLoadingDelegateArgs args);

    public delegate Stop getStop(int stopID);
    public delegate IEnumerable<Stop> getDirection(int stopID, int count);

    public class ErrorLoadingDelegateArgs : EventArgs
    {
        public enum Errors
        {
            NoFileToDeserialize,
            NoSourceFiles
        }

        public Errors Error { get; set; }
    }
}