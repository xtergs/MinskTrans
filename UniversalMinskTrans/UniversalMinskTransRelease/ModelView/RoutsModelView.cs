using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using CommonLibrary.Comparer;
using GalaSoft.MvvmLight.Command;
using MinskTrans.Context.Base;
using MinskTrans.Context.Base.BaseModel;
using MinskTrans.DesctopClient;
using MinskTrans.DesctopClient.Model;
using MinskTrans.DesctopClient.Modelview;


namespace MinskTrans.Universal.ModelView
{
	public class RoutsModelView:BaseModelView
	{

		
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

		//public RoutesModelview()
		//{
		//	OnPropertyChanged("RouteNums");
		//}

		public RoutsModelView(IContext context)
			: base(context)
		{
			Context.PropertyChanged += (sender, args) =>
			{
				if (args.PropertyName == "Routs")
					OnPropertyChanged("RouteNums");
			};
			OnPropertyChanged("RouteNums");
		}

		public bool ShowFavourite { get; set; }

		public bool IsFavouriteRout
		{
			get { return Context.IsFavouriteRout(RouteNumSelectedValue); }
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
				if (Context.Routs != null && Context.Routs.Count() > 0)
					TypeTransport = Context.Routs.First().Transport;
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

		#region RouteNums
		public virtual IEnumerable<RoutWithDestinations> RouteNums
		{
			get
			{
				if (Context.Routs != null)
				{
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
                if (Context.Routs != null)
                {
                    IEnumerable<Rout> temp = Context.Routs.Distinct(new RoutsComparer());
                    if (RoutNum != null)
                        temp = temp.Where(x => x.RouteNum.Contains(routNum));
                    List<RoutWithDestinations> returnEnumerable = new List<RoutWithDestinations>();
                    foreach (var item in temp)
                    {
                        returnEnumerable.Add(new RoutWithDestinations(item, Context));
                    }
                    return returnEnumerable.GroupBy(x => x.Transport);
                }
                return null;
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
				OnPropertyChanged("StopSelectedIndex");
				OnPropertyChanged("IsRoutFavourite");
			}
		}
		#endregion

		public bool IsRoutFavourite
		{
			get { return Context.IsFavouriteRout(RouteNumSelectedValue); }
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

		public Stop StopSelectedValue
		{
			get
			{
				if (RouteSelectedValue != null)
					return RouteSelectedValue.Stops[StopSelectedIndex];
				return null;
			}
		}

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
			if (rout == null)
			{
				return null;
			}
			Schedule tempList = rout.Time;
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
			if (CurTime)
				foreach (var x in timesObservableCollection)
				{
					x.Times = x.Times.Where(d => d >= (curTime - 30)).ToList();
				}

			return timesObservableCollection;
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
				for (int i = 0; i < RouteSelectedValue.Stops.Count; i++)
					returnValue.Add(new KeyValuePair<Stop, List<Time>>(RouteSelectedValue.Stops[i], GetTimeCollection(RouteSelectedValue, i)));
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

		//public event Show ShowStop;
		//public event Show ShowRoute;
		//public delegate void Show(object sender, ShowArgs args);

		//public RelayCommand ShowStopMap
		//{
		//	get { return new RelayCommand(() => OnShowStop(new ShowArgs() { SelectedStop = StopSelectedValue }), () => StopSelectedValue != null); }
		//}

		//public RelayCommand ShowRouteMap
		//{
		//	get { return new RelayCommand(() => OnShowRoute(new ShowArgs() { SelectedRoute = RouteSelectedValue }), () => RouteSelectedValue != null); }
		//}

		//protected virtual void OnShowStop(ShowArgs args)
		//{
		//	var handler = ShowStop;
		//	if (handler != null) handler(this, args);
		//}

		//protected virtual void OnShowRoute(ShowArgs args)
		//{
		//	var handler = ShowRoute;
		//	if (handler != null) handler(this, args);
		//}

		#region Overrides of BaseModelView

		public override void RefreshView()
		{
			base.RefreshView();
			OnPropertyChanged("RouteNums");
		}

		#endregion
	}
}
