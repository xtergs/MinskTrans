using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MinskTrans.Context;
using MinskTrans.Context.Base.BaseModel;
using MinskTrans.Context.Geopositioning;
using MinskTrans.Context.UniversalModelView;
using MinskTrans.Universal.ModelView;
using MyLibrary;

namespace UniversalMinskTransRelease.ModelView
{
    public enum ResultType
    {
        Rout, Stop, None
    }
    public class ResultEntry
    {
        protected ResultEntry(ResultType type)
        {
            Type = type;
        }
        public ResultType Type { get;}
    }

    public class ResultRout : ResultEntry
    {
        public ResultRout() : base(ResultType.None)
        {
        }

        public ResultRout(Rout rout)
            : base(ResultType.Rout)
        {
            Rout = rout;
        }
        public Rout Rout { get; }
    }

    public class ResultStop : ResultEntry
    {
        public ResultStop() : base(ResultType.None)
        {
        }
        public ResultStop(Stop stop)
            : base(ResultType.Stop)
        {
            Stop = stop;
        }
        public Stop Stop { get; }
    }

    public class SingFilterForAllDataModelView : StopModelViewUIDispatcher
    {
        private string _filter;

        public SingFilterForAllDataModelView(IBussnessLogics newContext, ISettingsModelView settings, IExternalCommands commands, WebSeacher seacher, bool UseGPS = false) : base(newContext, settings, commands, seacher, UseGPS)
        {
        }

        public async Task<IEnumerable<ResultEntry>> FilterEntriesAsync(string filter)
        {
            if (sourceToken != null)
            {
                sourceToken?.Cancel();
                sourceToken = null;
            }
            try
            {
                using (sourceToken = new CancellationTokenSource())
                {
                    var token = sourceToken.Token;

                    var stopsTask =
                        (Task.Run(
                            () =>
                                Context.FilteredStops(filter, selectedTransport, Context.Geolocation.CurLocation,
                                    FuzzySearch).Select(
                                        st => (ResultEntry) new ResultStop(st)), token));
                    var routsTask =
                        Task.Run(
                            () =>
                                RoutsModelView.GetRouteNumsWithDestination(filter, TransportType.All, Context)
                                    .Select(rt => (ResultEntry) new ResultRout(rt)), token);
                    var resuls = (await Task.WhenAll(routsTask, stopsTask)).SelectMany(x => x);
                    if (token.IsCancellationRequested)
                        return null;
                    return resuls;
                }
            }
            finally
            {
                sourceToken = null;
            }
        }

        public async Task FilterEntriesAsync()
        {
            FilterResults = await FilterEntriesAsync(Filter).ConfigureAwait(false);
            OnPropertyChanged(nameof(FilterResults));
        }

        public string Filter
        {
            get { return _filter; }
            set
            {
                if (Equals(value, _filter))
                    return;
                _filter = value;
                FilterEntriesAsync().ConfigureAwait(false);
            }
        }

        public IEnumerable<ResultEntry> FilterResults { get; private set; }

    }
}
