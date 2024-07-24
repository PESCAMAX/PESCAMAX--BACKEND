using API.Data;
using API.Modelo;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [EnableCors("AllowSpecificOrigin")]
    [ApiController]
    public class AlertaController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AlertaController> _logger;

        public AlertaController(ApplicationDbContext context, ILogger<AlertaController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpPost]
        [EnableCors("AllowSpecificOrigin")]
        public async Task<ActionResult<Alerta>> CrearAlerta(Alerta alerta)
        {
            try
            {
                _logger.LogInformation($"Recibiendo alerta: {JsonSerializer.Serialize(alerta)}");
                if (alerta.FechaCreacion == DateTime.MinValue)
                {
                    alerta.FechaCreacion = DateTime.Now;
                }

                var commandText = "EXEC spCrearAlerta @EspecieID, @Nombre, @LoteID, @Descripcion, @FechaCreacion, @UserId";
                var parameters = new[]
                {
            new SqlParameter("@EspecieID", SqlDbType.Int) { Value = alerta.EspecieID },
            new SqlParameter("@Nombre", SqlDbType.NVarChar, 100) { Value = alerta.Nombre },
            new SqlParameter("@LoteID", SqlDbType.Int) { Value = alerta.LoteID },
            new SqlParameter("@Descripcion", SqlDbType.NVarChar, -1) { Value = alerta.Descripcion },
            new SqlParameter("@FechaCreacion", SqlDbType.DateTime) { Value = alerta.FechaCreacion },
            new SqlParameter("@UserId", SqlDbType.NVarChar, 450) { Value = alerta.UserId }
        };
                await _context.Database.ExecuteSqlRawAsync(commandText, parameters);
                return Ok(alerta);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al crear alerta: {ex.Message}");
                return BadRequest($"Error detallado: {ex.Message}\nStack Trace: {ex.StackTrace}");
            }
        }

        [HttpGet]
        [EnableCors("AllowSpecificOrigin")]
        public async Task<IActionResult> ObtenerAlertas()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("Usuario no autenticado");
                }

                var commandText = "EXEC spObtenerAlertas @UserId";
                var parameter = new SqlParameter("@UserId", SqlDbType.NVarChar, 450) { Value = userId };
                var alertas = await _context.Alertas.FromSqlRaw(commandText, parameter).ToListAsync();
                return Ok(alertas);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener alertas");
                return StatusCode(500, "Error interno del servidor");
            }
        }
    }
}