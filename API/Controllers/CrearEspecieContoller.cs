using API.Modelo;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using TuProyecto.Models;

namespace TuProyecto.Controllers
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

        // POST: CrearEspecie/Crear
        [HttpPost]
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
                        cmd.Parameters.AddWithValue("@Id", crearEspecie.Id);
                        cmd.Parameters.AddWithValue("@NombreEspecie", crearEspecie.NombreEspecie);
                        cmd.Parameters.AddWithValue("@TdsSeguro", crearEspecie.TdsSeguro);
                        cmd.Parameters.AddWithValue("@TdsPeligroso", crearEspecie.TdsPeligroso);
                        cmd.Parameters.AddWithValue("@TemperaturaSeguro", crearEspecie.TemperaturaSeguro);
                        cmd.Parameters.AddWithValue("@TemperaturaPeligroso", crearEspecie.TemperaturaPeligroso);
                        cmd.Parameters.AddWithValue("@PhSeguro", crearEspecie.PhSeguro);
                        cmd.Parameters.AddWithValue("@PhPeligroso", crearEspecie.PhPeligroso);

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

        // POST: CrearEspecie/Modificar
        [HttpPost]
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
                        cmd.Parameters.AddWithValue("@TdsSeguro", modificarEspecie.TdsSeguro);
                        cmd.Parameters.AddWithValue("@TdsPeligroso", modificarEspecie.TdsPeligroso);
                        cmd.Parameters.AddWithValue("@TemperaturaSeguro", modificarEspecie.TemperaturaSeguro);
                        cmd.Parameters.AddWithValue("@TemperaturaPeligroso", modificarEspecie.TemperaturaPeligroso);
                        cmd.Parameters.AddWithValue("@PhSeguro", modificarEspecie.PhSeguro);
                        cmd.Parameters.AddWithValue("@PhPeligroso", modificarEspecie.PhPeligroso);

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

        // POST: CrearEspecie/Eliminar
        [HttpPost]
        [Route("Eliminar")]
        public async Task<IActionResult> Eliminar([FromBody] int id)
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
    }
}