// ARCHIVO COMENTADO PARA ESTUDIO
// Capa Datos para reportes.
// Ejecuta el procedimiento almacenado de ocupación por fecha.

using GranOriente.Entidades;
using Microsoft.Data.SqlClient;
using System.Data;

namespace GranOriente.Datos;

public class ReporteDatos
{
    private readonly Db _db;
    public ReporteDatos(Db db)=>_db=db;

    // Genera los datos del reporte de ocupación.

    // Consulta el reporte de ocupación para una fecha determinada.

    public async Task<List<ReporteOcupacion>> OcupacionAsync(DateTime fecha)
    {
        var l=new List<ReporteOcupacion>();
        await using var cn=_db.CrearConexion(); await cn.OpenAsync();
        await using var cmd=new SqlCommand("sp_ReporteOcupacion",cn){CommandType=CommandType.StoredProcedure};
        cmd.Parameters.AddWithValue("@fecha",fecha.Date);
        await using var rd=await cmd.ExecuteReaderAsync();
        while(await rd.ReadAsync())
            l.Add(new ReporteOcupacion{NumeroHabitacion=rd["numero_habitacion"].ToString()??"",
                TipoHabitacion=rd["tipo_hab"].ToString()??"",Estado=rd["estado"].ToString()??"",
                Ocupada=Convert.ToInt32(rd["ocupada"])});
        return l;
    }
}
