# Versión 7 - Factura detallada

Se mejoró el proceso de Check-out y la factura.

## Antes del cobro

Ahora la pantalla de Check-out muestra:

- Huésped.
- Habitación.
- Tipo de habitación.
- Fecha de ingreso.
- Fecha de salida.
- Cantidad de noches.
- Precio por noche.
- Total del alojamiento.
- Cada consumo registrado.
- Cantidad y precio de cada consumo.
- Total de consumos.
- Subtotal antes del descuento.

Esto permite al cajero revisar la cuenta antes de cerrar la reserva.

## Factura generada

Después del cobro se muestra una factura con:

### Alojamiento

Ejemplo:

Habitación 201 - Matrimonial  
3 noches x Bs 250,00 = Bs 750,00

### Consumos

Cada consumo aparece separado:

- Desayuno: 2 x Bs 35,00 = Bs 70,00
- Lavandería: 1 x Bs 45,00 = Bs 45,00
- Minibar: 3 x Bs 12,00 = Bs 36,00

### Totales

- Total alojamiento.
- Total consumos.
- Subtotal.
- Descuento.
- Total final.

## Base de datos

No fue necesario modificar la estructura de las tablas.

La información ya existe en:

- RESERVA
- DETALLE_RESERVA
- HABITACION
- TIPO_HABITACION
- CHECK_IN_OUT
- FACTURA
- CONSUMO
- SERVICIO
- PERSONA
- HUESPED

La factura detallada se reconstruye mediante JOIN.

También se incluye:

`Database/05_Factura_Detallada_Consulta.sql`

como ejemplo académico de cómo obtener el mismo detalle desde un procedimiento almacenado.
