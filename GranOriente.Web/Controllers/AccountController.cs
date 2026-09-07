using GranOriente.Negocio;
using Microsoft.AspNetCore.Mvc;

namespace GranOriente.Web.Controllers;

// Controlador encargado del inicio y cierre de sesión.
public class AccountController : Controller
{
    private readonly HotelNegocio _n;

    // ASP.NET inyecta HotelNegocio automáticamente.
    public AccountController(HotelNegocio n)
    {
        _n = n;
    }

    // Muestra la pantalla de Login.
    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    // Recibe usuario y contraseña desde el formulario.
    [HttpPost]
    public async Task<IActionResult> Login(
        string usuario,
        string password)
    {
        // Busca al usuario en la base de datos.
        var u = await _n.Usuarios.LoginAsync(
            usuario,
            password
        );

        // Si las credenciales son incorrectas,
        // vuelve a mostrar el Login con un mensaje.
        if (u is null)
        {
            ViewBag.Error =
                "Usuario o contraseña incorrectos.";

            return View();
        }

        // Guarda datos importantes en sesión.
        HttpContext.Session.SetInt32(
            "IdUsuario",
            u.IdUsuario
        );

        HttpContext.Session.SetInt32(
            "IdPersona",
            u.IdPersona
        );

        HttpContext.Session.SetString(
            "Usuario",
            u.NombreUsuario
        );

        HttpContext.Session.SetString(
            "Rol",
            u.Rol
        );

        // Después de iniciar sesión se dirige al panel principal.
        return RedirectToAction(
            "Index",
            "Home"
        );
    }

    // Elimina los datos de sesión y vuelve al Login.
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();

        return RedirectToAction("Login");
    }

    // Pantalla académica preparada para integración de reconocimiento facial.
    [HttpGet]
    public IActionResult Facial()
    {
        ViewBag.Mensaje =
            "Módulo académico preparado para integrar un proveedor real de reconocimiento facial.";

        return View();
    }
}
