using System.ComponentModel.DataAnnotations;

namespace API.Modelo
{
    public class HorasSeleccionadas
    {
        public int Id { get; set; }
        public int Hour { get; set; }  // Asegúrate de que el nombre de la propiedad es 'Hour'
        public bool Am { get; set; }
        public bool Pm { get; set; }
        public string UserId { get; set; }
    }

}

