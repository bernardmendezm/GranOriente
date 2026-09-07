# Guía corta para defender el sistema

## 1. Problema principal
El hotel maneja reservas, consumos y facturación en sistemas separados. El prototipo integra el flujo completo.

## 2. Arquitectura
Se usa arquitectura de 3 capas:
- Presentación: ASP.NET MVC.
- Negocio: validaciones y reglas.
- Datos: consultas y ejecución de procedimientos almacenados.
Las entidades se separan para compartir los modelos.

## 3. Caso de uso principal para exponer
**Registrar reserva**.
1. Recepcionista elige fechas.
2. Sistema consulta habitaciones disponibles.
3. El usuario elige huésped y habitación.
4. `sp_CrearReserva` inicia una transacción `SERIALIZABLE`.
5. Verifica que no exista solapamiento de fechas.
6. Busca temporada y tarifa.
7. Calcula noches x precio.
8. Inserta RESERVA y DETALLE_RESERVA.
9. Confirma la transacción.

Este caso demuestra maestro-detalle:
- Maestro: RESERVA.
- Detalle: DETALLE_RESERVA.

## 4. Stored procedures fáciles de explicar
- `sp_CrearReserva`: registra la reserva y evita sobreventa.
- `sp_RegistrarCheckIn`: cambia la reserva a CHECKIN.
- `sp_RegistrarConsumo`: agrega servicios.
- `sp_CheckOut_GenerarFactura`: suma alojamiento + consumos y crea FACTURA.
- `sp_ReporteOcupacion`: muestra ocupación por fecha.

## 5. Función
`fn_PrecioNoche` devuelve la tarifa correspondiente al tipo de habitación y temporada.

## 6. Triggers
Hay triggers de bitácora para RESERVA, CONSUMO y FACTURA. Registran INSERT, UPDATE y DELETE.

## 7. Seguridad
Se implementa usuario/contraseña con SHA-256, roles y sesión. El reconocimiento facial está preparado como punto de integración y debe conectarse a un servicio biométrico real para cumplir completamente el requisito.

## 8. Qué decir si preguntan por concurrencia
Se usa `SERIALIZABLE` al reservar, porque dos recepcionistas podrían intentar reservar la misma habitación al mismo tiempo. La transacción protege el rango crítico.

## 9. Reportes
El prototipo incluye ocupación por fecha. Los otros reportes del caso se pueden agregar sobre la misma capa de datos sin cambiar la arquitectura.
