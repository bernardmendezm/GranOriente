using GranOriente.Negocio;
using Microsoft.AspNetCore.Mvc;

namespace GranOriente.Web.Controllers;

// Controla reportes. En esta versión solo ADMINISTRADOR tiene acceso.

public class ReportesController : BaseController
{
    private readonly HotelNegocio _n;

    public ReportesController(HotelNegocio n)
        => _n = n;

    public async Task<IActionResult> Ocupacion(DateTime? fecha)
    {
        var acceso = RequiereRoles("ADMINISTRADOR");

        if (acceso != null)
            return acceso;

        var f = fecha ?? DateTime.Today;

        ViewBag.Fecha = f;

        return View(
            await _n.Reportes.OcupacionAsync(f)
        );
    }
}
