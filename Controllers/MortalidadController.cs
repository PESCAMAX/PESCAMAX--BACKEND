using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Security.Claims;
using API.Modelo;
using System.Data.SqlClient;


namespace API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class MortalidadController : ControllerBase
    {
        private readonly string _cadenaSQL;
        private readonly ILogger<MortalidadController> _logger;

        public MortalidadController(IConfiguration configuration, ILogger<MortalidadController> logger)
        {
            _cadenaSQL = configuration.GetConnectionString("DefaultConnection");
            _logger = logger;
        }

        private string GetUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }

        [HttpPost("Registrar")]
        [EnableCors("AllowSpecificOrigin")]
        public async Task<IActionResult> RegistrarMortalidad([FromBody] Mortalidad mortalidad)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            try
            {
                using (var conexion = new SqlConnection(_cadenaSQL))
                {
                    await conexion.OpenAsync();
                    var cmd = new SqlCommand("RegistrarMortalidad", conexion)
                    {
                        CommandType = CommandType.StoredProcedure
                    };

                    cmd.Parameters.AddWithValue("@UserId", userId);
                    cmd.Parameters.AddWithValue("@LoteId", mortalidad.LoteId);
                    cmd.Parameters.AddWithValue("@CantidadMuertos", mortalidad.CantidadMuertos);

                    await cmd.ExecuteNonQueryAsync();
                }

                return Ok(new { mensaje = "Mortalidad registrada correctamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar mortalidad");
                return StatusCode(500, new { mensaje = "Error interno del servidor" });
            }
        }

        [HttpGet("ObtenerTotal/{loteId}")]
        [EnableCors("AllowSpecificOrigin")]
        public async Task<IActionResult> ObtenerMortalidadTotal(int loteId)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            try
            {
                using (var conexion = new SqlConnection(_cadenaSQL))
                {
                    await conexion.OpenAsync();
                    var cmd = new SqlCommand("ObtenerMortalidadTotal", conexion)
                    {
                        CommandType = CommandType.StoredProcedure
                    };

                    cmd.Parameters.AddWithValue("@UserId", userId);
                    cmd.Parameters.AddWithValue("@LoteId", loteId);

                    var result = await cmd.ExecuteScalarAsync();
                    int totalMortalidad = result != DBNull.Value ? Convert.ToInt32(result) : 0;

                    return Ok(new { totalMortalidad });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener mortalidad total");
                return StatusCode(500, new { mensaje = "Error interno del servidor" });
            }
        }

        [HttpGet("Listar")]
        [EnableCors("AllowSpecificOrigin")]
        public async Task<IActionResult> ListarMortalidad()
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            try
            {
                using (var conexion = new SqlConnection(_cadenaSQL))
                {
                    await conexion.OpenAsync();
                    var cmd = new SqlCommand("ListarMortalidad", conexion)
                    {
                        CommandType = CommandType.StoredProcedure
                    };

                    cmd.Parameters.AddWithValue("@UserId", userId);

                    var mortalidades = new List<Mortalidad>();
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            mortalidades.Add(new Mortalidad
                            {
                                Id = reader.GetInt32(0),
                                LoteId = reader.GetInt32(1),
                                Fecha = reader.GetDateTime(2),
                                CantidadMuertos = reader.GetInt32(3)
                            });
                        }
                    }

                    return Ok(mortalidades);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al listar mortalidad");
                return StatusCode(500, new { mensaje = "Error interno del servidor" });
            }
        }
    }
}
