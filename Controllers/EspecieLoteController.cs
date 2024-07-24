using API.Modelo;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using API.Data;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EspecieLoteController : ControllerBase
    {
        private readonly IEspecieLoteService _especieLoteService;

        public EspecieLoteController(IEspecieLoteService especieLoteService)
        {
            _especieLoteService = especieLoteService;
        }

        [HttpPost("Asignar")]
        public async Task<IActionResult> AsignarEspecieALote([FromBody] EspecieLote especieLote)
        {
            try
            {
                await _especieLoteService.AsignarEspecieALote(especieLote);
                return Ok(new { mensaje = "Especie asignada al lote correctamente" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        [HttpGet("Obtener/{userId}")]
        public async Task<IActionResult> ObtenerEspeciePorLote(string userId)
        {
            try
            {
                var especies = await _especieLoteService.ObtenerEspeciePorLote(userId);
                return Ok(especies);
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }
    }
}