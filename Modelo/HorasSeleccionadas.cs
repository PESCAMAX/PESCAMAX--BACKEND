using System.ComponentModel.DataAnnotations;

namespace API.Modelo
{
    public class HorasSeleccionadas
    {
        [Key]
        public int Id { get; set; }

        public int Hora { get; set; }

        public bool Am { get; set; }

        public bool Pm { get; set; }
    }
}
