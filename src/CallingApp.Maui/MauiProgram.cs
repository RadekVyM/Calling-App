using Microsoft.Extensions.Logging;
using SimpleToolkit.Core;

namespace CallingApp.Maui;

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
                fonts.AddFont("Anton.ttf", "TimeFont");
                fonts.AddFont("AccidentalPresidency.ttf", "PriceFont");
            });

        builder.UseSimpleToolkit();
        
#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}