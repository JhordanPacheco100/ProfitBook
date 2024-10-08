namespace ProfitBook.Models
{
    public class VentasSemanalViewModel
    {
        public int Semana { get; set; } // Número de la semana dentro del mes
        public DateTime FechaInicioSemana { get; set; } // Fecha de inicio de la semana
        public DateTime FechaFinSemana { get; set; } // Fecha de fin de la semana
        public decimal Ventas { get; set; } // Total de ventas
        public decimal Inversion { get; set; } // Total de inversión
        public decimal Ganancia { get; set; } // Total de ganancia
    }
}
