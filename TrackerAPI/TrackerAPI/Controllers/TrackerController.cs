using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using TrackerAPI.Data;

namespace TrackerAPI.Controllers
{
    [ApiController]
    [AllowAnonymous]
    [Route("api/[controller]")]
    public class TrackerController : ControllerBase
    {
        private readonly IDbConnection _db;

        public TrackerController(IDbConnection db)
        {
            _db = db;
        }

        // Definimos el DTO
        public record LocationDto(string DeviceId, string PairingCode, string Name, double Lat, double Lon);

        [HttpPost("update")]
        public async Task<IActionResult> UpdateLocation([FromBody] LocationDto data)
        {
            if (string.IsNullOrEmpty(data.PairingCode))
                return BadRequest("Falta el Código de Vinculación (PairingCode)");

            // --- SINTAXIS POSTGRESQL ---
            // 'EXCLUDED' se refiere a los valores nuevos que intentaste insertar
            var sqlUpsert = @"
                INSERT INTO UserLocations (DeviceId, PairingCode, Name, Latitude, Longitude, LastUpdate) 
                VALUES (@DeviceId, @PairingCode, @Name, @Lat, @Lon, @LastUpdated)
                ON CONFLICT (DeviceId) DO UPDATE SET
                    PairingCode = EXCLUDED.PairingCode,
                    Latitude = EXCLUDED.Latitude,
                    Longitude = EXCLUDED.Longitude,
                    LastUpdate = EXCLUDED.LastUpdate,
                    -- Lógica: Si viene un nombre nuevo, úsalo. Si no, mantén el viejo (UserLocations.Name)
                    Name = CASE WHEN @Name IS NOT NULL AND @Name != '' THEN @Name ELSE UserLocations.Name END;
            ";

            await _db.ExecuteAsync(sqlUpsert, new
            {
                data.DeviceId,
                data.PairingCode,
                data.Name,
                data.Lat,
                data.Lon,
                LastUpdated = DateTime.UtcNow
            });

            // Buscar a la pareja
            var sqlGetPartner = @"
                SELECT * FROM UserLocations 
                WHERE PairingCode = @PairingCode 
                  AND DeviceId != @DeviceId 
                ORDER BY LastUpdate DESC 
                LIMIT 1";

            var partner = await _db.QueryFirstOrDefaultAsync<UserLocation>(sqlGetPartner, new { data.PairingCode, data.DeviceId });

            if (partner == null)
            {
                return Ok(new { message = $"Esperando a alguien con el código: {data.PairingCode}..." });
            }

            double km = Haversine(data.Lat, data.Lon, partner.Latitude, partner.Longitude);

            return Ok(new
            {
                target = partner.Name,
                distanceKm = Math.Round(km, 2),
                lastSeen = partner.LastUpdate,
                message = $"Estás a {Math.Round(km, 2)} km de {partner.Name}"
            });
        }

        // --- GET para ver datos desde el navegador (Debugging) ---
        [HttpGet("{deviceId}")]
        public async Task<IActionResult> GetByDevice(string deviceId)
        {
            var sql = "SELECT * FROM UserLocations WHERE DeviceId = @deviceId";
            var user = await _db.QueryFirstOrDefaultAsync<UserLocation>(sql, new { deviceId });

            if (user == null) return NotFound("Dispositivo no encontrado");

            return Ok(user);
        }

        private double Haversine(double lat1, double lon1, double lat2, double lon2)
        {
            var R = 6371;
            var dLat = ToRadians(lat2 - lat1);
            var dLon = ToRadians(lon2 - lon1);
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c;
        }
        
        private double ToRadians(double angle) => Math.PI * angle / 180.0;
    }
}