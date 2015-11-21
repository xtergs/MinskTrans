
using System;
using GalaSoft.MvvmLight.Command;
using MinskTrans.Context.Base.BaseModel;
using MinskTrans.DesctopClient;

namespace MinskTrans.Context.UniversalModelView
{
    public interface IExternalCommands
    {
        event Show ShowStop;
        event Show ShowRoute;
        event EventHandler BackPressed;

        RelayCommand<Stop> ShowStopMap { get; }

        RelayCommand<Rout> ShowRouteMap { get; }
        RelayCommand BackPressedCommand { get; }

    }
        public delegate void Show(object sender, ShowArgs args);
}
