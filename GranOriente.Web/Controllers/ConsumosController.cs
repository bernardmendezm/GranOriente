using GranOriente.Negocio;
using Microsoft.AspNetCore.Mvc;

namespace GranOriente.Web.Controllers;

// Controla consumos. Solo ADMINISTRADOR y CAJERO pueden acceder.

public class ConsumosController : BaseController
{
    private readonly HotelNegocio _n;

    public ConsumosController(HotelNegocio n)
        => _n = n;

    [HttpGet]
    public async Task<IActionResult> Index(int idReserva)
    {
        var acceso = RequiereRoles(
            "ADMINISTRADOR",
            "CAJERO"
        );

        if (acceso != null)
            return acceso;

        ViewBag.IdReserva = idReserva;
        ViewBag.Servicios =
            await _n.Consumos.ListarServiciosAsync();

        return View(
            await _n.Consumos.ListarPorReservaAsync(idReserva)
        );
    }

    [HttpPost]
    public async Task<IActionResult> Registrar(
        int idReserva,
        int idServicio,
        int cantidad,
        string? observaciones)
    {
        var acceso = RequiereRoles(
            "ADMINISTRADOR",
            "CAJERO"
        );

        if (acceso != null)
            return acceso;

        await _n.Consumos.RegistrarAsync(
            idReserva,
            idServicio,
            cantidad,
            observaciones
        );

        return RedirectToAction(
            nameof(Index),
            new { idReserva }
        );
    }
}
