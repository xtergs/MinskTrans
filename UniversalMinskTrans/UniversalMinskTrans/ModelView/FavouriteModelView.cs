using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MinskTrans.DesctopClient;
using MinskTrans.DesctopClient.Modelview;

namespace MinskTrans.Universal.ModelView
{
	public class FavouriteModelView:FindModelView
	{
		
		public FavouriteModelView(IContext newContext, SettingsModelView settingsModelView) : base(newContext, settingsModelView)
		{
			
		}
	}
}
