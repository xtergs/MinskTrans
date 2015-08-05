using System.ComponentModel;

namespace MyLibrary
{
	public interface ISettingsModelView:INotifyPropertyChanged
	{
		int TimeInPast { get; set; }
        bool Develop { get; set; }
        bool CurrentDate { get; set; }
      //  bool DayOfWeek { get; set; }
        bool UpdateOnWiFi { get; set; }
		bool UpdateOnMobileData { get; set; }
	}
}