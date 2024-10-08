using Microsoft.AspNetCore.Mvc;
using ProfitBook.Models;
using ProfitBook.Servicios;
using System.Diagnostics;

namespace ProfitBook.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IServicioReportes servicioReportes;
        private readonly IServicioUsuarios servicioUsuarios;

        public HomeController(ILogger<HomeController> logger, IServicioReportes servicioReportes, IServicioUsuarios servicioUsuarios)
        {
            _logger = logger;
            this.servicioReportes = servicioReportes;
            this.servicioUsuarios = servicioUsuarios;
        }

        public async Task<IActionResult> Index()
        {
            var mesActual = DateTime.Today.Month;
            int añoActual = DateTime.Today.Year;
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();

            // Obtener el resumen de ventas por año
            var resumenMensual = await servicioReportes.ObtenerResumenVentasPorAnio(añoActual, usuarioId) 
                                    ?? new List<ResumenVentasMensualViewModel>();
            var estadisticasMensuales = await servicioReportes.ObtenerEstadisticasMensuales(usuarioId, mesActual, añoActual)
                                    ?? new EstadisticasMensualesViewModel();
            var recordsDeVentas = await servicioReportes.ObtenerRecordsDeVentas(usuarioId)
                                    ?? new RecordMesViewModel();

            // Calcular los totales anuales
            var totalVentas = resumenMensual.Any() ? resumenMensual.Sum(r => r.Ventas) : 0;
            var totalInversion = resumenMensual.Any() ? resumenMensual.Sum(r => r.Inversion) : 0;
            var totalGanancia = resumenMensual.Any() ? resumenMensual.Sum(r => r.Ganancia) : 0;

            var model = new VentasIndexViewModel
            {
                Año = añoActual,
                ResumenVentasMensual = resumenMensual,
                TotalVentas = totalVentas,
                TotalInversion = totalInversion,
                TotalGanancia = totalGanancia,
                EstadisticasMensuales = estadisticasMensuales,
                RecordsDeVentas = recordsDeVentas
            };

            return View(model);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
