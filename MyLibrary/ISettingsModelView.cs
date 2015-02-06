using System.ComponentModel;

namespace MinskTrans.DesctopClient.Modelview
{
	public interface ISettingsModelView:INotifyPropertyChanged
	{
		int TimeInPast { get; set; }
		bool UpdateOnWiFi { get; set; }
		bool UpdateOnMobileData { get; set; }
	}
}