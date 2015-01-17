using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace MinskTrans
{
	public class RoutesModelview:ShedulerModelView
	{

		public RoutesModelview()
			:base()
		{
			OnPropertyChanged("RouteNums");
		}

		public RoutesModelview(Context context)
			:base(context)
		{
			OnPropertyChanged("RouteNums");
		}



		private string routNum;
		private IEnumerable<string> routeNums;
		private int selectedRouteNumIndex;
		private string routeNumSelectedValue;

		private ObservableCollection<Rout> routeObservableCollection;
		private int routeNamesIndex;
		private Rout routeSelectedValue;

		private List<Stop> stopsObservableCollection;
		private int stopIndex;
		
		
		private List<Time> timesObservableCollection;
		
		
		private int stopsIndex;
		private Stop stopSelectedValue;
		private string typeTransport;
		private bool curTime;

		public string TypeTransport
		{
			get
			{
				if (String.IsNullOrWhiteSpace(typeTransport))
					TypeTransport = Context.Routs[0].Transport;
				return typeTransport;
			}
			set
			{
				if (value == typeTransport) return;
				typeTransport = value;
				OnPropertyChanged();
				OnPropertyChanged("RouteNums");
			}
		}


		public IEnumerable<string> RouteNums
		{
			get
			{
				var temp = Context.Routs.Where(x => x.Transport == TypeTransport).Select(x => x.RouteNum).Distinct();
				if (RoutNum != null)
					temp = temp.Where(x => x.Contains(routNum));
				return temp;
				//return routeNums;
			}
		}

		public string RouteNumSelectedValue
		{
			get { return routeNumSelectedValue; }
			set
			{
				if (value == routeNumSelectedValue) return;
				routeNumSelectedValue = value;
				OnPropertyChanged();
				OnPropertyChanged("RouteNames");
				OnPropertyChanged("StopsObservableCollection");
				OnPropertyChanged("TimesObservableCollection");
			}
		}
		public ObservableCollection<Rout> RouteNames
		{
			get
			{
				routeObservableCollection = new ObservableCollection<Rout>(Context.Routs.Where(x => x.RouteNum == RouteNumSelectedValue));
				return routeObservableCollection;
			}
		}

		public Rout RouteSelectedValue
		{
			get { return routeSelectedValue; }
			set
			{
				if (Equals(value, routeSelectedValue)) return;
				routeSelectedValue = value;
				OnPropertyChanged();
				OnPropertyChanged("StopsObservableCollection");
				OnPropertyChanged("TimesObservableCollection");
			}
		}
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
			get { return stopSelectedValue; }
			set
			{
				if (Equals(value, stopSelectedValue)) return;
				stopSelectedValue = value;
				OnPropertyChanged();
				OnPropertyChanged("TimesObservableCollection");
			}
		}
		public int StopSelectedIndex
		{
			get { return stopsIndex; }
			set
			{
				if (value == stopsIndex) return;
				if (value < 0)
					value = 0;
				stopsIndex = value;
				OnPropertyChanged();
				OnPropertyChanged("TimesObservableCollection");
			}
		}

		public List<Time> TimesObservableCollection
		{
			get
			{
				if (RouteSelectedValue != null)
				{
					var tempList = RouteSelectedValue.Time;
					if (tempList == null)
						return null;
					timesObservableCollection = tempList.TimesDictionary[StopSelectedIndex];

					int curTime;
#if DEBUG
					curTime = DateTime.Now.Hour * 60 + DateTime.Now.Minute;
					//CurTime = true;
#else
				curTime = DateTime.Now.Hour*60 + DateTime.Now.Minute;
#endif
					if (CurTime)
						timesObservableCollection.ForEach(x =>
						{
							x.Times = x.Times.Where(d => d >= (curTime - 30)).ToList();
						});
				}
				return timesObservableCollection;
			}
		}

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

		public int SelectedRouteNumIndex
		{
			get { return selectedRouteNumIndex; }
			set
			{
				if (value < 0)
					value = 0;
				selectedRouteNumIndex = value;
				OnPropertyChanged();
				OnPropertyChanged("RouteNames");
			}
		}


		public int RouteNamesIndex
		{
			get { return routeNamesIndex; }
			set
			{
				if (value == routeNamesIndex) return;
				if (value < 0)
					value = 0;
				routeNamesIndex = value;
				OnPropertyChanged();
				OnPropertyChanged("StopsObservableCollection");
			}
		}



		

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

		public int StopIndex
		{
			get { return stopIndex; }
			set
			{
				if (value == stopIndex) return;
				stopIndex = value;
				OnPropertyChanged();
			}
		}

		public ActionCommand ShowBusCommand
		{
			get { return new ActionCommand(x=> TypeTransport = "bus");}
		}

		public ActionCommand ShowTrolCommand
		{
			get { return new ActionCommand(x => TypeTransport = "trol"); }
		}
		public ActionCommand ShowTramCommand
		{
			get { return new ActionCommand(x => TypeTransport = "tram"); }
		}
	}
}
