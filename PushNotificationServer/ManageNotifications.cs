using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using GalaSoft.MvvmLight.CommandWpf;
using Microsoft.Azure.NotificationHubs;
using PropertyChanged;
using PushNotificationServer.Annotations;
using PushNotificationServer.Properties;

namespace PushNotificationServer
{
    [ImplementPropertyChanged]
    public class ManageNotificationsSettigns : INotifyPropertyChanged
    {
        public int CountOfDiffDays
        {
            get { return Settings.Default.CountOfDiffDays; }
            set { Settings.Default.CountOfDiffDays = value; }
        }

        public int CleanIntervalHours
        {
            get { return Settings.Default.CleanIntervalHours; }
            set { Settings.Default.CleanIntervalHours = value; }
        }

        public bool DeleteRegistrations
        {
            get { return Settings.Default.DeleteRegistrations; }
            set { Settings.Default.DeleteRegistrations = value; }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }


    [ImplementPropertyChanged]
    public class ManageNotifications
    {
        private NotifyTimeTableChanges _pushService;
        private ManageNotificationsSettigns settigns;


        private Timer timer;
        private Timer elapse;

        private async void CleanNotUsedRegistrationsCallback(object state)
        {
            ToElaps = TimeSpan.Zero;
            await CleanNotUsedRegistrations();
            LastCheckUtc = DateTime.UtcNow;
            ChangeInterval();

        }

        public ManageNotifications(NotifyTimeTableChanges pushSErChanges, ManageNotificationsSettigns settigns)
        {
            if (pushSErChanges == null)
                throw new ArgumentNullException();
            if (settigns == null)
                throw new ArgumentNullException();
            _pushService = pushSErChanges;
            this.settigns = settigns;

            timer = new Timer(CleanNotUsedRegistrationsCallback);
            elapse = new Timer(UntilCleanCallback, null, second, second);

            settigns.PropertyChanged += SettignsOnPropertyChanged;
            

            
                ChangeInterval();

        }

        private void SettignsOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            ChangeInterval();
        }

        private void ChangeInterval()
        {
            if (!settigns.DeleteRegistrations)
            {
                ToElaps = TimeSpan.Zero;
                return;
            }
            var interval = TimeSpan.FromHours(settigns.CleanIntervalHours);
            ToElaps = interval;
            timer.Change(interval, TimeSpan.FromTicks(-1));
        }

        readonly TimeSpan second = new TimeSpan(0, 0, 0, 1, 0);

        private void UntilCleanCallback(object state)
        {
            if (ToElaps > TimeSpan.Zero)
                ToElaps = ToElaps.Subtract(second);
        }

        public TimeSpan ToElaps { get; set; }
        public DateTime LastCheckUtc { get; set; }
        public DateTime LastCheckLocal => LastCheckUtc.ToLocalTime();


        private async Task CleanNotUsedRegistrations()
        {
            var registrations = await _pushService.GetRegistrations();
            var regs = registrations
                .Where(x => x.Tags != null && x.Tags.Any())
                .Select(x => new { Registration = x, DateOrRegistrations = DateTime.FromBinary(long.Parse(x.Tags.First())) })
                .Where(r => (DateTime.UtcNow.Date - r.DateOrRegistrations).TotalDays > settigns.CountOfDiffDays)
                .Select(r => r.Registration).ToArray();

            Debug.WriteLine($"Need to delete {regs.Count()} from {registrations.Length}");

            await _pushService.DeleteRegistrations(regs);

        }

        public RelayCommand GetAllRegistrationsCommand => new RelayCommand(async () =>
        {
            CleanNotUsedRegistrationsCallback(null);
        }
);

        public ManageNotificationsSettigns Settigns
        {
            get { return settigns; }
        }

        public List<NotificationDetails> NotificationStatuses { get; private set; } =
            new List<NotificationDetails>();

        private List<string> observingIds { get; set; } = new List<string>();

        public void AddNotificationObserv(string notificationId)
        {
            observingIds.Add(notificationId);
        }


        public bool GettingStatusesOfNotifications { get; private set; } = false;

        public RelayCommand CheckNotificationsCommand => new RelayCommand(CheckNotifications);

        public async void CheckNotifications()
        {
            var details = await _pushService.GetNotificationDetails(observingIds.ToArray());
            NotificationStatuses = details.ToList();
        }
    }
}
