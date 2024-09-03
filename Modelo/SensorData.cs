namespace API.Modelo
{
    public class SensorData
    {
        public int Id { get; set; } // Clave primaria
        public float Temperatura { get; set; }
        public float Tds { get; set; }
        public float Ph { get; set; }
        public int LoteID { get; set; }
        public string UserID { get; set; }
    }
}
