using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using GalaSoft.MvvmLight.Command;
using MinskTrans.DesctopClient;
using MinskTrans.DesctopClient.Modelview;
using MinskTrans.Universal.Model;

namespace MinskTrans.Universal.ModelView
{
	public class RoutsModelView:BaseModelView
	{
		private bool curTime;
		private string routNum;
		private int routeNamesIndex;
		private RoutWithDestinations routeNumSelectedValue;
		private IEnumerable<string> routeNums;

		private ObservableCollection<Rout> routeObservableCollection;
		private Rout routeSelectedValue;
		private int selectedRouteNumIndex;

		private int stopIndex;


		private Stop stopSelectedValue;
		private int stopsIndex;
		private List<Stop> stopsObservableCollection;
		//private List<Time> timesObservableCollection;
		private string typeTransport;

		//public RoutesModelview()
		//{
		//	OnPropertyChanged("RouteNums");
		//}

		public RoutsModelView(Context context)
			: base(context)
		{
			OnPropertyChanged("RouteNums");
		}

		public string TypeTransport
		{
			get
			{
				if (String.IsNullOrWhiteSpace(typeTransport))
					if (Context.Routs != null && Context.Routs.Count > 0) 
						TypeTransport = Context.Routs[0].Transport;
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
		public IEnumerable<RoutWithDestinations> RouteNums
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
						returnEnumerable.Add(new RoutWithDestinations(item, Context.Routs.Where(x => x.RouteNum == item.RouteNum && x.Transport == item.Transport && (x.RoutType.Contains("A>B") || x.RoutType.Contains("B>A")) ).Select(x => x.RouteName)));
					}
					return returnEnumerable;
				}
				return null;
			}
		}

		public RoutWithDestinations RouteNumSelectedValue
		{
			get { return routeNumSelectedValue; }
			set
			{
				//if (value == routeNumSelectedValue) return;
				routeNumSelectedValue = value;
				OnPropertyChanged();
				OnPropertyChanged("RouteNames");
				OnPropertyChanged("RouteSelectedValue");
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
						new ObservableCollection<Rout>(Context.Routs.Where(x => RouteNumSelectedValue != null && x.RouteNum == RouteNumSelectedValue.Rout.RouteNum));
				return routeObservableCollection;
			}
		}

		public Rout RouteSelectedValue
		{
			get { return routeSelectedValue; }
			set
			{
				//if (Equals(value, routeSelectedValue)) return;
				routeSelectedValue = value;
				OnPropertyChanged();
				OnPropertyChanged("TimesObservableCollection");
				OnPropertyChanged("StopSelectedIndex");
			}
		}
		#endregion

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
			get { return new RelayCommand(() => TypeTransport = "bus"); }
		}

		public RelayCommand ShowTrolCommand
		{
			get { return new RelayCommand(() => TypeTransport = "trol"); }
		}

		public RelayCommand ShowTramCommand
		{
			get { return new RelayCommand(() => TypeTransport = "tram"); }
		}

		public event Show ShowStop;
		public event Show ShowRoute;
		public delegate void Show(object sender, ShowArgs args);

		public RelayCommand ShowStopMap
		{
			get { return new RelayCommand(() => OnShowStop(new ShowArgs() { SelectedStop = StopSelectedValue }), () => StopSelectedValue != null); }
		}

		public RelayCommand ShowRouteMap
		{
			get { return new RelayCommand(() => OnShowRoute(new ShowArgs() { SelectedRoute = RouteSelectedValue }), () => RouteSelectedValue != null); }
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

		#region Overrides of BaseModelView

		public override void RefreshView()
		{
			base.RefreshView();
			OnPropertyChanged("RouteNums");
		}

		#endregion
	}
}
