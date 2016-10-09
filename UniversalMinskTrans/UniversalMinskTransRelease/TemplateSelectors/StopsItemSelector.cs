using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using MinskTrans.Context;
using MinskTrans.Context.Base.BaseModel;
using UniversalMinskTransRelease.ViewModel;

namespace UniversalMinskTransRelease.TemplateSelectors
{
	class StopsItemSelector : DataTemplateSelector
	{
		public DataTemplate SimpleStop { get; set; }
		public DataTemplate FavouriteStop { get; set; }

		protected override DataTemplate SelectTemplateCore(object item)
		{
            if (item is StopSearchResult)
                return SimpleStop;
            if (item is FavouriteStop)
				return FavouriteStop;
			if (item is Stop)
				return SimpleStop;
			return base.SelectTemplateCore(item);
		}
	}
}
