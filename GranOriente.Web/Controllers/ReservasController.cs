using GranOriente.Entidades;
using GranOriente.Negocio;
using Microsoft.AspNetCore.Mvc;

namespace GranOriente.Web.Controllers;

// Este Controller recibe las acciones relacionadas con reservas.
// Hereda de BaseController para reutilizar validación de sesión y roles.
public class ReservasController : BaseController
{
    // Objeto de la capa de negocio.
    private readonly HotelNegocio _n;

    // Constructor.
    // ASP.NET Core inyecta automáticamente HotelNegocio.
    public ReservasController(HotelNegocio n)
    {
        _n = n;
    }

    // Muestra la lista de reservas.
    public async Task<IActionResult> Index()
    {
        // Verificamos que el usuario tenga alguno de estos roles.
        // Los tres pueden consultar reservas.
        var acceso = RequiereRoles(
            "ADMINISTRADOR",
            "RECEPCIONISTA",
            "CAJERO"
        );

        // Si RequiereRoles devuelve algo diferente de null,
        // significa que el acceso no está permitido.
        if (acceso != null)
            return acceso;

        // Guardamos el rol actual en ViewBag.
        // La vista lo utiliza para decidir qué botones mostrar.
        ViewBag.Rol = Rol;

        // Pedimos a la capa de datos, por medio de negocio,
        // todas las reservas existentes.
        var reservas =
            await _n.Reservas.ListarAsync();

        // Enviamos la lista de reservas a Views/Reservas/Index.cshtml.
        return View(reservas);
    }

    // GET: muestra el formulario para crear una reserva.
    [HttpGet]
    public async Task<IActionResult> Crear(
        DateTime? ingreso,
        DateTime? salida)
    {
        // Solo administrador y recepcionista pueden crear reservas.
        var acceso = RequiereRoles(
            "ADMINISTRADOR",
            "RECEPCIONISTA"
        );

        if (acceso != null)
            return acceso;

        // Si no se enviaron fechas,
        // proponemos mañana como ingreso y pasado mañana como salida.
        var i =
            ingreso ?? DateTime.Today.AddDays(1);

        var s =
            salida ?? DateTime.Today.AddDays(2);

        // Cargamos huéspedes para llenar el combo del formulario.
        ViewBag.Huespedes =
            await _n.Huespedes.ListarAsync();

        // Cargamos únicamente habitaciones disponibles
        // para el rango de fechas seleccionado.
        ViewBag.Habitaciones =
            await _n.Habitaciones
                .ListarDisponiblesAsync(i, s);

        // Enviamos las fechas elegidas a la vista.
        ViewBag.Ingreso = i;
        ViewBag.Salida = s;

        // Mostramos Views/Reservas/Crear.cshtml.
        return View();
    }

    // POST: recibe los datos del formulario y crea la reserva.
    [HttpPost]
    public async Task<IActionResult> Crear(Reserva r)
    {
        // Volvemos a comprobar permisos.
        // La seguridad no debe depender solo de ocultar botones en HTML.
        var acceso = RequiereRoles(
            "ADMINISTRADOR",
            "RECEPCIONISTA"
        );

        if (acceso != null)
            return acceso;

        try
        {
            // Asociamos la reserva con el empleado
            // que está actualmente autenticado.
            r.IdEmpleado = IdEmpleado;

            // Ejecutamos validaciones de negocio.
            _n.ValidarReserva(r);

            // Si las validaciones pasan,
            // enviamos la reserva a la capa de datos.
            await _n.Reservas.CrearAsync(r);

            // Después de guardar,
            // volvemos al listado de reservas.
            return RedirectToAction(
                nameof(Index)
            );
        }
        catch (Exception ex)
        {
            // Si ocurre un error, enviamos el mensaje a la vista.
            ViewBag.Error = ex.Message;

            // Como volveremos a mostrar el formulario,
            // debemos volver a cargar sus combos.
            ViewBag.Huespedes =
                await _n.Huespedes.ListarAsync();

            ViewBag.Habitaciones =
                await _n.Habitaciones
                    .ListarDisponiblesAsync(
                        r.FechaIngreso,
                        r.FechaSalida
                    );

            // Conservamos las fechas que el usuario había seleccionado.
            ViewBag.Ingreso =
                r.FechaIngreso;

            ViewBag.Salida =
                r.FechaSalida;

            // Devolvemos el mismo objeto Reserva
            // para que el formulario no pierda los datos ingresados.
            return View(r);
        }
    }

    // POST: registra el check-in de una reserva.
    [HttpPost]
    public async Task<IActionResult> CheckIn(int id)
    {
        // Solo administrador o recepcionista pueden realizar check-in.
        var acceso = RequiereRoles(
            "ADMINISTRADOR",
            "RECEPCIONISTA"
        );

        if (acceso != null)
            return acceso;

        // Ejecutamos el proceso de check-in.
        // Enviamos el id de reserva y el empleado que realizó la acción.
        await _n.Reservas.CheckInAsync(
            id,
            IdEmpleado
        );

        // Volvemos al listado de reservas.
        return RedirectToAction(
            nameof(Index)
        );
    }
}
