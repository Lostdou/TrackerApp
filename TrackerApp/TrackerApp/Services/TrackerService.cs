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
                // 1. Recuperar datos guardados
                var deviceId = Preferences.Get("my_device_id", "");
                var pairingCode = Preferences.Get("pairing_code", "");

                if (string.IsNullOrEmpty(deviceId) || string.IsNullOrEmpty(pairingCode))
                    return; // No estamos listos todavía

                // 2. Obtener mi GPS actual
                // Nota: Pedimos "LastKnownLocation" primero porque es más rápido y gasta menos batería
                var location = await Geolocation.GetLastKnownLocationAsync() ?? await Geolocation.GetLocationAsync(new GeolocationRequest
                {
                    DesiredAccuracy = GeolocationAccuracy.Medium,
                    Timeout = TimeSpan.FromSeconds(10)
                });

                if (location == null) return;

                // 3. Enviar a la API
                var userName = Preferences.Get("user_name", $"Android {deviceId.Substring(0, 3)}");

                var data = new
                {
                    DeviceId = deviceId,
                    PairingCode = pairingCode,
                    Name = userName,
                    Lat = location.Latitude,
                    Lon = location.Longitude
                };
                var response = await _http.PostAsJsonAsync("api/Tracker/update", data);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<TrackerResponse>();

                    if (result != null && result.distanceKm > 0)
                    {
                        // 4. Calcular "Hace cuánto"
                        var timeSpan = DateTime.Now - result.lastSeen;
                        string tiempoTexto = timeSpan.TotalMinutes < 60
                            ? $"{Math.Ceiling(timeSpan.TotalMinutes)} min"
                            : $"{Math.Round(timeSpan.TotalHours, 1)} hs";

                        // 5. Lanzar Notificación (Formato pedido)
                        await ShowNotification(result.target, result.distanceKm, tiempoTexto);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en Background: {ex.Message}");
            }
        }

        private async Task ShowNotification(string nombre, double distancia, string tiempo)
        {
            var request = new NotificationRequest
            {
                NotificationId = 100,
                Title = "DouTracker",
                // FORMATO: {nombre} esta a {distancia} || Hace {tiempo}
                Description = $"{nombre} está a {distancia} km || Hace {tiempo}",
                BadgeNumber = 1,
                Schedule = new NotificationRequestSchedule
                {
                    NotifyTime = DateTime.Now.AddSeconds(1) // Mostrar inmediatamente
                },
                Android = new Plugin.LocalNotification.AndroidOption.AndroidOptions
                {
                    IconSmallName = { ResourceName = "appicon" },
                    VisibilityType = Plugin.LocalNotification.AndroidOption.AndroidVisibilityType.Public,
                    Priority = Plugin.LocalNotification.AndroidOption.AndroidPriority.High
                }
            };
            await _notificationService.Show(request);
        }

        // Clase auxiliar para leer respuesta
        public class TrackerResponse
        {
            public string target { get; set; }
            public double distanceKm { get; set; }
            public DateTime lastSeen { get; set; }
        }
    }
}