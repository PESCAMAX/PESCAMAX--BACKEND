using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Cors;
using API.Modelo;
using System.Threading;


namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MonitoreoController : ControllerBase
    {
        private readonly string _cadenaSQL;
        private readonly ILogger<MonitoreoController> _logger;

        public MonitoreoController(IConfiguration configuration, ILogger<MonitoreoController> logger)
        {
            _cadenaSQL = configuration.GetConnectionString("DefaultConnection");
            _logger = logger;
        }

        [HttpGet]
        [EnableCors("AllowSpecificOrigin")]
        [Route("Leer")]
        public IActionResult Leer(int? id_m = null)  // Parámetro opcional para filtrar por ID_M
        {
            List<Monitoreo> lista = new List<Monitoreo>();

            try
            {
                using (var conexion = new SqlConnection(_cadenaSQL))
                {
                    conexion.Open();
                    var cmd = new SqlCommand("ListarMonitoreos", conexion)
                    {
                        CommandType = CommandType.StoredProcedure
                    };

                    // Agregar el parámetro @ID_M si se especifica
                    if (id_m.HasValue)
                    {
                        cmd.Parameters.AddWithValue("@ID_M", id_m.Value);
                    }

                    using (var rd = cmd.ExecuteReader())
                    {
                        while (rd.Read())
                        {
                            lista.Add(new Monitoreo
                            {
                                ID_M = Convert.ToInt32(rd["ID_M"]),
                                tds = Convert.ToSingle(rd["tds"]),
                                Temperatura = Convert.ToSingle(rd["Temperatura"]),
                                PH = Convert.ToSingle(rd["PH"]),
                                FechaHora = Convert.ToDateTime(rd["FechaHora"]),
                                LoteID = Convert.ToInt32(rd["LoteID"])
                            });
                        }
                    }
                }

                return StatusCode(StatusCodes.Status200OK, new { mensaje = "ok", response = lista });
            }
            catch (Exception error)
            {
                _logger.LogError($"Error en Leer: {error.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { mensaje = error.Message, response = lista });
            }
        }

        [HttpPost]
        [EnableCors("AllowSpecificOrigin")]
        [Route("Crear")]
        public IActionResult Crear([FromBody] Monitoreo monitoreo)
        {
            try
            {
                using (var conexion = new SqlConnection(_cadenaSQL))
                {
                    conexion.Open();
                    using (var transaction = conexion.BeginTransaction())
                    {
                        var cmd = new SqlCommand("GuardarMonitoreo", conexion, transaction)
                        {
                            CommandType = CommandType.StoredProcedure
                        };
                        cmd.Parameters.AddWithValue("@tds", monitoreo.tds);
                        cmd.Parameters.AddWithValue("@Temperatura", monitoreo.Temperatura);
                        cmd.Parameters.AddWithValue("@PH", monitoreo.PH);
                        cmd.Parameters.AddWithValue("@LoteID", monitoreo.LoteID);

                        cmd.ExecuteNonQuery();
                        transaction.Commit();
                    }
                }

                return StatusCode(StatusCodes.Status200OK, new { mensaje = "Monitoreo creado correctamente." });
            }
            catch (Exception error)
            {
                _logger.LogError($"Error en Crear: {error.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { mensaje = error.Message });
            }
        }

        [HttpPut]
        [EnableCors("AllowSpecificOrigin")]
        [Route("Actualizar")]
        public IActionResult Actualizar([FromBody] Monitoreo monitoreo)
        {
            try
            {
                using (var conexion = new SqlConnection(_cadenaSQL))
                {
                    conexion.Open();
                    using (var transaction = conexion.BeginTransaction())
                    {
                        var cmd = new SqlCommand("ActualizarMonitoreo", conexion, transaction)
                        {
                            CommandType = CommandType.StoredProcedure
                        };
                        cmd.Parameters.AddWithValue("@ID_M", monitoreo.ID_M);
                        cmd.Parameters.AddWithValue("@tds", monitoreo.tds);
                        cmd.Parameters.AddWithValue("@Temperatura", monitoreo.Temperatura);
                        cmd.Parameters.AddWithValue("@PH", monitoreo.PH);
                        cmd.Parameters.AddWithValue("@LoteID", monitoreo.LoteID);

                        cmd.ExecuteNonQuery();
                        transaction.Commit();
                    }
                }

                return StatusCode(StatusCodes.Status200OK, new { mensaje = "Monitoreo actualizado correctamente." });
            }
            catch (Exception error)
            {
                _logger.LogError($"Error en Actualizar: {error.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { mensaje = error.Message });
            }
        }

        [HttpDelete]
        [EnableCors("AllowSpecificOrigin")]
        [Route("Eliminar/{id_m}")]
        public IActionResult Eliminar(int id_m)
        {
            try
            {
                using (var conexion = new SqlConnection(_cadenaSQL))
                {
                    conexion.Open();
                    using (var transaction = conexion.BeginTransaction())
                    {
                        var cmd = new SqlCommand("EliminarMonitoreo", conexion, transaction)
                        {
                            CommandType = CommandType.StoredProcedure
                        };
                        cmd.Parameters.AddWithValue("@ID_M", id_m);

                        cmd.ExecuteNonQuery();
                        transaction.Commit();
                    }
                }

                return StatusCode(StatusCodes.Status200OK, new { mensaje = "Monitoreo eliminado correctamente." });
            }
            catch (Exception error)
            {
                _logger.LogError($"Error en Eliminar: {error.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { mensaje = error.Message });
            }
        }
    }
}
