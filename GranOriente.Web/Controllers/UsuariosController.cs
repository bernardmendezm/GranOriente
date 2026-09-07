using GranOriente.Entidades;
using GranOriente.Negocio;
using Microsoft.AspNetCore.Mvc;

namespace GranOriente.Web.Controllers;

// Controla creación y listado de funcionarios/usuarios. Solo ADMINISTRADOR.

public class UsuariosController : BaseController
{
    private readonly HotelNegocio _n;

    public UsuariosController(HotelNegocio n)
    {
        _n = n;
    }

    private IActionResult? RequiereAdministrador()
    {
        return RequiereRoles("ADMINISTRADOR");
    }

    public async Task<IActionResult> Index()
    {
        var acceso = RequiereAdministrador();

        if (acceso != null)
            return acceso;

        return View(await _n.Funcionarios.ListarAsync());
    }

    [HttpGet]
    public async Task<IActionResult> Crear()
    {
        var acceso = RequiereAdministrador();

        if (acceso != null)
            return acceso;

        ViewBag.Roles = await _n.Funcionarios.ListarRolesAsync();

        return View(new FuncionarioUsuario
        {
            FechaIngreso = DateTime.Today,
            TipoDocumento = "CI"
        });
    }

    [HttpPost]
    public async Task<IActionResult> Crear(FuncionarioUsuario f)
    {
        var acceso = RequiereAdministrador();

        if (acceso != null)
            return acceso;

        try
        {
            if (string.IsNullOrWhiteSpace(f.Nombre))
                throw new Exception("Debe ingresar el nombre.");

            if (string.IsNullOrWhiteSpace(f.NumeroDocumento))
                throw new Exception("Debe ingresar el número de documento.");

            if (string.IsNullOrWhiteSpace(f.Cargo))
                throw new Exception("Debe ingresar el cargo del funcionario.");

            if (string.IsNullOrWhiteSpace(f.NombreUsuario))
                throw new Exception("Debe ingresar el nombre de usuario.");

            if (string.IsNullOrWhiteSpace(f.Password))
                throw new Exception("Debe ingresar una contraseña.");

            if (f.Password.Length < 6)
                throw new Exception("La contraseña debe tener como mínimo 6 caracteres.");

            if (f.IdRol <= 0)
                throw new Exception("Debe seleccionar un rol.");

            await _n.Funcionarios.CrearAsync(f);

            TempData["Mensaje"] = "Funcionario y usuario creados correctamente.";

            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ViewBag.Error = ex.Message;
            ViewBag.Roles = await _n.Funcionarios.ListarRolesAsync();

            return View(f);
        }
    }
}
