using Microsoft.AspNetCore.Mvc;
using ProfitBook.Models;
using ProfitBook.Servicios;

namespace ProfitBook.Controllers
{
    public class ProductosController: Controller
    {
        private readonly IRepositorioProductos repositorioProductos;
        private readonly IServicioUsuarios servicioUsuarios;

        public ProductosController(IRepositorioProductos repositorioProductos, IServicioUsuarios servicioUsuarios)
        {
            this.repositorioProductos = repositorioProductos;
            this.servicioUsuarios = servicioUsuarios;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();
            var productos = await repositorioProductos.ObtenerProductos(usuarioId);
            return View(productos);
        }

        [HttpGet]
        public IActionResult Crear()
        {
            return View();
        }
        
        [HttpPost]
        public async Task<IActionResult> Crear(Productos producto)
        {
            if (!ModelState.IsValid)
            {
                return View(producto);
            }

            producto.UsuarioId = servicioUsuarios.ObtenerUsuarioId();

            var yaExisteProducto = await repositorioProductos.Existe(producto.Nombre, producto.UsuarioId);

            if (yaExisteProducto)
            {
                ModelState.AddModelError(nameof(producto.Nombre),
                    $"El nombre {producto.Nombre} ya existe.");

                return View(producto);
            }

            await repositorioProductos.Crear(producto);

            return RedirectToAction("Index");
        }
        [HttpGet]
        public async Task<ActionResult> Editar(int id)
        {
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();
            var producto = await repositorioProductos.ObtenerPorId(id, usuarioId);

            if (producto is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            return View(producto);
        }
        [HttpPost]
        public async Task<ActionResult> Editar(Productos producto)
        {
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();
            var productoExiste = await repositorioProductos.ObtenerPorId(producto.Id, usuarioId);

            if (productoExiste is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            await repositorioProductos.Editar(producto);
            return RedirectToAction("Index");
        }
        [HttpGet]
        public async Task<IActionResult> Borrar(int id)
        {
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();
            var producto = await repositorioProductos.ObtenerPorId(id, usuarioId);
            if (producto == null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }
            return View(producto);
        }
        [HttpPost]
        public async Task<IActionResult> BorrarProducto(int id)
        {
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();
            var producto = repositorioProductos.ObtenerPorId(id, usuarioId);
            if (producto == null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }
            await repositorioProductos.Borrar(id);
            return RedirectToAction("Index");
        }
    }
}
