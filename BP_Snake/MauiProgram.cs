using BP_Snake.Services;
using Microsoft.Extensions.Logging;
using BP_Snake.Models.GameCore;

namespace BP_Snake
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

#if DEBUG
    		builder.Services.AddBlazorWebViewDeveloperTools();
    		builder.Logging.AddDebug();
#endif

            builder.Services.AddSingleton<IDatabaseService, DatabaseService>();
            builder.Services.AddSingleton<GameLoopService>();
            builder.Services.AddSingleton<CollisionService>();
            builder.Services.AddSingleton<FoodService>();
            builder.Services.AddSingleton<LevelService>();

            builder.Services.AddTransient<GameEngine>();

            return builder.Build();
        }
    }
}
