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

        public record LocationDto(string DeviceId, string PairingCode, string Name, double Lat, double Lon);

        // Actualizar ubicación
        [HttpPost]
        [Route("update")]
        public async Task<ActionResult<ResponseModel<TrackerResponseDto>>> UpdateLocation([FromBody] LocationDto data)
        {
            if (string.IsNullOrEmpty(data.PairingCode))
            {
                return Ok(new ResponseModel<TrackerResponseDto> { Code = "400", Message = "Falta PairingCode" });
            }

            var sqlUpsert = @"
                MERGE INTO UserLocations AS target
                USING (SELECT @DeviceId AS DeviceId) AS source
                ON (target.DeviceId = source.DeviceId)
                WHEN MATCHED THEN
                    UPDATE SET 
                        PairingCode = @PairingCode,
                        Latitude = @Lat,
                        Longitude = @Lon,
                        LastUpdate = @LastUpdated,
                        Name = CASE WHEN @Name IS NOT NULL AND @Name != '' THEN @Name ELSE target.Name END
                WHEN NOT MATCHED THEN
                    INSERT (DeviceId, PairingCode, Name, Latitude, Longitude, LastUpdate)
                    VALUES (@DeviceId, @PairingCode, @Name, @Lat, @Lon, @LastUpdated);
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


            var sqlGetPartner = @"
                SELECT TOP 1 * FROM UserLocations 
                WHERE PairingCode = @PairingCode 
                  AND DeviceId != @DeviceId 
                ORDER BY LastUpdate DESC";

            var partner = await _db.QueryFirstOrDefaultAsync<UserLocation>(sqlGetPartner, new { data.PairingCode, data.DeviceId });

            if (partner == null)
            {
                return Ok(new ResponseModel<TrackerResponseDto>
                {
                    Code = "202",
                    Message = $"Esperando compañero ({data.PairingCode})...",
                    Detalle = null
                });
            }

            double km = Haversine(data.Lat, data.Lon, partner.Latitude, partner.Longitude);

            return Ok(new ResponseModel<TrackerResponseDto>
            {
                Code = "200",
                Message = "Ok",
                Detalle = new TrackerResponseDto
                {
                    Target = partner.Name,
                    DistanceKm = Math.Round(km, 2),
                    LastSeen = partner.LastUpdate,
                    Message = $"Estás a {Math.Round(km, 2)} km de {partner.Name}",

                    // Ahora tambien le envio las coordenadas del target
                    TargetLat = partner.Latitude,
                    TargetLon = partner.Longitude
                }
            });
        }

        private double Haversine(double lat1, double lon1, double lat2, double lon2)
        {
            var R = 6371;
            var dLat = ToRadians(lat2 - lat1);
            var dLon = ToRadians(lon2 - lon1);
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) + Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) * Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            return R * (2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a)));
        }
        private double ToRadians(double angle) => Math.PI * angle / 180.0;


        public class TrackerResponseDto
        {
            public string Target { get; set; } = "";
            public double DistanceKm { get; set; }
            public DateTime LastSeen { get; set; }
            public string Message { get; set; } = "";
            public double TargetLat { get; set; }
            public double TargetLon { get; set; }
        }
    }
}