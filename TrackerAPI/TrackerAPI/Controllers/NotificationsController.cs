using Dapper;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace TrackerAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationsController : ControllerBase
    {
        private readonly IDbConnection _db;

        public NotificationsController(IDbConnection db)
        {
            _db = db;
        }

        public record SendMessageDto(string SenderDeviceId, string SenderName, string PairingCode, string Content);
        public record MessageResultDto(int Id, string SenderName, string Content, DateTime CreatedAt);


        // Enviar post-it
        [HttpPost]
        [Route("send")]
        public async Task<IActionResult> SendNotification([FromBody] SendMessageDto data)
        {
            var sqlPartner = @"
                SELECT DeviceId FROM UserLocations 
                WHERE PairingCode = @PairingCode 
                  AND DeviceId != @SenderDeviceId";

            var targetDeviceId = await _db.QueryFirstOrDefaultAsync<string>(sqlPartner, new { data.PairingCode, data.SenderDeviceId });

            if (string.IsNullOrEmpty(targetDeviceId))
            {
                return BadRequest(new { message = "No se encontró un compañero con ese código." });
            }

            var sqlInsert = @"
                INSERT INTO PostItMessages (SenderName, TargetDeviceId, Content) 
                VALUES (@SenderName, @TargetDeviceId, @Content)";

            await _db.ExecuteAsync(sqlInsert, new
            {
                data.SenderName,
                TargetDeviceId = targetDeviceId,
                data.Content
            });

            return Ok(new { message = "Mensaje guardado para entrega." });
        }

        // Recuperar el buzon en busca de post-its nuevos
        [HttpGet]
        [Route("check/{deviceId}")]
        public async Task<IActionResult> CheckMessages(string deviceId)
        {
            var sqlGet = "SELECT Id, SenderName, Content, CreatedAt FROM PostItMessages WHERE TargetDeviceId = @DeviceId ORDER BY CreatedAt DESC";
            var messages = await _db.QueryAsync(sqlGet, new { DeviceId = deviceId });

            if (!messages.Any())
            {
                return Ok(new List<MessageResultDto>());
            }

            return Ok(messages.Select(m => new MessageResultDto((int)m.Id, m.SenderName, m.Content, m.CreatedAt)));
        }

        // Borrar nota leida 
        [HttpDelete]
        [Route("delete/{id}")]
        public async Task<IActionResult> DeleteMessage(int id)
        {
            var sqlDelete = "DELETE FROM PostItMessages WHERE Id = @Id";
            await _db.ExecuteAsync(sqlDelete, new { Id = id });
            return Ok(new { message = "Mensaje eliminado" });
        }
    }
}