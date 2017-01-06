using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml.Data;
using GalaSoft.MvvmLight.Command;
using MinskTrans.Context;
using MinskTrans.Context.Base;
using MinskTrans.Context.Base.BaseModel;
using MinskTrans.Context.Geopositioning;
using MinskTrans.Context.UniversalModelView;
using MinskTrans.Context.Utilites;
using MinskTrans.DesctopClient.Modelview;
using MyLibrary;
using UniversalMinskTransRelease.ViewModel;

namespace UniversalMinskTransRelease.ModelView
{
	public class StopModelViewUIDispatcher : StopModelView
	{
		private CoreDispatcher dispatcher;
		private IEnumerable<TimeLineModel> _timeSchedule;

		public StopModelViewUIDispatcher(IBussnessLogics newContext, ISettingsModelView settings,
			IExternalCommands commands, WebSeacher seacher, bool UseGPS = false)
			: base(newContext, settings, commands, seacher, UseGPS)
		{
			dispatcher = CoreWindow.GetForCurrentThread().Dispatcher;
			updateTimeLineTimer = new Timer((obj) => UpdateTimeScheduleAsync().ConfigureAwait(false), null, int.MaxValue,
				int.MaxValue);
		}

		public override Task<IEnumerable<Stop>> FilterStopsAsync()
		{
			UpdateStopsAsync().ConfigureAwait(false);
			return null;
		}

		public async Task<IEnumerable<Stop>> UpdateStopsAsync(string filter)
		{
			if (sourceToken != null)
				sourceToken.Cancel();
			IsWorking = true;
			using (sourceToken = new CancellationTokenSource())
			{
				var token = sourceToken.Token;
				if (string.IsNullOrWhiteSpace(filter))
					filter = "";
				var res =
					await
						Task.Run(
							() =>
								Context.FilteredStops(filter, selectedTransport, 
								Settings.ConsiderDistanceSortStops ? Context.Geolocation.CurLocation : null,
									FuzzySearch, Settings.ConsiderFrequencySortStops), token).ConfigureAwait(false);
				if (token.IsCancellationRequested)
					return null;
				sourceToken = null;
				IsWorking = false;
				return res?.ToArray() ?? new Stop[0];
			}
		}

		private Timer updateFavourite;
		public async Task UpdateStopsAsync()
		{
			if (IsShowFavouriteStops)
			{
				FilteredStopsStore = Context.Context.FavouriteStops.Select(x=> new FavouriteStop()
				{
					Stop = x,
					CurrentRouts = Context.GetStopTimeLine(x.ID, CurDay, CurTime, null, Settings.PrevFavouriteRouts, Settings.NextFavouriteRouts )
				});
				if (updateFavourite == null)
					updateFavourite = new Timer((obj) =>
					{
						foreach (var favourite in FilteredStopsStore.OfType<FavouriteStop>())
						{
							favourite.CurrentRouts =
									Context.GetStopTimeLine(favourite.ID, CurDay, CurTime, null, Settings.PrevFavouriteRouts, Settings.NextFavouriteRouts)
										;
						}
					}, null, new TimeSpan(0,0,0,30), new TimeSpan(0,0,0,30) );
				return;
			}
			updateFavourite?.Dispose();
			updateFavourite = null;
			var result =  await UpdateStopsAsync(StopNameFilter);
			if (result == null)
				return;
			FilteredStopsStore = result;

            if (SettingsModelView.UseWebSeacher)
                await GetWebResultsAsync().ConfigureAwait(false);
		}

		private Timer updateTimeLineTimer;
		public override Stop FilteredSelectedStop
		{
			get { return base.FilteredSelectedStop; }
			set
			{
				base.FilteredSelectedStop = value;
				OnPropertyChanged();
				UpdateTimeScheduleAsync().ConfigureAwait(false);
				if (value != null)
				{
					updateTimeLineTimer.Change(30*1000, 30*1000);
				}else 
					updateTimeLineTimer.Change(int.MaxValue, int.MaxValue);
			}
		}

		protected async Task UpdateTimeScheduleAsync()
		{
			var result = await Task.Run(() => base.TimeSchedule).ConfigureAwait(false);
			_timeSchedule = result;
			OnPropertyChanged(nameof(TimeSchedule));
		}

		public override async void NotifyTimeScheduleChanged()
		{
			await UpdateTimeScheduleAsync();
		}

		public override IEnumerable<TimeLineModel> TimeSchedule => _timeSchedule;

		public ObservableCollection<Stop> WebResults { get; set; }

		private async Task GetWebResultsAsync()
		{
			WebResults = new ObservableCollection<Stop>();
			if (FilteredStopsStore?.Count() > 6)
			{
				OnPropertyChanged(nameof(WebResults));
				IsShowWebResuls = WebResults.Count > 0;
				OnPropertyChanged(nameof(IsShowWebResuls));
				return;
			}
			Settings.UpdateNetworkData();
			if (!Settings.HaveConnection())
			{
				OnPropertyChanged(nameof(WebResults));
				IsShowWebResuls = WebResults.Count > 0;
				OnPropertyChanged(nameof(IsShowWebResuls));
				return;
			}

		        var webResults =
		            (await webSeacher.QueryToPosition(StopNameFilter))?.Where(x => x.Address.ToLower() != "минск").ToArray();
		        if (webResults == null || webResults.Length == 0)
		        {
		            OnPropertyChanged(nameof(WebResults));
		            IsShowWebResuls = WebResults.Count > 0;
		            OnPropertyChanged(nameof(IsShowWebResuls));
		            return;
		        }
		    var results = Context.FilteredStops(null, selectedTransport, webResults[0].Location, FuzzySearch);
			results = results.Except(FilteredStopsStore).Take(5);
			WebResults = new ObservableCollection<Stop>(results);
			OnPropertyChanged(nameof(WebResults));
			StreetName = webResults[0].Address;
			OnPropertyChanged(StreetName);
			IsShowWebResuls = WebResults.Count > 0;
			OnPropertyChanged(nameof(IsShowWebResuls));
		}

		public string StreetName { get; set; }
		public bool IsShowWebResuls { get; set; }

		#region Commands

		private RelayCommand _refreshTimeShcedule;

		public override RelayCommand RefreshTimeSchedule
		{
			get
			{
				if (_refreshTimeShcedule == null)
					_refreshTimeShcedule = new RelayCommand(
						() => UpdateTimeScheduleAsync().ConfigureAwait(false));
				return _refreshTimeShcedule;
			}
		}

		#endregion

		protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			dispatcher?.RunAsync(CoreDispatcherPriority.Normal, () => { base.OnPropertyChanged(propertyName); });
		}
	}
}