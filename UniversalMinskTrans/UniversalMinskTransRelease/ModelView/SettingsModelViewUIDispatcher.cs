using System;
using System.Diagnostics;
using Windows.ApplicationModel;
using Windows.UI.Core;
using MinskTrans.Context.Base;
using MinskTrans.Utilites.Base.Net;
using MyLibrary;
using PostSharp.Aspects;
using PostSharp.Patterns.Model;
using PostSharp.Serialization;
//using PropertyChanged;

namespace UniversalMinskTransRelease.ModelView
{
    [PSerializable]
    public class GoogleAnaliticsAspect : LocationInterceptionAspect
    {
        public override void OnSetValue(LocationInterceptionArgs args)
        {
            Debug.WriteLine("Intercepted");
            base.OnSetValue(args);
            GoogleAnalytics.EasyTracker.GetTracker().SendEvent("Settings", args.LocationName, args.Value.ToString(), 0);
        }
    }

    [PSerializable]
    public class GoogleAnaliticsAspectMethods : MethodInterceptionAspect
    {
        public override void OnInvoke(MethodInterceptionArgs args)
        {
            base.OnInvoke(args);
            GoogleAnalytics.EasyTracker.GetTracker().SendEvent("Settings", args.Method.Name, args.ReturnValue.ToString(), 0);
        }

    }

    [GoogleAnaliticsAspect]
    [GoogleAnaliticsAspectMethods]
    [NotifyPropertyChanged]
    internal class SettingsModelViewUIDispatcher : SettingsModelView
    {
        private CoreDispatcher dispatcher;

        public SettingsModelViewUIDispatcher(IApplicationSettingsHelper helper, InternetHelperBase internetHelper, IGeolocation geolocation)
            : base(helper, internetHelper)
        {
            dispatcher = CoreWindow.GetForCurrentThread()?.Dispatcher;
            _geolocation = geolocation;
            _geolocation.PermissionChanged += GeolocationOnPermissionChanged;
            _geolocation.StatusChanged += GeolocationOnStatusChanged;
        }

        private async void GeolocationOnStatusChanged(object sender, StatusChangedEventArgsArgs args)
        {
            try
            {
                this.PermissionDeniedToUseGeolocation = await _geolocation.CheckPermision() == Permision.Denied;
            }
            catch (Exception e)
            {
                
            }
        }

        private void GeolocationOnPermissionChanged(object sender, Permision permision)
        {
            PermissionDeniedToUseGeolocation = permision == Permision.Denied;
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
                                     "- Изменен стиль отображения избранных остановок\n" +
                                     "- Просмотр прибытия выбранного транспорта на остановки (Просмотр остановки)\n" +
                                     //"- Добавляю возможность выбора конкретного времени на вкладке транспорта\n" +
                                     "- Изменен алгоритм отображения остановок на карте\n"
                                    ;

        private IGeolocation _geolocation;

        public bool PermissionDeniedToUseGeolocation { get; private set; }

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
            dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => { base.OnPropertyChanged(propertyName); })
                .AsTask()
                .ConfigureAwait(false);
        }
    }
}