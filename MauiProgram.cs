using Alcedo.Services;
using Microsoft.Extensions.Logging;

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

            //builder.Services.AddSingleton<IImageTaggingService, OpenAIImageTaggingService>();
            builder.Services.AddSingleton<IImageTaggingService, OllamaImageTaggingService>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
