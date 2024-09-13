using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using API.Modelo;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClimaController : ControllerBase
    {
        private readonly string _cadenaSQL;
        private readonly ILogger<ClimaController> _logger;

        public ClimaController(IConfiguration configuration, ILogger<ClimaController> logger)
        {
            _cadenaSQL = configuration.GetConnectionString("DefaultConnection");
            _logger = logger;
        }
        [HttpPost]
        public IActionResult GuardarClima([FromBody] Clima clima)
        {
            using (SqlConnection connection = new SqlConnection(_cadenaSQL))
            {
                SqlCommand command = new SqlCommand("InsertarClima", connection);
                command.CommandType = System.Data.CommandType.StoredProcedure;

                command.Parameters.AddWithValue("@NombreCiudad", clima.NombreCiudad);
                command.Parameters.AddWithValue("@TemperaturaActual", clima.TemperaturaActual);
                command.Parameters.AddWithValue("@EstadoMeteoro", clima.EstadoMeteoro);
                command.Parameters.AddWithValue("@Humedad", clima.Humedad);
                command.Parameters.AddWithValue("@Nubes", clima.Nubes);
                command.Parameters.AddWithValue("@FechaHora", DateTime.Now);
                command.Parameters.AddWithValue("@UserId", clima.UserId);

                connection.Open();
                command.ExecuteNonQuery();
                connection.Close();
            }

            return Ok();
        }
        [HttpGet("{userId}")]
        public IActionResult ObtenerClima(string userId)
        {
            List<Clima> climas = new List<Clima>();

            using (SqlConnection connection = new SqlConnection(_cadenaSQL))
            {
                SqlCommand command = new SqlCommand("SELECT * FROM Clima WHERE UserId = @UserId", connection);
                command.Parameters.AddWithValue("@UserId", userId);

                connection.Open();
                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    Clima clima = new Clima
                    {
                        NombreCiudad = reader["NombreCiudad"].ToString(),
                        TemperaturaActual = Convert.ToDouble(reader["TemperaturaActual"]),
                        EstadoMeteoro = reader["EstadoMeteoro"].ToString(),
                        Humedad = Convert.ToInt32(reader["Humedad"]),
                        Nubes = Convert.ToInt32(reader["Nubes"]),
                        FechaHora = Convert.ToDateTime(reader["FechaHora"]),
                        UserId = reader["UserId"].ToString()
                    };
                    climas.Add(clima);
                }
                connection.Close();
            }

            return Ok(climas);
        }



        [HttpGet("ultimo/{userId}")]
        public IActionResult ObtenerUltimoClima(string userId)
        {
            Clima ultimoClima = null;

            using (SqlConnection connection = new SqlConnection(_cadenaSQL))
            {
                SqlCommand command = new SqlCommand("SELECT TOP 1 * FROM Clima WHERE UserId = @UserId ORDER BY FechaHora DESC", connection);
                command.Parameters.AddWithValue("@UserId", userId);

                connection.Open();
                SqlDataReader reader = command.ExecuteReader();
                if (reader.Read())
                {
                    ultimoClima = new Clima
                    {
                        NombreCiudad = reader["NombreCiudad"].ToString(),
                        TemperaturaActual = Convert.ToDouble(reader["TemperaturaActual"]),
                        EstadoMeteoro = reader["EstadoMeteoro"].ToString(),
                        Humedad = Convert.ToInt32(reader["Humedad"]),
                        Nubes = Convert.ToInt32(reader["Nubes"]),
                        FechaHora = Convert.ToDateTime(reader["FechaHora"]),
                        UserId = reader["UserId"].ToString()
                    };
                }
                connection.Close();
            }

            if (ultimoClima == null)
            {
                return NotFound("No climate data available.");
            }

            return Ok(ultimoClima);
        }
    }
}