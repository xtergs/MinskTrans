using GalaSoft.MvvmLight.Command;
using MinskTrans.DesctopClient.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinskTrans.DesctopClient
{
	public abstract class TimeTableRepositoryBase
	{
		readonly IContext context;
		public TimeTableRepositoryBase(IContext context)
		{
			this.context = context;
		}

		public IContext Context { get { return context; } }

		public abstract string TransportToString(Stop stop, TransportType type);

		public IList<RoutWithDestinations> FavouriteRouts
		{
			get
			{
				return Context.FavouriteRouts;
			}
		}

		public IList<Stop> ActualStops
		{
			get
			{
				return Context.ActualStops;
			}
		}

		public IList<Stop> FavouriteStops
		{
			get
			{
				return Context.FavouriteStops;
			}
		}
		public IList<GroupStop> Groups
		{
			get
			{
				return Context.Groups;
			}
		}

		public IList<Schedule> Times
		{
			get { return Context.Times; }
		}

		public IList<Stop> Stops
		{
			get { return Context.ActualStops; }
		}


		public IList<Rout> Routs
		{
			get { return Context.Routs; }
		}

		public uint GetCounter(Stop stop)
		{
			return Context.GetCounter(stop);
		}




		public abstract bool  RoutsHaveStopId(int stopId);

		public RelayCommand<RoutWithDestinations> AddFavouriteRoutCommand
		{
			get
			{
				return new RelayCommand<RoutWithDestinations>(async x =>
				{
					await Context.AddFavouriteRout(x);
				}, p => p != null && !FavouriteRouts.Contains(p));
			}
		}

		public RelayCommand<Stop> AddFavouriteSopCommand
		{
			get
			{
				return new RelayCommand<Stop>(async x =>
				{
					await Context.AddFavouriteStop(x);
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
					await Context.RemoveFavouriteRout(x);
				}, p => p != null && FavouriteRouts.Contains(p));
			}
		}

		public RelayCommand<Stop> RemoveFavouriteSopCommand
		{
			get
			{
				return new RelayCommand<Stop>(async x =>
				{
					await Context.RemoveFavouriteStop(x);
				}, p => p != null && FavouriteStops.Contains(p));
			}
		}

		public RelayCommand<Stop> AddRemoveFavouriteStop
		{
			get
			{
				return new RelayCommand<Stop>(async x =>
				{
					if (Context.IsFavouriteStop(x))
						await Context.RemoveFavouriteStop(x);
					else
						await Context.AddFavouriteStop(x);

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
					if (Context.IsFavouriteRout(x))
						await Context.RemoveFavouriteRout(x);
					else
						await Context.AddFavouriteRout(x);

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
					await Context.AddGroup(new GroupStop() { Name = x });
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
						await Context.RemoveGroup(x);
					}
				});
			}
		}
	}
}
