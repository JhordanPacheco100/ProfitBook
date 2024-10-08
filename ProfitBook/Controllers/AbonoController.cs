using Microsoft.AspNetCore.Mvc;
using ProfitBook.Models;
using ProfitBook.Servicios;

namespace ProfitBook.Controllers
{
    public class AbonoController : Controller
    {
        private readonly IRepositorioAbono repositorioAbono;
        private readonly IServicioUsuarios servicioUsuarios;

        public AbonoController(IRepositorioAbono repositorioAbono, IServicioUsuarios servicioUsuarios)
        {
            this.repositorioAbono = repositorioAbono;
            this.servicioUsuarios = servicioUsuarios;
        }
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();
            var fechas = await repositorioAbono.ObtenerFechasDisponibles(usuarioId);
            var modelo = new AbonoViewModel
            {
                FechasDisponibles = fechas ?? new List<DateTime>()
            };
            return View(modelo);
        }

        [HttpPost]
        public async Task<IActionResult> Abonar(AbonoViewModel model)
        {
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();
            if (!ModelState.IsValid)
            {
                model.FechasDisponibles = await repositorioAbono.ObtenerFechasDisponibles(usuarioId);
                return View("Index", model);
            }
            var inversiones = await repositorioAbono.ObtenerInversionesPorFecha(model.FechaSeleccionada.Value, usuarioId);
            return View(inversiones);
        }

        [HttpPost]
        public async Task<IActionResult> GuardarAbono(List<Abono> abonos)
        {
            if (ModelState.IsValid)
            {
                var usuarioId = servicioUsuarios.ObtenerUsuarioId();
                foreach (var abono in abonos)
                {
                    abono.UsuarioId = usuarioId;
                    await repositorioAbono.GuardarAbono(abono);
                }
                TempData["SuccessMessage"] = "Transaccion exitosa! El registro se guardo correctamente.";
                return RedirectToAction("Index");
            }
            return View("Abonar", abonos);
        }
    }
}
