namespace API.Modelo
{
    public class Monitoreo
    {
        public int ID_M { get; set; }
        public float Temperatura { get; set; }
        public float tds { get; set; }
        public float PH { get; set; }
        public DateTime FechaHora { get; set; }
        public int LoteID { get; set; }
        public string userId { get; set; }
    }

}