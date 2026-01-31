using System.Net.Http.Json;
using TrackerAPI.Data;

namespace TrackerAPI.Services
{
    public interface ITmdbService
    {
        Task<List<TmdbResult>> SearchMultiAsync(string query);
        Task<TmdbResult?> GetDetailsAsync(int id, string mediaType);
    }

    public class TmdbService : ITmdbService
    {
        private readonly HttpClient _http;
        private readonly string _apiKey;

        public TmdbService(HttpClient http, IConfiguration config)
        {
            _http = http;
            _apiKey = config["Tmdb:ApiKey"];
        }

        public async Task<List<TmdbResult>> SearchMultiAsync(string query)
        {
            var url = $"search/multi?api_key={_apiKey}&language=es-MX&query={Uri.EscapeDataString(query)}&include_adult=false";
            var response = await _http.GetFromJsonAsync<TmdbSearchResponse>(url);

            if (response?.Results == null) return new List<TmdbResult>();

            // Filtramos solo Películas y Series
            return response.Results
                .Where(x => x.MediaType == "movie" || x.MediaType == "tv")
                .ToList();
        }

        public async Task<TmdbResult?> GetDetailsAsync(int id, string mediaType)
        {
            // mediaType va a ser 'movie' o 'tv'
            var url = $"{mediaType}/{id}?api_key={_apiKey}&language=es-MX";

            var result = await _http.GetFromJsonAsync<TmdbResult>(url);

            if (result != null)
            {
                result.MediaType = mediaType;
            }
            return result;
        }
    }
}