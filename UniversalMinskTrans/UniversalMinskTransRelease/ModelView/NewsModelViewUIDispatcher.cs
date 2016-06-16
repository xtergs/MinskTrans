using System.Runtime.CompilerServices;
using Windows.UI.Core;
using MinskTrans.Net;

namespace UniversalMinskTransRelease.ModelView
{
    class NewsModelViewUIDispatcher : NewsModelView
    {
        private CoreDispatcher dispatcher;
        public NewsModelViewUIDispatcher(NewsManagerBase NewsManagerBase) : base(NewsManagerBase)
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
