using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommonLibrary.Comparer;
using GalaSoft.MvvmLight.Command;
using MinskTrans.Context;
using MinskTrans.Context.Base.BaseModel;
using MinskTrans.Context.UniversalModelView;
using MinskTrans.DesctopClient.Model;
using MinskTrans.DesctopClient.Modelview;


namespace MinskTrans.Universal.ModelView
{
	public class RoutsModelView:BaseModelView
	{

	    private IExternalCommands commands;
		private bool curTime;
		private string routNum;
		//private int routeNamesIndex;
		private Rout routeNumSelectedValue;
		private IEnumerable<string> routeNums;

		private ObservableCollection<Rout> routeObservableCollection;
		private Rout routeSelectedValue;
		//private int selectedRouteNumIndex;

		//private int stopIndex;


		//private Stop stopSelectedValue;
		private int stopsIndex;
		private List<Stop> stopsObservableCollection;
		//private List<Time> timesObservableCollection;
		private TransportType typeTransport;
	    private bool isShowFavouriteRouts;

	    //public RoutesModelview()
		//{
		//	OnPropertyChanged("RouteNums");
		//}

		public RoutsModelView(IBussnessLogics context, IExternalCommands commands)
			: base(context)
		{
		    if (commands == null)
		        throw new ArgumentNullException("commands");
		    this.commands = commands;
			Context.Context.PropertyChanged += (sender, args) =>
			{
				if (args.PropertyName == "Routs")
					OnPropertyChanged("RouteNums");
			};
			OnPropertyChanged("RouteNums");
		}

		public bool ShowFavourite { get; set; }

		public bool IsFavouriteRout
		{
			get { return Context.Context.IsFavouriteRout(RouteNumSelectedValue); }
		}

		//public RelayCommand<Rout> ShowRouteMap
		//{
		//	get { return new RelayCommand<Rout>((x) => OnShowRoute(new ShowArgs() { SelectedRoute = x }), (x) => x != null); }
		//}

		public TransportType TypeTransport
		{
			get
			{
				//if (String.IsNullOrWhiteSpace(typeTransport))
				if (Context.Context.Routs != null && Context.Context.Routs.Count() > 0)
					TypeTransport = Context.Context.Routs.First().Transport;
				return typeTransport;
			}
			set
			{
				//if (value == typeTransport) return;
				typeTransport = value;
				OnPropertyChanged();
				OnPropertyChanged("RouteNums");
				OnPropertyChanged("RouteNumSelectedValue");
			}
		}

	    public bool IsShowFavouriteRouts
	    {
	        get { return isShowFavouriteRouts; }
	        set
	        {
	            isShowFavouriteRouts = value;
	            OnPropertyChanged();
                OnPropertyChanged("RouteNums");
	        }
	    }

	    #region RouteNums
		public virtual IEnumerable<RoutWithDestinations> RouteNums
		{
			get
			{
				if (Context.Routs != null)
				{
				    if (IsShowFavouriteRouts)
				    {
				        return Context.Context.FavouriteRouts.Select(x => new RoutWithDestinations(x, Context));
				    }
					IEnumerable<Rout> temp = Context.Routs.Distinct(new RoutsComparer());
					if (RoutNum != null)
						temp = temp.Where(x => x.RouteNum.Contains(routNum));
					List<RoutWithDestinations> returnEnumerable = new List<RoutWithDestinations>();
					foreach (var item in temp)
					{
						returnEnumerable.Add(new RoutWithDestinations(item, Context));
					}
					return returnEnumerable;
				}
				return null;
			}
		}

	    public IEnumerable<IGrouping<TransportType,RoutWithDestinations>> RouteNumsGroups
	    {
            get
            {
                if (Context.Routs == null) return null;
                IEnumerable<Rout> temp = Context.Routs.Distinct(new RoutsComparer());
                if (RoutNum != null)
                    temp = temp.Where(x => x.RouteNum.Contains(routNum));
                List<RoutWithDestinations> returnEnumerable = 
                    temp.Select(item => new RoutWithDestinations(item, Context)).ToList();
                return returnEnumerable.GroupBy(x => x.Transport);
            }
        }

		public Rout RouteNumSelectedValue
		{
			get { return routeNumSelectedValue; }
			set
			{
				if (value == null)
					return;
				//if (value == routeNumSelectedValue) return;
				routeNumSelectedValue = value;
			    RouteSelectedValue = null;
                OnPropertyChanged();
				OnPropertyChanged("RouteNames");
				OnPropertyChanged("IsRoutFavourite");
				
				OnPropertyChanged("StopsObservableCollection");
			}
		}
		#endregion

		#region RouteNames
		public ObservableCollection<Rout> RouteNames
		{
			get
			{
				if (Context.Routs != null)
					routeObservableCollection =
						new ObservableCollection<Rout>(Context.Routs.Where(x => RouteNumSelectedValue != null 
							&& x.RouteNum == RouteNumSelectedValue.RouteNum 
							&& x.Transport == RouteNumSelectedValue.Transport));
			    if (routeObservableCollection != null && routeObservableCollection.Count == 1)
			        RouteSelectedValue = routeObservableCollection[0];
				return routeObservableCollection;
			}
		}

		public Rout RouteSelectedValue
		{
			get { return routeSelectedValue; }
			set
			{
				//if (value == null)
				//	return;
				//if (Equals(value, routeSelectedValue)) return;
				routeSelectedValue = value;
				OnPropertyChanged();
				OnPropertyChanged("TimesObservableCollection");
                OnPropertyChanged(nameof(RoutStopsTimeTable));
				OnPropertyChanged("StopSelectedIndex");
				OnPropertyChanged("IsRoutFavourite");
			}
		}
		#endregion

		public bool IsRoutFavourite
		{
			get { return Context.Context.IsFavouriteRout(RouteNumSelectedValue); }
			set { OnPropertyChanged();}
		}

		#region Stops
		public List<Stop> StopsObservableCollection
		{
			get
			{
				if (RouteSelectedValue != null)
					stopsObservableCollection = (RouteSelectedValue.Stops);
				return stopsObservableCollection;
			}
		}

		public Stop StopSelectedValue => RouteSelectedValue?.Stops[StopSelectedIndex];

	    public int StopSelectedIndex
		{
			get { return stopsIndex; }
			set
			{
				if (value < 0 || value >= StopsObservableCollection.Count)
					value = 0;
				stopsIndex = value;
				OnPropertyChanged();
				OnPropertyChanged("StopSelectedValue");
				OnPropertyChanged("TimesObservableCollection");
			}
		}
		#endregion

		#region time

		List<Time> GetTimeCollection(Rout rout, int stopIndex)
		{
		    Schedule tempList = rout?.Time;
			if (tempList == null)
				return null;
			//получить 
			var timesObservableCollection = tempList.TimesDictionary[stopIndex];

			int curTime;
#if DEBUG
			curTime = DateTime.Now.Hour * 60 + DateTime.Now.Minute;
			//CurTime = false;
#else
				curTime = DateTime.Now.Hour*60 + DateTime.Now.Minute;
#endif
			//TODO заменяет сущь-е время
		    if (!CurTime)
                return timesObservableCollection;
		    foreach (var x in timesObservableCollection)
		    {
		        x.Times = x.Times.Where(d => d >= (curTime - 30)).ToList();
		    }

		    return timesObservableCollection;
		}

	    public struct StopTimeTable
	    {
	        public StopTimeTable(Stop stop, List<Time> timeTable)
	        {
	            Stop = stop;
	            TimeTable = timeTable;
	        }

	        public Stop Stop { get; set; }
            public List<Time> TimeTable { get; set; }
	    }

	    public class RouteStopsTimeTableList : List<StopTimeTable>
	    {
	        public RouteStopsTimeTableList(int coutn)
	            : base(coutn)
            { }
	    }

        public RouteStopsTimeTableList RoutStopsTimeTable
        {
            get
            {
                if (RouteSelectedValue == null)
                {
                    return null;
                }
                var returnValue = new RouteStopsTimeTableList(RouteSelectedValue.Stops.Count);
                returnValue.AddRange(RouteSelectedValue.Stops.Select((t, i) =>
                    new StopTimeTable(t, GetTimeCollection(RouteSelectedValue, i))));
                return returnValue;
            }
        }

        public List<KeyValuePair<Stop, List<Time>>> TimesObservableCollection
		{
			get
			{
				if (RouteSelectedValue == null)
				{
					return null;
				}
                var returnValue = new List<KeyValuePair<Stop, List<Time>>>(RouteSelectedValue.Stops.Count);
			    returnValue.AddRange(RouteSelectedValue.Stops.Select(
                    (t, i) => new KeyValuePair<Stop, List<Time>>(t, GetTimeCollection(RouteSelectedValue, i))));
			    return returnValue;
			}
		}

		#endregion

		public bool CurTime
		{
			get { return curTime; }
			set
			{
				if (value.Equals(curTime)) return;
				curTime = value;
				OnPropertyChanged();
				OnPropertyChanged("TimesObservableCollection");
                OnPropertyChanged(nameof(RoutStopsTimeTable));
			}
		}

		//public int SelectedRouteNumIndex
		//{
		//	get { return selectedRouteNumIndex; }
		//	set
		//	{
		//		if (value < 0)
		//			value = 0;
		//		selectedRouteNumIndex = value;
		//		OnPropertyChanged();
		//		OnPropertyChanged("RouteNames");
		//	}
		//}


		//public int RouteNamesIndex
		//{
		//	get { return routeNamesIndex; }
		//	set
		//	{
		//		if (value == routeNamesIndex) return;
		//		if (value < 0)
		//			value = 0;
		//		routeNamesIndex = value;
		//		OnPropertyChanged();
		//		OnPropertyChanged("StopsObservableCollection");
		//	}
		//}


		public string RoutNum
		{
			get { return routNum; }
			set
			{
				if (value == routNum) return;
				routNum = value;
				OnPropertyChanged();
				OnPropertyChanged("RouteNums");
			}
		}

		//public int StopIndex
		//{
		//	get { return stopIndex; }
		//	set
		//	{
		//		if (value == stopIndex) return;
		//		stopIndex = value;
		//		OnPropertyChanged();
		//	}
		//}

		public RelayCommand ShowBusCommand
		{
			get { return new RelayCommand(() => TypeTransport = TransportType.Bus); }
		}

		public RelayCommand ShowTrolCommand
		{
			get { return new RelayCommand(() => TypeTransport = TransportType.Trol); }
		}

		public RelayCommand ShowTramCommand
		{
			get { return new RelayCommand(() => TypeTransport = TransportType.Tram); }
		}
public RelayCommand<Rout> AddRemoveFavouriteRout
        {
			get { return new RelayCommand<Rout>((x) => Context.AddRemoveFavouriteRoute(x)); }
		}
		
        
        public RelayCommand<Rout> ShowRouteMap => commands.ShowRouteMap;

	    public RelayCommand BackCommand => commands.BackPressedCommand;

	    #region Overrides of BaseModelView

        public override void RefreshView()
		{
			base.RefreshView();
			OnPropertyChanged("RouteNums");
		}

		#endregion
	}
}
