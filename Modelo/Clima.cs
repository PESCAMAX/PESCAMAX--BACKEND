namespace API.Modelo
{
    public class Clima
    {
        public string NombreCiudad { get; set; }
        public double TemperaturaActual { get; set; }
        public string EstadoMeteoro { get; set; }
        public int Humedad { get; set; }
        public int Nubes { get; set; }
        public DateTime FechaHora { get; set; }
        public string UserId { get; set; }
    }
}