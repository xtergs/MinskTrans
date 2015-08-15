//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using MinskTrans.AutoRouting.AutoRouting;
//using MinskTrans.Context.Base.BaseModel;

//namespace MinskTrans.Utilites.FuzzySearch
//{
//    public struct LocationXX
//    {
//        private static Location loc;
//        private static readonly object o = new object();

//        public static Location Get()
//        {
//            lock (o)
//            {
//                if (loc == null)
//                    loc = new Location(0, 0);
//            }
//            return loc;
//        }
//    }
//    class StopsFilterMaster
//    {
//        public bool UseFuzzySearch { get; set; }
//        public bool UseFrequencyView { get; set; }

//        public bool UseCoordinates { get; set; }

//        private TransportType selectedTransport;
//        public string StopNameFilter { get; set; }

//        List<Stop> Filter(List<Stop> inputList)
//        {
//            IList<Stop> returnList = inputList;
//            if (returnList != null)
//                returnList = returnList.Where(x => x.Routs.Any(d => selectedTransport.HasFlag(d.Transport))).ToList();
//            if (!string.IsNullOrWhiteSpace(StopNameFilter) && returnList != null)
//            {
//                var tempSt = StopNameFilter.ToLower();
//                IEnumerable<Stop> temp;
//                if (FuzzySearch)
//                    temp = Levenshtein.Search(tempSt, returnList, 0.4);
//                else
//                    temp = returnList.Where(
//                        x => x.SearchName.Contains(tempSt)).OrderBy(x => x.SearchName.StartsWith(tempSt));
//                if (!Equals(LocationXX.Get(), defaultLocation))
//                    return
//                        SmartSort(temp);
//                //Enumerable.OrderBy<Stop, double>(temp, (Func<Stop, double>) this.Distance)
//                //	.ThenByDescending((Func<Stop, uint>) Context.GetCounter);
//                else
//                    return temp.OrderByDescending(Context.GetCounter).ThenByDescending(x => x.SearchName.StartsWith(tempSt));

//            }
//            if (returnList != null)
//                return Equals(LocationXX.Get(), defaultLocation)
//                    ? returnList.OrderByDescending(Context.GetCounter).ThenBy(x => x.SearchName)
//                    //: Context.ActualStops.OrderBy(Distance).ThenByDescending(Context.GetCounter);
//                    : SmartSort(returnList);
//            return null;
//        }

//        EquirectangularDistance distance = new EquirectangularDistance();
//        private double Distance(Stop x)
//        {
//            return distance.CalculateDistance(LocationXX.Get().Latitude, LocationXX.Get().Longitude, x.Lat, x.Lng);
//            //return Math.Abs(Math.Sqrt(Math.Pow( - x.Lng, 2) + Math.Pow(LocationXX.Get().Latitude - x.Lat, 2)));
//        }

//        private IEnumerable<Stop> SmartSort(IEnumerable<Stop> stops)
//        {

//            var byDeistance = stops.OrderBy(Distance).ToList();
//            var result = stops.OrderByDescending(x => Context.GetCounter(x))
//                .Select((x, i) => new { x, byCounter = i, byDistance = byDeistance.IndexOf(x) })
//                .OrderBy(x => x.byCounter + x.byDistance)
//                .Select(x => x.x)
//                .ToList();
//            return result;

//            //return stops.OrderByDescending(x=>
//            //{
//            //	double counter = Context.GetCounter(x);
//            //	if (counter == 0)
//            //		counter = 0;
//            //	else
//            //		counter = counter/ Context.ActualStops.Count;
//            //	double dist = Distance(x);
//            //	dist = 1/dist;
//            //	return dist + counter;
//            //});
//        }
//    }
//}
