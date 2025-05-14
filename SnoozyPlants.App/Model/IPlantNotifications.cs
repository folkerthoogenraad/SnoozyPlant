
namespace SnoozyPlants.App.Model;

public interface IPlantNotifications
{
    public bool NotificationPermissionGranted { get; }

    public Task<bool> RequestNotificationPermission();

    public Task SendNotification(string title, string message);

    public Task CancelNotification();

    public Task<bool> AreNotificationsEnabled();

    public Task EnableNotifications();
    
    public Task DisableNotifications();
}

public static class PlantNotifications
{
    public const string NotificationTitleKey = "notification.title";
    public const string NotificationBodyKey = "notification.body";
}