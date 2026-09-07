// ARCHIVO COMENTADO PARA ESTUDIO
// Capa Datos para administración de funcionarios y usuarios.
// Registra PERSONA -> EMPLEADOS -> USUARIO dentro de una única transacción.

using GranOriente.Entidades;
using Microsoft.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;

namespace GranOriente.Datos;

public class FuncionarioUsuarioDatos
{
    private readonly Db _db;

    public FuncionarioUsuarioDatos(Db db)
    {
        _db = db;
    }

    // Lista los registros necesarios para mostrarlos en la vista.

    // Lista funcionarios, usuarios y roles uniendo PERSONA, EMPLEADOS, USUARIO y ROL.

    public async Task<List<FuncionarioUsuario>> ListarAsync()
    {
        var lista = new List<FuncionarioUsuario>();

        await using var cn = _db.CrearConexion();
        await cn.OpenAsync();

        const string sql = @"
            SELECT
                p.id_persona,
                p.nombre,
                p.apellido_paterno,
                p.apellido_materno,
                p.tipo_documento,
                p.numero_documento,
                p.telefono,
                p.email,
                p.fecha_nac,
                p.sexo,
                p.nacionalidad,
                e.cargo,
                e.fecha_ingreso,
                u.nombre_usuario,
                u.estado,
                r.nombre AS rol
            FROM EMPLEADOS e
            INNER JOIN PERSONA p
                ON p.id_persona = e.id_empleados
            INNER JOIN USUARIO u
                ON u.id_persona = p.id_persona
            INNER JOIN ROL r
                ON r.id_rol = u.id_rol
            ORDER BY p.nombre, p.apellido_paterno;";

        await using var cmd = new SqlCommand(sql, cn);
        await using var rd = await cmd.ExecuteReaderAsync();

        while (await rd.ReadAsync())
        {
            lista.Add(new FuncionarioUsuario
            {
                IdPersona = Convert.ToInt32(rd["id_persona"]),
                Nombre = rd["nombre"].ToString() ?? "",
                ApellidoPaterno = rd["apellido_paterno"].ToString() ?? "",
                ApellidoMaterno = rd["apellido_materno"].ToString() ?? "",
                TipoDocumento = rd["tipo_documento"].ToString() ?? "",
                NumeroDocumento = rd["numero_documento"].ToString() ?? "",
                Telefono = rd["telefono"] == DBNull.Value ? null : rd["telefono"].ToString(),
                Email = rd["email"] == DBNull.Value ? null : rd["email"].ToString(),
                FechaNacimiento = rd["fecha_nac"] == DBNull.Value
                    ? null
                    : Convert.ToDateTime(rd["fecha_nac"]),
                Sexo = rd["sexo"] == DBNull.Value ? null : rd["sexo"].ToString(),
                Nacionalidad = rd["nacionalidad"] == DBNull.Value ? null : rd["nacionalidad"].ToString(),
                Cargo = rd["cargo"].ToString() ?? "",
                FechaIngreso = Convert.ToDateTime(rd["fecha_ingreso"]),
                NombreUsuario = rd["nombre_usuario"].ToString() ?? "",
                Estado = rd["estado"].ToString() ?? "",
                Rol = rd["rol"].ToString() ?? ""
            });
        }

        return lista;
    }

    // Obtiene los roles activos del sistema.

    // Obtiene los roles activos para llenar el selector de roles.

    public async Task<List<RolSistema>> ListarRolesAsync()
    {
        var lista = new List<RolSistema>();

        await using var cn = _db.CrearConexion();
        await cn.OpenAsync();

        const string sql = @"
            SELECT id_rol, nombre
            FROM ROL
            WHERE estado = 'ACTIVO'
            ORDER BY nombre;";

        await using var cmd = new SqlCommand(sql, cn);
        await using var rd = await cmd.ExecuteReaderAsync();

        while (await rd.ReadAsync())
        {
            lista.Add(new RolSistema
            {
                IdRol = Convert.ToInt32(rd["id_rol"]),
                Nombre = rd["nombre"].ToString() ?? ""
            });
        }

        return lista;
    }

    // Crea un nuevo registro en la base de datos.

    // Crea PERSONA, EMPLEADO y USUARIO dentro de una única transacción para evitar registros incompletos.

    public async Task<int> CrearAsync(FuncionarioUsuario f)
    {
        await using var cn = _db.CrearConexion();
        await cn.OpenAsync();
        await using var tx = await cn.BeginTransactionAsync();

        try
        {
            // Evita documentos repetidos.
            const string sqlDocumento = @"
                SELECT COUNT(*)
                FROM PERSONA
                WHERE numero_documento = @documento;";

            await using (var cmdDoc = new SqlCommand(sqlDocumento, cn, (SqlTransaction)tx))
            {
                cmdDoc.Parameters.AddWithValue("@documento", f.NumeroDocumento);

                var cantidad = Convert.ToInt32(await cmdDoc.ExecuteScalarAsync());

                if (cantidad > 0)
                    throw new Exception("Ya existe una persona registrada con ese número de documento.");
            }

            // Evita nombres de usuario repetidos.
            const string sqlUsuario = @"
                SELECT COUNT(*)
                FROM USUARIO
                WHERE nombre_usuario = @usuario;";

            await using (var cmdUsuario = new SqlCommand(sqlUsuario, cn, (SqlTransaction)tx))
            {
                cmdUsuario.Parameters.AddWithValue("@usuario", f.NombreUsuario);

                var cantidad = Convert.ToInt32(await cmdUsuario.ExecuteScalarAsync());

                if (cantidad > 0)
                    throw new Exception("El nombre de usuario ya se encuentra registrado.");
            }

            // 1. PERSONA: exactamente los mismos datos personales que se usan para HUESPED.
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
                    @nombre,
                    @apellidoPaterno,
                    @apellidoMaterno,
                    @tipoDocumento,
                    @numeroDocumento,
                    @telefono,
                    @email,
                    @fechaNacimiento,
                    @sexo,
                    @nacionalidad
                );

                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            int idPersona;

            await using (var cmdPersona = new SqlCommand(sqlPersona, cn, (SqlTransaction)tx))
            {
                cmdPersona.Parameters.AddWithValue("@nombre", f.Nombre);
                cmdPersona.Parameters.AddWithValue("@apellidoPaterno", f.ApellidoPaterno);
                cmdPersona.Parameters.AddWithValue("@apellidoMaterno", f.ApellidoMaterno);
                cmdPersona.Parameters.AddWithValue("@tipoDocumento", f.TipoDocumento);
                cmdPersona.Parameters.AddWithValue("@numeroDocumento", f.NumeroDocumento);
                cmdPersona.Parameters.AddWithValue("@telefono", (object?)f.Telefono ?? DBNull.Value);
                cmdPersona.Parameters.AddWithValue("@email", (object?)f.Email ?? DBNull.Value);
                cmdPersona.Parameters.AddWithValue("@fechaNacimiento", (object?)f.FechaNacimiento ?? DBNull.Value);
                cmdPersona.Parameters.AddWithValue("@sexo", (object?)f.Sexo ?? DBNull.Value);
                cmdPersona.Parameters.AddWithValue("@nacionalidad", (object?)f.Nacionalidad ?? DBNull.Value);

                idPersona = Convert.ToInt32(await cmdPersona.ExecuteScalarAsync());
            }

            // 2. EMPLEADOS.
            const string sqlEmpleado = @"
                INSERT INTO EMPLEADOS
                (
                    id_empleados,
                    cargo,
                    fecha_ingreso
                )
                VALUES
                (
                    @idPersona,
                    @cargo,
                    @fechaIngreso
                );";

            await using (var cmdEmpleado = new SqlCommand(sqlEmpleado, cn, (SqlTransaction)tx))
            {
                cmdEmpleado.Parameters.AddWithValue("@idPersona", idPersona);
                cmdEmpleado.Parameters.AddWithValue("@cargo", f.Cargo);
                cmdEmpleado.Parameters.AddWithValue("@fechaIngreso", f.FechaIngreso.Date);

                await cmdEmpleado.ExecuteNonQueryAsync();
            }

            // 3. USUARIO.
            // Se mantiene SHA-256 para ser coherente con LoginAsync del proyecto académico.
            string passwordHash = Convert.ToHexString(
                SHA256.HashData(
                    Encoding.UTF8.GetBytes(f.Password)
                )
            );

            const string sqlCrearUsuario = @"
                INSERT INTO USUARIO
                (
                    id_persona,
                    id_rol,
                    nombre_usuario,
                    password,
                    estado
                )
                VALUES
                (
                    @idPersona,
                    @idRol,
                    @usuario,
                    @password,
                    'ACTIVO'
                );";

            await using (var cmdCrearUsuario = new SqlCommand(sqlCrearUsuario, cn, (SqlTransaction)tx))
            {
                cmdCrearUsuario.Parameters.AddWithValue("@idPersona", idPersona);
                cmdCrearUsuario.Parameters.AddWithValue("@idRol", f.IdRol);
                cmdCrearUsuario.Parameters.AddWithValue("@usuario", f.NombreUsuario);
                cmdCrearUsuario.Parameters.AddWithValue("@password", passwordHash);

                await cmdCrearUsuario.ExecuteNonQueryAsync();
            }

            await tx.CommitAsync();

            return idPersona;
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }
}
