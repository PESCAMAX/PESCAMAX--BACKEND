using System.ComponentModel.DataAnnotations;

namespace TuProyecto.Models
{
    public class CrearEspecie
    {
        public int Id { get; set; }

        [Required]
        [StringLength(40)]
        public string NombreEspecie { get; set; }

        public float TdsMinimo { get; set; }
        public float TdsMaximo { get; set; }
        public float TemperaturaMinimo { get; set; }
        public float TemperaturaMaximo { get; set; }
        public float PhMinimo { get; set; }
        public float PhMaximo { get; set; }
    }
}

