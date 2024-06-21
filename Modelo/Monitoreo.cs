namespace API.Modelo
{
    public class Monitoreo
    {

            public int ID_M { get; set; }
            public float tds { get; set; }
            public float Temperatura { get; set; }
            public float PH { get; set; }
            public DateTime FechaHora { get; set; }  // Agregado campo FechaHora
            public int LoteID { get; set; }
    }
}

