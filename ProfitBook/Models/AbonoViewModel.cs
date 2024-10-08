using System.ComponentModel.DataAnnotations;

namespace ProfitBook.Models
{
    public class AbonoViewModel
    {
        public List<DateTime> FechasDisponibles { get; set; }

        [Required(ErrorMessage = "Debes seleccionar una fecha.")]
        public DateTime? FechaSeleccionada { get; set; }
    }
}
