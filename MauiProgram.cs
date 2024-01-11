using MudBlazor.Services;
using QuestPDF.Infrastructure;

namespace BisleriumCafe
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiAppAsync()
        {
            MauiAppBuilder builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            builder.Services.AddMauiBlazorWebView();

#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
			QuestPDF.Settings.License = LicenseType.Community;
#endif

			builder.Services.AddMudServices(config =>
            {
                config.SnackbarConfiguration.VisibleStateDuration = 4000;
                config.SnackbarConfiguration.HideTransitionDuration = 200;
                config.SnackbarConfiguration.ShowTransitionDuration = 200;
                config.SnackbarConfiguration.MaxDisplayedSnackbars = 6;
                config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomStart;
            });

			QuestPDF.Settings.License = LicenseType.Community;

			builder.Services.AddJsonFileProvider();

            builder.Services.AddRepository();

            builder.Services.AddSeeder();

            builder.Services.AddSession();

            builder.Services.AddAuth();

            return builder.Build();
        }
    }
}



