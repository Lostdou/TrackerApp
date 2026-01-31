using Microsoft.Extensions.Logging;
using Plugin.LocalNotification;
using TrackerApp.Services;
using Microsoft.Extensions.Configuration; // Necesario
using System.Reflection; // Necesario

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

            var assembly = Assembly.GetExecutingAssembly();
            using var stream = assembly.GetManifestResourceStream("TrackerApp.appsettings.json");

            if (stream != null)
            {
                var config = new ConfigurationBuilder()
                    .AddJsonStream(stream)
                    .Build();

                builder.Configuration.AddConfiguration(config);
            }

            string baseUrl = builder.Configuration["ApiSettings:BaseUrl"]
                            ?? throw new InvalidOperationException("No encuentra la URL base");

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