
namespace MinskTrans.DesctopClient.Modelview
{
#if !WINDOWS_PHONE_APP && !WINDOWS_AP
using MinskTrans.DesctopClient.Properties;
using MinskTrans.Library;
#else
	using Windows.Storage;

#endif

	public class SettingsModelView : BaseModelView, ISettingsModelView
	{


		public SettingsModelView(Context newContext)
			: base(newContext)
		{

		}

		public int TimeInPast
		{
			get
			{
#if !WINDOWS_PHONE_APP && !WINDOWS_AP
				return Settings.Default.TimeInPast;
#else
				if (!ApplicationData.Current.RoamingSettings.Values.ContainsKey("TimeInPast"))
					TimeInPast = 15;
				return (int) ApplicationData.Current.RoamingSettings.Values["TimeInPast"];
#endif
			}
			set
			{
#if !WINDOWS_PHONE_APP && !WINDOWS_AP
		
				Settings.Default.TimeInPast = value;
#else
				if (!ApplicationData.Current.RoamingSettings.Values.ContainsKey("TimeInPast"))
					ApplicationData.Current.RoamingSettings.Values.Add("TimeInPast", value);
				else
					ApplicationData.Current.RoamingSettings.Values["TimeInPast"] = value;
#endif
				OnPropertyChanged();
				OnPropertyChanged("TimeSchedule");
			}
		}
	}
}