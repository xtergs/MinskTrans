using System.Configuration;
using System.Threading;
using MapControl;
using GalaSoft.MvvmLight.CommandWpf;
using Autofac;
using CommonLibrary.Notify;
using MetroLog;
using MinskTrans.Context;
using MinskTrans.Context.Base;
using MinskTrans.Context.Desktop;
using MinskTrans.Context.Fakes;
using MinskTrans.Context.UniversalModelView;
using MinskTrans.DesctopClient.ViewModel;
using MinskTrans.Net;
using MinskTrans.Net.Base;
using MinskTrans.Utilites;
using MinskTrans.Utilites.Base.IO;
using MinskTrans.Utilites.Base.Net;
using MinskTrans.Utilites.Desktop;
using MyLibrary;

namespace MinskTrans.DesctopClient.Modelview
{
	public class MainModelView : BaseModelView
	{
		//private readonly IContext context;
		private readonly MapModelView mapModelView;
		//private readonly IContext timeTable;
		static MainModelView model;

		public static MainModelView Get()
		{
			if (model == null)
				model = new MainModelView();
			return model;
		}

	    private readonly IContainer container;

		private MainModelView()
		{
			var builder = new ContainerBuilder();
			builder.RegisterType<FileHelperDesktop>().As<FileHelperBase>().SingleInstance();
			//builder.RegisterType<SqlEFContext>().As<IContext>().SingleInstance().WithParameter("connectionString", @"Data Source=(localdb)\ProjectsV12;Initial Catalog=Entity6_Test_MinskTrans;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False;MultipleActiveResultSets=True").SingleInstance();
			builder.RegisterType<Context.Context>().As<IContext>().SingleInstance();
			builder.RegisterType<UpdateManagerBase>().SingleInstance();
			builder.RegisterType<ShedulerParser>().As<ITimeTableParser>().SingleInstance();
			builder.RegisterType<InternetHelperDesktop>().As<InternetHelperBase>().SingleInstance();
            //builder.RegisterType<Context>().As<IContext>();
            builder.RegisterType<BussnessLogic>().As<IBussnessLogics>().SingleInstance();
            builder.RegisterType<DesktopApplicationHelper>().As<IApplicationSettingsHelper>().SingleInstance().WithParameter("settingsBase", Properties.Settings.Default);
		    builder.RegisterType<SettingsModelView>().As<ISettingsModelView>().SingleInstance();
            builder.RegisterType<NewsManagerDesktop>().As<NewsManagerBase>().SingleInstance();
		    builder.RegisterType<RoutesModelview>();
            builder.RegisterType<StopModelView>();
            builder.RegisterType<StopModelView>();
            builder.RegisterType<GroupStopsModelView>();
            builder.RegisterType<FindModelView>();
            builder.RegisterType<FakeGeolocation>().As<IGeolocation>().SingleInstance();
            builder.RegisterType<FakeSettingsModelView>().As<ISettingsModelView>().SingleInstance();
            builder.RegisterType<ExternalCommands>().As<IExternalCommands>().SingleInstance();
            builder.RegisterInstance<ILogManager>(LogManagerFactory.DefaultLogManager).SingleInstance();
            builder.RegisterType<NotifyHelperDesctop>().As<INotifyHelper>().SingleInstance();
            builder.RegisterType<FilePathsSettings>().SingleInstance();

            //builder.As<ApplicationSettingsBase>();


            container = builder.Build();

			context = container.Resolve<IBussnessLogics>();
			//timeTable = container.Resolve<IContext>();

		    


		}

		public MainModelView(Map map)
			: this()
		{
			mapModelView = new MapModelView(Context, map, SettingsModelView, container.Resolve<IGeolocation>());
			model = this;
		}

		public MapModelView MapModelView
		{
			get
			{
				return mapModelView;
			}
		}

		public FindModelView FindModelView
		{
			get { return container.Resolve<FindModelView>(); }
		}

		public StopModelView StopMovelView
		{
			get { return container.Resolve<StopModelView>(); }
		}

		public RoutesModelview RoutesModelview
		{
			get { return container.Resolve<RoutesModelview>(); }
		}

		public GroupStopsModelView GroupStopsModelView
		{
			get { return container.Resolve<GroupStopsModelView>(); }
		}

		public FavouriteModelView FavouriteModelView
		{
			get
			{
				return container.Resolve<FavouriteModelView>();
			}
		}

		//public IContext Context { get { return context; } }

		public UpdateManagerBase UpdateManager
		{
			get
			{
				return container.Resolve<UpdateManagerBase>();
			}
		}

		object lockObject = new object();
		bool updateing = false;
		RelayCommand updateDataCommand;
	    private CancellationTokenSource soruce;
		public RelayCommand UpdateDataCommand
		{
			get
			{
				if (updateDataCommand == null)
					updateDataCommand = new RelayCommand(async () =>
					{
					    if (updateing)
					        return;
						updateing = true;
					    using (soruce = new CancellationTokenSource())
					    {
					        await Context.UpdateTimeTableAsync(soruce.Token);
					        if (await Context.UpdateNewsTableAsync(soruce.Token))
					            await container.Resolve<NewsManagerBase>().Load();
					    }
					    updateing = false;
					}, () => !updateing);

				return updateDataCommand;
			}
		}

		public ISettingsModelView SettingsModelView
		{
			get
			{
				return container.Resolve<ISettingsModelView>();
			}
		}

        //public GalaSoft.MvvmLight.Command.RelayCommand<Stop> ShowStopMap
        //{
        //	get { return new GalaSoft.MvvmLight.Command.RelayCommand<Stop>((x) => OnShowStop(new ShowArgs() { SelectedStop = x }), (x) => x != null); }
        //}

        //public GalaSoft.MvvmLight.Command.RelayCommand<Rout> ShowRouteMap
        //{
        //	get { return new GalaSoft.MvvmLight.Command.RelayCommand<Rout>((x) => OnShowRoute(new ShowArgs() { SelectedRoute = x }), (x) => x != null); }
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

        //public IContext TimeTable
        //{
        //	get
        //	{
        //		return timeTable;
        //	}
        //}
        private IExternalCommands ExternalCommands { get { return container.Resolve<IExternalCommands>(); } }
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
