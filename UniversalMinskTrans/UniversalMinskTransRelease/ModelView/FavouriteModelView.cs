using MinskTrans.Context;
using MyLibrary;

namespace MinskTrans.Universal.ModelView
{
	public class FavouriteModelView:FindModelView
	{
		
		public FavouriteModelView(IBussnessLogics newContext, ISettingsModelView settingsModelView) : base(newContext, settingsModelView)
		{
			
		}
	}
}
