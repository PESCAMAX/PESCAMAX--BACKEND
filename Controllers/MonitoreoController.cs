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
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MonitoreoController : ControllerBase
    {
        private readonly string _cadenaSQL;
        private readonly ILogger<MonitoreoController> _logger;
        private readonly IServiceProvider _serviceProvider;

        public MonitoreoController(IConfiguration configuration, ILogger<MonitoreoController> logger, IServiceProvider serviceProvider)
        {
            _cadenaSQL = configuration.GetConnectionString("DefaultConnection");
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        #region Listar
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
        #endregion

        #region Crear
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
        #endregion

        #region Actualizar
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
        #endregion

        #region Eliminar
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
        #endregion

        #region Importar Datos
        [HttpGet]
        [Route("ImportarDatos")]
        public async Task<IActionResult> ImportarDatos()
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    HttpResponseMessage response = await client.GetAsync("http://192.168.20.33/data");
                    response.EnsureSuccessStatusCode();
                    string responseBody = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation($"Datos recibidos del servidor: {responseBody}");

                    var sensorData = JsonConvert.DeserializeObject<Monitoreo>(responseBody);
                    _logger.LogInformation($"Datos deserializados: Temperatura={sensorData.Temperatura}, tds={sensorData.tds}, PH={sensorData.PH}, loteID={sensorData.LoteID}");

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

                    _logger.LogInformation($"Insertando datos en la base de datos: Temperatura={datosSensor.Temperatura}, tds={datosSensor.tds}, PH={datosSensor.PH}, loteID={datosSensor.LoteID}");

                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error al insertar datos: {ex.Message}");
                }
            }
        }
        #endregion

        #region Servicio de fondo
        public class DataFetcherService : BackgroundService
        {
            private readonly ILogger<DataFetcherService> _logger;
            private readonly IHttpClientFactory _httpClientFactory;
            private readonly IServiceProvider _serviceProvider;

            public DataFetcherService(ILogger<DataFetcherService> logger, IHttpClientFactory httpClientFactory, IServiceProvider serviceProvider)
            {
                _logger = logger;
                _httpClientFactory = httpClientFactory;
                _serviceProvider = serviceProvider;
            }

            protected override async Task ExecuteAsync(CancellationToken stoppingToken)
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        using (var scope = _serviceProvider.CreateScope())
                        {
                            var client = _httpClientFactory.CreateClient();
                            HttpResponseMessage response = await client.GetAsync("http://192.168.43.110/data");
                            response.EnsureSuccessStatusCode();
                            string responseBody = await response.Content.ReadAsStringAsync();
                            _logger.LogInformation($"Datos recibidos del servidor: {responseBody}");

                            var sensorData = JsonConvert.DeserializeObject<Monitoreo>(responseBody);
                            _logger.LogInformation($"Datos deserializados: Temperatura={sensorData.Temperatura}, tds={sensorData.tds}, PH={sensorData.PH}, loteID={sensorData.LoteID}");

                            var dataService = scope.ServiceProvider.GetRequiredService<IDataService>();
                            dataService.GuardarEnBaseDeDatos(sensorData);
                        }

                        _logger.LogInformation("Datos importados correctamente.");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Error al importar datos: {ex.Message}");
                    }

                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken); // Intervalo de 5 minutos
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

                        _logger.LogInformation($"Insertando datos en la base de datos: Temperatura={datosSensor.Temperatura}, tds={datosSensor.tds}, PH={datosSensor.PH}, loteID={datosSensor.LoteID}");

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
