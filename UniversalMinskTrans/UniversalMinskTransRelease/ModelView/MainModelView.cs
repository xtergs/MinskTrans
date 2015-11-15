using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Windows.UI.Xaml;
using CommonLibrary;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using MinskTrans.DesctopClient;
using MinskTrans.DesctopClient.Modelview;
using MyLibrary;
using MinskTrans.DesctopClient;
using Autofac;
using CommonLibrary.IO;
using MinskTrans.Context;
using MinskTrans.Context.Base;
using MinskTrans.Context.Base.BaseModel;
using MinskTrans.DesctopClient.Model;
using MinskTrans.Net;
using MinskTrans.Net.Base;
using MinskTrans.Utilites;
using MinskTrans.Utilites.Base.IO;
using MinskTrans.Utilites.Base.Net;
using UniversalMinskTransRelease.Context;
using UniversalMinskTransRelease.ModelView;

namespace MinskTrans.Universal.ModelView
{

	public class MainModelView : BaseModelView, INotifyPropertyChanged
	{
		private static MainModelView mainModelView;
		//private readonly IContext context;
	    private FindModelView findModelView;
	    private readonly NewsManagerBase newsManager;


	    public UpdateManagerBase UpdateManager { get; }


	    public static MainModelView MainModelViewGet
		{
			get
			{
				if (mainModelView == null)
					mainModelView = new MainModelView();
				return mainModelView;}
		}

		private MainModelView()
		{
            var builder = new ContainerBuilder();
            builder.RegisterType<FileHelper>().As<FileHelperBase>().SingleInstance();
            //builder.RegisterType<SqliteContext>().As<IContext>().SingleInstance();
            builder.RegisterType<Context.Context>().As<IContext>().SingleInstance();
            builder.RegisterType<UpdateManagerBase>();
            builder.RegisterType<ShedulerParser>().As<ITimeTableParser>();
            builder.RegisterType<InternetHelperUniversal>().As<InternetHelperBase>();
            builder.RegisterType<NewsManager>().As<NewsManagerBase>();
		    builder.RegisterType<BussnessLogic>().As<IBussnessLogics>().SingleInstance();
		    builder.RegisterType<UniversalGeolocator>().As<IGeolocation>().SingleInstance();
		    builder.RegisterType<SettingsModelView>().As<ISettingsModelView>().SingleInstance();
		    builder.RegisterType<UniversalApplicationSettingsHelper>().As<IApplicationSettingsHelper>();

            var container = builder.Build();

            context = container.Resolve<IBussnessLogics>();
            newsManager = container.Resolve<NewsManagerBase>();
            UpdateManager = container.Resolve<UpdateManagerBase>();

            //var fileHelper = new FileHelper();
            //var internetHelper = new InternetHelperUniversal(fileHelper);
            //context = new UniversalContext(fileHelper, internetHelper);
            //updateManager = new UpdateManagerUniversal(fileHelper, internetHelper);

            SettingsModelView = container.Resolve<ISettingsModelView>();
			//routesModelview = new RoutsModelView(context);
			//stopMovelView = new StopModelView(context, settingsModelView, true);
			GroupStopsModelView = new GroupStopsModelView(context, SettingsModelView);
			FavouriteModelView = new FavouriteModelView(context, SettingsModelView);
		    NewsModelView = new NewsModelView(NewsManager);
		}

		//private MainModelView(Context newContext)
		//{
		//	context = newContext;
		//	settingsModelView = new SettingsModelView();
		//	//routesModelview = new RoutsModelView(context);
		//	//stopMovelView = new StopModelView(context, settingsModelView, true);
		//	groupStopsModelView = new GroupStopsModelView(context, settingsModelView);
		//	favouriteModelView = new FavouriteModelView(context, settingsModelView);
			
		//	newsManager = new NewsManager();
		//	//Context.VariantLoad = SettingsModelView.VariantConnect;
			
		//}

		public NewsManagerBase NewsManager
		{
			get { return newsManager;}
		}

		public MapModelView MapModelView { get; set; }

	    public ISettingsModelView SettingsModelView { get; }

	    public FindModelView FindModelView
		{
			get
			{
				if (findModelView == null)
					findModelView = new FindModelView(context, SettingsModelView, true);
				return findModelView;
			}
		}

		public StopModelView StopMovelView { get; }

	    public RoutsModelView RoutsModelView { get; }

	    public GroupStopsModelView GroupStopsModelView { get; }

	    public FavouriteModelView FavouriteModelView { get; }

	    //public IContext Context { get { return context; } }


		public List<NewsEntry> AllNews
		{
			get
			{
				if (NewsManager != null)
				{
#if DEBUG
					var xxx = NewsManager.AllHotNews.Concat(newsManager.NewNews).ToList();
#endif
					return NewsManager.AllHotNews.Concat(newsManager.NewNews).OrderByDescending(key => key.PostedUtc).ThenByDescending(key=> key.RepairedLineUtc).ToList();
				}
				return null;
			}
			set
			{

				OnPropertyChanged("AllNews");
			}
		}
		bool updating = false;
		RelayCommand updateDataCommand;
		public RelayCommand UpdateDataCommand
		{
			get
			{
				if (updateDataCommand == null)
					updateDataCommand = new RelayCommand(async () =>
					{
						updating = true;
						UpdateDataCommand.RaiseCanExecuteChanged();

						await Context.UpdateTimeTableAsync();
					    await Context.UpdateNewsTableAsync();
						updating = false;
						UpdateDataCommand.RaiseCanExecuteChanged();
					}, () => !updating);
				return updateDataCommand;
			}
		}

		

	    public NewsModelView NewsModelView { get; }
	}
}