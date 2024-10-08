namespace ProfitBook.Models
{
    public class Abono
    {
        public int Id { get; set; }
        public int ProductoId { get; set; }
        public string ProductoNombre { get; set; }
        public decimal Total { get; set; }
        public decimal MontoAbono { get; set; }
        public decimal Saldo { get; set; }
        public DateTime FechaAbono { get; set; }
        public string Estado { get; set; }
        public int IdInversion { get; set; }
        public int UsuarioId { get; set; }
    }
}
