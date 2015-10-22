﻿using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Autofac;
using CommonLibrary;
using CommonLibrary.IO;
using GalaSoft.MvvmLight.Command;
using MinskTrans.Context.Base;
using MinskTrans.Context.Base.BaseModel;
using MinskTrans.DesctopClient;
using MinskTrans.DesctopClient.Modelview;
using MinskTrans.Net;
using MinskTrans.Net.Base;
using MinskTrans.Utilites;
using MinskTrans.Utilites.Base.IO;
using MinskTrans.Utilites.Base.Net;
using MyLibrary;

namespace MinskTrans.Universal.ModelView
{

	public class MainModelView : BaseModelView, INotifyPropertyChanged
	{
		private static MainModelView mainModelView;
		//private readonly IContext context;
		private readonly GroupStopsModelView groupStopsModelView;
		private readonly RoutsModelView routesModelview;
		private readonly SettingsModelView settingsModelView;
		private readonly StopModelView stopMovelView;
		private readonly FavouriteModelView favouriteModelView;
		private readonly FindModelView findModelView;
		private MapModelView mapMOdelView;
		private readonly NewsManagerBase newsManager;
		
		private readonly UpdateManagerBase updateManagerBase;
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
			builder.RegisterType<FileHelper>().As<FileHelperBase>();
			//builder.RegisterType<SqlEFContext>().As<IContext>().SingleInstance().WithParameter("connectionString", @"Data Source=(localdb)\ProjectsV12;Initial Catalog=Entity3_Test_MinskTrans;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False");
			builder.RegisterType<Context.Context>().As<IContext>().SingleInstance();
			builder.RegisterType<UpdateManagerBase>();
			builder.RegisterType<ShedulerParser>().As<ITimeTableParser>();
			builder.RegisterType<InternetHelperUniversal>().As<InternetHelperBase>();
			builder.RegisterType<NewsManager>().As<NewsManagerBase>();
            
			//builder.RegisterType<Context>().As<IContext>();

			var container = builder.Build();

			context = container.Resolve<IContext>();
			newsManager = container.Resolve<NewsManagerBase>();
			updateManagerBase = container.Resolve<UpdateManagerBase>();

			settingsModelView = new SettingsModelView();
			groupStopsModelView = new GroupStopsModelView(context, settingsModelView);
			favouriteModelView = new FavouriteModelView(context, settingsModelView);
			findModelView = new FindModelView(context, settingsModelView);
		}

		public NewsManagerBase NewsManager
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

		

		public UpdateManagerBase UpdateManagerBase
		{
			get
			{
				return updateManagerBase;
			}
		}

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

					if (await UpdateManagerBase.DownloadUpdate())
					{
						var timeTable = await UpdateManagerBase.GetTimeTable();
						if (await Context.HaveUpdate(timeTable.Routs, timeTable.Stops, timeTable.Time))
							await Context.ApplyUpdate(timeTable.Routs, timeTable.Stops, timeTable.Time);
					}
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
	}
}