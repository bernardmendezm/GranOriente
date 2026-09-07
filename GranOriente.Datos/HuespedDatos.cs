using GranOriente.Entidades;
using Microsoft.Data.SqlClient;
using System.Data;

namespace GranOriente.Datos;

// Esta clase pertenece a la CAPA DE DATOS.
// Su responsabilidad es comunicarse directamente con SQL Server
// para obtener y guardar información relacionada con huéspedes.
public class HuespedDatos
{
    // Guardamos una referencia a la clase Db.
    // Db sabe cómo crear conexiones hacia SQL Server.
    private readonly Db _db;

    // Constructor de la clase.
    // Recibe la conexión configurada desde la capa de negocio.
    public HuespedDatos(Db db)
    {
        _db = db;
    }

    // Este método devuelve una lista completa de huéspedes.
    // "async" indica que el método puede ejecutar tareas sin bloquear la aplicación.
    // "Task<List<Huesped>>" significa que al finalizar devolverá una lista de objetos Huesped.
    public async Task<List<Huesped>> ListarAsync()
    {
        // Creamos una lista vacía.
        // Aquí iremos agregando cada huésped leído desde la base de datos.
        var lista = new List<Huesped>();

        // Creamos una nueva conexión utilizando la clase Db.
        // "await using" garantiza que la conexión será liberada automáticamente
        // cuando termine el método, incluso si ocurre un error.
        await using var cn = _db.CrearConexion();

        // Abrimos físicamente la conexión con SQL Server.
        await cn.OpenAsync();

        // Preparamos la consulta SQL.
        // JOIN permite relacionar HUESPED con PERSONA
        // porque los datos personales se encuentran en PERSONA.
        const string sql = @"
            SELECT
                h.id_huesped,
                h.fecha_registro,
                p.*
            FROM HUESPED h
            INNER JOIN PERSONA p
                ON p.id_persona = h.id_huesped
            ORDER BY
                p.nombre,
                p.apellido_paterno";

        // SqlCommand representa la instrucción SQL que se ejecutará.
        // Le pasamos la consulta y la conexión abierta.
        await using var cmd = new SqlCommand(sql, cn);

        // Ejecutamos la consulta con ExecuteReaderAsync.
        // Este método devuelve un lector de filas llamado SqlDataReader.
        await using var rd = await cmd.ExecuteReaderAsync();

        // ReadAsync avanza una fila por vez.
        // Mientras existan filas, el ciclo continuará ejecutándose.
        while (await rd.ReadAsync())
        {
            // Convertimos la fila actual de SQL Server en un objeto Huesped.
            lista.Add(new Huesped
            {
                // Leemos el identificador del huésped.
                IdHuesped =
                    rd.GetInt32(
                        rd.GetOrdinal("id_huesped")
                    ),

                // Leemos la fecha en que se registró el huésped.
                FechaRegistro =
                    rd.GetDateTime(
                        rd.GetOrdinal("fecha_registro")
                    ),

                // Creamos el objeto Persona asociado al huésped.
                Persona = new Persona
                {
                    // Identificador de la persona.
                    IdPersona =
                        rd.GetInt32(
                            rd.GetOrdinal("id_persona")
                        ),

                    // ToString convierte el valor leído a texto.
                    // "??" evita que quede null.
                    Nombre =
                        rd["nombre"].ToString() ?? "",

                    ApellidoPaterno =
                        rd["apellido_paterno"].ToString() ?? "",

                    ApellidoMaterno =
                        rd["apellido_materno"].ToString() ?? "",

                    TipoDocumento =
                        rd["tipo_documento"].ToString() ?? "",

                    NumeroDocumento =
                        rd["numero_documento"].ToString() ?? "",

                    // Si la columna contiene DBNull.Value significa que
                    // en SQL Server no existe un dato almacenado.
                    Telefono =
                        rd["telefono"] == DBNull.Value
                            ? null
                            : rd["telefono"].ToString(),

                    Email =
                        rd["email"] == DBNull.Value
                            ? null
                            : rd["email"].ToString(),

                    FechaNacimiento =
                        rd["fecha_nac"] == DBNull.Value
                            ? null
                            : Convert.ToDateTime(
                                rd["fecha_nac"]
                            ),

                    Sexo =
                        rd["sexo"] == DBNull.Value
                            ? null
                            : rd["sexo"].ToString(),

                    Nacionalidad =
                        rd["nacionalidad"] == DBNull.Value
                            ? null
                            : rd["nacionalidad"].ToString()
                }
            });
        }

        // Al terminar el ciclo devolvemos la lista completa.
        return lista;
    }

    // Este método registra una nueva persona y luego la convierte en huésped.
    // Devuelve el id generado para la persona.
    public async Task<int> CrearAsync(Persona p)
    {
        // Creamos y abrimos la conexión.
        await using var cn = _db.CrearConexion();
        await cn.OpenAsync();

        // Iniciamos una transacción.
        // Esto permite que PERSONA y HUESPED se guarden como una sola operación.
        await using var tx = await cn.BeginTransactionAsync();

        try
        {
            // Consulta que inserta los datos personales.
            // SCOPE_IDENTITY devuelve el id generado automáticamente.
            const string sqlPersona = @"
                INSERT INTO PERSONA
                (
                    nombre,
                    apellido_paterno,
                    apellido_materno,
                    tipo_documento,
                    numero_documento,
                    telefono,
                    email,
                    fecha_nac,
                    sexo,
                    nacionalidad
                )
                VALUES
                (
                    @n,
                    @ap,
                    @am,
                    @td,
                    @nd,
                    @t,
                    @e,
                    @fn,
                    @s,
                    @na
                );

                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            // Creamos el comando SQL dentro de la transacción actual.
            await using var cmd =
                new SqlCommand(
                    sqlPersona,
                    cn,
                    (SqlTransaction)tx
                );

            // Los parámetros evitan concatenar texto directamente en SQL.
            // Esto ayuda a prevenir errores e inyección SQL.
            cmd.Parameters.AddWithValue("@n", p.Nombre);
            cmd.Parameters.AddWithValue("@ap", p.ApellidoPaterno);
            cmd.Parameters.AddWithValue("@am", p.ApellidoMaterno);
            cmd.Parameters.AddWithValue("@td", p.TipoDocumento);
            cmd.Parameters.AddWithValue("@nd", p.NumeroDocumento);

            // Si el valor es null enviamos DBNull.Value a SQL Server.
            cmd.Parameters.AddWithValue(
                "@t",
                (object?)p.Telefono ?? DBNull.Value
            );

            cmd.Parameters.AddWithValue(
                "@e",
                (object?)p.Email ?? DBNull.Value
            );

            cmd.Parameters.AddWithValue(
                "@fn",
                (object?)p.FechaNacimiento ?? DBNull.Value
            );

            cmd.Parameters.AddWithValue(
                "@s",
                (object?)p.Sexo ?? DBNull.Value
            );

            cmd.Parameters.AddWithValue(
                "@na",
                (object?)p.Nacionalidad ?? DBNull.Value
            );

            // ExecuteScalarAsync devuelve un único valor.
            // En este caso devuelve el id generado en PERSONA.
            int id =
                (int)(await cmd.ExecuteScalarAsync())!;

            // Ahora registramos ese mismo id dentro de HUESPED.
            await using var cmd2 =
                new SqlCommand(
                    "INSERT INTO HUESPED(id_huesped,fecha_registro) VALUES(@id,GETDATE())",
                    cn,
                    (SqlTransaction)tx
                );

            // Asignamos el id generado al parámetro @id.
            cmd2.Parameters.AddWithValue("@id", id);

            // Ejecutamos el INSERT de HUESPED.
            await cmd2.ExecuteNonQueryAsync();

            // Si todo salió bien confirmamos la transacción.
            await tx.CommitAsync();

            // Devolvemos el id generado.
            return id;
        }
        catch
        {
            // Si ocurre cualquier error deshacemos todos los INSERT realizados.
            await tx.RollbackAsync();

            // "throw" vuelve a lanzar el error para que otra capa pueda manejarlo.
            throw;
        }
    }
}
