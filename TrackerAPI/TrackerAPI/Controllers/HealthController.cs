using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace TrackerAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class HealthController : ControllerBase
    {
        private readonly IDbConnection _db;

        public HealthController(IDbConnection db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> GetStatus()
        {
            var respuesta = new Dictionary<string, object>
            {
                { "Estado API", "Conectado" }
            };

            try
            {
                await _db.ExecuteAsync("SELECT 1");

                respuesta.Add("Estado BBDD", "Conectado");
                respuesta.Add("hora", DateTime.UtcNow);

                return Ok(respuesta);
            }
            catch (Exception)
            {
                respuesta.Add("Estado BBDD", "No Conectado");
                respuesta.Add("hora", DateTime.UtcNow);

                return StatusCode(500, respuesta);
            }
        }
    }
}