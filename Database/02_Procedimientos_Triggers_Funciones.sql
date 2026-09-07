USE GranOriente;
GO

CREATE OR ALTER FUNCTION dbo.fn_PrecioNoche(@id_habitacion INT,@fecha DATE)
RETURNS DECIMAL(12,2)
AS
BEGIN
 DECLARE @precio DECIMAL(12,2);
 SELECT TOP 1 @precio=t.precio_noche
 FROM HABITACION h
 JOIN TARIFA t ON t.id_tipo_habitacion=h.id_tipo_habitacion AND t.estado='ACTIVO'
 JOIN TEMPORADA te ON te.id_temporada=t.id_temporada AND te.estado='ACTIVO'
 WHERE h.id_habitacion=@id_habitacion AND @fecha BETWEEN te.fecha_inicio AND te.fecha_fin
 ORDER BY te.fecha_inicio DESC;
 RETURN ISNULL(@precio,0);
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_CrearReserva
 @id_huesped INT,
 @id_habitacion INT,
 @fecha_ingreso DATE,
 @fecha_salida DATE,
 @cantidad_persona INT,
 @tipo_reserva VARCHAR(20),
 @id_empleado INT,
 @id_agencia INT=NULL,
 @observaciones VARCHAR(300)=NULL,
 @id_reserva INT OUTPUT
AS
BEGIN
 SET NOCOUNT ON;
 SET XACT_ABORT ON;
 SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;

 IF @fecha_salida<=@fecha_ingreso THROW 51001,'La salida debe ser posterior al ingreso.',1;

 BEGIN TRAN;
 IF EXISTS(
   SELECT 1 FROM RESERVA r JOIN DETALLE_RESERVA d ON d.id_reserva=r.id_reserva
   WHERE d.id_habitacion=@id_habitacion
     AND r.estado IN('RESERVADA','CHECKIN')
     AND @fecha_ingreso<r.fecha_salida AND @fecha_salida>r.fecha_ingreso
 )
 BEGIN
   ROLLBACK;
   THROW 51002,'La habitación ya está reservada para ese rango de fechas.',1;
 END;

 DECLARE @capacidad INT=(SELECT capacidad FROM HABITACION WHERE id_habitacion=@id_habitacion);
 IF @capacidad IS NULL BEGIN ROLLBACK; THROW 51003,'Habitación inexistente.',1; END;
 IF @cantidad_persona>@capacidad BEGIN ROLLBACK; THROW 51004,'La cantidad de personas supera la capacidad.',1; END;

 DECLARE @id_temporada INT=(
   SELECT TOP 1 id_temporada FROM TEMPORADA
   WHERE estado='ACTIVO' AND @fecha_ingreso BETWEEN fecha_inicio AND fecha_fin
   ORDER BY fecha_inicio DESC
 );
 IF @id_temporada IS NULL BEGIN ROLLBACK; THROW 51005,'No existe temporada para la fecha de ingreso.',1; END;

 DECLARE @precio DECIMAL(12,2)=dbo.fn_PrecioNoche(@id_habitacion,@fecha_ingreso);
 IF @precio=0 BEGIN ROLLBACK; THROW 51006,'No existe tarifa para la habitación y temporada.',1; END;

 DECLARE @dias INT=DATEDIFF(DAY,@fecha_ingreso,@fecha_salida);
 DECLARE @total DECIMAL(12,2)=@dias*@precio;
 DECLARE @id_tarifa INT=(
    SELECT TOP 1 t.id_tarifa FROM HABITACION h
    JOIN TARIFA t ON t.id_tipo_habitacion=h.id_tipo_habitacion
    WHERE h.id_habitacion=@id_habitacion AND t.id_temporada=@id_temporada AND t.estado='ACTIVO'
 );

 INSERT INTO RESERVA(id_temporada,fecha_ingreso,fecha_salida,estado,observaciones,precio_reserva,id_huesped,tiempo_limite,id_agencia,tipo_reserva,id_empleados)
 VALUES(@id_temporada,@fecha_ingreso,@fecha_salida,'RESERVADA',@observaciones,@total,@id_huesped,DATEADD(HOUR,24,GETDATE()),@id_agencia,@tipo_reserva,@id_empleado);

 SET @id_reserva=SCOPE_IDENTITY();

 INSERT INTO DETALLE_RESERVA(id_reserva,id_habitacion,cantidad_persona,id_tarifa,precio_noche)
 VALUES(@id_reserva,@id_habitacion,@cantidad_persona,@id_tarifa,@precio);

 COMMIT;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_RegistrarCheckIn
 @id_reserva INT,@id_empleado INT
AS
BEGIN
 SET NOCOUNT ON; SET XACT_ABORT ON;
 BEGIN TRAN;
 IF NOT EXISTS(SELECT 1 FROM RESERVA WHERE id_reserva=@id_reserva AND estado='RESERVADA')
 BEGIN ROLLBACK; THROW 51010,'La reserva no está disponible para check-in.',1; END;

 INSERT INTO CHECK_IN_OUT(id_reserva,fecha_hora_check_in,observaciones,id_empleado_cajero)
 VALUES(@id_reserva,GETDATE(),'Check-in registrado',@id_empleado);

 UPDATE RESERVA SET estado='CHECKIN' WHERE id_reserva=@id_reserva;
 COMMIT;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_RegistrarConsumo
 @id_reserva INT,@id_servicio INT,@cantidad INT,@observaciones VARCHAR(200)=NULL
AS
BEGIN
 SET NOCOUNT ON;
 IF NOT EXISTS(SELECT 1 FROM RESERVA WHERE id_reserva=@id_reserva AND estado='CHECKIN')
   THROW 51020,'Solo se pueden registrar consumos a reservas con check-in.',1;

 DECLARE @precio DECIMAL(12,2)=(SELECT precio FROM SERVICIO WHERE id_servicio=@id_servicio);
 IF @precio IS NULL THROW 51021,'Servicio inexistente.',1;

 INSERT INTO CONSUMO(id_reserva,id_servicio,cantidad,observaciones,precio_unitario)
 VALUES(@id_reserva,@id_servicio,@cantidad,@observaciones,@precio);
END;
GO

IF OBJECT_ID('dbo.SeqFactura','SO') IS NULL EXEC('CREATE SEQUENCE dbo.SeqFactura AS INT START WITH 1 INCREMENT BY 1');
GO

CREATE OR ALTER PROCEDURE dbo.sp_CheckOut_GenerarFactura
 @id_reserva INT,
 @id_empleado_cajero INT,
 @metodo_pago VARCHAR(30),
 @descuento DECIMAL(12,2)=0,
 @id_factura INT OUTPUT
AS
BEGIN
 SET NOCOUNT ON; SET XACT_ABORT ON;
 BEGIN TRAN;

 DECLARE @id_check INT=(SELECT id_check_in FROM CHECK_IN_OUT WHERE id_reserva=@id_reserva);
 IF @id_check IS NULL BEGIN ROLLBACK; THROW 51030,'La reserva no tiene check-in.',1; END;
 IF EXISTS(SELECT 1 FROM CHECK_IN_OUT WHERE id_check_in=@id_check AND fecha_hora_check_out IS NOT NULL)
 BEGIN ROLLBACK; THROW 51031,'La reserva ya tiene check-out.',1; END;

 DECLARE @alojamiento DECIMAL(12,2)=(SELECT precio_reserva FROM RESERVA WHERE id_reserva=@id_reserva);
 DECLARE @consumos DECIMAL(12,2)=ISNULL((SELECT SUM(cantidad*precio_unitario) FROM CONSUMO WHERE id_reserva=@id_reserva),0);
 DECLARE @subtotal DECIMAL(12,2)=@alojamiento+@consumos;
 IF @descuento<0 OR @descuento>@subtotal BEGIN ROLLBACK; THROW 51032,'Descuento inválido.',1; END;
 DECLARE @total DECIMAL(12,2)=@subtotal-@descuento;

 UPDATE CHECK_IN_OUT SET fecha_hora_check_out=GETDATE(),id_empleado_cajero=@id_empleado_cajero
 WHERE id_check_in=@id_check;
 UPDATE RESERVA SET estado='FINALIZADA' WHERE id_reserva=@id_reserva;

 DECLARE @numero VARCHAR(30)=CONCAT('FAC-',FORMAT(GETDATE(),'yyyy'),'-',RIGHT('000000'+CAST(NEXT VALUE FOR dbo.SeqFactura AS VARCHAR(10)),6));
 INSERT INTO FACTURA(id_check_out,descuento,observaciones,estado_factura,numero_factura,subtotal,total,metodo_pago)
 VALUES(@id_check,@descuento,'Generada automáticamente en check-out','EMITIDA',@numero,@subtotal,@total,@metodo_pago);
 SET @id_factura=SCOPE_IDENTITY();

 COMMIT;
END;
GO


CREATE OR ALTER PROCEDURE dbo.sp_ReporteOcupacion @fecha DATE
AS
BEGIN
 SELECT h.numero_habitacion,th.tipo_hab,eh.estado,
   CASE WHEN EXISTS(
      SELECT 1 FROM DETALLE_RESERVA d JOIN RESERVA r ON r.id_reserva=d.id_reserva
      WHERE d.id_habitacion=h.id_habitacion AND r.estado IN('RESERVADA','CHECKIN')
      AND @fecha>=r.fecha_ingreso AND @fecha<r.fecha_salida
   ) THEN 1 ELSE 0 END AS ocupada
 FROM HABITACION h
 JOIN TIPO_HABITACION th ON th.id_tipo_habitacion=h.id_tipo_habitacion
 JOIN ESTADO_HABITACION eh ON eh.id_estado_habitacion=h.id_estado_habitacion
 ORDER BY h.numero_habitacion;
END;
GO

CREATE OR ALTER TRIGGER dbo.tr_Reserva_Bitacora ON dbo.RESERVA
AFTER INSERT,UPDATE,DELETE
AS
BEGIN
 SET NOCOUNT ON;
 INSERT INTO BITACORA(id_usuario,tabla_afectada,descripcion,ip_origen,accion)
 SELECT NULL,'RESERVA',
        CONCAT('Reserva ',COALESCE(CAST(i.id_reserva AS VARCHAR),CAST(d.id_reserva AS VARCHAR))),
        NULL,
        CASE WHEN i.id_reserva IS NOT NULL AND d.id_reserva IS NULL THEN 'INSERT'
             WHEN i.id_reserva IS NOT NULL AND d.id_reserva IS NOT NULL THEN 'UPDATE'
             ELSE 'DELETE' END
 FROM inserted i FULL OUTER JOIN deleted d ON d.id_reserva=i.id_reserva;
END;
GO

CREATE OR ALTER TRIGGER dbo.tr_Consumo_Bitacora ON dbo.CONSUMO
AFTER INSERT,UPDATE,DELETE
AS
BEGIN
 SET NOCOUNT ON;
 INSERT INTO BITACORA(id_usuario,tabla_afectada,descripcion,ip_origen,accion)
 SELECT NULL,'CONSUMO',
        CONCAT('Consumo ',COALESCE(CAST(i.id_consumo AS VARCHAR),CAST(d.id_consumo AS VARCHAR))),
        NULL,
        CASE WHEN i.id_consumo IS NOT NULL AND d.id_consumo IS NULL THEN 'INSERT'
             WHEN i.id_consumo IS NOT NULL AND d.id_consumo IS NOT NULL THEN 'UPDATE'
             ELSE 'DELETE' END
 FROM inserted i FULL OUTER JOIN deleted d ON d.id_consumo=i.id_consumo;
END;
GO

CREATE OR ALTER TRIGGER dbo.tr_Factura_Bitacora ON dbo.FACTURA
AFTER INSERT,UPDATE,DELETE
AS
BEGIN
 SET NOCOUNT ON;
 INSERT INTO BITACORA(id_usuario,tabla_afectada,descripcion,ip_origen,accion)
 SELECT NULL,'FACTURA',
        CONCAT('Factura ',COALESCE(CAST(i.id_factura AS VARCHAR),CAST(d.id_factura AS VARCHAR))),
        NULL,
        CASE WHEN i.id_factura IS NOT NULL AND d.id_factura IS NULL THEN 'INSERT'
             WHEN i.id_factura IS NOT NULL AND d.id_factura IS NOT NULL THEN 'UPDATE'
             ELSE 'DELETE' END
 FROM inserted i FULL OUTER JOIN deleted d ON d.id_factura=i.id_factura;
END;
GO
