using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Autofac;
using CommonLibrary;
using CommonLibrary.IO;
using CommonLibrary.Notify;
using GalaSoft.MvvmLight.Command;
using MetroLog;
using MinskTrans.Context;
using MinskTrans.Context.Base;
using MinskTrans.Context.Base.BaseModel;
using MinskTrans.Context.UniversalModelView;
using MinskTrans.DesctopClient;
using MinskTrans.DesctopClient.Modelview;
using MinskTrans.Net;
using MinskTrans.Net.Base;
using MinskTrans.Utilites;
using MinskTrans.Utilites.Base.IO;
using MinskTrans.Utilites.Base.Net;
using UniversalMinskTransRelease.Nofity;
using MyLibrary;
using UniversalMinskTransRelease.ModelView;

namespace MinskTrans.Universal.ModelView
{

	public class MainModelView : BaseModelView, INotifyPropertyChanged
	{
	    private IContainer container;
		private static MainModelView mainModelView;
		//private readonly IContext context;
	    private readonly NewsManagerBase newsManager;

	    private bool updating;

		//public static MainModelView Create(Context newContext)
		//{
		//	if (mainModelView == null)
		//		mainModelView = new MainModelView(newContext);
		//	return mainModelView;
		//}

		public static MainModelView MainModelViewGet
		{
			get {
				if (mainModelView == null)
					mainModelView = new MainModelView();
				return mainModelView;}
		}

		

		private MainModelView()
		{
			var builder = new ContainerBuilder();
			builder.RegisterType<FileHelper>().As<FileHelperBase>().SingleInstance();
            //builder.RegisterType<SqlEFContext>().As<IContext>().SingleInstance().WithParameter("connectionString", @"Data Source=(localdb)\ProjectsV12;Initial Catalog=Entity3_Test_MinskTrans;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False");
            builder.RegisterType<Context.Context>().As<IContext>().SingleInstance();
            builder.RegisterType<UpdateManagerBase>();
            builder.RegisterType<ShedulerParser>().As<ITimeTableParser>();
            builder.RegisterType<InternetHelperUniversal>().As<InternetHelperBase>();
            builder.RegisterType<NewsManager>().As<NewsManagerBase>().SingleInstance();
            builder.RegisterType<BussnessLogic>().As<IBussnessLogics>().SingleInstance();
            builder.RegisterType<UniversalGeolocator>().As<IGeolocation>().SingleInstance();
            builder.RegisterType<SettingsModelView>().As<ISettingsModelView>().SingleInstance();
            builder.RegisterType<UniversalApplicationSettingsHelper>().As<IApplicationSettingsHelper>();
            builder.RegisterType<GroupStopsModelView>().SingleInstance();
            builder.RegisterType<FavouriteModelView>().SingleInstance();
            builder.RegisterType<NewsModelView>().SingleInstance();
            builder.RegisterType<FindModelView>().SingleInstance().WithParameter("UseGPS", true);
            builder.RegisterType<ExternalCommands>().As<IExternalCommands>().SingleInstance();
            builder.RegisterInstance<ILogManager>(LogManagerFactory.DefaultLogManager).SingleInstance();
            builder.RegisterType<NotifyHelperUniversal>().As<INotifyHelper>();

            //builder.RegisterType<Context>().As<IContext>();

            container = builder.Build();

            context = container.Resolve<IBussnessLogics>();
            newsManager = container.Resolve<NewsManagerBase>();
            UpdateManager = container.Resolve<UpdateManagerBase>();
            notifyHelper = container.Resolve<INotifyHelper>();
            log = container.Resolve<ILogManager>().GetLogger<MainModelView>();

            ExternalCommands = container.Resolve<IExternalCommands>();
        }

	    public ILogger log { get; set; }

	    public UpdateManagerBase UpdateManager { get; set; }

	    public IGeolocation Geolocation
        { get { return container.Resolve<IGeolocation>(); } }

        public NewsManagerBase NewsManager
		{
            get { return container.Resolve<NewsManagerBase>(); }
        }

		public MapModelView MapModelView { get; set; }

	    public ISettingsModelView SettingsModelView { get { return container.Resolve<ISettingsModelView>(); } }

	    public FindModelView FindModelView { get { return container.Resolve<FindModelView>(); } }

	    public GroupStopsModelView GroupStopsModelView { get { return container.Resolve<GroupStopsModelView>(); } }

	    public FavouriteModelView FavouriteModelView { get { return container.Resolve<FavouriteModelView>(); } }

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
				
				//var handle = PropertyChangedHandler;
				//if (handle != null)
				//	PropertyChangedHandler.Invoke(this, new PropertyChangedEventArgs("AllNews"));
				OnPropertyChanged("AllNews");
			}
		}

		

		public UpdateManagerBase UpdateManagerBase { get { return container.Resolve<UpdateManagerBase>(); } }

	    RelayCommand updateDataCommand;
	    private INotifyHelper notifyHelper;

	    public RelayCommand UpdateDataCommand
		{
			get
			{
				if (updateDataCommand == null)
					updateDataCommand = new RelayCommand(async () =>
				{
					updating = true;
					UpdateDataCommand.RaiseCanExecuteChanged();

				    await Context.UpdateTimeTableAsync(false);
					updating = false;
					UpdateDataCommand.RaiseCanExecuteChanged();
				}, () => !updating);
				return updateDataCommand;
			}
		}

        //public RelayCommand<Stop> ShowStopMap
        //{
        //	get { return new RelayCommand<Stop>((x) => OnShowStop(new ShowArgs() { SelectedStop = x }), (x) => x != null); }
        //}

        //public RelayCommand<Rout> ShowRouteMap
        //{
        //	get { return new RelayCommand<Rout>((x) => OnShowRoute(new ShowArgs() { SelectedRoute = x }), (x) => x != null); }
        //}
        //public event Show ShowStop;
        //public event Show ShowRoute;
        //public delegate void Show(object sender, ShowArgs args);

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
        private IExternalCommands ExternalCommands { get; }
        public event Show ShowStop
        {
            add { ExternalCommands.ShowStop += value; }
            remove { ExternalCommands.ShowStop -= value; }
        }

        public event Show ShowRoute
        {
            add { ExternalCommands.ShowRoute += value; }
            remove { ExternalCommands.ShowRoute -= value; }
        }
    }
}