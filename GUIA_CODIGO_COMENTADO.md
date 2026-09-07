# Guía para estudiar el código comentado

Esta versión conserva la misma funcionalidad de la versión 4,
pero agrega comentarios pedagógicos para facilitar la defensa y el estudio.

## Cómo leer el proyecto

### 1. GranOriente.Entidades
Empieza por `Modelos.cs`.

Aquí están las clases que representan los datos del sistema:
Persona, Huesped, Habitacion, Reserva, Consumo, Factura, etc.

Estas clases no consultan la base de datos.
Solo transportan información entre las capas.

### 2. GranOriente.Datos
Después revisa las clases terminadas en `Datos.cs`.

Ejemplo:

- `HuespedDatos.cs`
- `HabitacionDatos.cs`
- `ReservaDatos.cs`
- `ConsumoDatos.cs`

Aquí se encuentran las consultas SQL y la ejecución de procedimientos almacenados.

### 3. GranOriente.Negocio
Revisa `Servicios.cs`.

Esta capa conecta la aplicación Web con la capa Datos y contiene
validaciones sencillas de negocio.

### 4. GranOriente.Web / Controllers
Los Controllers reciben las acciones del usuario.

Ejemplo:

`ReservasController`

1. Recibe una solicitud desde la vista.
2. Comprueba el rol.
3. Ejecuta una operación mediante HotelNegocio.
4. Devuelve una vista o redirige a otra pantalla.

### 5. Views
Las vistas `.cshtml` contienen HTML y Razor.

Razor permite mezclar HTML con pequeñas instrucciones C#.

Ejemplo:

```cshtml
@if (rol == "ADMINISTRADOR")
{
    <a class="btn">Nueva habitación</a>
}
```

Eso significa:

"Solo mostrar el botón cuando el usuario sea ADMINISTRADOR".

## Flujo recomendado para explicar una reserva

1. `Views/Reservas/Crear.cshtml`
2. `ReservasController.cs`
3. `Servicios.cs`
4. `ReservaDatos.cs`
5. `sp_CrearReserva` en SQL Server
6. Tablas `RESERVA` y `DETALLE_RESERVA`

## Flujo recomendado para explicar el check-out

1. `Views/Facturacion/CheckOut.cshtml`
2. `FacturacionController.cs`
3. `FacturacionDatos.cs`
4. `sp_CheckOut_GenerarFactura`
5. Tabla `FACTURA`

## Control de roles

Los roles se controlan en dos lugares:

1. En las Views:
   ocultan botones o menús.
2. En los Controllers:
   bloquean el acceso real.

Esto es importante porque ocultar un botón por sí solo no sería seguridad suficiente.
