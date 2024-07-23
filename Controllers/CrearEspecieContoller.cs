using API.Modelo;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Data;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class CrearEspecieController : ControllerBase
    {
        private readonly string _cadenaSQL;
        private readonly ILogger<CrearEspecieController> _logger;

        public CrearEspecieController(IConfiguration configuration, ILogger<CrearEspecieController> logger)
        {
            _cadenaSQL = configuration.GetConnectionString("DefaultConnection");
            _logger = logger;
        }

        private string GetUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        [HttpPost("Crear/{userId}")]
        [EnableCors("AllowSpecificOrigin")]
        public async Task<IActionResult> Crear(string userId, [FromBody] CrearEspecie crearEspecie)
        {
            try
            {
                var authenticatedUserId = GetUserId();
                if (string.IsNullOrEmpty(authenticatedUserId))
                {
                    return Unauthorized();
                }

                // Verificar que el userId en la URL coincida con el usuario autenticado
                if (userId != authenticatedUserId)
                {
                    return Forbid();
                }

                // Resto de la lógica para crear la especie
                using (var conexion = new SqlConnection(_cadenaSQL))
                {
                    await conexion.OpenAsync();
                    using (var transaction = conexion.BeginTransaction())
                    {
                        var cmd = new SqlCommand("crear_especie", conexion, transaction)
                        {
                            CommandType = CommandType.StoredProcedure
                        };

                        cmd.Parameters.AddWithValue("@NombreEspecie", crearEspecie.NombreEspecie);
                        cmd.Parameters.AddWithValue("@TdsMinimo", crearEspecie.TdsMinimo);
                        cmd.Parameters.AddWithValue("@TdsMaximo", crearEspecie.TdsMaximo);
                        cmd.Parameters.AddWithValue("@TemperaturaMinimo", crearEspecie.TemperaturaMinimo);
                        cmd.Parameters.AddWithValue("@TemperaturaMaximo", crearEspecie.TemperaturaMaximo);
                        cmd.Parameters.AddWithValue("@PhMinimo", crearEspecie.PhMinimo);
                        cmd.Parameters.AddWithValue("@PhMaximo", crearEspecie.PhMaximo);
                        cmd.Parameters.AddWithValue("@UserId", authenticatedUserId);

                        await cmd.ExecuteNonQueryAsync();
                        transaction.Commit();
                    }
                }
                return Ok(new { mensaje = "Especie creada correctamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear especie");
                return StatusCode(500, new { mensaje = "Error interno del servidor" });
            }
        }

        [HttpPut("Modificar/{userId}")]
        [EnableCors("AllowSpecificOrigin")]
        public async Task<IActionResult> Modificar(string userId, [FromBody] CrearEspecie modificarEspecie)
        {
            try
            {
                var authenticatedUserId = GetUserId();
                if (string.IsNullOrEmpty(authenticatedUserId))
                {
                    return Unauthorized();
                }
                if (userId != authenticatedUserId)
                {
                    return Forbid();
                }

                using (var conexion = new SqlConnection(_cadenaSQL))
                {
                    await conexion.OpenAsync();
                    using (var transaction = conexion.BeginTransaction())
                    {
                        var cmd = new SqlCommand("modificar_especie", conexion, transaction)
                        {
                            CommandType = CommandType.StoredProcedure
                        };
                        cmd.Parameters.AddWithValue("@Id", modificarEspecie.Id);
                        cmd.Parameters.AddWithValue("@NombreEspecie", modificarEspecie.NombreEspecie);
                        cmd.Parameters.AddWithValue("@TdsMinimo", modificarEspecie.TdsMinimo);
                        cmd.Parameters.AddWithValue("@TdsMaximo", modificarEspecie.TdsMaximo);
                        cmd.Parameters.AddWithValue("@TemperaturaMinimo", modificarEspecie.TemperaturaMinimo);
                        cmd.Parameters.AddWithValue("@TemperaturaMaximo", modificarEspecie.TemperaturaMaximo);
                        cmd.Parameters.AddWithValue("@PhMinimo", modificarEspecie.PhMinimo);
                        cmd.Parameters.AddWithValue("@PhMaximo", modificarEspecie.PhMaximo);
                        cmd.Parameters.AddWithValue("@UserId", userId);

                        await cmd.ExecuteNonQueryAsync();
                        transaction.Commit();
                    }
                }

                return Ok(new { mensaje = "Especie modificada correctamente." });
            }
            catch (Exception error)
            {
                _logger.LogError($"Error en Modificar: {error.Message}");
                return StatusCode(500, new { mensaje = error.Message });
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
                        var cmd = new SqlCommand("eliminar_especie", conexion, transaction)
                        {
                            CommandType = CommandType.StoredProcedure
                        };
                        cmd.Parameters.AddWithValue("@Id", id);
                        cmd.Parameters.AddWithValue("@UserId", userId);

                        await cmd.ExecuteNonQueryAsync();
                        transaction.Commit();
                    }
                }

                return Ok(new { mensaje = "Especie eliminada correctamente." });
            }
            catch (Exception error)
            {
                _logger.LogError($"Error en Eliminar: {error.Message}");
                return StatusCode(500, new { mensaje = error.Message });
            }
        }

        [HttpGet("Listar/{userId?}")]
        [EnableCors("AllowSpecificOrigin")]
        public async Task<IActionResult> Listar(string userId = null)
        {
            var authenticatedUserId = GetUserId();
            if (string.IsNullOrEmpty(authenticatedUserId))
            {
                return Unauthorized();
            }
            userId = userId ?? authenticatedUserId;

            try
            {
                using (var conexion = new SqlConnection(_cadenaSQL))
                {
                    await conexion.OpenAsync();
                    var cmd = new SqlCommand("listar_especies", conexion)
                    {
                        CommandType = CommandType.StoredProcedure
                    };
                    cmd.Parameters.AddWithValue("@UserId", userId);

                    var especies = new List<CrearEspecie>();
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            especies.Add(new CrearEspecie
                            {
                                Id = reader.GetInt32(0),
                                NombreEspecie = reader.GetString(1),
                                TdsMinimo = (float)reader.GetDouble(2),
                                TdsMaximo = (float)reader.GetDouble(3),
                                TemperaturaMinimo = (float)reader.GetDouble(4),
                                TemperaturaMaximo = (float)reader.GetDouble(5),
                                PhMinimo = (float)reader.GetDouble(6),
                                PhMaximo = (float)reader.GetDouble(7)
                            });
                        }
                    }

                    return Ok(especies);
                }
            }
            catch (Exception error)
            {
                _logger.LogError($"Error en Listar: {error.Message}");
                return StatusCode(500, new { mensaje = error.Message });
            }
        }
    }
}
