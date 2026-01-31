using System.Net.Http.Json;
using Plugin.LocalNotification;

namespace TrackerApp.Services
{
    public class TrackerService
    {
        private readonly HttpClient _http;
        private readonly INotificationService _notificationService;

        public TrackerService(HttpClient http, INotificationService notificationService)
        {
            _http = http;
            _notificationService = notificationService;
        }

        public async Task CheckAndNotify()
        {
            try
            {
                // Obtenemos preferencias (deviceId y pairing code)
                var deviceId = Preferences.Get("my_device_id", "");
                var pairingCode = Preferences.Get("pairing_code", "");

                if (string.IsNullOrEmpty(deviceId) || string.IsNullOrEmpty(pairingCode)) return;

                // Chequeamos ubicacion
                var location = await Geolocation.GetLastKnownLocationAsync() ?? await Geolocation.GetLocationAsync(new GeolocationRequest
                {
                    DesiredAccuracy = GeolocationAccuracy.Medium,
                    Timeout = TimeSpan.FromSeconds(5)
                });

                if (location != null)
                {
                    var userName = Preferences.Get("user_name", $"User {deviceId.Substring(0, 3)}");
                    var data = new { DeviceId = deviceId, PairingCode = pairingCode, Name = userName, Lat = location.Latitude, Lon = location.Longitude };

                    var response = await _http.PostAsJsonAsync("api/Tracker/update", data);

                    if (response.IsSuccessStatusCode)
                    {
                        var result = await response.Content.ReadFromJsonAsync<TrackerResponse>();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en TrackerService: {ex.Message}");
            }
        }

        private async Task ShowNotification(string titulo, string descripcion)
        {
            var request = new NotificationRequest
            {
                NotificationId = new Random().Next(1000, 9999),
                Title = titulo,
                Description = descripcion,
                BadgeNumber = 1,
                Schedule = new NotificationRequestSchedule { NotifyTime = DateTime.Now.AddSeconds(1) },
                Android = new Plugin.LocalNotification.AndroidOption.AndroidOptions
                {
                    IconSmallName = { ResourceName = "appicon" },
                    Priority = Plugin.LocalNotification.AndroidOption.AndroidPriority.High,
                    VisibilityType = Plugin.LocalNotification.AndroidOption.AndroidVisibilityType.Public
                }
            };
            await _notificationService.Show(request);
        }

        public class TrackerResponse
        {
            public string target { get; set; }
            public double distanceKm { get; set; }
            public DateTime lastSeen { get; set; }
            public string Message { get; set; }
        }

        public class MessageDto
        {
            public string SenderName { get; set; }
            public string Content { get; set; }
            public DateTime CreatedAt { get; set; }
        }
    }
}