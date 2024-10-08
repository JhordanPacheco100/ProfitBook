namespace ProfitBook.Models
{
    public class DateNavigationViewModel
    {
        public int MesActual { get; set; }
        public int AñoActual { get; set; }
        public int MesAnterior { get; set; }
        public int AñoAnterior { get; set; }
        public int MesPosterior { get; set; }
        public int AñoPosterior { get; set; }
        public DateTime FechaActual { get; set; }
        public string ActionName { get; set; }
    }
}
