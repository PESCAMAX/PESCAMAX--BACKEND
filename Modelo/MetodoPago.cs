using API.Modelo;

public class MétodoPago
{
    public int MétodoPagoId { get; set; }
    public string UsuarioId { get; set; }
    public ApplicationUser Usuario { get; set; }
    public string Tipo { get; set; }
    public string Detalle { get; set; }
}
