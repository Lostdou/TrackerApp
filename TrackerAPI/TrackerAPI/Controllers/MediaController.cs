using Dapper;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using TrackerAPI.Data;
using TrackerAPI.Services;

namespace TrackerAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MediaController : ControllerBase
    {
        private readonly IDbConnection _db;
        private readonly ITmdbService _tmdb;

        public MediaController(IDbConnection db, ITmdbService tmdb)
        {
            _db = db;
            _tmdb = tmdb;
        }

        // Busqueda en TMDB
        [HttpGet]
        [Route("search")]
        public async Task<ActionResult<ResponseModel<object>>> Search([FromQuery] string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return Ok(new ResponseModel<object>
                {
                    Code = "400",
                    Message = "La búsqueda no puede estar vacía",
                    Detalle = null
                });
            }

            var results = await _tmdb.SearchMultiAsync(query);

            var simplified = results.Select(r => new
            {
                TmdbId = r.TmdbId,
                Title = r.DisplayTitle,
                Year = r.DisplayYear,
                Poster = r.FullPosterUrl,
                Type = r.MediaType == "movie" ? "Pelicula" : "Serie",
                MediaTypeKey = r.MediaType,
                Overview = r.Overview
            });

            return Ok(new ResponseModel<object>
            {
                Code = "200",
                Message = $"Se encontraron {results.Count} resultados",
                Detalle = simplified
            });
        }

        // Añadir nueva recomendacion
        [HttpPost]
        [Route("add")]
        public async Task<ActionResult<ResponseModel<string>>> Add([FromBody] AddMediaRequest req)
        {
            if (string.IsNullOrEmpty(req.PairingCode))
                return Ok(new ResponseModel<string> { Code = "400", Message = "Falta el PairingCode" });

            // Verificar duplicados
            var exists = await _db.ExecuteScalarAsync<int>(
                "SELECT COUNT(1) FROM Recommendations WHERE PairingCode = @PairingCode AND TmdbId = @TmdbId",
                new { req.PairingCode, req.TmdbId });

            if (exists > 0)
                return Ok(new ResponseModel<string> { Code = "409", Message = "Este título ya está en la lista" });

            // Buscar en TMDB
            var details = await _tmdb.GetDetailsAsync(req.TmdbId, req.MediaType);
            if (details == null)
                return Ok(new ResponseModel<string> { Code = "404", Message = "No encontrado en TMDB" });

            var sql = @"
                INSERT INTO Recommendations 
                (PairingCode, TmdbId, Title, MediaType, ReleaseYear, Creator, CoverUrl, AddedByDevice, CurrentStatus, CreatedAt)
                VALUES 
                (@PairingCode, @TmdbId, @Title, @MediaType, @ReleaseYear, @Creator, @CoverUrl, @AddedByDevice, 'Pendiente', SYSUTCDATETIME())";

            await _db.ExecuteAsync(sql, new
            {
                req.PairingCode,
                TmdbId = details.TmdbId,
                Title = details.DisplayTitle,
                MediaType = req.MediaType == "movie" ? "Pelicula" : "Serie",
                ReleaseYear = int.TryParse(details.DisplayYear, out int y) ? y : 0,
                Creator = "TMDB",
                CoverUrl = details.FullPosterUrl,
                req.AddedByDevice
            });

            return Ok(new ResponseModel<string> { Code = "200", Message = "Agregado correctamente", Detalle = details.DisplayTitle });
        }

        // Recuperar lista de recomendaciones (y puntuacion)
        [HttpGet]
        [Route("{pairingCode}/{myDeviceId}")]
        public async Task<ActionResult<ResponseModel<IEnumerable<RecommendationItem>>>> Get(string pairingCode, string myDeviceId)
        {
            var sql = @"
                SELECT 
                    r.*,
                    COALESCE((SELECT AVG(CAST(Score AS FLOAT)) FROM RecommendationRatings WHERE RecommendationId = r.Id), 0) as AverageScore,
                    COALESCE((SELECT Score FROM RecommendationRatings WHERE RecommendationId = r.Id AND DeviceId = @myDeviceId), 0) as MyScore
                FROM Recommendations r
                WHERE r.PairingCode = @pairingCode
                ORDER BY r.CreatedAt DESC";

            var list = await _db.QueryAsync<RecommendationItem>(sql, new { pairingCode, myDeviceId });

            return Ok(new ResponseModel<IEnumerable<RecommendationItem>>
            {
                Code = "200",
                Message = "Lista obtenida",
                Detalle = list
            });
        }

        // Añadir calificacion
        [HttpPost]
        [Route("rate")]
        public async Task<ActionResult<ResponseModel<string>>> Rate([FromBody] RateMediaRequest req)
        {
            var sql = @"
                MERGE INTO RecommendationRatings AS target
                USING (SELECT @RecommendationId AS RId, @DeviceId AS DId) AS source
                ON (target.RecommendationId = source.RId AND target.DeviceId = source.DId)
                WHEN MATCHED THEN
                    UPDATE SET Score = @Score, UserName = @UserName
                WHEN NOT MATCHED THEN
                    INSERT (RecommendationId, DeviceId, UserName, Score)
                    VALUES (@RecommendationId, @DeviceId, @UserName, @Score);";

            await _db.ExecuteAsync(sql, req);

            return Ok(new ResponseModel<string> { Code = "200", Message = "Calificación guardada" });
        }

        // Cambiar estado
        [HttpPost]
        [Route("status")]
        public async Task<ActionResult<ResponseModel<string>>> UpdateStatus([FromBody] UpdateStatusRequest req)
        {
            var sql = "UPDATE Recommendations SET CurrentStatus = @NewStatus WHERE Id = @RecommendationId";
            await _db.ExecuteAsync(sql, req);

            return Ok(new ResponseModel<string> { Code = "200", Message = "Estado actualizado" });
        }
    }
}