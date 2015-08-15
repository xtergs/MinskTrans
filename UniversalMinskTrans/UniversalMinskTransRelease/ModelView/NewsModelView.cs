using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MinskTrans.DesctopClient.Modelview;
using MinskTrans.Net;
using MyLibrary;

namespace UniversalMinskTransRelease.ModelView
{
   public class NewsModelView :BaseModelView
    {
        private NewsModelView() { }

        private NewsManagerBase newsManager;
        public NewsModelView(NewsManagerBase NewsManagerBase)
        {
            if (NewsManagerBase == null)
                throw new ArgumentNullException();
            newsManager = NewsManagerBase;
            NewsManagerBase.Load().ContinueWith(x=> {OnPropertyChanged("FilteredStops");});
        }

        public List<DateTime> ListDates
        {
            get
            {
                var tmp =
                    newsManager.AllHotNews.Concat(newsManager.NewNews)
                        .Select(x => x.PostedLocal.Date).Distinct()
                        .OrderByDescending(x => x)
                        .ToList();
                tmp.Insert(0, default(DateTime));
                return tmp;
            }
        }

       public List<string> ListOfDatesStrings
       {
           get
           {
               return ListDates.Select(val =>
               {
                   if (val == default(DateTime))
                       return "За все время";
                   if (val.Date == DateTime.Now.Date)
                       return "Сегодня";
                   if (val.Date == DateTime.Now.Subtract(new TimeSpan(1, 0, 0, 0)).Date)
                       return "Вчера";
                   return val.DayOfWeek + " " + val.Date;
               }
                   ).ToList();
           }
       }

        private DateTime selectedDate;
       private int selectedDateIndex;

       public List<NewsEntry> FilteredStops
        {
            get
            {
                if (SelectedDate == default(DateTime))
                    return newsManager.AllHotNews.Concat(newsManager.NewNews).OrderByDescending(key => key.PostedUtc).ThenByDescending(key => key.RepairedLineUtc).ToList();
                return
                    newsManager.AllHotNews.Concat(newsManager.NewNews)
                        .Where(x => x.PostedLocal.Date == SelectedDate.Date)
                        .OrderByDescending(key => key.PostedUtc)
                        .ThenByDescending(key => key.RepairedLineUtc)
                        .ToList();
            }
        }

       public int SelectedDateIndex
       {
           get { return selectedDateIndex; }
           set
           {
               selectedDateIndex = value;
               SelectedDate = ListDates[value];
               OnPropertyChanged("SelectedDate");
           }
       }

       public DateTime SelectedDate
        {
            get { return selectedDate; }
            set
            {
                selectedDate = value;
                OnPropertyChanged();
                OnPropertyChanged("FilteredStops");
            }
        }
    }
}
