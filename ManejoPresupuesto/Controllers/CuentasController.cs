using ManejoPresupuesto.Models;
using ManejoPresupuesto.Servicios;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ManejoPresupuesto.Controllers
{
    public class CuentasController: Controller
    {
        private readonly IRepositorioTiposCuentas repositorioTiposCuentas;
        private readonly IServiciosUsuarios serviciosUsuarios;
        private readonly IRepositorioCuentas repositorioCuentas;

        public CuentasController(IRepositorioTiposCuentas repositorioTiposCuentas,IServiciosUsuarios serviciosUsuarios, IRepositorioCuentas repositorioCuentas)
        {
            this.repositorioTiposCuentas = repositorioTiposCuentas;
            this.serviciosUsuarios = serviciosUsuarios;
            this.repositorioCuentas = repositorioCuentas;
        }


        [HttpGet]
        public async  Task<IActionResult> Crear()
        {
            var usuarioId = serviciosUsuarios.ObtenerUsuarioId();
            var modelo = new CuentaCreacionViewModel();
            modelo.TiposCuentas = await ObtenerTiposCuentas(usuarioId);

            return View(modelo);
        }

        [HttpPost]
        public async Task<IActionResult> Crear(CuentaCreacionViewModel cuenta)
        {
            
            var usuarioId = serviciosUsuarios.ObtenerUsuarioId();
            var tipoCuenta = await repositorioTiposCuentas.Obtener(usuarioId);

            if (tipoCuenta is null)
            {
                return RedirectToAction("NoEncontrado","Home");
            }

            if (!ModelState.IsValid)
            {
                cuenta.TiposCuentas = await ObtenerTiposCuentas(usuarioId);
                return View(cuenta);
            }

            await repositorioCuentas.Crear(cuenta);
            return RedirectToAction("Index");
        }

        private async Task<IEnumerable<SelectListItem>> ObtenerTiposCuentas(int usuarioId)
        {
            var tiposCuentas = await repositorioTiposCuentas.Obtener(usuarioId);
            return tiposCuentas.Select(x => new SelectListItem(x.Nombre, x.Id.ToString()));
        }
    }
}
