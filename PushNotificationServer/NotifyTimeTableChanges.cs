using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Documents;
using GalaSoft.MvvmLight.CommandWpf;
using Microsoft.Azure.NotificationHubs;
using MinskTrans.Net.Base;
using PropertyChanged;
using PushNotificationServer.Annotations;
using PushNotificationServer.Properties;
using UniversalMinskTransRelease.Helpers;

namespace PushNotificationServer
{
	[ImplementPropertyChanged]
	public class NotifyTimeTableChangesSettings : INotifyPropertyChanged
	{
		public NotifyTimeTableChangesSettings()
		{
			UsePushNotifications = Settings.Default.UsePushNotifications;
			ChanelEndPoint = Settings.Default.ChanelEndPoint ?? AppServerConstants.PushNotificationChanelEndPoint;
			HubName = Settings.Default.HubName ?? AppServerConstants.PushNotificationChanelHubName;
			SaveCommand = new RelayCommand(() =>
			{
				Settings.Default.UsePushNotifications = UsePushNotifications;
				Settings.Default.ChanelEndPoint = ChanelEndPoint;
				Settings.Default.HubName = HubName;
				Settings.Default.Save();
			});
		}

		public bool UsePushNotifications { get; set; } = false;
		public string ChanelEndPoint { get; set; } = AppServerConstants.PushNotificationChanelEndPoint;
		public string HubName { get; set; } = AppServerConstants.PushNotificationChanelHubName;

		public RelayCommand SaveCommand { get; }

		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
	public class NotifyTimeTableChanges
	{
		public NotifyTimeTableChangesSettings Settings { get; }

		public NotifyTimeTableChanges()
		{
			Settings = new NotifyTimeTableChangesSettings();
		}

		private NotificationHubClient GetHub()
		{
			NotificationHubClient hub = NotificationHubClient
				.CreateClientFromConnectionString(Settings.ChanelEndPoint, Settings.HubName);
			return hub;
		}
		public async void SendNotificationAsync(string text)
		{
			if (!Settings.UsePushNotifications)
				return;
			var hub = GetHub();
			var toast = $@"<toast><visual><binding template=""ToastText01""><text id=""1"">{text}</text></binding></visual></toast>";
			var result = await hub.SendWindowsNativeNotificationAsync(toast);
			
		}

		public async Task<NotificationOutcome> SendNotificationAsync(TypeOfUpdates updates)
		{
			if (!Settings.UsePushNotifications)
				return new NotificationOutcome();
			var hub = GetHub();
			var toast = $@"{updates}";
			var notification = new WindowsNotification(toast);
			notification.ContentType = "application/octet-stream";
			notification.Headers["X-WNS-Type"] = @"wns/raw";
			var result  = await hub.SendNotificationAsync(notification);
			return result;
		}

		public async Task<RegistrationDescription[]> GetRegistrations()
		{
			if (!Settings.UsePushNotifications)
				return new RegistrationDescription[0];
			var hub = GetHub();
			var notifications = await hub.GetAllRegistrationsAsync(0);
			return notifications.ToArray();
		}

		public Task DeleteRegistrations(RegistrationDescription[] registrations)
		{
			if (registrations == null || !registrations.Any())
				return Task.FromResult(true);
			var hub = GetHub();
			List<Task> tasks = new List<Task>(registrations.Length);
			for (int i = 0; i < registrations.Count(); i++)
				tasks.Add(hub.DeleteRegistrationAsync(registrations[i]));
			return Task.WhenAll(tasks);
		}

		public Task<NotificationDetails[]> GetNotificationDetails(string[] notificationIds)
		{
			var hub = GetHub();
			var tasks = new List<Task<NotificationDetails>>(notificationIds.Length);
			tasks.AddRange(notificationIds.Select(notificationId => hub.GetNotificationOutcomeDetailsAsync(notificationId)));
			return Task.WhenAll(tasks);
			
		}
	}
}