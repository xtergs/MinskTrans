using System.ComponentModel;

namespace MyLibrary
{
    public class FakeSettingsModelView: ISettingsModelView
    {
        #region Implementation of INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Implementation of ISettingsModelView

        public int TimeInPast { get; set; }
        public bool Develop { get; set; }
        public bool CurrentDate { get; set; }
        public bool UpdateOnWiFi { get; set; }
        public bool UpdateOnMobileData { get; set; }

        #endregion
    }
}
