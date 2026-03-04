using BPSnake.Services;
using Microsoft.Extensions.Logging;
using BPSnake.Models.GameCore;

namespace BPSnake
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

            builder.Services.AddSingleton<IDatabaseService, DatabaseService>(); // Databáze může zůstat Singleton (sdílený přístup k souboru), případně by mohla být též Scoped podle zvoleného ORM (např. EF Core).
            // Database service exposes ScoresChanged event; no separate leaderboard wrapper required.
            
            // Herní služby převedené na Scoped (každý hráč/session dostane vlastní instanci služeb i vlastní herní smyčku)
            builder.Services.AddScoped<GameLoopService>();
            builder.Services.AddScoped<CollisionService>();
            builder.Services.AddScoped<FoodService>();
            builder.Services.AddScoped<LevelService>();
            builder.Services.AddScoped<GameStateService>();
            builder.Services.AddScoped<ScoreService>();

            // Přidáno jako Scoped, kde jeden uživatel (nebo jedna instance MAUI aplikace) má jednu svou instanci hry.
            // Aplikujeme "Inversion of Control" - o injektování nežádáme konkrétní třídou, ale rozhraním IGameEngine.
            builder.Services.AddScoped<IGameEngine, GameEngine>();

            return builder.Build();
        }
    }
}
