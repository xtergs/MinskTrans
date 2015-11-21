using GalaSoft.MvvmLight.Command;
using MinskTrans.Context.Base.BaseModel;
using MinskTrans.DesctopClient;

namespace MinskTrans.Context.UniversalModelView
{
    public class ExternalCommands: IExternalCommands
    {
        public event Show ShowStop;
        public event Show ShowRoute;

        public RelayCommand<Stop> ShowStopMap
        {
            get { return new RelayCommand<Stop>((stop) => OnShowStop(new ShowArgs() { SelectedStop = stop }), (stop) => stop != null); }
        }

        public RelayCommand<Rout> ShowRouteMap
        {
            get { return new RelayCommand<Rout>((rout) => OnShowRoute(new ShowArgs() { SelectedRoute = rout }), (rout) => rout != null); }
        }

        protected virtual void OnShowStop(ShowArgs args)
        {
            var handler = ShowStop;
            if (handler != null) handler(this, args);
        }

        protected virtual void OnShowRoute(ShowArgs args)
        {
            var handler = ShowRoute;
            if (handler != null) handler(this, args);
        }
    }
}
