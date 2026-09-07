using Microsoft.AspNetCore.Mvc;

namespace GranOriente.Web.Controllers;

// Todos los controladores heredan de BaseController.
// Aquí se concentran funciones comunes relacionadas con sesión y roles.
public class BaseController : Controller
{
    // Indica si existe un usuario autenticado en la sesión.
    protected bool EstaAutenticado =>
        HttpContext.Session.GetInt32("IdUsuario").HasValue;

    // Obtiene el id de la persona/empleado que inició sesión.
    protected int IdEmpleado =>
        HttpContext.Session.GetInt32("IdPersona") ?? 0;

    // Obtiene el rol actual del usuario.
    protected string Rol =>
        HttpContext.Session.GetString("Rol") ?? "";

    // Si no hay sesión, redirige al Login.
    protected IActionResult? RequiereLogin()
        => EstaAutenticado
            ? null
            : RedirectToAction("Login", "Account");

    // Verifica si el rol actual coincide con alguno de los roles permitidos.
    protected bool TieneRol(params string[] roles)
        => roles.Any(r =>
            string.Equals(
                Rol,
                r,
                StringComparison.OrdinalIgnoreCase
            )
        );

    // Combina dos validaciones:
    // 1. Debe existir una sesión.
    // 2. El usuario debe tener uno de los roles permitidos.
    protected IActionResult? RequiereRoles(params string[] roles)
    {
        var login = RequiereLogin();

        if (login != null)
            return login;

        if (!TieneRol(roles))
            return RedirectToAction("Index", "Home");

        return null;
    }
}
