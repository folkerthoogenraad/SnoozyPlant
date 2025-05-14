using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Telephony.Data;
using Android.Util;
using Android.Views;
using SnoozyPlants.App.Model;
using static Android.Views.View;

namespace SnoozyPlants.App
{
    //class TestInsetAnimationCallback : WindowInsetsAnimationCompat.Callback
    //{
    //    public TestInsetAnimationCallback() : base(DispatchModeStop)
    //    {
    //    }

    //    public override WindowInsetsAnimationCompat.BoundsCompat OnStart(WindowInsetsAnimationCompat animation, WindowInsetsAnimationCompat.BoundsCompat bounds)
    //    {
    //        return bounds;
    //    }

    //    public override void OnPrepare(WindowInsetsAnimationCompat animation)
    //    {
    //        //_startHeight = _controller.WindowInsets.GetInsets(WindowInsetsCompat.Type.Ime()).Bottom;
    //        base.OnPrepare(animation);
    //    }

    //    public override WindowInsetsCompat OnProgress(WindowInsetsCompat insets, IList<WindowInsetsAnimationCompat> runningAnimations)
    //    {
    //        WindowInsetsAnimationCompat imeAnimation = null;
    //        foreach (var animation in runningAnimations)
    //        {
    //            if ((animation.TypeMask & WindowInsetsCompat.Type.Ime()) != 0)
    //            {
    //                imeAnimation = animation;
    //                break;
    //            }
    //        }
    //        if (imeAnimation != null)
    //        {
    //            _controller._frame.TranslationY = (_endHeight - _startHeight) * (1 - imeAnimation.InterpolatedFraction);
    //        }
    //        return insets;
    //    }
    //}

    public class Test : Java.Lang.Object, IOnApplyWindowInsetsListener
    {
        public WindowInsets OnApplyWindowInsets(Android.Views.View v, WindowInsets insets)
        {
            bool imeVisible = insets.IsVisible(WindowInsets.Type.Ime());
            int imeHeight = insets.GetInsets(WindowInsets.Type.Ime()).Bottom;

            DeviceState.SoftKeyboardOpen = imeVisible;
            DeviceState.ScreenMarginBottom = imeHeight;

            return insets;
        }
    }

    //[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    [Activity(Theme = "@style/SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            var settings = IPlatformApplication.Current?.Services.GetService<AppSettings>();
            var service = IPlatformApplication.Current?.Services.GetService<IPlantNotifications>();

            if (settings != null)
            {

                settings.SyncNotifications().GetAwaiter().GetResult();
            }

            if (service != null)
            {
                service.CancelNotification();
            }

            var view = Window?.DecorView;


            var metrics = new DisplayMetrics();

            WindowManager?.DefaultDisplay.GetMetrics(metrics);

            DeviceState.DevicePixelRatio = metrics.Density;

            if(view != null)
            {
                view.SetOnApplyWindowInsetsListener(new Test());
            }

            Window?.AddFlags(WindowManagerFlags.LayoutNoLimits);
        }

        protected override void OnNewIntent(Intent? intent)
        {
            base.OnNewIntent(intent);

            CreateNotificationFromIntent(intent);
        }

        static void CreateNotificationFromIntent(Intent ?intent)
        {
            if (intent == null) return;
            if (intent?.Extras == null) return;

            string? title = intent.GetStringExtra(PlantNotifications.NotificationTitleKey);
            string? message = intent.GetStringExtra(PlantNotifications.NotificationBodyKey);

            if (title == null || message == null)
            {
                return;
            }

            if (IPlatformApplication.Current == null) return;

            var service = IPlatformApplication.Current.Services.GetService<IPlantNotifications>();

            service?.CancelNotification();
        }

    }
}
