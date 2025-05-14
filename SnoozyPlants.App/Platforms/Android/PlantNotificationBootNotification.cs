using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Util;
using SnoozyPlants.App.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnoozyPlants.App.Platforms.Android;

[BroadcastReceiver(Enabled = true, Exported = true)]
[IntentFilter(new[] { Intent.ActionBootCompleted })]
[Register("com.justfgames.snoozyplants.PlantNotificationBootNotification")]
public class PlantNotificationBootNotification : BroadcastReceiver
{
    public override void OnReceive(Context? context, Intent? intent)
    {
        Log.Debug("SnoozyPlants", "Received boot notification!");

        var appSettings = IPlatformApplication.Current?.Services.GetService<AppSettings>();
        var notifications = IPlatformApplication.Current?.Services.GetService<IPlantNotifications>();

        if (appSettings == null || notifications == null)
        {
            Log.Debug("SnoozyPlants", "Cannot get either notifications or appsettings");
            return;
        }

        appSettings.SetLastBootNotification(DateTime.UtcNow).GetAwaiter().GetResult();
        appSettings.SyncNotifications().GetAwaiter().GetResult();
    }
}
