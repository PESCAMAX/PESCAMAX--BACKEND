using API.Data;
using API.Modelo;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [EnableCors("AllowSpecificOrigin")]
    [ApiController]
    public class AlertaController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AlertaController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        [EnableCors("AllowSpecificOrigin")]
        public async Task<ActionResult<Alerta>> CrearAlerta(Alerta alerta)
        {
            try
            {
                var commandText = "EXEC spCrearAlerta @especieID, @Nombre, @loteID, @descripcion";
                var parameters = new[]
                {
                    new SqlParameter("@especieID", alerta.EspecieID),
                    new SqlParameter("@Nombre", alerta.Nombre),
                    new SqlParameter("@loteID", alerta.LoteID),
                    new SqlParameter("@descripcion", alerta.Descripcion)
                };

                await _context.Database.ExecuteSqlRawAsync(commandText, parameters);
                return Ok(alerta);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        [EnableCors("AllowSpecificOrigin")]
        public async Task<ActionResult<IEnumerable<Alerta>>> ObtenerAlertas()
        {
            try
            {
                var alertas = await _context.Alertas.FromSqlRaw("EXEC spObtenerAlertas").ToListAsync();
                return Ok(alertas);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
