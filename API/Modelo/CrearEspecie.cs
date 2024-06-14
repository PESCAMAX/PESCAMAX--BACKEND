using System.ComponentModel.DataAnnotations;

namespace API.Modelo
{
    public class CrearEspecie
    {
        public int Id { get; set; }

        [Required]
        [StringLength(40)]
        public string NombreEspecie { get; set; }

        public int TdsSeguro { get; set; }
        public int TdsPeligroso { get; set; }
        public int TemperaturaSeguro { get; set; }
        public int TemperaturaPeligroso { get; set; }
        public int PhSeguro { get; set; }
        public int PhPeligroso { get; set; }
    }
}

