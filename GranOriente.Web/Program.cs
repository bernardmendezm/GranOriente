using GranOriente.Negocio;

// Crea el constructor de la aplicación ASP.NET Core.
var builder = WebApplication.CreateBuilder(args);

// Habilita Controllers y Views de ASP.NET Core MVC.
builder.Services.AddControllersWithViews();

// Configura la sesión.
// La sesión se usa para guardar IdUsuario, IdPersona, Usuario y Rol.
builder.Services.AddSession(o =>
{
    o.IdleTimeout = TimeSpan.FromMinutes(30);
    o.Cookie.HttpOnly = true;
    o.Cookie.IsEssential = true;
});

// Lee la cadena de conexión HotelDb desde appsettings.json.
string cs = builder.Configuration
    .GetConnectionString("HotelDb")
    ?? throw new Exception(
        "Falta ConnectionStrings:HotelDb en appsettings.json"
    );

// Registra HotelNegocio para poder inyectarlo en los Controllers.
builder.Services.AddSingleton(
    new HotelNegocio(cs)
);

// Construye la aplicación.
var app = builder.Build();

// En producción redirige los errores a una página controlada.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

// Permite servir archivos CSS, JS e imágenes desde wwwroot.
app.UseStaticFiles();

// Habilita el sistema de rutas.
app.UseRouting();

// Activa la sesión.
app.UseSession();

// Define la ruta por defecto.
// El sistema abre inicialmente Account/Login.
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}"
);

// Inicia la aplicación.
app.Run();
