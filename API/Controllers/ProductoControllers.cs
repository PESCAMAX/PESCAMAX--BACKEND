using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection; // Necesario para IConfiguration
using Microsoft.Extensions.Hosting; // Necesario para IWebHostEnvironment
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using API.Modelo;

namespace API.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class ProductoControllers : ControllerBase
    {
        private readonly string cadenaSQL;
        public ProductoControllers(IConfiguration config)
        {
            cadenaSQL = config.GetConnectionString("CadenaSQL");
        }
        [HttpGet]
        [Route("Lista")]

        public IActionResult Lista()
        {

            List<Producto> lista = new List<Producto>(); try

            {

                using (var conexion = new SqlConnection(cadenaSQL))

                {

                    conexion.Open();

                    var cmd = new SqlCommand("sp_listar_product", conexion);
                    cmd.CommandType = CommandType.StoredProcedure;

                    using (var reader = cmd.ExecuteReader())

                    {

                        while (reader.Read())

                        {

                            lista.Add(new Producto

                            {

                                id = Convert.ToInt32(reader["id"]),
                                Fecha = reader["Fecha"].ToString(),
                                NumerodeLote = reader["NumerodeLote"].ToString(),
                                CantidadPeces = Convert.ToInt32(reader["CantidadPeces"]),
                                PesoTotal = Convert.ToInt32(reader["PesoTotal"]),
                                alimentoConsumido = Convert.ToInt32(reader["alimentoConsumido"]),
                                costoAlimento = Convert.ToInt32(reader["costoAlimento"]),
                                tasaMortalidad = Convert.ToDecimal(reader["tasaMortalidad"])

                            });

                        }

                    }

                }

                return StatusCode(StatusCodes.Status200OK, new { mensaje = "ok", response = lista });

            }

            catch (Exception error)
            {

                return StatusCode(StatusCodes.Status500InternalServerError, new { mensaje = error.Message, response = lista });
            }
        }
        [HttpDelete]
        [Route("Borrar/{id}")]
        public IActionResult Borrar(int id)
        {
            try
            {
                using (var conexion = new SqlConnection(cadenaSQL))
                {
                    conexion.Open();
                    var cmd = new SqlCommand("sp_borrar_productiv", conexion);
                    cmd.CommandType = CommandType.StoredProcedure;

                    // Agregar el parámetro de ID para el borrado
                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    int filasAfectadas = cmd.ExecuteNonQuery();

                    if (filasAfectadas > 0)
                    {
                        return StatusCode(StatusCodes.Status200OK, new { mensaje = " borrado." });
                    }
                    else
                    {
                        return StatusCode(StatusCodes.Status404NotFound, new { mensaje = "no encontrado." });
                    }
                }
            }
            catch (Exception error)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { mensaje = error.Message });
            }
        }
        [HttpPut]
        [Route("Actualizar/{id}")]
        public IActionResult Actualizar(int id, [FromBody] Producto productoActualizado)
        {
            try
            {
                using (var conexion = new SqlConnection(cadenaSQL))
                {
                    conexion.Open();
                    var cmd = new SqlCommand("sp_actualizar_productivi", conexion);
                    cmd.CommandType = CommandType.StoredProcedure;

                    // Agregar los parámetros necesarios para la actualización
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    cmd.Parameters.Add(new SqlParameter("@Fecha", productoActualizado.Fecha));
                    cmd.Parameters.Add(new SqlParameter("@NumerodeLote", productoActualizado.NumerodeLote));
                    cmd.Parameters.Add(new SqlParameter("@CantidadPeces", productoActualizado.CantidadPeces));
                    cmd.Parameters.Add(new SqlParameter("@PesoTotal", productoActualizado.PesoTotal));
                    cmd.Parameters.Add(new SqlParameter("@alimentoConsumido", productoActualizado.alimentoConsumido));
                    cmd.Parameters.Add(new SqlParameter("@costoAlimento", productoActualizado.costoAlimento));
                    cmd.Parameters.Add(new SqlParameter("@tasaMortalidad", productoActualizado.tasaMortalidad)); 

                    int filasAfectadas = cmd.ExecuteNonQuery();

                    if (filasAfectadas > 0)
                    {
                        return StatusCode(StatusCodes.Status200OK, new { mensaje = "se ha sido actualizado." });
                    }
                    else
                    {
                        return StatusCode(StatusCodes.Status404NotFound, new { mensaje = "no encontrado." });
                    }
                }
            }
            catch (Exception error)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { mensaje = error.Message });
            }
        }
        [HttpPost]
        [Route("Ingresar")]
        public IActionResult Ingresar([FromBody] Producto nuevoProducto)
        {
            try
            {
                using (var conexion = new SqlConnection(cadenaSQL))
                {
                    conexion.Open();
                    var cmd = new SqlCommand("sp_ingresar_Product", conexion);
                    cmd.CommandType = CommandType.StoredProcedure;

                    // Agregar los parámetros necesarios para la inserción
                    cmd.Parameters.Add(new SqlParameter("@Fecha", nuevoProducto.Fecha));
                    cmd.Parameters.Add(new SqlParameter("@NumerodeLote", nuevoProducto.NumerodeLote));
                    cmd.Parameters.Add(new SqlParameter("@CantidadPeces", nuevoProducto.CantidadPeces));
                    cmd.Parameters.Add(new SqlParameter("@PesoTotal", nuevoProducto.PesoTotal));
                    cmd.Parameters.Add(new SqlParameter("@alimentoConsumido", nuevoProducto.alimentoConsumido));
                    cmd.Parameters.Add(new SqlParameter("@costoAlimento", nuevoProducto.costoAlimento));
                    cmd.Parameters.Add(new SqlParameter("@tasaMortalidad", nuevoProducto.tasaMortalidad));
                  
                    int filasAfectadas = cmd.ExecuteNonQuery();

                    if (filasAfectadas > 0)
                    {
                        return StatusCode(StatusCodes.Status201Created, new { mensaje = "se ha sido ingresado." });
                    }
                    else
                    {
                        return StatusCode(StatusCodes.Status400BadRequest, new { mensaje = "Error al ingresar." });
                    }
                }
            }
            catch (Exception error)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { mensaje = error.Message });
            }
        }
    }
}

    