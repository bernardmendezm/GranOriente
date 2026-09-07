using Microsoft.Data.SqlClient;

namespace GranOriente.Datos;

// Esta clase contiene las consultas utilizadas por el panel principal.
public class DashboardDatos
{
    private readonly Db _db;

    // Recibe la clase Db para poder conectarse a SQL Server.
    public DashboardDatos(Db db)
    {
        _db = db;
    }

    // Devuelve cuántas habitaciones están marcadas como DISPONIBLE.
    public async Task<int> HabitacionesDisponiblesAsync()
    {
        await using var cn = _db.CrearConexion();
        await cn.OpenAsync();

        const string sql = @"
            SELECT COUNT(*)
            FROM HABITACION h
            INNER JOIN ESTADO_HABITACION e
                ON e.id_estado_habitacion = h.id_estado_habitacion
            WHERE e.estado = 'DISPONIBLE'";

        await using var cmd = new SqlCommand(sql, cn);

        // ExecuteScalar devuelve un solo valor.
        return Convert.ToInt32(await cmd.ExecuteScalarAsync());
    }

    // Cuenta reservas que todavía están activas.
    public async Task<int> ReservasActivasAsync()
    {
        await using var cn = _db.CrearConexion();
        await cn.OpenAsync();

        const string sql = @"
            SELECT COUNT(*)
            FROM RESERVA
            WHERE estado IN ('RESERVADA','CHECKIN')";

        await using var cmd = new SqlCommand(sql, cn);

        return Convert.ToInt32(await cmd.ExecuteScalarAsync());
    }

    // Cuenta los check-in registrados en la fecha actual.
    public async Task<int> CheckInHoyAsync()
    {
        await using var cn = _db.CrearConexion();
        await cn.OpenAsync();

        const string sql = @"
            SELECT COUNT(*)
            FROM CHECK_IN_OUT
            WHERE CAST(fecha_hora_check_in AS DATE)
                  = CAST(GETDATE() AS DATE)";

        await using var cmd = new SqlCommand(sql, cn);

        return Convert.ToInt32(await cmd.ExecuteScalarAsync());
    }

    // Cuenta las facturas emitidas hoy.
    public async Task<int> FacturasHoyAsync()
    {
        await using var cn = _db.CrearConexion();
        await cn.OpenAsync();

        const string sql = @"
            SELECT COUNT(*)
            FROM FACTURA
            WHERE CAST(fecha_emision AS DATE)
                  = CAST(GETDATE() AS DATE)";

        await using var cmd = new SqlCommand(sql, cn);

        return Convert.ToInt32(await cmd.ExecuteScalarAsync());
    }
}
