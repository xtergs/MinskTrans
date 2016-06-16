using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Core;
using MapControl;
using MinskTrans.Context;
using MinskTrans.Context.Base;
using MinskTrans.Context.Base.BaseModel;
using MinskTrans.DesctopClient.Modelview;
using MinskTrans.Universal;
using MyLibrary;

namespace UniversalMinskTransRelease.ModelView
{
    public class MapModelViewUIDispatcher : MapModelView
    {
        private CoreDispatcher dispatcher;

        public MapModelViewUIDispatcher(IBussnessLogics context, Map map, ISettingsModelView newSettigns, IGeolocation geolocation, StopModelView stopModelView, PushPinBuilder pushPinBuilder = null) : base(context, map, newSettigns, geolocation, stopModelView, pushPinBuilder)
        {
            dispatcher = CoreWindow.GetForCurrentThread().Dispatcher;
        }

        private CancellationTokenSource _cancelPrepareStops = null;
        protected override void PreperPushpinsForView(IEnumerable<Stop> needStops)
        {
            if (_cancelPrepareStops != null)
                _cancelPrepareStops.Cancel();
            _cancelPrepareStops = new CancellationTokenSource();
            List<Pushpin> pushs = new List<Pushpin>(10);
            List<Pushpin> toRemove = new List<Pushpin>(10);
            List<Pushpin> toAdd = new List<Pushpin>(10);
            var token = _cancelPrepareStops.Token;
            Task.Run(async () =>
            {
                foreach (var needShowStop in needStops)
                {
                    var tempPushpin = allPushpins.FirstOrDefault(push => push.Stop.ID == needShowStop.ID);
                    if (tempPushpin == null)
                    {
                        tempPushpin = await CreatePushpin(needShowStop);
                        allPushpins.Add(tempPushpin);
                    }
                    await dispatcher?.RunAsync(CoreDispatcherPriority.Normal, () => {
                        pushs.Add(tempPushpin.Pushpin);
                    });
                    if (token.IsCancellationRequested)
                        return;
                }
                //stopsOnMap.AddRange(Pushpins);
                //var temp = map.Children.OfType<Pushpin>().ToArray();
                var except = StopsOnMap.Concat(Pushpins).Except(pushs).ToList();
                toRemove.AddRange(except);
                except = pushs.Except(StopsOnMap).ToList();
                toAdd.AddRange(except);
                if (token.IsCancellationRequested)
                    return;
                StopsOnMap = pushs.Distinct().ToArray();
            }, token).ContinueWith((o) =>
            {
                if (token.IsCancellationRequested || ( !toRemove.Any() && !toAdd.Any()))
                    return;
                    dispatcher?.RunAsync(CoreDispatcherPriority.Normal,()=> ShowOnMap(toRemove.ToArray(), toAdd.ToArray()));
                }, token);
        }

        public override void RefreshPushPinsAsync()
        {
            if (showAllPushpins && map != null && Context.Context.ActualStops != null)
            {
                double zoomLevel = map.ZoomLevel;
                //Pushpins.Clear();
                if (zoomLevel <= MaxZoomLevel)
                {
                    PreperPushpinsForView(new List<Stop>(0));
                    //ShowOnMap();
                    return;
                }

                var northWest = map.ViewportPointToLocation(new Windows.Foundation.Point(0, 0));
                var southEast = map.ViewportPointToLocation(new Windows.Foundation.Point(map.ActualWidth, map.ActualHeight));

                var needShowStops =
                    Context.Context.ActualStops.Where(child => child.Lat <= northWest.Latitude && child.Lng >= northWest.Longitude &&
                                                       child.Lat >= southEast.Latitude && child.Lng <= southEast.Longitude).ToList();

                if (Ipushpin != null)
                    Pushpins.Add(Ipushpin);
                PreperPushpinsForView(needShowStops);
            }
        }

        protected new async Task<PushpinLocation> CreatePushpin(Stop st)
        {
            PushpinLocation tempPushPin = null;
            await dispatcher?.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                tempPushPin = base.CreatePushpin(st);


                if (pushBuilder != null)
                {
                    tempPushPin.Pushpin = pushBuilder.CreatePushPin(tempPushPin.Location);
                    tempPushPin.Pushpin.Tag = st;
                    tempPushPin.Pushpin.Content = st.Name;
#if DEBUG
                    tempPushPin.Pushpin.Content += string.Format("\n {0} \n {1} ", st.ID, st.SearchName);
#endif
                }
            });
            return tempPushPin;
        }

        protected override void OnPropertyChanged(string propertyName = null)
        {
            dispatcher?.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                base.OnPropertyChanged(propertyName);
            });
        }
    }
}
