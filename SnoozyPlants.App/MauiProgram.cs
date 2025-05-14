using Microsoft.Extensions.Logging;
using SnoozyPlants.App.Model;
using SnoozyPlants.App.Server;
using SnoozyPlants.Core;

namespace SnoozyPlants.App
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            builder.Services.AddMauiBlazorWebView();

            builder.Services.AddSingleton(sp => GetPlantDatabaseConfiguration());
            builder.Services.AddSingleton<PlantRepository>();
            builder.Services.AddSingleton<AppSettings>();
            builder.Services.AddSingleton<ApplicationState>();
            builder.Services.AddSingleton<PlantImageServer>();
#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
    		builder.Logging.AddDebug();
#endif

#if ANDROID
            builder.Services.AddTransient<IPlantNotifications, Platforms.Android.AndroidPlantNotifications>();
#else
            builder.Services.AddTransient<IPlantNotifications, DummyPlantNotifications>();          
#endif

            return builder.Build();
        }

        public static PlantDatabaseConfiguration GetPlantDatabaseConfiguration()
        {
            string path = Path.Combine(FileSystem.AppDataDirectory, "plant_database.sqlite");
            
            return new PlantDatabaseConfiguration()
            {
                FilePath = path,
            };
        }
    }
}
