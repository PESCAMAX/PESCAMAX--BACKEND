using API.Modelo;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HorasController : ControllerBase
    {
        private readonly ILogger<HorasController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly IHttpClientFactory _clientFactory;
        private readonly IConfiguration _configuration;

        public HorasController(ILogger<HorasController> logger, ApplicationDbContext context, IHttpClientFactory clientFactory, IConfiguration configuration)
        {
            _logger = logger;
            _context = context;
            _clientFactory = clientFactory;
            _configuration = configuration;
        }

        [HttpPost("guardarHoras")]
        public async Task<IActionResult> GuardarHoras([FromBody] List<HourSelection> hours)
        {
            if (hours == null || !hours.Any())
            {
                return BadRequest("No se proporcionaron horas.");
            }

            try
            {
                foreach (var hour in hours)
                {
                    _context.HorasSeleccionadas.Add(new HorasSeleccionadas
                    {
                        Hour = hour.hour,
                        Am = hour.am,
                        Pm = hour.pm,
                        UserId = hour.UserId
                    });
                }
                await _context.SaveChangesAsync();

                // Enviar horas al ESP8266
                var json = JsonConvert.SerializeObject(new { hours });
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var client = _clientFactory.CreateClient();
                var esp8266Url = _configuration["ESP8266:Url"];
                var response = await client.PostAsync($"{esp8266Url}/setHours", content);

                if (response.IsSuccessStatusCode)
                {
                    return Ok("Horas guardadas y enviadas al ESP8266 exitosamente.");
                }
                else
                {
                    _logger.LogWarning($"Error al enviar horas al ESP8266. Status code: {response.StatusCode}");
                    return StatusCode((int)response.StatusCode, "Error al enviar horas al ESP8266.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al guardar o enviar horas: {ex.Message}");
                return StatusCode(500, "Error interno al procesar la solicitud.");
            }
        }

        [HttpPost("enviarHorasAESP")]
        public async Task<IActionResult> EnviarHorasAESP()
        {
            var horasSeleccionadas = _context.HorasSeleccionadas.ToList();

            var hoursToSend = horasSeleccionadas.Select(h => new HourSelection
            {
                hour = h.Hour,  // Cambiado de 'Hora' a 'Hour'
                am = h.Am,
                pm = h.Pm,
                UserId = h.UserId  // Incluir UserId
            }).ToList();

            var json = JsonConvert.SerializeObject(new { hours = hoursToSend });
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var client = _clientFactory.CreateClient();
            var response = await client.PostAsync("http://192.168.130.155/setHours", content);

            if (response.IsSuccessStatusCode)
            {
                return Ok("Horas enviadas al ESP8266 exitosamente.");
            }
            else
            {
                return StatusCode((int)response.StatusCode, "Error al enviar horas al ESP8266.");
            }
        }

        [HttpPost("recibirDatos")]
        public async Task<IActionResult> RecibirDatos([FromBody] SensorData data)
        {
            try
            {
                _context.SensorData.Add(data);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Datos recibidos y guardados para LoteID: {data.LoteID}");
                return Ok("Datos recibidos y guardados.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al recibir datos del sensor: {ex.Message}");
                return StatusCode(500, "Error al procesar los datos del sensor.");
            }
        }

        public class HourSelection
        {
            public int hour { get; set; }
            public bool am { get; set; }
            public bool pm { get; set; }
            public string UserId { get; set; }  // Agregar UserId
        }
    }
}
