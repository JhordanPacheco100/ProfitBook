namespace ProfitBook.Models
{
    public class VentasIndexViewModel
    {
        public IEnumerable<ResumenVentasVIewModel> ResumenVentas { get; set; }
        public DateNavigationViewModel DateNavigation { get; set; }
        public IEnumerable<VentasSemanalViewModel> ResumenVentasSemanal { get; set; }
        public IEnumerable<ResumenVentasMensualViewModel> ResumenVentasMensual { get; set; }

        public int Año { get; set; }

        //Propiedades para los totales anuales
        public decimal TotalVentas { get; set; }
        public decimal TotalInversion { get; set; }
        public decimal TotalGanancia { get; set; }

        public EstadisticasMensualesViewModel EstadisticasMensuales { get; set; }
        public RecordMesViewModel RecordsDeVentas { get; set; }
    }
}
