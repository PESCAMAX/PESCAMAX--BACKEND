using API.Modelo;

public class DetalleOrden


{
    public int DetalleOrdenId { get; set; } // Esta será la clave primaria
    public int OrdenId { get; set; }
    public int ProductoId { get; set; }
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }

    // Relaciones
    public Orden Orden { get; set; }
    public Producto Producto { get; set; }
}
