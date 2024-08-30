namespace API.Modelo
{
    public class Mortalidad
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public int LoteId { get; set; }
        public DateTime Fecha { get; set; }
        public int CantidadMuertos { get; set; }
    }
}