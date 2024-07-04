using System;
namespace API.Modelo
{
    public class Alerta
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public int EspecieID { get; set; }
        public int LoteID { get; set; }
        public string Descripcion { get; set; }
        public DateTime Fecha { get; set; }
    }
}