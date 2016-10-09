using System;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using CommonLibrary;
using MinskTrans.Context.Base;

namespace UniversalMinskTransRelease.Service
{
    public class UWPGeolocator : UniversalGeolocator, IGeolocation
    {
        public override async Task<Permision> CheckPermision()
        {
            var statusAccess = await Geolocator.RequestAccessAsync();
            Permision permis = Permision.Unspecified;
            switch (statusAccess)
            {
                    case GeolocationAccessStatus.Denied:
                    permis =  Permision.Denied;
                    break;
                case GeolocationAccessStatus.Allowed:
                    permis = Permision.Allow;
                    break;
                case GeolocationAccessStatus.Unspecified:
                    permis=  Permision.Unspecified;
                    break;
            }
            OnPermissionChanged(permis);
            return permis;
        }
    }
}
