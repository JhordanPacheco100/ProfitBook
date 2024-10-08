namespace ProfitBook.Models
{
    public class InversionIndexViewModel
    {
        public List<DateTime> FechasDisponibles { get; set; }
        public List<Inversion> Inversiones { get; set; }
        public DateTime? FechaSeleccionada { get; set; }
    }
}
