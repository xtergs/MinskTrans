﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight.Command;
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
            NewsManagerBase.PropertyChanged += (sender, args) =>
            {
                var x = FilteredStops; OnPropertyChanged(); OnPropertyChanged(nameof(FilteredStops)); };
            NewsManagerBase.Load().ContinueWith(x=> {OnPropertyChanged("FilteredStops"); OnPropertyChanged();});
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
                   return val.ToString("M");
               }
                   ).ToList();
           }
       }

        private DateTime selectedDate;
       private int selectedDateIndex;

       private ObservableCollection<NewsEntry> filteredStops = new ObservableCollection<NewsEntry>();
       public ObservableCollection<NewsEntry> FilteredStops
        {
            get
            {
                filteredStops.Clear();
                if (SelectedDate == default(DateTime))
                {
                 var news =    newsManager.AllHotNews.Concat(newsManager.NewNews)
                        .OrderByDescending(key => key.PostedUtc)
                        .ThenByDescending(key => key.RepairedLineUtc);
                    foreach (var newsEntry in news)
                    {
                        filteredStops.Add(newsEntry);
                    }
                }
                
                    var newss = newsManager.AllHotNews.Concat(newsManager.NewNews)
                        .Where(x => x.PostedLocal.Date == SelectedDate.Date)
                        .OrderByDescending(key => key.PostedUtc)
                        .ThenByDescending(key => key.RepairedLineUtc);
                foreach (var newsEntry in newss)
                {
                    filteredStops.Add(newsEntry);
                }
                return filteredStops;
            }
           set { OnPropertyChanged(); }
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