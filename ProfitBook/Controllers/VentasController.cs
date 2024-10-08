using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProfitBook.Models;
using ProfitBook.Servicios;

namespace ProfitBook.Controllers
{
    public class VentasController: Controller
    {
        private readonly IRepositorioTiposCuentas repositorioTiposCuentas;
        private readonly IServicioUsuarios servicioUsuarios;
        private readonly IRepositorioProductos repositorioProductos;
        private readonly IRepositorioTransacciones repositorioTransacciones;
        private readonly IServicioReportes servicioReportes;

        public VentasController(IRepositorioTiposCuentas repositorioTiposCuentas, IServicioUsuarios servicioUsuarios,
            IRepositorioProductos repositorioProductos, IRepositorioTransacciones repositorioTransacciones,
            IServicioReportes servicioReportes)
        {
            this.repositorioTiposCuentas = repositorioTiposCuentas;
            this.servicioUsuarios = servicioUsuarios;
            this.repositorioProductos = repositorioProductos;
            this.repositorioTransacciones = repositorioTransacciones;
            this.servicioReportes = servicioReportes;
        }

        public async Task<IActionResult> Index(int? mes, int? año)
        {
            var fechaActual = DateTime.Now;
            int mesActual = mes ?? fechaActual.Month;
            int añoActual = año ?? fechaActual.Year;
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();

            var resumenVentas = await repositorioTransacciones.ObtenerResumenVentasPorMes(mesActual, añoActual, usuarioId);
            var navegacion = servicioReportes.ObtenerNavegacionFechas(mesActual, añoActual, nameof(Index));
            var modelo = new VentasIndexViewModel
            {
                ResumenVentas = resumenVentas,
                DateNavigation = navegacion
            };

            return View(modelo);
        }
        [HttpGet]
        public async Task<IActionResult> Crear()
        {
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();
            var cuentas = await repositorioTiposCuentas.Obtener(usuarioId);
            var productos = await repositorioProductos.ObtenerProductos(usuarioId);

            ViewBag.FechaActual = DateTime.Now.ToString("yyyy-MM-dd");
            ViewBag.Cuentas = cuentas;
            ViewBag.Productos = productos;

            var viewModel = new TransaccionMultipleViewModel
            {
                Transaccion = new TransaccionesVenta(),
                Inversiones = new List<InversionDia>(),
            };

            return View(viewModel);
        }
        
        [HttpPost]
        public async Task<IActionResult> Crear(TransaccionMultipleViewModel model)
        {
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();
            if (ModelState.IsValid)
            {
                model.Transaccion.UsuarioId = usuarioId;
                await repositorioTransacciones.CrearTransaccion(model.Transaccion);
                foreach (var inversion in model.Inversiones)
                {
                    inversion.TipoCuentaId = model.Transaccion.CuentaId;
                    inversion.Fecha = model.Transaccion.FechaTransaccion;
                    inversion.UsuarioId = usuarioId;
                    await repositorioTransacciones.GuardarInversionDia(inversion);
                }

                return RedirectToAction("Index");
            }
            var cuentas = await repositorioTiposCuentas.Obtener(usuarioId);
            var productos = await repositorioProductos.ObtenerProductos(usuarioId);

            ViewBag.Cuentas = cuentas;
            ViewBag.Productos = productos;

            return View(model);
        }
        public async Task<IActionResult> Semanal(int? mes, int? año)
        {
            var fechaActual = DateTime.Now;
            int mesActual = mes ?? fechaActual.Month;
            int añoActual = año ?? fechaActual.Year;
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();
            var resumenSemanal = await servicioReportes.ObtenerResumenVentasPorSemana(mesActual, añoActual, usuarioId);
            var navegacion = servicioReportes.ObtenerNavegacionFechas(mesActual, añoActual, nameof(Semanal));

            var modelo = new VentasIndexViewModel
            {
                ResumenVentasSemanal = resumenSemanal,
                DateNavigation = navegacion
            };

            return View(modelo);
        }
        public async Task<IActionResult> Mensual(int? año)
        {
            int añoActual = año ?? DateTime.Today.Year;
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();
            var resumenMensual = await servicioReportes.ObtenerResumenVentasPorAnio(añoActual, usuarioId);

            // Calcular los totales anuales
            var totalVentas = resumenMensual.Sum(r => r.Ventas);
            var totalInversion = resumenMensual.Sum(r => r.Inversion);
            var totalGanancia = resumenMensual.Sum(r => r.Ganancia);

            var model = new VentasIndexViewModel
            {
                Año = añoActual,
                ResumenVentasMensual = resumenMensual,
                TotalVentas = totalVentas,
                TotalInversion = totalInversion,
                TotalGanancia = totalGanancia
            };
            return View(model);
        }

        public async Task<IActionResult> ExportarExcel(int mes, int año)
        {
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();
            var resumenSemanal = await servicioReportes.ObtenerResumenVentasPorSemana(mes, año, usuarioId);

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Ventas Semanales");

            // Encabezados de las columnas
            worksheet.Cell(1, 1).Value = "Semana";
            worksheet.Cell(1, 2).Value = "Fecha Inicio";
            worksheet.Cell(1, 3).Value = "Fecha Fin";
            worksheet.Cell(1, 4).Value = "Ventas";
            worksheet.Cell(1, 5).Value = "Inversión";
            worksheet.Cell(1, 6).Value = "Ganancia";

            // Formato de los encabezados
            var headerRange = worksheet.Range("A1:F1");
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;

            // Llenar los datos de ventas semanales
            int row = 2;
            foreach (var resumen in resumenSemanal)
            {
                worksheet.Cell(row, 1).Value = resumen.Semana;
                worksheet.Cell(row, 2).Value = resumen.FechaInicioSemana.ToString("yyyy-MM-dd");
                worksheet.Cell(row, 3).Value = resumen.FechaFinSemana.ToString("yyyy-MM-dd");
                worksheet.Cell(row, 4).Value = resumen.Ventas;
                worksheet.Cell(row, 5).Value = resumen.Inversion;
                worksheet.Cell(row, 6).Value = resumen.Ganancia;
                row++;
            }

            // Ajustar el ancho de las columnas automáticamente
            worksheet.Columns().AdjustToContents();

            // Guardar el archivo Excel en memoria y devolverlo al usuario
            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Seek(0, SeekOrigin.Begin);

            return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Ventas_Semanales_{mes}_{año}.xlsx");
        }

        public async Task<IActionResult> ExportarVentasAnuales(int año)
        {
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();
            var resumenMensual = await servicioReportes.ObtenerResumenVentasPorAnio(año, usuarioId);

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add($"Ventas {año}");

            // Encabezados de las columnas
            worksheet.Cell(1, 1).Value = "Mes";
            worksheet.Cell(1, 2).Value = "Ventas";
            worksheet.Cell(1, 3).Value = "Inversión";
            worksheet.Cell(1, 4).Value = "Ganancia";

            // Formato de los encabezados
            var headerRange = worksheet.Range("A1:D1");
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;

            // Llenar los datos de ventas mensuales
            int row = 2;
            foreach (var resumen in resumenMensual)
            {
                worksheet.Cell(row, 1).Value = resumen.Mes;
                worksheet.Cell(row, 2).Value = resumen.Ventas;
                worksheet.Cell(row, 3).Value = resumen.Inversion;
                worksheet.Cell(row, 4).Value = resumen.Ganancia;
                row++;
            }

            // Agregar el resumen anual
            worksheet.Cell(row + 1, 1).Value = "Totales:";
            worksheet.Cell(row + 1, 2).Value = resumenMensual.Sum(r => r.Ventas);
            worksheet.Cell(row + 1, 3).Value = resumenMensual.Sum(r => r.Inversion);
            worksheet.Cell(row + 1, 4).Value = resumenMensual.Sum(r => r.Ganancia);

            // Ajustar el ancho de las columnas automáticamente
            worksheet.Columns().AdjustToContents();

            // Guardar el archivo Excel en memoria y devolverlo al usuario
            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Seek(0, SeekOrigin.Begin);

            return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Ventas_Anuales_{año}.xlsx");
        }
    }
}
