

using System.Xml.Linq;
using MinskTrans.DesctopClient.Model;

namespace MinskTrans.DesctopClient.Modelview
{
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
#if !WINDOWS_PHONE_APP && !WINDOWS_AP
using GalaSoft.MvvmLight.CommandWpf;
#else
using GalaSoft.MvvmLight.Command;
#endif
	public class RoutesModelview : BaseModelView
	{
		private bool curTime;
		private string routNum;
		private int routeNamesIndex;
		private string routeNumSelectedValue;
		private IEnumerable<string> routeNums;

		private ObservableCollection<Rout> routeObservableCollection;
		private Rout routeSelectedValue;
		private int selectedRouteNumIndex;

		private int stopIndex;


		private Stop stopSelectedValue;
		private int stopsIndex;
		private List<Stop> stopsObservableCollection;
		//private List<Time> timesObservableCollection;
		private TransportType typeTransport = TransportType.Bus;

		//public RoutesModelview()
		//{
		//	OnPropertyChanged("RouteNums");
		//}

		public RoutesModelview(Context context)
			: base(context)
		{
			OnPropertyChanged("RouteNums");
		}

		public TransportType TypeTransport
		{
			get
			{
				//if (String.IsNullOrWhiteSpace(typeTransport))
					if (typeTransport == TransportType.None && Context.Routs != null && Context.Routs.Count > 0) 
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
		public IEnumerable<string> RouteNums
		{
			get
			{
				if (Context.Routs != null)
				{
					IEnumerable<string> temp = Context.Routs.Where(rout=> rout.Transport == TypeTransport).Select(x => x.RouteNum).Distinct();
					if (RoutNum != null)
						temp = temp.Where(x => x.Contains(routNum));
					if (temp.Any())
						RouteNumSelectedValue = temp.First();
					return temp;
				}
				//return routeNums;
				return null;
			}
		}

		public string RouteNumSelectedValue
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
						new ObservableCollection<Rout>(Context.Routs.Where(x => x.RouteNum == RouteNumSelectedValue && x.Transport == TypeTransport));
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
				OnPropertyChanged("StopsObservableCollection");
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

		public List<Time> TimesObservableCollection
		{
			get
			{
				if (RouteSelectedValue == null)
				{
					return null;
				}
				Schedule tempList = RouteSelectedValue.Time;
				if (tempList == null)
					return null;
				//получить 
				var timesObservableCollection = tempList.TimesDictionary[StopSelectedIndex];

				int curTime;
#if DEBUG
					curTime = DateTime.Now.Hour*60 + DateTime.Now.Minute;
					CurTime = true;
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