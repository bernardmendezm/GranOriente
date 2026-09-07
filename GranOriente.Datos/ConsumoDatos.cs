// ARCHIVO COMENTADO PARA ESTUDIO
// Capa Datos para consumos.
// Permite listar servicios, registrar consumos y consultar consumos de una reserva.

using GranOriente.Entidades;
using Microsoft.Data.SqlClient;
using System.Data;

namespace GranOriente.Datos;

public class ConsumoDatos
{
    private readonly Db _db;
    public ConsumoDatos(Db db) => _db = db;

    // Obtiene los servicios adicionales del hotel.

    // Obtiene desde SQL Server el catálogo de servicios disponibles para poder mostrarlos en el formulario de consumos.

    public async Task<List<Servicio>> ListarServiciosAsync()
    {
        var l = new List<Servicio>();
        await using var cn = _db.CrearConexion(); await cn.OpenAsync();
        await using var cmd = new SqlCommand("SELECT id_servicio,nombre_servicio,precio FROM SERVICIO ORDER BY nombre_servicio", cn);
        await using var rd = await cmd.ExecuteReaderAsync();
        while(await rd.ReadAsync())
            l.Add(new Servicio{IdServicio=(int)rd["id_servicio"],NombreServicio=rd["nombre_servicio"].ToString()??"",Precio=(decimal)rd["precio"]});
        return l;
    }

    // Registra una operación relacionada con este módulo.

    // Registra un nuevo consumo ejecutando la operación correspondiente en la base de datos.

    public async Task RegistrarAsync(int idReserva,int idServicio,int cantidad,string? observaciones)
    {
        await using var cn = _db.CrearConexion(); await cn.OpenAsync();
        await using var cmd = new SqlCommand("sp_RegistrarConsumo",cn){CommandType=CommandType.StoredProcedure};
        cmd.Parameters.AddWithValue("@id_reserva",idReserva);
        cmd.Parameters.AddWithValue("@id_servicio",idServicio);
        cmd.Parameters.AddWithValue("@cantidad",cantidad);
        cmd.Parameters.AddWithValue("@observaciones",(object?)observaciones??DBNull.Value);
        await cmd.ExecuteNonQueryAsync();
    }

    // Obtiene los registros asociados a una reserva.

    // Obtiene todos los consumos que pertenecen a una reserva específica.

    public async Task<List<Consumo>> ListarPorReservaAsync(int idReserva)
    {
        var l=new List<Consumo>();
        await using var cn=_db.CrearConexion(); await cn.OpenAsync();
        const string sql=@"SELECT c.*,s.nombre_servicio FROM CONSUMO c JOIN SERVICIO s ON s.id_servicio=c.id_servicio WHERE c.id_reserva=@id ORDER BY c.fecha_hora DESC";
        await using var cmd=new SqlCommand(sql,cn); cmd.Parameters.AddWithValue("@id",idReserva);
        await using var rd=await cmd.ExecuteReaderAsync();
        while(await rd.ReadAsync())
            l.Add(new Consumo{IdConsumo=(int)rd["id_consumo"],IdReserva=(int)rd["id_reserva"],IdServicio=(int)rd["id_servicio"],
                Cantidad=(int)rd["cantidad"],FechaHora=(DateTime)rd["fecha_hora"],Observaciones=rd["observaciones"] as string,
                PrecioUnitario=(decimal)rd["precio_unitario"],Servicio=rd["nombre_servicio"].ToString()});
        return l;
    }
}
