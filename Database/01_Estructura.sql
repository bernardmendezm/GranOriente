CREATE DATABASE GranOriente;
GO
USE GranOriente;
GO

CREATE TABLE PERSONA(
 id_persona INT IDENTITY PRIMARY KEY,
 nombre VARCHAR(80) NOT NULL,
 apellido_paterno VARCHAR(80) NOT NULL DEFAULT '',
 apellido_materno VARCHAR(80) NOT NULL DEFAULT '',
 tipo_documento VARCHAR(20) NOT NULL,
 numero_documento VARCHAR(30) NOT NULL UNIQUE,
 telefono VARCHAR(30) NULL,
 email VARCHAR(120) NULL,
 fecha_nac DATE NULL,
 sexo VARCHAR(20) NULL,
 nacionalidad VARCHAR(50) NULL
);

CREATE TABLE HUESPED(
 id_huesped INT PRIMARY KEY,
 fecha_registro DATETIME NOT NULL DEFAULT GETDATE(),
 CONSTRAINT FK_HUESPED_PERSONA FOREIGN KEY(id_huesped) REFERENCES PERSONA(id_persona)
);

CREATE TABLE EMPLEADOS(
 id_empleados INT PRIMARY KEY,
 cargo VARCHAR(50) NOT NULL,
 fecha_ingreso DATE NOT NULL,
 CONSTRAINT FK_EMPLEADO_PERSONA FOREIGN KEY(id_empleados) REFERENCES PERSONA(id_persona)
);

CREATE TABLE ROL(
 id_rol INT IDENTITY PRIMARY KEY,
 nombre VARCHAR(30) NOT NULL UNIQUE,
 descripcion VARCHAR(150) NULL,
 estado VARCHAR(20) NOT NULL DEFAULT 'ACTIVO'
);

CREATE TABLE USUARIO(
 id_usuario INT IDENTITY PRIMARY KEY,
 id_persona INT NOT NULL,
 id_rol INT NOT NULL,
 nombre_usuario VARCHAR(50) NOT NULL UNIQUE,
 password VARCHAR(64) NOT NULL,
 estado VARCHAR(20) NOT NULL DEFAULT 'ACTIVO',
 ultimo_acceso DATETIME NULL,
 FOREIGN KEY(id_persona) REFERENCES PERSONA(id_persona),
 FOREIGN KEY(id_rol) REFERENCES ROL(id_rol)
);

CREATE TABLE BITACORA(
 id_bitacora BIGINT IDENTITY PRIMARY KEY,
 id_usuario INT NULL,
 tabla_afectada VARCHAR(80) NOT NULL,
 descripcion VARCHAR(300) NULL,
 ip_origen VARCHAR(45) NULL,
 accion VARCHAR(20) NOT NULL,
 fecha_hora DATETIME NOT NULL DEFAULT GETDATE(),
 FOREIGN KEY(id_usuario) REFERENCES USUARIO(id_usuario)
);

CREATE TABLE ESTADO_HABITACION(
 id_estado_habitacion INT IDENTITY PRIMARY KEY,
 estado VARCHAR(30) NOT NULL UNIQUE,
 descripcion VARCHAR(120) NULL
);

CREATE TABLE TIPO_HABITACION(
 id_tipo_habitacion INT IDENTITY PRIMARY KEY,
 tipo_hab VARCHAR(60) NOT NULL UNIQUE,
 descripcion VARCHAR(150) NULL,
 id_estado_habitacion INT NULL,
 FOREIGN KEY(id_estado_habitacion) REFERENCES ESTADO_HABITACION(id_estado_habitacion)
);

CREATE TABLE HABITACION(
 id_habitacion INT IDENTITY PRIMARY KEY,
 id_tipo_habitacion INT NOT NULL,
 id_estado_habitacion INT NOT NULL,
 capacidad INT NOT NULL CHECK(capacidad>0),
 descripcion VARCHAR(150) NULL,
 numero_habitacion VARCHAR(15) NOT NULL UNIQUE,
 piso INT NOT NULL,
 FOREIGN KEY(id_tipo_habitacion) REFERENCES TIPO_HABITACION(id_tipo_habitacion),
 FOREIGN KEY(id_estado_habitacion) REFERENCES ESTADO_HABITACION(id_estado_habitacion)
);

CREATE TABLE TEMPORADA(
 id_temporada INT IDENTITY PRIMARY KEY,
 nombre_temporada VARCHAR(60) NOT NULL,
 descripcion VARCHAR(150) NULL,
 fecha_inicio DATE NOT NULL,
 fecha_fin DATE NOT NULL,
 estado VARCHAR(20) NOT NULL DEFAULT 'ACTIVO',
 CHECK(fecha_fin>=fecha_inicio)
);

CREATE TABLE TARIFA(
 id_tarifa INT IDENTITY PRIMARY KEY,
 id_tipo_habitacion INT NOT NULL,
 id_temporada INT NOT NULL,
 precio_noche DECIMAL(12,2) NOT NULL CHECK(precio_noche>=0),
 descripcion VARCHAR(150) NULL,
 estado VARCHAR(20) NOT NULL DEFAULT 'ACTIVO',
 FOREIGN KEY(id_tipo_habitacion) REFERENCES TIPO_HABITACION(id_tipo_habitacion),
 FOREIGN KEY(id_temporada) REFERENCES TEMPORADA(id_temporada),
 CONSTRAINT UQ_TARIFA UNIQUE(id_tipo_habitacion,id_temporada)
);

CREATE TABLE AGENCIA(
 id_agencia INT IDENTITY PRIMARY KEY,
 nombre_agencia VARCHAR(100) NOT NULL,
 telefono VARCHAR(30) NULL,
 email VARCHAR(100) NULL,
 direccion VARCHAR(150) NULL,
 estado VARCHAR(20) NOT NULL DEFAULT 'ACTIVO'
);

CREATE TABLE RESERVA(
 id_reserva INT IDENTITY PRIMARY KEY,
 id_temporada INT NOT NULL,
 fecha_reserva DATETIME NOT NULL DEFAULT GETDATE(),
 fecha_ingreso DATE NOT NULL,
 fecha_salida DATE NOT NULL,
 estado VARCHAR(20) NOT NULL DEFAULT 'RESERVADA',
 observaciones VARCHAR(300) NULL,
 precio_reserva DECIMAL(12,2) NOT NULL DEFAULT 0,
 id_huesped INT NOT NULL,
 tiempo_limite DATETIME NULL,
 id_agencia INT NULL,
 tipo_reserva VARCHAR(20) NOT NULL DEFAULT 'PRESENCIAL',
 id_empleados INT NOT NULL,
 FOREIGN KEY(id_temporada) REFERENCES TEMPORADA(id_temporada),
 FOREIGN KEY(id_huesped) REFERENCES HUESPED(id_huesped),
 FOREIGN KEY(id_agencia) REFERENCES AGENCIA(id_agencia),
 FOREIGN KEY(id_empleados) REFERENCES EMPLEADOS(id_empleados),
 CHECK(fecha_salida>fecha_ingreso)
);

CREATE TABLE DETALLE_RESERVA(
 id_reserva INT NOT NULL PRIMARY KEY,
 id_habitacion INT NOT NULL,
 cantidad_persona INT NOT NULL CHECK(cantidad_persona>0),
 id_tarifa INT NOT NULL,
 precio_noche DECIMAL(12,2) NOT NULL,
 FOREIGN KEY(id_reserva) REFERENCES RESERVA(id_reserva),
 FOREIGN KEY(id_habitacion) REFERENCES HABITACION(id_habitacion),
 FOREIGN KEY(id_tarifa) REFERENCES TARIFA(id_tarifa)
);

CREATE TABLE SERVICIO(
 id_servicio INT IDENTITY PRIMARY KEY,
 nombre_servicio VARCHAR(100) NOT NULL,
 precio DECIMAL(12,2) NOT NULL CHECK(precio>=0)
);

CREATE TABLE CONSUMO(
 id_consumo INT IDENTITY PRIMARY KEY,
 id_reserva INT NOT NULL,
 id_servicio INT NOT NULL,
 cantidad INT NOT NULL CHECK(cantidad>0),
 fecha_hora DATETIME NOT NULL DEFAULT GETDATE(),
 observaciones VARCHAR(200) NULL,
 precio_unitario DECIMAL(12,2) NOT NULL CHECK(precio_unitario>=0),
 FOREIGN KEY(id_reserva) REFERENCES RESERVA(id_reserva),
 FOREIGN KEY(id_servicio) REFERENCES SERVICIO(id_servicio)
);

CREATE TABLE CHECK_IN_OUT(
 id_check_in INT IDENTITY PRIMARY KEY,
 id_reserva INT NOT NULL UNIQUE,
 fecha_hora_check_in DATETIME NOT NULL,
 observaciones VARCHAR(200) NULL,
 fecha_hora_check_out DATETIME NULL,
 id_empleado_cajero INT NULL,
 FOREIGN KEY(id_reserva) REFERENCES RESERVA(id_reserva),
 FOREIGN KEY(id_empleado_cajero) REFERENCES EMPLEADOS(id_empleados)
);

CREATE TABLE FACTURA(
 id_factura INT IDENTITY PRIMARY KEY,
 id_check_out INT NOT NULL UNIQUE,
 fecha_emision DATETIME NOT NULL DEFAULT GETDATE(),
 descuento DECIMAL(12,2) NOT NULL DEFAULT 0,
 observaciones VARCHAR(200) NULL,
 estado_factura VARCHAR(20) NOT NULL DEFAULT 'EMITIDA',
 numero_factura VARCHAR(30) NOT NULL UNIQUE,
 subtotal DECIMAL(12,2) NOT NULL,
 total DECIMAL(12,2) NOT NULL,
 metodo_pago VARCHAR(30) NOT NULL,
 FOREIGN KEY(id_check_out) REFERENCES CHECK_IN_OUT(id_check_in)
);

CREATE INDEX IX_RESERVA_FECHAS ON RESERVA(fecha_ingreso,fecha_salida,estado);
CREATE INDEX IX_DETALLE_HABITACION ON DETALLE_RESERVA(id_habitacion,id_reserva);
CREATE INDEX IX_CONSUMO_RESERVA ON CONSUMO(id_reserva);
GO
