using API.Modelo;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HorasController : ControllerBase
    {
        private readonly ILogger<HorasController> _logger;
        private readonly ApplicationDbContext _context;

        public HorasController(ILogger<HorasController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        [HttpPost("guardarHoras")]
        public IActionResult GuardarHoras([FromBody] List<HourSelection> hours)
        {
            if (hours == null || !hours.Any())
            {
                return BadRequest("No se proporcionaron horas.");
            }

            foreach (var hour in hours)
            {
                _context.HorasSeleccionadas.Add(new HorasSeleccionadas
                {
                    Hora = hour.hour,
                    Am = hour.am,
                    Pm = hour.pm
                });
            }

            _context.SaveChanges();

            return Ok("Horas guardadas exitosamente.");
        }
    }

    public class HourSelection
    {
        public int hour { get; set; }
        public bool am { get; set; }
        public bool pm { get; set; }
    }
}
