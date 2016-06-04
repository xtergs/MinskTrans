

using System.Collections.Generic;

namespace MinskTrans.Context.Geopositioning
{
    struct AdressLocation
    {
        public Location Location { get; set; }
        public string Adress { get; set; }
    }
    class AdressResults
    {
        public AdressLocation Address { get; set; }
       public List<AdressLocation> PosibleAddresses { get; set; }  
    }
    interface IPositionAddress
    {
        Location GetLocationFromAddress(string query);
        AdressResults[] GetAddressByLocation(Location loc);
    }
}
