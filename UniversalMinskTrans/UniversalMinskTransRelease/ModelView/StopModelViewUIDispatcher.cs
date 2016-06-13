using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml.Data;
using MinskTrans.Context;
using MinskTrans.Context.Base;
using MinskTrans.Context.Base.BaseModel;
using MinskTrans.Context.Geopositioning;
using MinskTrans.Context.UniversalModelView;
using MinskTrans.DesctopClient.Modelview;
using MyLibrary;

namespace UniversalMinskTransRelease.ModelView
{
    class StopModelViewUIDispatcher : StopModelView
    {
        private CoreDispatcher dispatcher;
        private IEnumerable<TimeLineModel> _timeSchedule;

        public StopModelViewUIDispatcher(IBussnessLogics newContext, ISettingsModelView settings, IExternalCommands commands, WebSeacher seacher, bool UseGPS = false) : base(newContext, settings, commands, seacher, UseGPS)
        {
            dispatcher = CoreWindow.GetForCurrentThread().Dispatcher;
        }

        public override Task<IEnumerable<Stop>> FilterStopsAsync()
        {
            UpdateStopsAsync();
            return null;
        }

        public async Task UpdateStopsAsync()
        {
            if (IsShowFavouriteStops)
            {
                FilteredStopsStore = Context.Context.FavouriteStops;
                return;
            }
            if (sourceToken != null)
                sourceToken.Cancel();
            IsWorking = true;
            using (sourceToken = new CancellationTokenSource())
            {
                var token = sourceToken.Token;

                var res = await Task.Run(() => FilterStops(), token).ConfigureAwait(false);
                if (token.IsCancellationRequested)
                    return;
                sourceToken = null;
                IsWorking = false;
                FilteredStopsStore = res.ToArray();
                
                    GetWebResultsAsync();
            }
        }

        public override Stop FilteredSelectedStop {
            get { return base.FilteredSelectedStop; }
            set
            {
                base.FilteredSelectedStop = value;
                UpdateTimeScheduleAsync();
            }
        }

        protected async Task UpdateTimeScheduleAsync()
        {
            var result = await Task.Run(() => base.TimeSchedule).ConfigureAwait(false);
            _timeSchedule = result;
            OnPropertyChanged(nameof(TimeSchedule));
        }

        public override IEnumerable<TimeLineModel> TimeSchedule => _timeSchedule;

        public ObservableCollection<Stop> WebResults { get; set; }
        private async Task GetWebResultsAsync()
        {
                WebResults = new ObservableCollection<Stop>();
            if (FilteredStopsStore.Count() > 6)
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
            var webResults = (await webSeacher.QueryToPosition(StopNameFilter))?.Where(x=> x.Address.ToLower() != "минск").ToArray();
            if (webResults == null || webResults.Length == 0)
            {
                OnPropertyChanged(nameof(WebResults));
                IsShowWebResuls = WebResults.Count > 0;
                OnPropertyChanged(nameof(IsShowWebResuls));
                return;
            }
            var results  = Context.FilteredStops(null, selectedTransport, webResults[0].Location, FuzzySearch);
            results = results.Except(FilteredStopsStore).Take(5);
            WebResults = new ObservableCollection<Stop>( results);
            OnPropertyChanged(nameof(WebResults));
            StreetName = webResults[0].Address;
            OnPropertyChanged(StreetName);
            IsShowWebResuls = WebResults.Count > 0;
            OnPropertyChanged(nameof(IsShowWebResuls));
        }
        public string StreetName { get; set; }
        public bool IsShowWebResuls { get; set; }


        protected override void OnPropertyChanged(string propertyName = null)
        {
            dispatcher?.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                base.OnPropertyChanged(propertyName);
            });
        }
    }
}
