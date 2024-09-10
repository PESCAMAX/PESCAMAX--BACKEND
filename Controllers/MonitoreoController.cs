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
using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableCors("AllowSpecificOrigin")]
    public class MonitoreoController : ControllerBase
    {
        private readonly string _cadenaSQL;
        private readonly ILogger<MonitoreoController> _logger;

        public MonitoreoController(IConfiguration configuration, ILogger<MonitoreoController> logger)
        {
            _cadenaSQL = configuration.GetConnectionString("DefaultConnection");
            _logger = logger;
        }

        private string GetUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        [HttpGet("Leer/{userId?}")]
        [EnableCors("AllowSpecificOrigin")]
        public IActionResult Leer(string userId = null)
        {
            List<Monitoreo> lista = new List<Monitoreo>();
            userId = userId ?? GetUserId();
            _logger.LogInformation($"Iniciando Leer con UserId: {userId}");

            try
            {
                using (var conexion = new SqlConnection(_cadenaSQL))
                {
                    conexion.Open();
                    var cmd = new SqlCommand("ListarMonitoreo", conexion)
                    {
                        CommandType = CommandType.StoredProcedure
                    };

                    cmd.Parameters.AddWithValue("@UserId", userId);

                    using (var rd = cmd.ExecuteReader())
                    {
                        while (rd.Read())
                        {
                            var monitoreo = new Monitoreo
                            {
                                ID_M = Convert.ToInt32(rd["ID_M"]),
                                tds = Convert.ToSingle(rd["tds"]),
                                Temperatura = Convert.ToSingle(rd["Temperatura"]),
                                PH = Convert.ToSingle(rd["PH"]),
                                FechaHora = Convert.ToDateTime(rd["FechaHora"]),
                                LoteID = Convert.ToInt32(rd["LoteID"]),
                                userId = rd["UserId"].ToString()
                            };
                            lista.Add(monitoreo);
                        }
                    }
                }

                return Ok(new { mensaje = "ok", response = lista });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error en Leer: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { mensaje = ex.Message, response = lista });
            }
        }

        [EnableCors("AllowSpecificOrigin")]
        [HttpPost("Crear")]
        public IActionResult Crear([FromBody] Monitoreo monitoreo)
        {
            try
            {
                monitoreo.userId = GetUserId();
                using (var conexion = new SqlConnection(_cadenaSQL))
                {
                    conexion.Open();
                    using (var cmd = new SqlCommand("GuardarMonitoreo", conexion))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@tds", monitoreo.tds);
                        cmd.Parameters.AddWithValue("@Temperatura", monitoreo.Temperatura);
                        cmd.Parameters.AddWithValue("@PH", monitoreo.PH);
                        cmd.Parameters.AddWithValue("@LoteID", monitoreo.LoteID);
                        cmd.Parameters.AddWithValue("@UserId", monitoreo.userId);

                        cmd.ExecuteNonQuery();
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

        [HttpPut("Actualizar")]
        public IActionResult Actualizar([FromBody] Monitoreo monitoreo)
        {
            try
            {
                var userId = GetUserId();
                if (monitoreo.userId != userId)
                {
                    return StatusCode(StatusCodes.Status403Forbidden, new { mensaje = "No tienes permiso para actualizar este monitoreo." });
                }

                using (var conexion = new SqlConnection(_cadenaSQL))
                {
                    conexion.Open();
                    using (var cmd = new SqlCommand("ActualizarMonitoreo", conexion))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@ID_M", monitoreo.ID_M);
                        cmd.Parameters.AddWithValue("@tds", monitoreo.tds);
                        cmd.Parameters.AddWithValue("@Temperatura", monitoreo.Temperatura);
                        cmd.Parameters.AddWithValue("@PH", monitoreo.PH);
                        cmd.Parameters.AddWithValue("@LoteID", monitoreo.LoteID);
                        cmd.Parameters.AddWithValue("@UserId", userId);

                        cmd.ExecuteNonQuery();
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

        [HttpDelete("Eliminar/{id_m}")]
        [EnableCors("AllowSpecificOrigin")]
        public IActionResult Eliminar(int id_m)
        {
            try
            {
                var userId = GetUserId();
                using (var conexion = new SqlConnection(_cadenaSQL))
                {
                    conexion.Open();
                    using (var cmd = new SqlCommand("EliminarMonitoreo", conexion))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@ID_M", id_m);
                        cmd.Parameters.AddWithValue("@UserId", userId);

                        cmd.ExecuteNonQuery();
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

        #region Importar Datos
        [Authorize]
        [HttpGet]
        [Route("ImportarDatos")]
        public async Task<IActionResult> ImportarDatos()
        {
            try
            {
                var userId = GetUserId();
                using (HttpClient client = new HttpClient())
                {
                    HttpResponseMessage response = await client.GetAsync("http://192.168.43.110/data");
                    response.EnsureSuccessStatusCode();
                    string responseBody = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation($"Datos recibidos del servidor: {responseBody}");

                    var sensorData = JsonConvert.DeserializeObject<Monitoreo>(responseBody);
                    _logger.LogInformation($"Datos deserializados: Temperatura={sensorData.Temperatura}, tds={sensorData.tds}, PH={sensorData.PH}, loteID={sensorData.LoteID}");

                    sensorData.userId = userId; // Assign the current user's ID

                    GuardarEnBaseDeDatos(sensorData);
                }

                _logger.LogInformation("Datos importados correctamente.");
                return StatusCode(StatusCodes.Status200OK, new { mensaje = "Datos importados correctamente." });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al importar datos: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { mensaje = ex.Message });
            }
        }
        #endregion

        private void GuardarEnBaseDeDatos(Monitoreo datosSensor)
        {
            using (var conexion = new SqlConnection(_cadenaSQL))
            {
                try
                {
                    conexion.Open();
                    var cmd = new SqlCommand("GuardarMonitoreo", conexion)
                    {
                        CommandType = CommandType.StoredProcedure
                    };
                    cmd.Parameters.AddWithValue("@Temperatura", datosSensor.Temperatura);
                    cmd.Parameters.AddWithValue("@tds", datosSensor.tds);
                    cmd.Parameters.AddWithValue("@PH", datosSensor.PH);
                    cmd.Parameters.AddWithValue("@loteID", datosSensor.LoteID);
                    cmd.Parameters.AddWithValue("@UserId", datosSensor.userId); // Añadir el UserId al procedimiento almacenado

                    _logger.LogInformation($"Insertando datos en la base de datos: Temperatura={datosSensor.Temperatura}, tds={datosSensor.tds}, PH={datosSensor.PH}, loteID={datosSensor.LoteID}, UserId={datosSensor.userId}");

                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error al insertar datos: {ex.Message}");
                }
            }
        }

        #region Servicio de fondo
        public class MonitoreoBackgroundService : BackgroundService
        {
            private readonly ILogger<MonitoreoBackgroundService> _logger;
            private readonly IServiceProvider _serviceProvider;
            private readonly HttpClient _httpClient;

            public MonitoreoBackgroundService(ILogger<MonitoreoBackgroundService> logger, IServiceProvider serviceProvider)
            {
                _logger = logger;
                _serviceProvider = serviceProvider;
                _httpClient = new HttpClient();
            }

            protected override async Task ExecuteAsync(CancellationToken stoppingToken)
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        await ObtenerYGuardarDatos();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Error en el servicio de fondo: {ex.Message}");
                    }

                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                }
            }

            private async Task ObtenerYGuardarDatos()
            {
                try
                {
                    var response = await _httpClient.GetAsync("http://192.168.43.110/data");
                    response.EnsureSuccessStatusCode();
                    var content = await response.Content.ReadAsStringAsync();

                    _logger.LogInformation($"Datos recibidos: {content}");

                    var monitoreo = JsonConvert.DeserializeObject<Monitoreo>(content);
                    monitoreo.FechaHora = DateTime.Now;

                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                        dbContext.Monitoreo.Add(monitoreo);
                        await dbContext.SaveChangesAsync();
                    }

                    _logger.LogInformation($"Datos guardados: Temperatura={monitoreo.Temperatura}, TDS={monitoreo.tds}, PH={monitoreo.PH}, LoteID={monitoreo.LoteID}");
                }
                catch (HttpRequestException ex)
                {
                    _logger.LogError($"Error al obtener datos del endpoint: {ex.Message}");
                }
                catch (JsonException ex)
                {
                    _logger.LogError($"Error al deserializar los datos: {ex.Message}");
                }
                catch (DbUpdateException ex)
                {
                    _logger.LogError($"Error al guardar en la base de datos: {ex.Message}");
                    if (ex.InnerException != null)
                    {
                        _logger.LogError($"Inner exception: {ex.InnerException.Message}");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error inesperado: {ex.Message}");
                }
            }
        }
        #endregion

        #region Servicio de datos
        public interface IDataService
        {
            void GuardarEnBaseDeDatos(Monitoreo datosSensor);
        }

        public class DataService : IDataService
        {
            private readonly ILogger<DataService> _logger;
            private readonly string _cadenaSQL;

            public DataService(ILogger<DataService> logger, IConfiguration configuration)
            {
                _logger = logger;
                _cadenaSQL = configuration.GetConnectionString("DefaultConnection");
            }

            public void GuardarEnBaseDeDatos(Monitoreo datosSensor)
            {
                using (var conexion = new SqlConnection(_cadenaSQL))
                {
                    try
                    {
                        conexion.Open();
                        var cmd = new SqlCommand("GuardarMonitoreo", conexion)
                        {
                            CommandType = CommandType.StoredProcedure
                        };
                        cmd.Parameters.AddWithValue("@Temperatura", datosSensor.Temperatura);
                        cmd.Parameters.AddWithValue("@tds", datosSensor.tds);
                        cmd.Parameters.AddWithValue("@PH", datosSensor.PH);
                        cmd.Parameters.AddWithValue("@loteID", datosSensor.LoteID);
                        cmd.Parameters.AddWithValue("@UserId", datosSensor.userId); // Añadir el UserId al procedimiento almacenado

                        _logger.LogInformation($"Insertando datos en la base de datos: Temperatura={datosSensor.Temperatura}, tds={datosSensor.tds}, PH={datosSensor.PH}, loteID={datosSensor.LoteID}, UserId={datosSensor.userId}");

                        cmd.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Error al insertar datos: {ex.Message}");
                    }
                }
            }
        }
        #endregion
    }
}