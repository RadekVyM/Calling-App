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
                fonts.AddFont("Gabarito-Medium.ttf", "RegularFont");
                fonts.AddFont("Gabarito-SemiBold.ttf", "SemiBoldFont");
                fonts.AddFont("Gabarito-Bold.ttf", "BoldFont");
                fonts.AddFont("Anton.ttf", "TimeFont");
            });

        builder.UseSimpleToolkit();
        
#if DEBUG
        builder.Logging.AddDebug();
#endif

#if ANDROID || IOS
        builder.DisplayContentBehindBars();
#endif

#if ANDROID
        builder.SetDefaultNavigationBarAppearance(Colors.Transparent);
        builder.SetDefaultStatusBarAppearance(Colors.Transparent, false);
#endif

        return builder.Build();
    }
}