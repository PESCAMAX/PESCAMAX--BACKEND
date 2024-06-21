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
using TuProyecto.Models;

namespace API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CrearEspecieController : ControllerBase
    {
        private readonly string _cadenaSQL;
        private readonly ILogger<CrearEspecieController> _logger;

        public CrearEspecieController(IConfiguration configuration, ILogger<CrearEspecieController> logger)
        {
            _cadenaSQL = configuration.GetConnectionString("DefaultConnection");
            _logger = logger;
        }

        #region Crear especie
        [HttpPost]
        [EnableCors("AllowSpecificOrigin")]
        [Route("Crear")]
        public async Task<IActionResult> Crear([FromBody] CrearEspecie crearEspecie)
        {
            try
            {
                using (var conexion = new SqlConnection(_cadenaSQL))
                {
                    conexion.Open();
                    using (var transaction = conexion.BeginTransaction())
                    {
                        var cmd = new SqlCommand("sp_crear_especie", conexion, transaction)
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

                        cmd.ExecuteNonQuery();
                        transaction.Commit();
                    }
                }

                return StatusCode(StatusCodes.Status200OK, new { mensaje = "Especie creada correctamente." });
            }
            catch (Exception error)
            {
                _logger.LogError($"Error en Crear: {error.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { mensaje = error.Message });
            }
        }
        #endregion

        #region Modificar especie
        [HttpPut]
        [EnableCors("AllowSpecificOrigin")]
        [Route("Modificar")]
        public async Task<IActionResult> Modificar([FromBody] CrearEspecie modificarEspecie)
        {
            try
            {
                using (var conexion = new SqlConnection(_cadenaSQL))
                {
                    conexion.Open();
                    using (var transaction = conexion.BeginTransaction())
                    {
                        var cmd = new SqlCommand("sp_modificar_especie", conexion, transaction)
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

                        cmd.ExecuteNonQuery();
                        transaction.Commit();
                    }
                }

                return StatusCode(StatusCodes.Status200OK, new { mensaje = "Especie modificada correctamente." });
            }
            catch (Exception error)
            {
                _logger.LogError($"Error en Modificar: {error.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { mensaje = error.Message });
            }
        }
        #endregion

        #region Eliminar especie
        [HttpDelete]
        [EnableCors("AllowSpecificOrigin")]
        [Route("Eliminar/{id}")]
        public IActionResult Eliminar([FromRoute] int id)
        {
            try
            {
                using (var conexion = new SqlConnection(_cadenaSQL))
                {
                    conexion.Open();
                    using (var transaction = conexion.BeginTransaction())
                    {
                        var cmd = new SqlCommand("sp_eliminar_especie", conexion, transaction)
                        {
                            CommandType = CommandType.StoredProcedure
                        };
                        cmd.Parameters.AddWithValue("@Id", id);

                        cmd.ExecuteNonQuery();
                        transaction.Commit();
                    }
                }

                return StatusCode(StatusCodes.Status200OK, new { mensaje = "Especie eliminada correctamente." });
            }
            catch (Exception error)
            {
                _logger.LogError($"Error en Eliminar: {error.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { mensaje = error.Message });
            }
        }
        #endregion

        #region Listar especie
        [HttpGet]
        [EnableCors("AllowSpecificOrigin")]
        [Route("Listar")]
        public async Task<IActionResult> Listar()
        {
            try
            {
                var especies = new List<CrearEspecie>();

                using (var conexion = new SqlConnection(_cadenaSQL))
                {
                    conexion.Open();
                    var cmd = new SqlCommand("sp_listar_especies", conexion)
                    {
                        CommandType = CommandType.StoredProcedure
                    };

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var especie = new CrearEspecie
                            {
                                Id = reader.GetInt32(0),
                                NombreEspecie = reader.GetString(1),
                                TdsMinimo = (float)reader.GetDouble(2),
                                TdsMaximo = (float)reader.GetDouble(3),
                                TemperaturaMinimo = (float)reader.GetDouble(4),
                                TemperaturaMaximo = (float)reader.GetDouble(5),
                                PhMinimo = (float)reader.GetDouble(6),
                                PhMaximo = (float)reader.GetDouble(7)
                            };
                            especies.Add(especie);
                        }
                    }
                }

                return Ok(especies);
            }
            catch (Exception error)
            {
                _logger.LogError($"Error en Listar: {error.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { mensaje = error.Message });
            }
        }
        #endregion
    }
}
