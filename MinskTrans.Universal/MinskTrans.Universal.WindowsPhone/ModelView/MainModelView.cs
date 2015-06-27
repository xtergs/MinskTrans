using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Windows.UI.Xaml;
using BackgroundUpdateTask;
using CommonLibrary;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using MinskTrans.DesctopClient;
using MinskTrans.DesctopClient.Modelview;
using MyLibrary;

namespace MinskTrans.Universal.ModelView
{

	public class MainModelView : ViewModelBase, INotifyPropertyChanged
	{
		private static MainModelView mainModelView;
		private readonly Context context;
		private readonly GroupStopsModelView groupStopsModelView;
		private readonly RoutsModelView routesModelview;
		private readonly SettingsModelView settingsModelView;
		private readonly StopModelView stopMovelView;
		private readonly FavouriteModelView favouriteModelView;
		private readonly FindModelView findModelView;
		private MapModelView mapMOdelView;
		private readonly NewsManager newsManager;

		public static MainModelView Create(Context newContext)
		{
			if (mainModelView == null)
				mainModelView = new MainModelView(newContext);
			return mainModelView;
		}

		public static MainModelView MainModelViewGet
		{
			get { return mainModelView;}
		}

		private MainModelView(Context newContext)
		{
			context = newContext;
			settingsModelView = new SettingsModelView();
			routesModelview = new RoutsModelView(context);
			stopMovelView = new StopModelView(context, settingsModelView, true);
			groupStopsModelView = new GroupStopsModelView(context, settingsModelView);
			favouriteModelView = new FavouriteModelView(context, settingsModelView);
			findModelView = new FindModelView(context, settingsModelView);
			newsManager = new NewsManager();
			//Context.VariantLoad = SettingsModelView.VariantConnect;
			if (IsInDesignMode)
			{
				StopMovelView.FilteredSelectedStop = Context.ActualStops.First(x => x.SearchName.Contains("шепичи"));
			}
		}

		public NewsManager NewsManager
		{
			get { return newsManager;}
		}

		public MapModelView MapModelView
		{
			get { return mapMOdelView;}
			set { mapMOdelView = value; }
		}

		public SettingsModelView SettingsModelView
		{
			get { return settingsModelView; }
		}

		public FindModelView FindModelView
		{
			get { return findModelView;}
		}

		public StopModelView StopMovelView
		{
			get { return stopMovelView; }
		}

		public RoutsModelView RoutsModelView
		{
			get { return routesModelview; }
		}

		public GroupStopsModelView GroupStopsModelView
		{
			get { return groupStopsModelView; }
		}

		public FavouriteModelView FavouriteModelView
		{
			get
			{
				return favouriteModelView;
			}
		}

		public Context Context { get { return context; } }


		public List<NewsEntry> AllNews
		{
			get
			{
				if (NewsManager != null)
				{
#if DEBUG
					var xxx = NewsManager.AllHotNews.Concat(newsManager.NewNews).ToList();
#endif
					return NewsManager.AllHotNews.Concat(newsManager.NewNews).OrderByDescending(key => key.Posted).ThenByDescending(key=> key.RepairedLIne).ToList();
				}
				return null;
			}
			set
			{
				
				var handle = PropertyChangedHandler;
				if (handle != null)
					PropertyChangedHandler.Invoke(this, new PropertyChangedEventArgs("AllNews"));
			}
		}

	}
}