using System.Text.Json.Serialization;

namespace TrackerApp.Data
{
    // --- WRAPPER GENÉRICO (Coincide con ResponseModel<T> de la API) ---
    public class ResponseModel<T>
    {
        [JsonPropertyName("code")]
        public string Code { get; set; } = "200";

        [JsonPropertyName("message")]
        public string Message { get; set; } = "";

        [JsonPropertyName("detalle")]
        public T Detalle { get; set; }
    }

    // --- DTOs TRACKER ---
    public class TrackerResponseDto
    {
        public string Target { get; set; }
        public double DistanceKm { get; set; }
        public DateTime LastSeen { get; set; }
        public string Message { get; set; }
    }

    // --- DTOs MEDIA HUB ---
    public class RecommendationItem
    {
        public int Id { get; set; }
        public int TmdbId { get; set; }
        public string Title { get; set; }
        public string MediaType { get; set; } // "Pelicula" o "Serie"
        public int ReleaseYear { get; set; }
        public string Creator { get; set; }
        public string CoverUrl { get; set; }
        public string CurrentStatus { get; set; } // "Pendiente", "Viendo", "Terminado"
        public double AverageScore { get; set; }
        public int MyScore { get; set; }
    }

    public class TmdbResultDto
    {
        public int TmdbId { get; set; }
        public string Title { get; set; }
        public string Year { get; set; }
        public string Poster { get; set; }
        public string Type { get; set; }
        public string MediaTypeKey { get; set; } // "movie" o "tv"
        public string Overview { get; set; }
    }

    // --- REQUESTS (Para enviar datos a la API) ---
    public class AddMediaRequest
    {
        public int TmdbId { get; set; }
        public string MediaType { get; set; }
        public string PairingCode { get; set; }
        public string AddedByDevice { get; set; }
    }

    public class RateMediaRequest
    {
        public int RecommendationId { get; set; }
        public string DeviceId { get; set; }
        public string UserName { get; set; }
        public int Score { get; set; }
    }

    public class UpdateStatusRequest
    {
        public int RecommendationId { get; set; }
        public string NewStatus { get; set; }
    }
}