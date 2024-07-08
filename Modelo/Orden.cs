using API.Modelo;

public class Orden
{
    public int OrdenId { get; set; }
    public string UsuarioId { get; set; }
    public ApplicationUser Usuario { get; set; }
    public decimal Total { get; set; }
    public DateTime FechaOrden { get; set; }
    public string Estado { get; set; }
}
