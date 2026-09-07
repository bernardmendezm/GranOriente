using GranOriente.Datos;
using GranOriente.Entidades;

namespace GranOriente.Negocio;

// Esta clase representa la capa de negocio.
// Agrupa los objetos de acceso a datos para que los controladores
// trabajen con un único punto central.
public class HotelNegocio
{
    // Módulos disponibles desde la capa de negocio.
    public HuespedDatos Huespedes { get; }
    public HabitacionDatos Habitaciones { get; }
    public ReservaDatos Reservas { get; }
    public ConsumoDatos Consumos { get; }
    public FacturacionDatos Facturacion { get; }
    public UsuarioDatos Usuarios { get; }
    public ReporteDatos Reportes { get; }
    public DashboardDatos Dashboard { get; }
    public FuncionarioUsuarioDatos Funcionarios { get; }

    // Constructor principal.
    // Recibe la cadena de conexión y crea una sola instancia de Db.
    public HotelNegocio(string cs)
    {
        var db = new Db(cs);

        // Cada módulo recibe la misma clase Db.
        Huespedes = new HuespedDatos(db);
        Habitaciones = new HabitacionDatos(db);
        Reservas = new ReservaDatos(db);
        Consumos = new ConsumoDatos(db);
        Facturacion = new FacturacionDatos(db);
        Usuarios = new UsuarioDatos(db);
        Reportes = new ReporteDatos(db);
        Dashboard = new DashboardDatos(db);
        Funcionarios = new FuncionarioUsuarioDatos(db);
    }

    // Valida reglas básicas antes de enviar una reserva a la base de datos.
    public void ValidarReserva(Reserva r)
    {
        // La salida siempre debe ser posterior al ingreso.
        if (r.FechaSalida <= r.FechaIngreso)
            throw new Exception(
                "La fecha de salida debe ser posterior a la fecha de ingreso."
            );

        // Debe existir al menos una persona.
        if (r.CantidadPersona <= 0)
            throw new Exception(
                "La cantidad de personas debe ser mayor a cero."
            );
    }
}
