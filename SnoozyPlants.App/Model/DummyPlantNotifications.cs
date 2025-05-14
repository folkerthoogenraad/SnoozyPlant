using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnoozyPlants.App.Model
{
    internal class DummyPlantNotifications : IPlantNotifications
    {
        public bool NotificationPermissionGranted => false;

        public Task<bool> AreNotificationsEnabled()
        {
            return Task.FromResult(false);
        }

        public Task CancelNotification()
        {
            return Task.CompletedTask;
        }

        public Task DisableNotifications()
        {
            return Task.CompletedTask;
        }

        public Task EnableNotifications()
        {
            return Task.CompletedTask;
        }

        public Task<bool> RequestNotificationPermission()
        {
            return Task.FromResult(false);
        }

        public Task SendNotification(string title, string message)
        {
            return Task.CompletedTask;
        }
    }
}
