using System;
using System.ComponentModel.DataAnnotations;

namespace API.Modelo
{

    public class EspecieLote
    {
        public int Id { get; set; }
        public int EspecieId { get; set; }
        public int LoteId { get; set; }
        public string UserId { get; set; }

        // Propiedades de navegación si estás usando Entity Framework
    }

    // EspecieLoteDTO.cs (para transferencia de datos)
    public class EspecieLoteDTO
    {
        public int LoteId { get; set; }
        public int EspecieId { get; set; }
        public string NombreEspecie { get; set; }
    }
}

