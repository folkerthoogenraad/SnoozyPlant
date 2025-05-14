using Android;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using AndroidX.Core.App;
using SnoozyPlants.App.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Android.Icu.Text.CaseMap;
using static Android.Provider.CalendarContract;

namespace SnoozyPlants.App.Platforms.Android;

internal class NotificationPermission : Permissions.BasePlatformPermission
{
    public override (string androidPermission, bool isRuntime)[] RequiredPermissions
    {
        get
        {
            var result = new List<(string androidPermission, bool isRuntime)>();
            if (OperatingSystem.IsAndroidVersionAtLeast(33))
                result.Add((Manifest.Permission.PostNotifications, true));
            return result.ToArray();
        }
    }
}


internal class AndroidPlantNotifications : IPlantNotifications
{
    private static AndroidPlantNotifications _instance;

    public static AndroidPlantNotifications Instance
    {
        get
        {
            if(_instance == null)
            {
                _instance = new();
            }

            return _instance;
        }
    }

    const int messageId = 0;

    bool channelInitialized = false;
    const string channelId = "default";
    const string channelName = "Default";
    const string channelDescription = "The default channel for notifications.";

    const int BACKGROUND_TASK_PENDING_INTENT_ID = 0;
    const int NOTIFICATION_MESSAGE_INTEND_ID = 1;

    private PermissionStatus _status = PermissionStatus.Unknown;

    private NotificationManagerCompat compatManager;

    public bool NotificationPermissionGranted => _status == PermissionStatus.Granted;


    public AndroidPlantNotifications()
    {
        _instance = this;
        CreateNotificationChannel();
    }

    public async Task<bool> RequestNotificationPermission()
    {
        _status = await Permissions.RequestAsync<NotificationPermission>();

        return NotificationPermissionGranted;
    }

    public Task SendNotification(string title, string message)
    {
        if (!channelInitialized)
        {
            CreateNotificationChannel();
        }

        Intent intent = new Intent(Platform.AppContext, typeof(MainActivity));
        intent.PutExtra(PlantNotifications.NotificationTitleKey, title);
        intent.PutExtra(PlantNotifications.NotificationBodyKey, message);
        intent.SetFlags(ActivityFlags.SingleTop | ActivityFlags.ClearTop);

        var pendingIntentFlags = (Build.VERSION.SdkInt >= BuildVersionCodes.S)
            ? PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable
        : PendingIntentFlags.UpdateCurrent;

        PendingIntent pendingIntent = PendingIntent.GetActivity(Platform.AppContext, NOTIFICATION_MESSAGE_INTEND_ID, intent, pendingIntentFlags);

        NotificationCompat.Builder builder = new NotificationCompat.Builder(Platform.AppContext, channelId)
            .SetContentIntent(pendingIntent)
            .SetContentTitle(title)
            .SetContentText(message)
            //.SetLargeIcon(BitmapFactory.DecodeResource(Platform.AppContext.Resources, Resource.Drawable.splash))
            .SetSmallIcon(Resource.Drawable.splash);

        Notification notification = builder.Build();
        compatManager.Notify(messageId, notification);


        return Task.CompletedTask;
    }

    public Task CancelNotification()
    {
        if (!channelInitialized)
        {
            CreateNotificationChannel();
        }

        compatManager.Cancel(messageId);
        
        return Task.CompletedTask;
    }

    private void CreateNotificationChannel()
    {
        if (channelInitialized)
        {
            return;
        }

        // Create the notification channel, but only on API 26+.
        if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
        {
            var channelNameJava = new Java.Lang.String(channelName);
            var channel = new NotificationChannel(channelId, channelNameJava, NotificationImportance.Default)
            {
                Description = channelDescription
            };

            // Register the channel
            NotificationManager manager = (NotificationManager)Platform.AppContext.GetSystemService(Context.NotificationService);
            manager.CreateNotificationChannel(channel);
        }

        compatManager = NotificationManagerCompat.From(Platform.AppContext);

        channelInitialized = true;
    }

    private PendingIntent? CreateAlarmPendingIntent(PendingIntentFlags flags)
    {
        var alarmIntent = new Intent(Platform.AppContext, typeof(PlantNotificationAlarmReceiver));
        alarmIntent.SetAction("com.justfgames.snoozyplants.DAILY_ALARM");

        return PendingIntent.GetBroadcast(
            Platform.AppContext,
            BACKGROUND_TASK_PENDING_INTENT_ID,
            alarmIntent,
            flags | PendingIntentFlags.Immutable);
    }

    public Task EnableNotifications()
    {
        var alarmManager = (AlarmManager?)Platform.AppContext.GetSystemService(Context.AlarmService);
        var pendingIntent = CreateAlarmPendingIntent(PendingIntentFlags.CancelCurrent);

        alarmManager.SetInexactRepeating(
            AlarmType.RtcWakeup,
            GetNotifyTime(DateTime.Now.AddDays(1).Date.Add(TimeSpan.FromHours(8))),
            (long)TimeSpan.FromDays(1).TotalMilliseconds,
            pendingIntent);

        return Task.CompletedTask;
    }

    public Task DisableNotifications()
    {
        var alarmManager = (AlarmManager?)Platform.AppContext.GetSystemService(Context.AlarmService);
        var pendingIntent = CreateAlarmPendingIntent(PendingIntentFlags.CancelCurrent);

        alarmManager.Cancel(pendingIntent);

        return Task.CompletedTask;
    }

    public Task<bool> AreNotificationsEnabled()
    {
        var pendingIntent = CreateAlarmPendingIntent(PendingIntentFlags.NoCreate);
        bool isEnabled = pendingIntent != null;

        return Task.FromResult(isEnabled);
    }


    long GetNotifyTime(DateTime notifyTime)
    {
        DateTime utcTime = TimeZoneInfo.ConvertTimeToUtc(notifyTime);
        double epochDiff = (new DateTime(1970, 1, 1) - DateTime.MinValue).TotalSeconds;
        long utcAlarmTime = utcTime.AddSeconds(-epochDiff).Ticks / 10000;
        return utcAlarmTime; // milliseconds
    }
}