using MinskTrans.Context;
using MinskTrans.Context.Base;
using MinskTrans.Context.UniversalModelView;
using MinskTrans.Context.Utilites;
using MinskTrans.DesctopClient;
using MinskTrans.DesctopClient.Modelview;
using MyLibrary;

namespace MinskTrans.Universal.ModelView
{
	public class FavouriteModelView:FindModelView
	{
		
		public FavouriteModelView(IBussnessLogics newContext, ISettingsModelView settingsModelView, IExternalCommands commands) : base(newContext, settingsModelView, commands)
		{
			
		}
	}
}
