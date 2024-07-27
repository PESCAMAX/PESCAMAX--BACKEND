using API.Modelo;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Data.SqlClient;
using System.Security.Claims;

namespace API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class AsignarController : ControllerBase
    {
        private readonly string _cadenaSQL;
        private readonly ILogger<AsignarController> _logger;

        public AsignarController(IConfiguration configuration, ILogger<AsignarController> logger)
        {
            _cadenaSQL = configuration.GetConnectionString("DefaultConnection");
            _logger = logger;
        }

        private string GetUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }

        [HttpPost("Crear")]
        [EnableCors("AllowSpecificOrigin")]
        public async Task<IActionResult> Crear([FromBody] Asignar asignar)
        {
            try
            {
                var authenticatedUserId = GetUserId();
                if (string.IsNullOrEmpty(authenticatedUserId))
                {
                    return Unauthorized();
                }

                // Verificar que el UserId en el cuerpo de la solicitud coincida con el usuario autenticado
                if (asignar.UserId != authenticatedUserId)
                {
                    return Forbid();
                }

                using (var conexion = new SqlConnection(_cadenaSQL))
                {
                    await conexion.OpenAsync();
                    using (var transaction = conexion.BeginTransaction())
                    {
                        var cmd = new SqlCommand("spGuardarAsignar", conexion, transaction)
                        {
                            CommandType = CommandType.StoredProcedure
                        };

                        cmd.Parameters.AddWithValue("@EspecieNomb", asignar.EspecieNomb);
                        cmd.Parameters.AddWithValue("@LoteID", asignar.LoteID);
                        cmd.Parameters.AddWithValue("@UserId", authenticatedUserId);

                        await cmd.ExecuteNonQueryAsync();
                        transaction.Commit();
                    }
                }
                return Ok(new { mensaje = "Asignación creada correctamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear asignación");
                return StatusCode(500, new { mensaje = "Error interno del servidor" });
            }
        }

        [HttpPut("Modificar")]
        [EnableCors("AllowSpecificOrigin")]
        public async Task<IActionResult> Modificar([FromBody] Asignar asignar)
        {
            try
            {
                var authenticatedUserId = GetUserId();
                if (string.IsNullOrEmpty(authenticatedUserId))
                {
                    return Unauthorized();
                }

                if (asignar.UserId != authenticatedUserId)
                {
                    return Forbid();
                }

                using (var conexion = new SqlConnection(_cadenaSQL))
                {
                    await conexion.OpenAsync();
                    using (var transaction = conexion.BeginTransaction())
                    {
                        var cmd = new SqlCommand("spModificarAsignar", conexion, transaction)
                        {
                            CommandType = CommandType.StoredProcedure
                        };

                        cmd.Parameters.AddWithValue("@ID_A", asignar.Id);
                        cmd.Parameters.AddWithValue("@EspecieNomb", asignar.EspecieNomb);
                        cmd.Parameters.AddWithValue("@LoteID", asignar.LoteID);
                        cmd.Parameters.AddWithValue("@UserId", authenticatedUserId);

                        await cmd.ExecuteNonQueryAsync();
                        transaction.Commit();
                    }
                }
                return Ok(new { mensaje = "Asignación modificada correctamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al modificar asignación");
                return StatusCode(500, new { mensaje = "Error interno del servidor" });
            }
        }

        [HttpDelete("Eliminar/{id}")]
        [EnableCors("AllowSpecificOrigin")]
        public async Task<IActionResult> Eliminar(int id)
        {
            try
            {
                var userId = GetUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }

                using (var conexion = new SqlConnection(_cadenaSQL))
                {
                    await conexion.OpenAsync();
                    using (var transaction = conexion.BeginTransaction())
                    {
                        var cmd = new SqlCommand("[spEliminarAsignar]", conexion, transaction)
                        {
                            CommandType = CommandType.StoredProcedure
                        };

                        cmd.Parameters.AddWithValue("@ID_A", id);

                        await cmd.ExecuteNonQueryAsync();
                        transaction.Commit();
                    }
                }

                return Ok(new { mensaje = "Asignación eliminada correctamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar asignación");
                return StatusCode(500, new { mensaje = "Error interno del servidor" });
            }
        }

        [HttpGet("ListarAsignar")]
        [EnableCors("AllowSpecificOrigin")]
        public async Task<IActionResult> ListarAsignar()
        {
            var authenticatedUserId = GetUserId();
            if (string.IsNullOrEmpty(authenticatedUserId))
            {
                return Unauthorized();
            }

            try
            {
                using (var conexion = new SqlConnection(_cadenaSQL))
                {
                    await conexion.OpenAsync();
                    var cmd = new SqlCommand("spListarAsignar", conexion)
                    {
                        CommandType = CommandType.StoredProcedure
                    };

                    cmd.Parameters.AddWithValue("@UserId", authenticatedUserId);

                    var asignaciones = new List<Asignar>();
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            asignaciones.Add(new Asignar
                            {
                                Id = reader.GetInt32(0),
                                EspecieNomb = reader.GetString(1),
                                LoteID = reader.GetInt32(2),
                                UserId = reader.GetString(3)
                            });
                        }
                    }

                    return Ok(asignaciones);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al listar asignaciones");
                return StatusCode(500, new { mensaje = "Error interno del servidor" });
            }
        }
    }
}
