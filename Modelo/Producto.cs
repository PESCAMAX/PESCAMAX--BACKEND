using API.Modelo;

using System.Text.Json.Serialization;

public class Producto
{
    [JsonPropertyName("productoId")]
    public int ProductoId { get; set; }

    [JsonPropertyName("nombre")]
    public string Nombre { get; set; }

    [JsonPropertyName("descripción")]
    public string Descripción { get; set; }

    [JsonPropertyName("precio")]
    public decimal Precio { get; set; }

    [JsonPropertyName("categoríaId")]
    public int CategoríaId { get; set; }

    [JsonPropertyName("categoría")]
    public Categoría Categoría { get; set; }
}