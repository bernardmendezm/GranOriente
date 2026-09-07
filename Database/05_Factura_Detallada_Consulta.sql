USE GranOrienteDb;
GO

/*
    Este script NO modifica las tablas.

    La versión 7 obtiene el detalle de la factura desde C# mediante JOIN.

    Este procedimiento se incluye como apoyo académico para mostrar
    cómo también podría centralizarse la cabecera de la factura en SQL Server.
*/

CREATE OR ALTER PROCEDURE dbo.sp_ObtenerFacturaDetallada
    @id_factura INT
AS
BEGIN
    SET NOCOUNT ON;

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
        DATEDIFF(
            DAY,
            r.fecha_ingreso,
            r.fecha_salida
        ) AS dias_estadia
    FROM FACTURA f
    INNER JOIN CHECK_IN_OUT cio
        ON cio.id_check_in = f.id_check_out
    INNER JOIN RESERVA r
        ON r.id_reserva = cio.id_reserva
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
    WHERE f.id_factura = @id_factura;

    /*
        Segundo conjunto de resultados:
        cada consumo aparece por separado.
    */
    SELECT
        s.nombre_servicio,
        c.cantidad,
        c.precio_unitario,
        c.cantidad * c.precio_unitario AS importe,
        c.fecha_hora,
        c.observaciones
    FROM CONSUMO c
    INNER JOIN SERVICIO s
        ON s.id_servicio = c.id_servicio
    INNER JOIN CHECK_IN_OUT cio
        ON cio.id_reserva = c.id_reserva
    INNER JOIN FACTURA f
        ON f.id_check_out = cio.id_check_in
    WHERE f.id_factura = @id_factura
    ORDER BY
        c.fecha_hora,
        c.id_consumo;
END;
GO
