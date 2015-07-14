﻿


namespace MinskTrans.DesctopClient.Modelview
{
	using System.ComponentModel;
	using System.Runtime.CompilerServices;
#if !WINDOWS_PHONE_APP && !WINDOWS_UAP
	using MinskTrans.DesctopClient.Annotations;
	using GalaSoft.MvvmLight.CommandWpf;
	using MinskTrans.DesctopClient.Model;
	using System.Text;
	using System.Linq;
#else
	using MinskTrans.Universal.Annotations;
	using GalaSoft.MvvmLight.Command;
	using MinskTrans.DesctopClient.Model;
	using System.Text;
	using System.Linq;

#endif
	public class BaseModelView : INotifyPropertyChanged
	{
		protected IContext context;
		//protected readonly ISettingsModelView settingsModelView;

		//public BaseModelView()
		//	: this(null)
		//{
		//}

		public BaseModelView(IContext newContext)
		{
			context = newContext;
			
		}

		protected BaseModelView()
		{ }

		public string TransportToString(Stop stop, TransportType type)
		{
			switch (type)
			{
				case TransportType.Bus:
					{
						var temp = stop.Routs.Where(rout => rout.Transport == TransportType.Bus).Select(rout => rout.RouteNum).Distinct().ToList();
						if (temp.Count == 0)
							return "";
						StringBuilder builder = new StringBuilder("Авт: ");
						foreach (var rout in temp)
						{
							builder.Append(rout).Append(", ");
						}
						//builder.Append(temp.Select(x => x.RouteNum + ", ").ToList());
						builder.Remove(builder.Length - 2, 2);
						return builder.ToString();
					}
				case TransportType.Tram:
					{
						var temp = stop.Routs.Where(rout => rout.Transport == TransportType.Tram).Select(rout => rout.RouteNum).Distinct().ToList();
						if (temp.Count == 0)
							return "";
						StringBuilder builder = new StringBuilder("Трам: ");
						foreach (var rout in temp)
						{
							builder.Append(rout).Append(", ");
						}
						//builder.Append(temp.Select(x => x.RouteNum + ", "));
						builder.Remove(builder.Length - 2, 2);

						return builder.ToString();
					}
				case TransportType.Metro:
					{
						var temp = stop.Routs.Where(rout => rout.Transport == TransportType.Metro).Select(rout => rout.RouteNum).Distinct().ToList();
						if (temp.Count == 0)
							return "";
						StringBuilder builder = new StringBuilder("Метро: ");
						foreach (var rout in temp)
						{
							builder.Append(rout).Append(", ");
						}
						//builder.Append(temp.Select(x => x.RouteNum + ", "));
						builder.Remove(builder.Length - 2, 2);
						return builder.ToString();
					}
				case TransportType.Trol:
					{
						var temp = stop.Routs.Where(rout => rout.Transport == TransportType.Trol).Select(rout => rout.RouteNum).Distinct().ToList();
						if (temp.Count == 0)
							return "";
						StringBuilder builder = new StringBuilder("трол: ");
						foreach (var rout in temp)
						{
							builder.Append(rout).Append(", ");
						}
						//builder.Append(temp.Select(x => x.RouteNum + ", "));
						builder.Remove(builder.Length - 2, 2);
						return builder.ToString();
					}
			}
			return "";
		}

		public virtual void RefreshView()
		{
			
		}

		

		RelayCommand<Rout> addFavouriteRoutCommandBack;
		public RelayCommand<Rout> AddFavouriteRoutCommand
		{
			get
			{
				if (addFavouriteRoutCommandBack == null)
					addFavouriteRoutCommandBack = new RelayCommand<Rout>(async x =>
				   {
					   await Context.AddFavouriteRout(x);
				   }, p => p != null && !Context.IsFavouriteRout(p));
				return addFavouriteRoutCommandBack;
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

			  , p => p != null && Context.FavouriteStops != null && !Context.FavouriteStops.Contains(p));
			}
		}
		public RelayCommand<RoutWithDestinations> RemoveFavouriteRoutCommand
		{
			get
			{
				return new RelayCommand<RoutWithDestinations>(async x =>
				{
					await Context.RemoveFavouriteRout(x);
				}, p => p != null && Context.IsFavouriteRout(p));
			}
		}

		public RelayCommand<Stop> RemoveFavouriteSopCommand
		{
			get
			{
				return new RelayCommand<Stop>(async x =>
				{
					await Context.RemoveFavouriteStop(x);
				}, p => p != null && Context.FavouriteStops.Contains(p));
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

		//public ISettingsModelView SettingsModelView
		//{
		//	get { return settingsModelView;}
		//}

		public IContext Context
		{
			get { return context; }
		}

		public virtual void Refresh()
		{
			
		}


		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			var handler = PropertyChanged;
			if (handler == null)
				return;
			if (handler != null)
				handler(this, new PropertyChangedEventArgs(propertyName));
		}

		
	}
}