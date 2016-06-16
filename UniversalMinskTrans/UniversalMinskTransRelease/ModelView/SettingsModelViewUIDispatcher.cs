using Windows.ApplicationModel;
using Windows.UI.Core;
using MinskTrans.Utilites.Base.Net;
using MyLibrary;

namespace UniversalMinskTransRelease.ModelView
{
    internal class SettingsModelViewUIDispatcher : SettingsModelView
    {
        private CoreDispatcher dispatcher;

        public SettingsModelViewUIDispatcher(IApplicationSettingsHelper helper, InternetHelperBase internetHelper)
            : base(helper, internetHelper)
        {
            dispatcher = CoreWindow.GetForCurrentThread().Dispatcher;
        }

        public static string GetAppVersion()
        {
            Package package = Package.Current;
            PackageId packageId = package.Id;
            PackageVersion version = packageId.Version;

            return string.Format("{0}.{1}.{2}.{3}", version.Major, version.Minor, version.Build, version.Revision);
        }

        private string SavedVersion
        {
            get { return helper.SimpleGet(""); }
            set { helper.SimpleSet(value); }
        }

        protected string changeLog = "- Веб поиск Яндекс Геолокатор API\n" +
                                   "- Повышена производительность\n" +
                                   "- Изменен алгоритм отображения остановок на карте\n" +
                                   "- Временно отключена вкладка Группы";
        public override string ChangeLogOnce
        {
            get
            {
                var version = GetAppVersion().Trim().ToLower();
                if (version != SavedVersion)
                {
                    SavedVersion = version;
                    return $"{version}\nПриложение обновлено:\n\n{changeLog}";
                    ;
                }
                return "";
            }
        }

        public override string ChangeLog => changeLog;

        protected override void OnPropertyChanged(string propertyName = null)
        {
            dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => { base.OnPropertyChanged(propertyName); });
        }
    }
}