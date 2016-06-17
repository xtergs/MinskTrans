using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using UniversalMinskTransRelease.ModelView;

namespace UniversalMinskTransRelease.View
{
    class TypeResultTemplateSelector: DataTemplateSelector
    {
        public DataTemplate ResultRout { get; set; }
        public DataTemplate ResultStop { get; set; }

        protected override DataTemplate SelectTemplateCore(object item)
        {
            if (item is ResultRout)
                return ResultRout;
            if (item is ResultStop)
                return ResultStop;
            return base.SelectTemplateCore(item);
        }
    }
}
