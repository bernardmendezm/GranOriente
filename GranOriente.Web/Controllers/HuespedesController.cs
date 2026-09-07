using GranOriente.Entidades;
using GranOriente.Negocio;
using Microsoft.AspNetCore.Mvc;

namespace GranOriente.Web.Controllers;

// Controla el módulo Huéspedes. Solo ADMINISTRADOR y RECEPCIONISTA pueden acceder.

public class HuespedesController : BaseController
{
    private readonly HotelNegocio _n;

    public HuespedesController(HotelNegocio n)
        => _n = n;

    public async Task<IActionResult> Index()
    {
        var acceso = RequiereRoles("ADMINISTRADOR", "RECEPCIONISTA");

        if (acceso != null)
            return acceso;

        return View(await _n.Huespedes.ListarAsync());
    }

    [HttpGet]
    public IActionResult Crear()
    {
        var acceso = RequiereRoles("ADMINISTRADOR", "RECEPCIONISTA");

        if (acceso != null)
            return acceso;

        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Crear(Persona p)
    {
        var acceso = RequiereRoles("ADMINISTRADOR", "RECEPCIONISTA");

        if (acceso != null)
            return acceso;

        if (string.IsNullOrWhiteSpace(p.Nombre) ||
            string.IsNullOrWhiteSpace(p.NumeroDocumento))
        {
            ModelState.AddModelError(
                "",
                "Nombre y documento son obligatorios."
            );

            return View(p);
        }

        await _n.Huespedes.CrearAsync(p);

        return RedirectToAction(nameof(Index));
    }
}
