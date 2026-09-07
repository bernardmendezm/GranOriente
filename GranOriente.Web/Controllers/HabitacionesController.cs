using GranOriente.Entidades;
using GranOriente.Negocio;
using Microsoft.AspNetCore.Mvc;

namespace GranOriente.Web.Controllers;

// Controla Habitaciones. RECEPCIONISTA consulta; solo ADMINISTRADOR puede crear.

public class HabitacionesController : BaseController
{
    private readonly HotelNegocio _n;

    public HabitacionesController(HotelNegocio n)
        => _n = n;

    public async Task<IActionResult> Index()
    {
        var acceso = RequiereRoles("ADMINISTRADOR", "RECEPCIONISTA");

        if (acceso != null)
            return acceso;

        ViewBag.Rol = Rol;

        return View(await _n.Habitaciones.ListarAsync());
    }

    [HttpGet]
    public async Task<IActionResult> Crear()
    {
        var acceso = RequiereRoles("ADMINISTRADOR");

        if (acceso != null)
            return acceso;

        ViewBag.Tipos = await _n.Habitaciones.ListarTiposAsync();
        ViewBag.Estados = await _n.Habitaciones.ListarEstadosAsync();

        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Crear(Habitacion h)
    {
        var acceso = RequiereRoles("ADMINISTRADOR");

        if (acceso != null)
            return acceso;

        try
        {
            if (string.IsNullOrWhiteSpace(h.NumeroHabitacion))
                throw new Exception(
                    "Debe ingresar el número de habitación."
                );

            if (h.Capacidad <= 0)
                throw new Exception(
                    "La capacidad debe ser mayor a cero."
                );

            await _n.Habitaciones.CrearAsync(h);

            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ViewBag.Error = ex.Message;
            ViewBag.Tipos = await _n.Habitaciones.ListarTiposAsync();
            ViewBag.Estados = await _n.Habitaciones.ListarEstadosAsync();

            return View(h);
        }
    }
}
