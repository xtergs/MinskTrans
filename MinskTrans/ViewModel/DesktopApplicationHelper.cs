using System;
using System.ComponentModel;
using System.Configuration;
using MinskTrans.DesctopClient.Properties;
using MyLibrary;

namespace MinskTrans.DesctopClient.ViewModel
{
    class DesktopApplicationHelper: IApplicationSettingsHelper
    {

        private LocalFileSettingsProvider settingsProvider = new LocalFileSettingsProvider() {ApplicationName = "skdjfksdjfksjd"};
        static System.Configuration.UserScopedSettingAttribute attr = new UserScopedSettingAttribute();
        private SettingsAttributeDictionary SettingsmAttributeDictionary = new SettingsAttributeDictionary() { { typeof(UserScopedSettingAttribute), attr}};
        private ApplicationSettingsBase settings;
        #region Implementation of IApplicationSettingsHelper

        public DesktopApplicationHelper(ApplicationSettingsBase settingsBase)
        {
            settings = settingsBase;
            settings.Reload();
        }

        public void SimpleSet<T>(T value, string key = null)
        {
           
            var prop = settings.Properties[key];
            if (prop == null)
                settings.Properties.Add(new SettingsProperty(key, typeof (T), settingsProvider, false, default(T),
                    SettingsSerializeAs.Xml, SettingsmAttributeDictionary, true, true));
            settings[key] = (T) value;
            settings.Save();
        }

        public T SimpleGet<T>(T defValue = default(T), string key = null)
        {
            var prop = settings.Properties[key];
            if (prop != null)
                return ((T)settings[key]);
            settings.Properties.Add(new SettingsProperty(key, typeof(T), settingsProvider, false, defValue, SettingsSerializeAs.Xml, SettingsmAttributeDictionary, true,true));
            settings.Reload();
            prop = settings.Properties[key];
            if (prop == null)
                settings.Save();
            else
                return ((T)settings[key]);

            return defValue;
        }

        public Error SimpleGet(Error defValue = Error.None, string key = null)
        {
            return SimpleGet<Error>(defValue, key);
        }

        public void SimpleSet<T>(Error value, string key = null)
        {
            var prop = settings.Properties[key];
            if (prop == null)
                settings.Properties.Add(new SettingsProperty(key, typeof(Error), settingsProvider, false, default(Error),
                    SettingsSerializeAs.Xml, SettingsmAttributeDictionary, true, true));
            settings[key] = (Error)value;
            settings.Save();
        }

        public DateTime SimbleGet(DateTime value = new DateTime(), string key = null)
        {
            return SimpleGet<DateTime>(value, key);
        }

        public void SimpleSet(DateTime value, string key = null)
        {
            settings[key] = (DateTime)value;
        }

        public T SimpleEnumGet<T>(Enum defValue = null, string key = null)
        {
            return SimpleGet<T>(default(T), key);
        }

        public void SimpleEnumSet<T>(Enum value, string key = null)
        {
            SimpleSet<Enum>(value, key);
        }

        #endregion
    }
}
