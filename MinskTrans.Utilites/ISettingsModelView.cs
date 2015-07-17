using System.ComponentModel;

namespace MyLibrary
{
	public interface ISettingsModelView:INotifyPropertyChanged
	{
		int TimeInPast { get; set; }
		bool UpdateOnWiFi { get; set; }
		bool UpdateOnMobileData { get; set; }
	}
}