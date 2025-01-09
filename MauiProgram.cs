using Alcedo.Services.ImageTaggingService;
using Alcedo.Services.SettingsService;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace Alcedo
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
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    fonts.AddFont("MaterialSymbolsOutlined.ttf", "MSO");
                    fonts.AddFont("MaterialSymbolsRounded.ttf", "MSR");
                    fonts.AddFont("MaterialSymbolsSharp.ttf", "MSS");
                });

            using var stream = Assembly.GetExecutingAssembly()
                .GetManifestResourceStream("Alcedo.appsettings.json")
                ?? throw new FileNotFoundException("appsettings.json not found in the resources.");

            var config = new ConfigurationBuilder()
                .AddJsonStream(stream)
                .Build();

            builder.Configuration.AddConfiguration(config);

            builder.Services.AddSingleton<ISettingsService, SettingsService>();
            builder.Services.AddSingleton<IImageTaggingService, OpenAIImageTaggingService>();
            //builder.Services.AddSingleton<IImageTaggingService, OllamaImageTaggingService>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
