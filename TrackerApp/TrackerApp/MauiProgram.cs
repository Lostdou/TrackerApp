using Microsoft.Extensions.Logging;
using Plugin.LocalNotification;
using TrackerApp.Services;

namespace TrackerApp
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseLocalNotification() 
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            string baseUrl = "http://localhost:5148";
#if ANDROID
            baseUrl = "http://10.0.2.2:5148";
#endif

            builder.Services.AddScoped(sp =>
            {
                var handler = new HttpClientHandler();
                handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
                return new HttpClient(handler) { BaseAddress = new Uri(baseUrl) };
            });

            builder.Services.AddScoped<TrackerService>();

            builder.Services.AddMauiBlazorWebView();

#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}