using MinskTrans.Context;
using MinskTrans.Context.UniversalModelView;
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
