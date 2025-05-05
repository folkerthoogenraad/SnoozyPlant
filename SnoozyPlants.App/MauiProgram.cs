using Microsoft.Extensions.Logging;
using SnoozyPlants.App.Model;
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
                    fonts.AddFont("BubblegumSans-Regular.ttf", "BubblegumSans");
                });

            builder.Services.AddMauiBlazorWebView();

            builder.Services.AddSingleton(sp => GetPlantDatabaseConfiguration());
            builder.Services.AddSingleton<PlantRepository>();
            builder.Services.AddSingleton<ApplicationState>();
#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
    		builder.Logging.AddDebug();
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
