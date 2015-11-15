using MinskTrans.Context;
using MinskTrans.Context.Base;
using MinskTrans.DesctopClient;
using MinskTrans.DesctopClient.Modelview;
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
