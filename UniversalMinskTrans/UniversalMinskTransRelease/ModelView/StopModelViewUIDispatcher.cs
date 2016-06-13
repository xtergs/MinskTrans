using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Core;
using MinskTrans.Context;
using MinskTrans.Context.Base;
using MinskTrans.Context.Base.BaseModel;
using MinskTrans.Context.UniversalModelView;
using MinskTrans.DesctopClient.Modelview;
using MyLibrary;

namespace UniversalMinskTransRelease.ModelView
{
    class StopModelViewUIDispatcher : StopModelView
    {
        private CoreDispatcher dispatcher;
        private IEnumerable<TimeLineModel> _timeSchedule;

        public StopModelViewUIDispatcher(IBussnessLogics newContext, ISettingsModelView settings, IExternalCommands commands, bool UseGPS = false) : base(newContext, settings, commands, UseGPS)
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
                FilteredStopsStore = res;
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

        protected override void OnPropertyChanged(string propertyName = null)
        {
            dispatcher?.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                base.OnPropertyChanged(propertyName);
            });
        }
    }
}
