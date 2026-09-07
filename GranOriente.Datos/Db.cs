using Microsoft.Data.SqlClient;

namespace GranOriente.Datos;

// Esta clase centraliza la creación de conexiones hacia SQL Server.
public class Db
{
    // Guarda la cadena de conexión recibida desde appsettings.json.
    private readonly string _connectionString;

    // Constructor: recibe la cadena de conexión.
    public Db(string connectionString)
    {
        _connectionString = connectionString;
    }

    // Crea una nueva conexión SQL.
    // La conexión todavía no se abre aquí.
    public SqlConnection CrearConexion()
    {
        return new SqlConnection(_connectionString);
    }
}
