using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MinskTrans.Context.Base;
using MinskTrans.Context.Base.BaseModel;
using MinskTrans.Utilites.FuzzySearch;

namespace MinskTrans.Context
{
    public class BussnessLogic :IBussnessLogic
    {
        private IContext context;

        public IContext Context { get { return context; } }
        public bool FuzzySearch { get; set; }

        //public IEnumerable<Stop> GetSimilarStops(string stopFilter = "", TransportType typeOfTransport = TransportType.None)
        //{
        //    var returnList = Context.ActualStops;
        //    if (returnList != null)
        //        returnList = returnList.Where(x => x.Routs.Any(d => typeOfTransport.HasFlag(d.Transport))).ToList();
        //    if (!string.IsNullOrWhiteSpace(stopFilter) && returnList != null)
        //    {
        //        var tempSt = stopFilter.ToLower();
        //        IEnumerable<Stop> temp;
        //        if (FuzzySearch)
        //            temp = Levenshtein.Search(tempSt,  returnList, 0.4);
        //        else
        //            temp = returnList.Where(
        //                x => x.SearchName.Contains(tempSt)).OrderBy(x => x.SearchName.StartsWith(tempSt));
        //        //if (!Equals(LocationXX.Get(), defaultLocation))
        //        //    return
        //        //        SmartSort(temp);
        //        //Enumerable.OrderBy<Stop, double>(temp, (Func<Stop, double>) this.Distance)
        //        //	.ThenByDescending((Func<Stop, uint>) Context.GetCounter);
        //       // else
        //            return temp.OrderByDescending(Context.GetCounter).ThenByDescending(x => x.SearchName.StartsWith(tempSt));

        //    }
        //    if (returnList != null)
        //        //return Equals(LocationXX.Get(), defaultLocation)
        //        return returnList.OrderByDescending(Context.GetCounter).ThenBy(x => x.SearchName);
        //            //: Context.ActualStops.OrderBy(Distance).ThenByDescending(Context.GetCounter);
        //            //: SmartSort(returnList);
        //    return null;
        //} 

    }
}
