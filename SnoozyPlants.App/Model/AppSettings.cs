using SnoozyPlants.Core;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnoozyPlants.App.Model;

public class AppSettings(IPlantNotifications notifications, PlantRepository repository)
{
    private bool _initialized;

    private bool _notificationsEnabled;

    public async Task<bool> AreNotificationsEnabled()
    {
        await Init();

        return _notificationsEnabled;
    }


    public async Task SyncNotifications()
    {
        await Init();

        if (_notificationsEnabled)
        {
            await notifications.EnableNotifications();
        }
        else
        {
            await notifications.DisableNotifications();
            await notifications.CancelNotification();
        }
    }

    public async Task SetNotificationsEnabled(bool enabled)
    {
        await Init();

        _notificationsEnabled = enabled;

        if (enabled)
        {
            await notifications.EnableNotifications();
        }
        else
        {
            await notifications.DisableNotifications();
            await notifications.CancelNotification();
        }

        await Save();
    }

    public async Task Init()
    {
        if (_initialized) return;

        _initialized = true;

        await Load();
    }

    public async Task Save()
    {
        await repository.SetSettingAsync("NotificationsEnabled", _notificationsEnabled ? "true" : "false");
    }

    public async Task Load()
    {
        _notificationsEnabled = (await repository.GetSettingAsync("NotificationsEnabled") == "true");
    }

    public async Task SetLastBootNotification(DateTime time)
    {
        var date = DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture);

        await repository.SetSettingAsync("LastBootNotification", date);
    }

    public async Task<DateTime> GetLastBootNotification()
    {
        var result = await repository.GetSettingAsync("LastBootNotification");

        if (result == null)
        {
            return DateTime.MinValue;
        }

        return DateTime.Parse(result);
    }
}
