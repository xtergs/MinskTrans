using Windows.UI.Core;
using MinskTrans.Context;
using MinskTrans.Context.Geopositioning;
using MinskTrans.Context.UniversalModelView;
using MinskTrans.Context.Utilites;
using MinskTrans.DesctopClient.Modelview;
using MyLibrary;

namespace UniversalMinskTransRelease.ModelView
{
    class GroupStopsModelViewUIDispatcher : GroupStopsModelView
    {
        private CoreDispatcher dispatcher;

        public GroupStopsModelViewUIDispatcher(IBussnessLogics newContext, ISettingsModelView settingsModelView, IExternalCommands commands, WebSeacher seacher)
            : base(newContext, settingsModelView, commands, seacher)
        {
            dispatcher = CoreWindow.GetForCurrentThread().Dispatcher;
        }

        protected override void OnPropertyChanged(string propertyName = null)
        {
            dispatcher?.RunAsync(CoreDispatcherPriority.Normal, () => { base.OnPropertyChanged(propertyName); });
        }
    }
}
