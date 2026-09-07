using GranOriente.Entidades;
using GranOriente.Negocio;
using Microsoft.AspNetCore.Mvc;

namespace GranOriente.Web.Controllers;

// Controlador de la pantalla principal.
public class HomeController : BaseController
{
    private readonly HotelNegocio _n;

    public HomeController(HotelNegocio n)
    {
        _n = n;
    }

    // Carga el panel principal.
    public async Task<IActionResult> Index()
    {
        // Primero comprueba que el usuario haya iniciado sesión.
        var r = RequiereLogin();

        if (r != null)
            return r;

        // Envía a la vista el nombre de usuario y su rol.
        ViewBag.Usuario =
            HttpContext.Session.GetString("Usuario");

        ViewBag.Rol = Rol;

        // Consulta los indicadores del dashboard.
        var dashboard = new Dashboard
        {
            HabitacionesDisponibles =
                await _n.Dashboard
                    .HabitacionesDisponiblesAsync(),

            ReservasActivas =
                await _n.Dashboard
                    .ReservasActivasAsync(),

            CheckInHoy =
                await _n.Dashboard
                    .CheckInHoyAsync(),

            FacturasHoy =
                await _n.Dashboard
                    .FacturasHoyAsync()
        };

        // Envía el modelo Dashboard a Index.cshtml.
        return View(dashboard);
    }
}
