using System.ComponentModel.DataAnnotations;

namespace API.Modelo
{
    public class CrearEspecie
    {
        public int Id { get; set; }
        public string NombreEspecie { get; set; }
        public float TdsMinimo { get; set; }
        public float TdsMaximo { get; set; }
        public float TemperaturaMinimo { get; set; }
        public float TemperaturaMaximo { get; set; }
        public float PhMinimo { get; set; }
        public float PhMaximo { get; set; }
        public int Cantidad { get; set; } // Nuevo campo
        public string UserId { get; set; }
    }
}
