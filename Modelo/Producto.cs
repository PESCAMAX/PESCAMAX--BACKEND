namespace API.Modelo
{
    public class Producto
    {
        public int id { get; set; }
        public string Fecha { get; set; }
        public string NumerodeLote { get; set; }
        public int CantidadPeces { get; set; }
        public int PesoTotal { get; set; }
        public int alimentoConsumido { get; set; }
        public int costoAlimento { get; set; }
        public decimal tasaMortalidad { get; set; }
    }
}
