using GranOriente.Entidades;
using Microsoft.Data.SqlClient;

namespace GranOriente.Datos;

// Esta clase contiene todas las operaciones SQL relacionadas con habitaciones.
public class HabitacionDatos
{
    private readonly Db _db;

    // Constructor: recibe el objeto Db que permite crear conexiones.
    public HabitacionDatos(Db db)
    {
        _db = db;
    }

    // Busca habitaciones disponibles entre una fecha de ingreso y una fecha de salida.
    public async Task<List<Habitacion>> ListarDisponiblesAsync(
        DateTime ingreso,
        DateTime salida)
    {
        // Lista donde guardaremos las habitaciones encontradas.
        var lista = new List<Habitacion>();

        // Creamos y abrimos la conexión.
        await using var cn = _db.CrearConexion();
        await cn.OpenAsync();

        // La consulta:
        // 1. Busca habitaciones cuyo estado físico sea DISPONIBLE.
        // 2. Excluye habitaciones que tengan una reserva que se cruce con las fechas solicitadas.
        const string sql = @"
            SELECT
                h.id_habitacion,
                h.id_tipo_habitacion,
                h.id_estado_habitacion,
                h.capacidad,
                h.descripcion,
                h.numero_habitacion,
                h.piso,
                th.tipo_hab,
                eh.estado
            FROM HABITACION h
            INNER JOIN TIPO_HABITACION th
                ON th.id_tipo_habitacion = h.id_tipo_habitacion
            INNER JOIN ESTADO_HABITACION eh
                ON eh.id_estado_habitacion = h.id_estado_habitacion
            WHERE eh.estado = 'DISPONIBLE'
            AND NOT EXISTS
            (
                SELECT 1
                FROM DETALLE_RESERVA dr
                INNER JOIN RESERVA r
                    ON r.id_reserva = dr.id_reserva
                WHERE dr.id_habitacion = h.id_habitacion
                  AND r.estado IN ('RESERVADA','CHECKIN')
                  AND @ingreso < r.fecha_salida
                  AND @salida > r.fecha_ingreso
            )
            ORDER BY h.numero_habitacion";

        // Creamos el comando SQL.
        await using var cmd =
            new SqlCommand(sql, cn);

        // Enviamos las fechas como parámetros.
        cmd.Parameters.AddWithValue(
            "@ingreso",
            ingreso
        );

        cmd.Parameters.AddWithValue(
            "@salida",
            salida
        );

        // Ejecutamos la consulta.
        await using var rd =
            await cmd.ExecuteReaderAsync();

        // Recorremos cada fila devuelta.
        while (await rd.ReadAsync())
        {
            // Convertimos la fila en un objeto Habitacion.
            lista.Add(new Habitacion
            {
                IdHabitacion =
                    (int)rd["id_habitacion"],

                IdTipoHabitacion =
                    (int)rd["id_tipo_habitacion"],

                IdEstadoHabitacion =
                    (int)rd["id_estado_habitacion"],

                Capacidad =
                    (int)rd["capacidad"],

                Descripcion =
                    rd["descripcion"].ToString() ?? "",

                NumeroHabitacion =
                    rd["numero_habitacion"].ToString() ?? "",

                Piso =
                    (int)rd["piso"],

                TipoHabitacion =
                    rd["tipo_hab"].ToString() ?? "",

                EstadoHabitacion =
                    rd["estado"].ToString() ?? ""
            });
        }

        // Devolvemos las habitaciones disponibles.
        return lista;
    }

    // Lista todas las habitaciones registradas.
    public async Task<List<Habitacion>> ListarAsync()
    {
        var lista = new List<Habitacion>();

        await using var cn = _db.CrearConexion();
        await cn.OpenAsync();

        // JOIN permite mostrar el nombre del tipo y el estado.
        const string sql = @"
            SELECT
                h.*,
                th.tipo_hab,
                eh.estado
            FROM HABITACION h
            INNER JOIN TIPO_HABITACION th
                ON th.id_tipo_habitacion = h.id_tipo_habitacion
            INNER JOIN ESTADO_HABITACION eh
                ON eh.id_estado_habitacion = h.id_estado_habitacion
            ORDER BY h.numero_habitacion";

        await using var cmd =
            new SqlCommand(sql, cn);

        await using var rd =
            await cmd.ExecuteReaderAsync();

        while (await rd.ReadAsync())
        {
            lista.Add(new Habitacion
            {
                IdHabitacion =
                    (int)rd["id_habitacion"],

                IdTipoHabitacion =
                    (int)rd["id_tipo_habitacion"],

                IdEstadoHabitacion =
                    (int)rd["id_estado_habitacion"],

                Capacidad =
                    (int)rd["capacidad"],

                Descripcion =
                    rd["descripcion"].ToString() ?? "",

                NumeroHabitacion =
                    rd["numero_habitacion"].ToString() ?? "",

                Piso =
                    (int)rd["piso"],

                TipoHabitacion =
                    rd["tipo_hab"].ToString() ?? "",

                EstadoHabitacion =
                    rd["estado"].ToString() ?? ""
            });
        }

        return lista;
    }

    // Registra una nueva habitación.
    public async Task<int> CrearAsync(Habitacion h)
    {
        await using var cn = _db.CrearConexion();
        await cn.OpenAsync();

        // INSERT de la nueva habitación.
        // Después devuelve el id generado.
        const string sql = @"
            INSERT INTO HABITACION
            (
                id_tipo_habitacion,
                id_estado_habitacion,
                capacidad,
                descripcion,
                numero_habitacion,
                piso
            )
            VALUES
            (
                @tipo,
                @estado,
                @capacidad,
                @descripcion,
                @numero,
                @piso
            );

            SELECT CAST(SCOPE_IDENTITY() AS INT);";

        await using var cmd =
            new SqlCommand(sql, cn);

        // Enviamos cada propiedad del objeto como parámetro SQL.
        cmd.Parameters.AddWithValue(
            "@tipo",
            h.IdTipoHabitacion
        );

        cmd.Parameters.AddWithValue(
            "@estado",
            h.IdEstadoHabitacion
        );

        cmd.Parameters.AddWithValue(
            "@capacidad",
            h.Capacidad
        );

        cmd.Parameters.AddWithValue(
            "@descripcion",
            (object?)h.Descripcion ?? DBNull.Value
        );

        cmd.Parameters.AddWithValue(
            "@numero",
            h.NumeroHabitacion
        );

        cmd.Parameters.AddWithValue(
            "@piso",
            h.Piso
        );

        // ExecuteScalar devuelve el id creado.
        return
            (int)(await cmd.ExecuteScalarAsync())!;
    }

    // Lista los tipos de habitación para llenar un <select> en la vista.
    public async Task<List<TipoHabitacion>> ListarTiposAsync()
    {
        var lista =
            new List<TipoHabitacion>();

        await using var cn =
            _db.CrearConexion();

        await cn.OpenAsync();

        const string sql = @"
            SELECT
                id_tipo_habitacion,
                tipo_hab
            FROM TIPO_HABITACION
            ORDER BY tipo_hab";

        await using var cmd =
            new SqlCommand(sql, cn);

        await using var rd =
            await cmd.ExecuteReaderAsync();

        while (await rd.ReadAsync())
        {
            lista.Add(new TipoHabitacion
            {
                IdTipoHabitacion =
                    (int)rd["id_tipo_habitacion"],

                Tipo =
                    rd["tipo_hab"].ToString() ?? ""
            });
        }

        return lista;
    }

    // Lista los estados posibles de una habitación.
    public async Task<List<EstadoHabitacion>> ListarEstadosAsync()
    {
        var lista =
            new List<EstadoHabitacion>();

        await using var cn =
            _db.CrearConexion();

        await cn.OpenAsync();

        const string sql = @"
            SELECT
                id_estado_habitacion,
                estado
            FROM ESTADO_HABITACION
            ORDER BY estado";

        await using var cmd =
            new SqlCommand(sql, cn);

        await using var rd =
            await cmd.ExecuteReaderAsync();

        while (await rd.ReadAsync())
        {
            lista.Add(new EstadoHabitacion
            {
                IdEstadoHabitacion =
                    (int)rd["id_estado_habitacion"],

                Estado =
                    rd["estado"].ToString() ?? ""
            });
        }

        return lista;
    }
}
