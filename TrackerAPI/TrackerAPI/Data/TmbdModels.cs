using System.Text.Json.Serialization;

namespace TrackerAPI.Data
{
    // === DTOs para TMDB (Internos) ===
    public class TmdbSearchResponse { [JsonPropertyName("results")] public List<TmdbResult> Results { get; set; } = new(); }

    public class TmdbResult
    {
        [JsonPropertyName("id")] public int TmdbId { get; set; }
        [JsonPropertyName("title")] public string? Title { get; set; }
        [JsonPropertyName("name")] public string? Name { get; set; }
        [JsonPropertyName("media_type")] public string MediaType { get; set; } = "movie";
        [JsonPropertyName("poster_path")] public string? PosterPath { get; set; }
        [JsonPropertyName("release_date")] public string? ReleaseDate { get; set; }
        [JsonPropertyName("first_air_date")] public string? FirstAirDate { get; set; }
        [JsonPropertyName("overview")] public string? Overview { get; set; }

        public string DisplayTitle => !string.IsNullOrEmpty(Title) ? Title : Name ?? "Sin Título";
        public string DisplayYear => (ReleaseDate?.Length >= 4 ? ReleaseDate.Substring(0, 4) : (FirstAirDate?.Length >= 4 ? FirstAirDate.Substring(0, 4) : "N/A"));
        public string FullPosterUrl => !string.IsNullOrEmpty(PosterPath) ? $"https://image.tmdb.org/t/p/w500{PosterPath}" : "";
    }

    // === Modelos de la Base de Datos ===
    public class RecommendationItem
    {
        public int Id { get; set; }
        public int TmdbId { get; set; } // Nuevo campo
        public string PairingCode { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string MediaType { get; set; } = "Pelicula";
        public int ReleaseYear { get; set; }
        public string Creator { get; set; } = string.Empty;
        public string CoverUrl { get; set; } = string.Empty;
        public string CurrentStatus { get; set; } = "Pendiente";

        // Vistas
        public double AverageScore { get; set; }
        public int MyScore { get; set; }
    }

    // === Requests ===
    public class AddMediaRequest
    {
        public int TmdbId { get; set; }
        public string MediaType { get; set; } = "movie";
        public string PairingCode { get; set; } = string.Empty;
        public string AddedByDevice { get; set; } = string.Empty;
    }

    public class UpdateStatusRequest
    {
        public int RecommendationId { get; set; }
        public string NewStatus { get; set; } = "Pendiente";
    }

    public class RateMediaRequest
    {
        public int RecommendationId { get; set; }
        public string DeviceId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public int Score { get; set; }
    }
}