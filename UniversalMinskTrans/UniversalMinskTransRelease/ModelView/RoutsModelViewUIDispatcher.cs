using System.Runtime.CompilerServices;
using Windows.UI.Core;
using MinskTrans.Context;
using MinskTrans.Context.UniversalModelView;
using MinskTrans.Universal.ModelView;
using MyLibrary;

namespace UniversalMinskTransRelease.ModelView
{
    class RoutsModelViewUIDispatcher : RoutsModelView
    {
        private CoreDispatcher dispatcher;

        public RoutsModelViewUIDispatcher(IBussnessLogics context, IExternalCommands commands, ISettingsModelView settings) : base(context, commands, settings)
        {
            dispatcher = CoreWindow.GetForCurrentThread().Dispatcher;
        }

        protected override void OnPropertyChanged([CallerMemberName]string propertyName = null)
        {
            dispatcher?.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                base.OnPropertyChanged(propertyName);
            });
        }
    }
}
