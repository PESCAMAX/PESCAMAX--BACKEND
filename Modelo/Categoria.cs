using API.Modelo;

public class Categoría
{
    public int CategoríaId { get; set; }
    public string Nombre { get; set; }
    public ICollection<Producto> Productos { get; set; }
}