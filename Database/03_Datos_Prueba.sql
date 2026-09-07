USE GranOriente;
GO
INSERT INTO ESTADO_HABITACION(estado,descripcion) VALUES
('DISPONIBLE','Puede ser reservada'),('MANTENIMIENTO','Fuera de servicio');

INSERT INTO TIPO_HABITACION(tipo_hab,descripcion,id_estado_habitacion) VALUES
('INDIVIDUAL','Habitación para una persona',1),
('FAMILIAR SIMPLE','Habitación familiar',1),
('SUITE DE LUJO','Suite con servicios premium',1);

INSERT INTO HABITACION(id_tipo_habitacion,id_estado_habitacion,capacidad,descripcion,numero_habitacion,piso) VALUES
(1,1,1,'Individual estándar','101',1),
(2,1,4,'Familiar simple','201',2),
(3,1,2,'Suite de lujo','301',3),
(2,2,4,'En mantenimiento','202',2);

INSERT INTO TEMPORADA(nombre_temporada,descripcion,fecha_inicio,fecha_fin) VALUES
('TEMPORADA 2026','Temporada académica de prueba','2026-01-01','2026-12-31');

INSERT INTO TARIFA(id_tipo_habitacion,id_temporada,precio_noche,descripcion) VALUES
(1,1,180,'Tarifa individual'),
(2,1,320,'Tarifa familiar'),
(3,1,550,'Tarifa suite');

INSERT INTO SERVICIO(nombre_servicio,precio) VALUES
('Restaurante',80),('Spa',150),('Salón de eventos',500);

INSERT INTO ROL(nombre,descripcion) VALUES
('ADMINISTRADOR','Acceso total'),
('RECEPCIONISTA','Huéspedes, habitaciones y reservas'),
('CAJERO','Check-out y facturación');

-- Empleado administrador
INSERT INTO PERSONA(nombre,apellido_paterno,apellido_materno,tipo_documento,numero_documento,telefono,email)
VALUES('Admin','Sistema','','CI','1000000','70000000','admin@granoriente.local');
DECLARE @idAdmin INT=SCOPE_IDENTITY();
INSERT INTO EMPLEADOS(id_empleados,cargo,fecha_ingreso) VALUES(@idAdmin,'Administrador',GETDATE());

-- Password: Admin123 (SHA-256)
INSERT INTO USUARIO(id_persona,id_rol,nombre_usuario,password,estado)
SELECT @idAdmin,id_rol,'admin',CONVERT(VARCHAR(64),HASHBYTES('SHA2_256','Admin123'),2),'ACTIVO'
FROM ROL WHERE nombre='ADMINISTRADOR';

-- Huésped de prueba
INSERT INTO PERSONA(nombre,apellido_paterno,apellido_materno,tipo_documento,numero_documento,telefono,email)
VALUES('Juan','Pérez','López','CI','5555555','71000000','juan@test.com');
DECLARE @idH INT=SCOPE_IDENTITY();
INSERT INTO HUESPED(id_huesped) VALUES(@idH);
GO
