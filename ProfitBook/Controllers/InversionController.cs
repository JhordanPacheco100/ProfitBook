using Microsoft.AspNetCore.Mvc;
using ProfitBook.Models;
using ProfitBook.Servicios;

namespace ProfitBook.Controllers
{
    public class InversionController: Controller
    {
        private readonly IRepositorioInversion repositorioInversion;
        private readonly IRepositorioProductos repositorioProductos;
        private readonly IServicioUsuarios servicioUsuarios;

        public InversionController(IRepositorioInversion repositorioInversion, IRepositorioProductos repositorioProductos,
            IServicioUsuarios servicioUsuarios)
        {
            this.repositorioInversion = repositorioInversion;
            this.repositorioProductos = repositorioProductos;
            this.servicioUsuarios = servicioUsuarios;
        }
        public async Task<IActionResult> Index(DateTime? fechaSeleccionada)
        {
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();

            var fechasDisponibles = await repositorioInversion.ObtenerFechasDisponibles(usuarioId);
            // Obtener las inversiones de la fecha seleccionada
            var inversiones = fechaSeleccionada.HasValue
                ? await repositorioInversion.ObtenerPorFecha(fechaSeleccionada.Value, usuarioId)
                : new List<Inversion>();

            var model = new InversionIndexViewModel
            {
                FechasDisponibles = fechasDisponibles,
                Inversiones = inversiones,
            };

            return View(model);
        }
        [HttpGet]
        public async Task<IActionResult> Crear()
        {
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();
            var productos = await repositorioProductos.ObtenerProductos(usuarioId);
            return View(productos);
        }
        [HttpPost]
        public async Task<IActionResult> Crear(DateTime Fecha, int[] ProductoId, decimal[] Cantidad, decimal[] PrecioUnitario, decimal[] Total, decimal[] Saldo)
        {
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();
            if (ProductoId.Length != Cantidad.Length || ProductoId.Length != PrecioUnitario.Length || ProductoId.Length != Total.Length)
            {
                ModelState.AddModelError("", "Error al procesar los datos.");
                var productos = await repositorioProductos.ObtenerProductos(usuarioId);
                return View(productos);
            }

            for (int i = 0; i < ProductoId.Length; i++)
            {
                if (Cantidad[i] > 0)
                {
                    var inversion = new Inversion
                    {
                        ProductoId = ProductoId[i],
                        Cantidad = Cantidad[i],
                        PrecioUnitario = PrecioUnitario[i],
                        Total = Total[i],
                        Fecha = Fecha,
                        Saldo = Saldo[i],
                        UsuarioId = usuarioId
                    };
                    await repositorioInversion.Insertar(inversion);
                }
            }
            TempData["SuccessMessage"] = "Transaccion exitosa! El registro se guardo correctamente.";
            return RedirectToAction("Index");
        }
        /*
        [HttpPost]
        public async Task<IActionResult> CargarInversionesPorFecha(DateTime fechaSeleccionada)
        {
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();
            var inversiones = await repositorioInversion.ObtenerPorFecha(fechaSeleccionada, usuarioId);

            var fechasDisponibles = await repositorioInversion.ObtenerFechasDisponibles(usuarioId);
            var modelo = new InversionIndexViewModel
            {
                FechasDisponibles = fechasDisponibles,
                Inversiones = inversiones
            };
            return View("Index", modelo);
        }*/

        [HttpPost]
        public async Task<IActionResult> CargarInversionesPorFecha(DateTime? fechaSeleccionada)
        {
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();
            var fechasDisponibles = await repositorioInversion.ObtenerFechasDisponibles(usuarioId);

            // Si no se seleccionó fecha, construimos el modelo con lista vacía
            var inversiones = fechaSeleccionada.HasValue
                ? await repositorioInversion.ObtenerPorFecha(fechaSeleccionada.Value, usuarioId)
                : new List<Inversion>();

            // Construimos el modelo de la vista
            var modelo = new InversionIndexViewModel
            {
                FechasDisponibles = fechasDisponibles,
                Inversiones = inversiones
            };

            return View("Index", modelo);
        }


    }
}
