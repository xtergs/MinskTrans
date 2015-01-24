using System.Collections.Generic;

using MinskTrans.DesctopClient.Model;
using MinskTrans.DesctopClient.Properties;
using MinskTrans.Library;

namespace MinskTrans.DesctopClient.Modelview
{
	public class SettingsModelView :BaseModelView, ISettingsModelView
	{
		

		public SettingsModelView(Context newContext)
			:base(newContext)
		{
			
		}

		public int TimeInPast
		{
			get { return Settings.Default.TimeInPast; }
			set
			{
				Settings.Default.TimeInPast = value;
				OnPropertyChanged();
			}
		}
	}
}