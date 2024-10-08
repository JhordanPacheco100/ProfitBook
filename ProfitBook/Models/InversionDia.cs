using System.ComponentModel.DataAnnotations;

namespace ProfitBook.Models
{
    public class InversionDia
    {
        public int Id { get; set; }
        public int ProductoId { get; set; }
        public decimal Precio { get; set; }
        public int TipoCuentaId { get; set; }
        [Required]
        public DateTime Fecha { get; set; } = DateTime.Now;

        public Productos Producto { get; set; }
        public int UsuarioId { get; set; }
    }
}
