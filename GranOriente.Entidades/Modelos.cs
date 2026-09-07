namespace GranOriente.Entidades;

// Esta clase representa a una persona del sistema.
// Se reutiliza tanto para huéspedes como para funcionarios.
public class Persona
{
    public int IdPersona { get; set; }

    // Datos personales básicos.
    public string Nombre { get; set; } = "";
    public string ApellidoPaterno { get; set; } = "";
    public string ApellidoMaterno { get; set; } = "";

    // Datos de identificación.
    public string TipoDocumento { get; set; } = "";
    public string NumeroDocumento { get; set; } = "";

    // Datos de contacto.
    public string? Telefono { get; set; }
    public string? Email { get; set; }

    // Datos adicionales de la persona.
    public DateTime? FechaNacimiento { get; set; }
    public string? Sexo { get; set; }
    public string? Nacionalidad { get; set; }
}

// Representa a un huésped.
// Se relaciona con PERSONA mediante el mismo identificador.
public class Huesped
{
    public int IdHuesped { get; set; }
    public DateTime FechaRegistro { get; set; }

    // Objeto Persona asociado al huésped.
    public Persona Persona { get; set; } = new();
}

// Representa una habitación del hotel.
public class Habitacion
{
    public int IdHabitacion { get; set; }
    public int IdTipoHabitacion { get; set; }
    public int IdEstadoHabitacion { get; set; }

    public int Capacidad { get; set; }
    public string Descripcion { get; set; } = "";
    public string NumeroHabitacion { get; set; } = "";
    public int Piso { get; set; }

    // Estos campos sirven para mostrar nombres legibles en las vistas.
    public string TipoHabitacion { get; set; } = "";
    public string EstadoHabitacion { get; set; } = "";
}

// Se usa para llenar el combo de tipos de habitación.
public class TipoHabitacion
{
    public int IdTipoHabitacion { get; set; }
    public string Tipo { get; set; } = "";
}

// Se usa para llenar el combo de estados de habitación.
public class EstadoHabitacion
{
    public int IdEstadoHabitacion { get; set; }
    public string Estado { get; set; } = "";
}

// Representa una reserva del hotel.
public class Reserva
{
    public int IdReserva { get; set; }
    public int IdTemporada { get; set; }

    public DateTime FechaReserva { get; set; }
    public DateTime FechaIngreso { get; set; }
    public DateTime FechaSalida { get; set; }

    public string Estado { get; set; } = "RESERVADA";
    public string? Observaciones { get; set; }

    public decimal PrecioReserva { get; set; }

    public int IdHuesped { get; set; }
    public DateTime? TiempoLimite { get; set; }

    public int? IdAgencia { get; set; }
    public string TipoReserva { get; set; } = "PRESENCIAL";

    public int IdEmpleado { get; set; }
    public int IdHabitacion { get; set; }

    public int CantidadPersona { get; set; }
    public int IdTarifa { get; set; }
    public decimal PrecioNoche { get; set; }

    // Campos de apoyo para mostrar información en tablas.
    public string? Huesped { get; set; }
    public string? NumeroHabitacion { get; set; }
}

// Representa un consumo adicional realizado durante la estadía.
public class Consumo
{
    public int IdConsumo { get; set; }
    public int IdReserva { get; set; }
    public int IdServicio { get; set; }

    public int Cantidad { get; set; }
    public DateTime FechaHora { get; set; }

    public string? Observaciones { get; set; }
    public decimal PrecioUnitario { get; set; }

    // Nombre del servicio para mostrarlo en la vista.
    public string? Servicio { get; set; }
}

// Representa un servicio adicional del hotel.
public class Servicio
{
    public int IdServicio { get; set; }
    public string NombreServicio { get; set; } = "";
    public decimal Precio { get; set; }
}

// Representa la factura final generada en el check-out.
public class Factura
{
    public int IdFactura { get; set; }
    public int IdCheckOut { get; set; }

    public DateTime FechaEmision { get; set; }
    public decimal Descuento { get; set; }

    public string? Observaciones { get; set; }
    public string EstadoFactura { get; set; } = "EMITIDA";
    public string NumeroFactura { get; set; } = "";

    public decimal Subtotal { get; set; }
    public decimal Total { get; set; }

    public string MetodoPago { get; set; } = "EFECTIVO";
}

// Contiene los datos principales del usuario que inició sesión.
public class UsuarioSesion
{
    public int IdUsuario { get; set; }
    public string NombreUsuario { get; set; } = "";
    public string Rol { get; set; } = "";
    public int IdPersona { get; set; }
}

// Modelo de apoyo para el reporte de ocupación.
public class ReporteOcupacion
{
    public string NumeroHabitacion { get; set; } = "";
    public string TipoHabitacion { get; set; } = "";
    public string Estado { get; set; } = "";
    public int Ocupada { get; set; }
}

// Modelo del panel principal.
// Guarda los cuatro indicadores que se muestran en el dashboard.
public class Dashboard
{
    public int HabitacionesDisponibles { get; set; }
    public int ReservasActivas { get; set; }
    public int CheckInHoy { get; set; }
    public int FacturasHoy { get; set; }
}

// Se usa para cargar los roles disponibles del sistema.
public class RolSistema
{
    public int IdRol { get; set; }
    public string Nombre { get; set; } = "";
}

// Modelo utilizado por el administrador para registrar funcionarios y usuarios.
public class FuncionarioUsuario
{
    public int IdPersona { get; set; }

    // Datos personales.
    public string Nombre { get; set; } = "";
    public string ApellidoPaterno { get; set; } = "";
    public string ApellidoMaterno { get; set; } = "";
    public string TipoDocumento { get; set; } = "";
    public string NumeroDocumento { get; set; } = "";
    public DateTime? FechaNacimiento { get; set; }
    public string? Sexo { get; set; }
    public string? Nacionalidad { get; set; }
    public string? Telefono { get; set; }
    public string? Email { get; set; }

    // Datos laborales y de acceso.
    public string Cargo { get; set; } = "";
    public DateTime FechaIngreso { get; set; } = DateTime.Today;
    public string NombreUsuario { get; set; } = "";
    public string Password { get; set; } = "";
    public int IdRol { get; set; }

    // Datos usados para mostrar el listado.
    public string Rol { get; set; } = "";
    public string Estado { get; set; } = "";
}


// Representa una línea de consumo que aparecerá separada en la factura.
public class FacturaConsumoDetalle
{
    // Nombre del servicio consumido.
    public string Servicio { get; set; } = "";

    // Cantidad solicitada de ese servicio.
    public int Cantidad { get; set; }

    // Precio que tenía el servicio al momento de registrar el consumo.
    public decimal PrecioUnitario { get; set; }

    // Importe total de esta línea: Cantidad x PrecioUnitario.
    public decimal Importe { get; set; }

    // Fecha y hora en la que se registró el consumo.
    public DateTime FechaHora { get; set; }

    // Observación opcional registrada junto al consumo.
    public string? Observaciones { get; set; }
}

// Modelo completo utilizado para imprimir la factura detallada.
// Reúne datos de FACTURA, RESERVA, HABITACION, HUESPED y CONSUMO.
public class FacturaDetalle
{
    // Datos propios de la factura.
    public int IdFactura { get; set; }
    public string NumeroFactura { get; set; } = "";
    public DateTime FechaEmision { get; set; }
    public string MetodoPago { get; set; } = "";
    public string EstadoFactura { get; set; } = "";

    // Datos de la reserva y del huésped.
    public int IdReserva { get; set; }
    public string Huesped { get; set; } = "";
    public string DocumentoHuesped { get; set; } = "";

    // Datos de la habitación.
    public string NumeroHabitacion { get; set; } = "";
    public string TipoHabitacion { get; set; } = "";

    // Fechas utilizadas para el cálculo del alojamiento.
    public DateTime FechaIngreso { get; set; }
    public DateTime FechaSalida { get; set; }

    // Cantidad de noches cobradas según la reserva.
    public int DiasEstadia { get; set; }

    // Precio de una noche almacenado en DETALLE_RESERVA.
    public decimal PrecioNoche { get; set; }

    // Total correspondiente únicamente al alojamiento.
    public decimal TotalAlojamiento { get; set; }

    // Suma de todos los consumos registrados.
    public decimal TotalConsumos { get; set; }

    // Subtotal antes del descuento.
    public decimal Subtotal { get; set; }

    // Descuento aplicado durante el check-out.
    public decimal Descuento { get; set; }

    // Total final pagado.
    public decimal Total { get; set; }

    // Cada consumo se almacena por separado para mostrarlo como una línea.
    public List<FacturaConsumoDetalle> Consumos { get; set; } = new();
}

// Modelo utilizado en la pantalla de Check-out ANTES de generar la factura.
// Permite que el cajero revise habitación, noches y consumos antes de cobrar.
public class ResumenCheckout
{
    public int IdReserva { get; set; }
    public string Huesped { get; set; } = "";
    public string NumeroHabitacion { get; set; } = "";
    public string TipoHabitacion { get; set; } = "";

    public DateTime FechaIngreso { get; set; }
    public DateTime FechaSalida { get; set; }

    public int DiasEstadia { get; set; }
    public decimal PrecioNoche { get; set; }
    public decimal TotalAlojamiento { get; set; }
    public decimal TotalConsumos { get; set; }

    // Subtotal = alojamiento + consumos.
    public decimal Subtotal { get; set; }

    // Lista de consumos que se mostrará antes de cobrar.
    public List<FacturaConsumoDetalle> Consumos { get; set; } = new();
}
