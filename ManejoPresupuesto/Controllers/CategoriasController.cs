using ManejoPresupuesto.Models;
using ManejoPresupuesto.Servicios;
using Microsoft.AspNetCore.Mvc;

namespace ManejoPresupuesto.Controllers
{
    public class CategoriasController : Controller
    {
        private readonly IServiciosUsuarios serviciosUsuarios;
        private readonly IRepositorioCategorias repositorioCategorias;

        public CategoriasController(IServiciosUsuarios serviciosUsuarios,IRepositorioCategorias repositorioCategorias)
        {
            this.serviciosUsuarios = serviciosUsuarios;
            this.repositorioCategorias = repositorioCategorias;
        }

        public async Task<IActionResult> Index()
        {
            var usuarioId = serviciosUsuarios.ObtenerUsuarioId();
            var categorias = await repositorioCategorias.Obtener(usuarioId);
            return View(categorias);
        }

        [HttpGet]
        public IActionResult Crear()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Crear(Categoria categoria)
        {
            if (!ModelState.IsValid)
            {
                return View(categoria);
            }

            var usuarioId = serviciosUsuarios.ObtenerUsuarioId();
            categoria.UsuarioId = usuarioId;
            await repositorioCategorias.Crear(categoria);
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Editar(int Id)
        {
            var usuarioId = serviciosUsuarios.ObtenerUsuarioId();
            var categoria = await repositorioCategorias.ObtenerPorId(Id, usuarioId);
            if (categoria is null)
            {
               return RedirectToAction("NoEncontrado", "Home");
            }

            return View(categoria);
        }

        [HttpPost]
        public async Task<IActionResult> Editar(Categoria categoria)
        {
            if (!ModelState.IsValid)
            {
                return View(categoria);
            }

            var usuarioId = serviciosUsuarios.ObtenerUsuarioId();
            var obtenerCategoria = await repositorioCategorias.ObtenerPorId(categoria.Id, usuarioId);
            if (obtenerCategoria is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }
            categoria.UsuarioId = usuarioId;
            await repositorioCategorias.Actualizar(categoria);
            return RedirectToAction("Index");

        }

        [HttpGet]
        public async Task<IActionResult> Borrar(int id)
        {
            var usuarioId = serviciosUsuarios.ObtenerUsuarioId();
            var categoria = await repositorioCategorias.ObtenerPorId(id, usuarioId);
            if (categoria is null)  
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            return View(categoria);
        }

        [HttpPost]
        public async Task<IActionResult> BorrarCategoria(int id)
        {
            var usuarioId = serviciosUsuarios.ObtenerUsuarioId();
            var categoria = await repositorioCategorias.ObtenerPorId(id, usuarioId);
            if (categoria is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            await repositorioCategorias.Borrar(id);
            return RedirectToAction("Index");
        }
    }
}
