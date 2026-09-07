// ARCHIVO COMENTADO PARA ESTUDIO
// Capa Datos para autenticación.
// Valida el usuario y compara la contraseña mediante SHA-256.

using GranOriente.Entidades;
using Microsoft.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;

namespace GranOriente.Datos;

public class UsuarioDatos
{
    private readonly Db _db;
    public UsuarioDatos(Db db)=>_db=db;

    // Valida las credenciales de acceso.

    // Busca al usuario, valida su contraseña y devuelve los datos necesarios para crear la sesión.

    public async Task<UsuarioSesion?> LoginAsync(string usuario,string password)
    {
        string hash=Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(password)));
        await using var cn=_db.CrearConexion(); await cn.OpenAsync();
        const string sql=@"SELECT u.id_usuario,u.nombre_usuario,u.id_persona,r.nombre
            FROM USUARIO u JOIN ROL r ON r.id_rol=u.id_rol
            WHERE u.nombre_usuario=@u AND u.password=@p AND u.estado='ACTIVO'";
        await using var cmd=new SqlCommand(sql,cn);
        cmd.Parameters.AddWithValue("@u",usuario); cmd.Parameters.AddWithValue("@p",hash);
        await using var rd=await cmd.ExecuteReaderAsync();
        if(!await rd.ReadAsync()) return null;
        return new UsuarioSesion{IdUsuario=(int)rd["id_usuario"],NombreUsuario=rd["nombre_usuario"].ToString()??"",
            IdPersona=(int)rd["id_persona"],Rol=rd["nombre"].ToString()??""};
    }
}
