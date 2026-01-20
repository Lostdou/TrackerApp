using Microsoft.Extensions.Logging;
using Plugin.LocalNotification;
using System.Net.Http; 
using Plugin.LocalNotification.AndroidOption;

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

            builder.Services.AddMauiBlazorWebView();

            builder.Services.AddScoped(sp => new HttpClient
            {
                // Desarrollo
                //BaseAddress = new Uri("http://10.0.2.2:5148/")
                // Prod
                BaseAddress = new Uri("https://doutracker-api.onrender.com/")
            });
            builder.Services.AddScoped<TrackerApp.Services.TrackerService>();

#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}