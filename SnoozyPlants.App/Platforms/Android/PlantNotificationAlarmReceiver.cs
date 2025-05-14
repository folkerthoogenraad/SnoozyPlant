using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Util;
using SnoozyPlants.App.Model;
using SnoozyPlants.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnoozyPlants.App.Platforms.Android;

[Register("com.justfgames.snoozyplants.PlantNotificationAlarmReceiver")]
[BroadcastReceiver(Enabled = true, Exported = true, Label = "Broadcast receiver for the plant notifications")]
[IntentFilter(new[] { "com.justfgames.snoozyplants.DAILY_ALARM" })]
public class PlantNotificationAlarmReceiver : BroadcastReceiver
{
    public override void OnReceive(Context? context, Intent? intent)
    {
        Log.Debug("SnoozyPlants", "Received alarm at " + DateTime.UtcNow);

        // TODO move this into a nicer spot, but whatever
        var repository = IPlatformApplication.Current?.Services?.GetService<PlantRepository>();
        var settings = IPlatformApplication.Current?.Services?.GetService<AppSettings>();
        var notifications = IPlatformApplication.Current?.Services?.GetService<IPlantNotifications>();


        if (repository == null)
        {
            Log.Debug("SnoozyPlants", "Repository is null. How does that happen?");
            return;
        }
        if (settings == null)
        {
            Log.Debug("SnoozyPlants", "Settings is null. How does that happen?");
            return;
        }

        if (notifications == null)
        {
            Log.Debug("SnoozyPlants", "Notifications is null. How does that happen?");
            return;
        }

        if (!settings.AreNotificationsEnabled().GetAwaiter().GetResult())
        {
            Log.Debug("SnoozyPlants", "Notifications should not be enabled?");
            notifications.DisableNotifications();

            return;
        }

        int count = 0;

        var plants = repository.GetPlantsAsync().GetAwaiter().GetResult();

        foreach (var plant in plants)
        {
            if (!plant.NextWateringDate.HasValue)
                continue;

            if(DateTime.Now > plant.NextWateringDate.Value)
            {
                count++;
            }
        }

        Log.Debug("SnoozyPlants", $"{count} plants are thirsty");

        if (count > 0)
        {
            string text = count <= 1 ?
                $"There is 1 plant that needs watering today!"
                : $"There are still {count} plants that need watering today!";

            string heading = count <= 1 ?
                $"Hey! A plant is thirsty!"
                : $"Hey! Some of your plants are thirsty!";

            notifications.SendNotification(heading, text);
        }
        else
        {
            notifications.CancelNotification();
        }
    }
}
