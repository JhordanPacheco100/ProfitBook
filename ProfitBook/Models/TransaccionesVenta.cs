namespace ProfitBook.Models
{
    public class TransaccionesVenta
    {
        public int Id { get; set; }
        public DateTime FechaTransaccion { get; set; } = DateTime.Now;
        public int CuentaId { get; set; }
        public decimal VentaMonto { get; set; }
        public decimal InversionMonto { get; set; } = 0;
        public int UsuarioId { get; set; }
    }
}
