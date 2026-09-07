// CORRECCIÓN RAZOR V8
// Esta clase mantiene la misma lógica de la versión 7.
// La corrección principal de esta versión está en las vistas Razor,
// donde ahora todas las llamadas ToString(...) se escriben dentro de @(...) 
// para evitar que Razor las muestre como texto literal.

using GranOriente.Entidades;
using Microsoft.Data.SqlClient;
using System.Data;

namespace GranOriente.Datos;

// Esta clase contiene todas las operaciones relacionadas con
// check-out, cobro y consulta de facturas.
public class FacturacionDatos
{
    // Objeto que permite crear conexiones hacia SQL Server.
    private readonly Db _db;

    // Constructor de la clase.
    public FacturacionDatos(Db db)
    {
        _db = db;
    }

    // Este método realiza el check-out.
    // Ejecuta el procedimiento almacenado que:
    // 1. Cierra el CHECK_IN_OUT.
    // 2. Cambia la reserva a FINALIZADA.
    // 3. Calcula alojamiento + consumos.
    // 4. Aplica el descuento.
    // 5. Inserta la FACTURA.
    // 6. Devuelve el id de la factura generada.
    public async Task<int> CheckOutAsync(
        int idReserva,
        int idEmpleado,
        string metodoPago,
        decimal descuento)
    {
        // Creamos una conexión hacia SQL Server.
        await using var cn = _db.CrearConexion();

        // Abrimos la conexión.
        await cn.OpenAsync();

        // Creamos el comando que ejecutará el procedimiento almacenado.
        await using var cmd =
            new SqlCommand(
                "sp_CheckOut_GenerarFactura",
                cn
            )
            {
                // Indicamos que el texto anterior es el nombre
                // de un procedimiento almacenado.
                CommandType = CommandType.StoredProcedure
            };

        // Enviamos el id de la reserva que será cerrada.
        cmd.Parameters.AddWithValue(
            "@id_reserva",
            idReserva
        );

        // Guardamos quién realizó el cobro.
        cmd.Parameters.AddWithValue(
            "@id_empleado_cajero",
            idEmpleado
        );

        // Enviamos el método de pago seleccionado.
        cmd.Parameters.AddWithValue(
            "@metodo_pago",
            metodoPago
        );

        // Enviamos el descuento ingresado.
        cmd.Parameters.AddWithValue(
            "@descuento",
            descuento
        );

        // Este parámetro es OUTPUT.
        // SQL Server escribirá aquí el id de la nueva factura.
        var parametroFactura =
            new SqlParameter(
                "@id_factura",
                SqlDbType.Int
            )
            {
                Direction =
                    ParameterDirection.Output
            };

        // Agregamos el parámetro de salida al comando.
        cmd.Parameters.Add(parametroFactura);

        // Ejecutamos el procedimiento.
        await cmd.ExecuteNonQueryAsync();

        // Devolvemos el id de factura generado por SQL Server.
        return (int)parametroFactura.Value;
    }

    // Obtiene un resumen de la reserva ANTES de realizar el cobro.
    // Se utiliza en la pantalla CheckOut.cshtml.
    public async Task<ResumenCheckout?> ObtenerResumenCheckoutAsync(
        int idReserva)
    {
        // Creamos y abrimos la conexión.
        await using var cn = _db.CrearConexion();
        await cn.OpenAsync();

        // Esta consulta reúne los datos principales de la reserva.
        // DATEDIFF calcula cuántas noches existen entre ingreso y salida.
        // precio_reserva es el valor total del alojamiento calculado al reservar.
        const string sqlCabecera = @"
            SELECT
                r.id_reserva,
                CONCAT(
                    p.nombre,
                    ' ',
                    p.apellido_paterno,
                    CASE
                        WHEN ISNULL(p.apellido_materno,'') = ''
                            THEN ''
                        ELSE ' ' + p.apellido_materno
                    END
                ) AS huesped,
                h.numero_habitacion,
                th.tipo_hab,
                r.fecha_ingreso,
                r.fecha_salida,
                CASE
                    WHEN DATEDIFF(DAY,r.fecha_ingreso,r.fecha_salida) <= 0
                        THEN 1
                    ELSE DATEDIFF(DAY,r.fecha_ingreso,r.fecha_salida)
                END AS dias_estadia,
                dr.precio_noche,
                r.precio_reserva AS total_alojamiento,
                ISNULL(
                    (
                        SELECT SUM(c.cantidad * c.precio_unitario)
                        FROM CONSUMO c
                        WHERE c.id_reserva = r.id_reserva
                    ),
                    0
                ) AS total_consumos
            FROM RESERVA r
            INNER JOIN HUESPED hu
                ON hu.id_huesped = r.id_huesped
            INNER JOIN PERSONA p
                ON p.id_persona = hu.id_huesped
            INNER JOIN DETALLE_RESERVA dr
                ON dr.id_reserva = r.id_reserva
            INNER JOIN HABITACION h
                ON h.id_habitacion = dr.id_habitacion
            INNER JOIN TIPO_HABITACION th
                ON th.id_tipo_habitacion = h.id_tipo_habitacion
            WHERE r.id_reserva = @id
              AND r.estado = 'CHECKIN';";

        // Creamos el comando para la cabecera.
        await using var cmd =
            new SqlCommand(
                sqlCabecera,
                cn
            );

        // Asignamos el id de reserva al parámetro @id.
        cmd.Parameters.AddWithValue(
            "@id",
            idReserva
        );

        // Ejecutamos la consulta.
        await using var rd =
            await cmd.ExecuteReaderAsync();

        // Si no existe la reserva o no está en CHECKIN,
        // devolvemos null.
        if (!await rd.ReadAsync())
            return null;

        // Construimos el objeto ResumenCheckout.
        var resumen = new ResumenCheckout
        {
            IdReserva =
                (int)rd["id_reserva"],

            Huesped =
                rd["huesped"].ToString() ?? "",

            NumeroHabitacion =
                rd["numero_habitacion"].ToString() ?? "",

            TipoHabitacion =
                rd["tipo_hab"].ToString() ?? "",

            FechaIngreso =
                (DateTime)rd["fecha_ingreso"],

            FechaSalida =
                (DateTime)rd["fecha_salida"],

            DiasEstadia =
                Convert.ToInt32(
                    rd["dias_estadia"]
                ),

            PrecioNoche =
                (decimal)rd["precio_noche"],

            TotalAlojamiento =
                (decimal)rd["total_alojamiento"],

            TotalConsumos =
                (decimal)rd["total_consumos"]
        };

        // Calculamos el subtotal de manera clara en C#.
        resumen.Subtotal =
            resumen.TotalAlojamiento +
            resumen.TotalConsumos;

        // Cerramos el DataReader antes de ejecutar una segunda consulta
        // utilizando la misma conexión.
        await rd.CloseAsync();

        // Ahora buscamos cada consumo por separado.
        resumen.Consumos =
            await ObtenerConsumosReservaAsync(
                cn,
                idReserva
            );

        return resumen;
    }

    // Obtiene la factura completa DESPUÉS del check-out.
    // Incluye:
    // - huésped
    // - habitación
    // - tipo de habitación
    // - fechas
    // - cantidad de noches
    // - precio por noche
    // - total de alojamiento
    // - consumos separados
    // - subtotal
    // - descuento
    // - total final
    public async Task<FacturaDetalle?> ObtenerDetalleAsync(
        int idFactura)
    {
        // Creamos y abrimos la conexión.
        await using var cn =
            _db.CrearConexion();

        await cn.OpenAsync();

        // Relacionamos FACTURA con CHECK_IN_OUT y RESERVA.
        // Desde RESERVA llegamos hasta el huésped y la habitación.
        const string sqlCabecera = @"
            SELECT
                f.id_factura,
                f.numero_factura,
                f.fecha_emision,
                f.metodo_pago,
                f.estado_factura,
                f.subtotal,
                f.descuento,
                f.total,

                r.id_reserva,
                r.fecha_ingreso,
                r.fecha_salida,
                r.precio_reserva AS total_alojamiento,

                CONCAT(
                    p.nombre,
                    ' ',
                    p.apellido_paterno,
                    CASE
                        WHEN ISNULL(p.apellido_materno,'') = ''
                            THEN ''
                        ELSE ' ' + p.apellido_materno
                    END
                ) AS huesped,

                CONCAT(
                    p.tipo_documento,
                    ' ',
                    p.numero_documento
                ) AS documento_huesped,

                h.numero_habitacion,
                th.tipo_hab,
                dr.precio_noche,

                CASE
                    WHEN DATEDIFF(
                        DAY,
                        r.fecha_ingreso,
                        r.fecha_salida
                    ) <= 0
                        THEN 1
                    ELSE DATEDIFF(
                        DAY,
                        r.fecha_ingreso,
                        r.fecha_salida
                    )
                END AS dias_estadia,

                ISNULL(
                    (
                        SELECT
                            SUM(
                                c.cantidad *
                                c.precio_unitario
                            )
                        FROM CONSUMO c
                        WHERE c.id_reserva =
                              r.id_reserva
                    ),
                    0
                ) AS total_consumos

            FROM FACTURA f

            INNER JOIN CHECK_IN_OUT cio
                ON cio.id_check_in =
                   f.id_check_out

            INNER JOIN RESERVA r
                ON r.id_reserva =
                   cio.id_reserva

            INNER JOIN HUESPED hu
                ON hu.id_huesped =
                   r.id_huesped

            INNER JOIN PERSONA p
                ON p.id_persona =
                   hu.id_huesped

            INNER JOIN DETALLE_RESERVA dr
                ON dr.id_reserva =
                   r.id_reserva

            INNER JOIN HABITACION h
                ON h.id_habitacion =
                   dr.id_habitacion

            INNER JOIN TIPO_HABITACION th
                ON th.id_tipo_habitacion =
                   h.id_tipo_habitacion

            WHERE f.id_factura = @id;";

        // Creamos el comando SQL.
        await using var cmd =
            new SqlCommand(
                sqlCabecera,
                cn
            );

        // Enviamos el id de factura.
        cmd.Parameters.AddWithValue(
            "@id",
            idFactura
        );

        // Ejecutamos la consulta.
        await using var rd =
            await cmd.ExecuteReaderAsync();

        // Si no se encuentra la factura,
        // devolvemos null.
        if (!await rd.ReadAsync())
            return null;

        // Convertimos la fila en un objeto FacturaDetalle.
        var factura = new FacturaDetalle
        {
            IdFactura =
                (int)rd["id_factura"],

            NumeroFactura =
                rd["numero_factura"].ToString() ?? "",

            FechaEmision =
                (DateTime)rd["fecha_emision"],

            MetodoPago =
                rd["metodo_pago"].ToString() ?? "",

            EstadoFactura =
                rd["estado_factura"].ToString() ?? "",

            IdReserva =
                (int)rd["id_reserva"],

            Huesped =
                rd["huesped"].ToString() ?? "",

            DocumentoHuesped =
                rd["documento_huesped"].ToString() ?? "",

            NumeroHabitacion =
                rd["numero_habitacion"].ToString() ?? "",

            TipoHabitacion =
                rd["tipo_hab"].ToString() ?? "",

            FechaIngreso =
                (DateTime)rd["fecha_ingreso"],

            FechaSalida =
                (DateTime)rd["fecha_salida"],

            DiasEstadia =
                Convert.ToInt32(
                    rd["dias_estadia"]
                ),

            PrecioNoche =
                (decimal)rd["precio_noche"],

            TotalAlojamiento =
                (decimal)rd["total_alojamiento"],

            TotalConsumos =
                (decimal)rd["total_consumos"],

            Subtotal =
                (decimal)rd["subtotal"],

            Descuento =
                (decimal)rd["descuento"],

            Total =
                (decimal)rd["total"]
        };

        // Cerramos el lector para ejecutar otra consulta.
        await rd.CloseAsync();

        // Recuperamos los consumos y los agregamos a la factura.
        factura.Consumos =
            await ObtenerConsumosReservaAsync(
                cn,
                factura.IdReserva
            );

        return factura;
    }

    // Método privado reutilizable.
    // Recibe una conexión ya abierta y obtiene cada consumo
    // registrado para una reserva.
    private static async Task<List<FacturaConsumoDetalle>>
        ObtenerConsumosReservaAsync(
            SqlConnection cn,
            int idReserva)
    {
        // Lista donde almacenaremos las líneas de consumo.
        var consumos =
            new List<FacturaConsumoDetalle>();

        // Esta consulta NO agrupa los consumos.
        // Cada registro aparece por separado en la factura.
        const string sql = @"
            SELECT
                s.nombre_servicio,
                c.cantidad,
                c.precio_unitario,
                c.cantidad * c.precio_unitario
                    AS importe,
                c.fecha_hora,
                c.observaciones
            FROM CONSUMO c
            INNER JOIN SERVICIO s
                ON s.id_servicio =
                   c.id_servicio
            WHERE c.id_reserva = @id
            ORDER BY
                c.fecha_hora,
                c.id_consumo;";

        // Creamos el comando.
        await using var cmd =
            new SqlCommand(
                sql,
                cn
            );

        // Enviamos el id de la reserva.
        cmd.Parameters.AddWithValue(
            "@id",
            idReserva
        );

        // Ejecutamos la consulta.
        await using var rd =
            await cmd.ExecuteReaderAsync();

        // Recorremos cada consumo.
        while (await rd.ReadAsync())
        {
            // Agregamos una nueva línea al detalle.
            consumos.Add(
                new FacturaConsumoDetalle
                {
                    Servicio =
                        rd["nombre_servicio"]
                            .ToString() ?? "",

                    Cantidad =
                        (int)rd["cantidad"],

                    PrecioUnitario =
                        (decimal)rd["precio_unitario"],

                    Importe =
                        (decimal)rd["importe"],

                    FechaHora =
                        (DateTime)rd["fecha_hora"],

                    Observaciones =
                        rd["observaciones"]
                            == DBNull.Value
                                ? null
                                : rd["observaciones"]
                                    .ToString()
                }
            );
        }

        return consumos;
    }
}
