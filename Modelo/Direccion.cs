using API.Modelo;

public class Dirección
{
    public int DirecciónId { get; set; }
    public string UsuarioId { get; set; }
    public ApplicationUser Usuario { get; set; }
    public string Calle { get; set; }
    public string Ciudad { get; set; }
    public string Estado { get; set; }
    public string CódigoPostal { get; set; }
    public string País { get; set; }
}
