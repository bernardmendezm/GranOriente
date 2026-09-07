using GranOriente.Negocio;
using Microsoft.AspNetCore.Mvc;

namespace GranOriente.Web.Controllers;

// Este Controller administra el proceso de cobro:
// 1. Muestra el resumen del check-out.
// 2. Procesa el pago.
// 3. Genera y muestra la factura detallada.
//
// Solo ADMINISTRADOR y CAJERO tienen permiso.
public class FacturacionController : BaseController
{
    // Acceso a la capa de negocio.
    private readonly HotelNegocio _n;

    // Constructor.
    public FacturacionController(
        HotelNegocio n)
    {
        _n = n;
    }

    // GET: muestra el resumen antes de cobrar.
    [HttpGet]
    public async Task<IActionResult> CheckOut(
        int idReserva)
    {
        // Validamos el rol.
        var acceso = RequiereRoles(
            "ADMINISTRADOR",
            "CAJERO"
        );

        // Si no tiene permiso se devuelve
        // la redirección generada por RequiereRoles.
        if (acceso != null)
            return acceso;

        // Consultamos todos los datos necesarios
        // para mostrar el detalle antes del cobro.
        var resumen =
            await _n.Facturacion
                .ObtenerResumenCheckoutAsync(
                    idReserva
                );

        // Si no existe la reserva o no está en CHECKIN,
        // devolvemos un 404.
        if (resumen is null)
            return NotFound();

        // Enviamos el resumen a CheckOut.cshtml.
        return View(resumen);
    }

    // POST: procesa el pago y genera la factura.
    [HttpPost]
    public async Task<IActionResult> CheckOut(
        int idReserva,
        string metodoPago,
        decimal descuento)
    {
        // Validamos nuevamente el rol.
        // No confiamos únicamente en los botones de la vista.
        var acceso = RequiereRoles(
            "ADMINISTRADOR",
            "CAJERO"
        );

        if (acceso != null)
            return acceso;

        // Ejecutamos el procedimiento de check-out.
        // Devuelve el id de la factura creada.
        var idFactura =
            await _n.Facturacion
                .CheckOutAsync(
                    idReserva,
                    IdEmpleado,
                    metodoPago,
                    descuento
                );

        // Redirigimos al método Factura
        // para mostrar el comprobante detallado.
        return RedirectToAction(
            nameof(Factura),
            new
            {
                id = idFactura
            }
        );
    }

    // GET: muestra la factura ya generada.
    public async Task<IActionResult> Factura(
        int id)
    {
        // Solo ADMINISTRADOR y CAJERO pueden verla.
        var acceso = RequiereRoles(
            "ADMINISTRADOR",
            "CAJERO"
        );

        if (acceso != null)
            return acceso;

        // Consultamos la factura con todos sus detalles.
        var factura =
            await _n.Facturacion
                .ObtenerDetalleAsync(id);

        // Si no existe devolvemos 404.
        if (factura is null)
            return NotFound();

        // Enviamos FacturaDetalle a Factura.cshtml.
        return View(factura);
    }
}
