namespace ProfitBook.Models
{
    public class Inversion
    {
        public int Id { get; set; }
        public int ProductoId { get; set; }
        public decimal Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Total { get; set; } //campo que se calculará como Cantidad * PrecioUnitario
        public decimal Saldo { get; set; }
        public DateTime Fecha { get; set; }
        public string ProductoNombre { get; set; } //mostrar el nombre del producto
        public string Estado { get; set; } //el estado de la inversión
        public decimal TotalInversion { get; set; }
        public int UsuarioId { get; set; }
    }
}
