using GranOriente.Entidades;
using Microsoft.Data.SqlClient;
using System.Data;

namespace GranOriente.Datos;

// Esta clase administra todas las operaciones de base de datos relacionadas con reservas.
public class ReservaDatos
{
    private readonly Db _db;

    // Constructor: recibe la clase Db desde la capa de negocio.
    public ReservaDatos(Db db)
    {
        _db = db;
    }

    // Devuelve todas las reservas del sistema.
    public async Task<List<Reserva>> ListarAsync()
    {
        // Creamos una lista vacía.
        // Cada fila obtenida desde SQL Server se convertirá en un objeto Reserva.
        var lista = new List<Reserva>();

        // Creamos una conexión utilizando la clase Db.
        await using var cn = _db.CrearConexion();

        // Abrimos la conexión de forma asíncrona.
        await cn.OpenAsync();

        // Consulta SQL que relaciona:
        // RESERVA -> HUESPED -> PERSONA
        // RESERVA -> DETALLE_RESERVA -> HABITACION
        // De esa manera podemos mostrar nombre del huésped y número de habitación.
        const string sql = @"
            SELECT
                r.id_reserva,
                r.fecha_reserva,
                r.fecha_ingreso,
                r.fecha_salida,
                r.estado,
                r.precio_reserva,
                r.tipo_reserva,
                p.nombre + ' ' + p.apellido_paterno AS huesped,
                h.numero_habitacion,
                dr.id_habitacion,
                dr.cantidad_persona,
                dr.id_tarifa,
                dr.precio_noche
            FROM RESERVA r
            INNER JOIN HUESPED hu
                ON hu.id_huesped = r.id_huesped
            INNER JOIN PERSONA p
                ON p.id_persona = hu.id_huesped
            INNER JOIN DETALLE_RESERVA dr
                ON dr.id_reserva = r.id_reserva
            INNER JOIN HABITACION h
                ON h.id_habitacion = dr.id_habitacion
            ORDER BY r.id_reserva DESC";

        // Creamos el comando que ejecutará la consulta.
        await using var cmd =
            new SqlCommand(sql, cn);

        // Ejecutamos la consulta y obtenemos un lector de filas.
        await using var rd =
            await cmd.ExecuteReaderAsync();

        // Recorremos una a una las filas retornadas.
        while (await rd.ReadAsync())
        {
            // Transformamos la fila actual en un objeto Reserva.
            lista.Add(new Reserva
            {
                // Convertimos el id de SQL Server a int.
                IdReserva =
                    (int)rd["id_reserva"],

                // Recuperamos la fecha en que fue creada la reserva.
                FechaReserva =
                    (DateTime)rd["fecha_reserva"],

                // Recuperamos la fecha de ingreso.
                FechaIngreso =
                    (DateTime)rd["fecha_ingreso"],

                // Recuperamos la fecha de salida.
                FechaSalida =
                    (DateTime)rd["fecha_salida"],

                // Recuperamos el estado actual.
                Estado =
                    rd["estado"].ToString() ?? "",

                // Recuperamos el precio total calculado.
                PrecioReserva =
                    (decimal)rd["precio_reserva"],

                // Recuperamos el tipo de reserva.
                TipoReserva =
                    rd["tipo_reserva"].ToString() ?? "",

                // Nombre completo del huésped generado en la consulta.
                Huesped =
                    rd["huesped"].ToString(),

                // Número de habitación mostrado al usuario.
                NumeroHabitacion =
                    rd["numero_habitacion"].ToString(),

                // Identificador interno de la habitación.
                IdHabitacion =
                    (int)rd["id_habitacion"],

                // Cantidad de personas de la reserva.
                CantidadPersona =
                    (int)rd["cantidad_persona"],

                // Tarifa aplicada.
                IdTarifa =
                    (int)rd["id_tarifa"],

                // Precio por noche almacenado en el detalle.
                PrecioNoche =
                    (decimal)rd["precio_noche"]
            });
        }

        // Devolvemos la lista terminada.
        return lista;
    }

    // Crea una nueva reserva utilizando el procedimiento almacenado sp_CrearReserva.
    public async Task<int> CrearAsync(Reserva r)
    {
        // Creamos la conexión.
        await using var cn =
            _db.CrearConexion();

        // Abrimos la conexión.
        await cn.OpenAsync();

        // Indicamos que ejecutaremos un procedimiento almacenado
        // en lugar de escribir un INSERT directamente.
        await using var cmd =
            new SqlCommand(
                "sp_CrearReserva",
                cn
            );

        // CommandType.StoredProcedure le dice a ADO.NET
        // que "sp_CrearReserva" es un procedimiento almacenado.
        cmd.CommandType =
            CommandType.StoredProcedure;

        // Enviamos al procedimiento los datos de la reserva.
        cmd.Parameters.AddWithValue(
            "@huesped",
            r.IdHuesped
        );

        cmd.Parameters.AddWithValue(
            "@habitacion",
            r.IdHabitacion
        );

        cmd.Parameters.AddWithValue(
            "@entrada",
            r.FechaIngreso
        );

        cmd.Parameters.AddWithValue(
            "@salida",
            r.FechaSalida
        );

        cmd.Parameters.AddWithValue(
            "@personas",
            r.CantidadPersona
        );

        cmd.Parameters.AddWithValue(
            "@tipo",
            r.TipoReserva
        );

        // Agencia puede ser nula.
        cmd.Parameters.AddWithValue(
            "@agencia",
            (object?)r.IdAgencia ?? DBNull.Value
        );

        // Guardamos qué empleado realizó la reserva.
        cmd.Parameters.AddWithValue(
            "@empleado",
            r.IdEmpleado
        );

        // Ejecutamos el procedimiento.
        // ExecuteScalarAsync devuelve el id de la reserva recién creada.
        var resultado =
            await cmd.ExecuteScalarAsync();

        // Convertimos el resultado a entero y lo devolvemos.
        return Convert.ToInt32(resultado);
    }

    // Registra el check-in de una reserva existente.
    public async Task CheckInAsync(
        int idReserva,
        int idEmpleado)
    {
        // Creamos y abrimos la conexión.
        await using var cn =
            _db.CrearConexion();

        await cn.OpenAsync();

        // Creamos un comando para ejecutar el procedimiento sp_RegistrarCheckIn.
        await using var cmd =
            new SqlCommand(
                "sp_RegistrarCheckIn",
                cn
            );

        // Indicamos que es un procedimiento almacenado.
        cmd.CommandType =
            CommandType.StoredProcedure;

        // Enviamos el identificador de la reserva.
        cmd.Parameters.AddWithValue(
            "@reserva",
            idReserva
        );

        // Enviamos el empleado que está realizando el check-in.
        cmd.Parameters.AddWithValue(
            "@empleado",
            idEmpleado
        );

        // Ejecutamos el procedimiento.
        // ExecuteNonQueryAsync se utiliza porque no necesitamos
        // una tabla de resultados como respuesta.
        await cmd.ExecuteNonQueryAsync();
    }
}
