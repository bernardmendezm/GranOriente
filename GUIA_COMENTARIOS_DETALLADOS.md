# Guía de la versión comentada a detalle

Esta versión fue preparada para estudiar el sistema línea por línea.

## Qué se comenta

Los comentarios explican:

- Qué hace cada método.
- Por qué existe cada variable.
- Qué significa `async`.
- Qué significa `await`.
- Para qué se usa `await using`.
- Qué hace `SqlCommand`.
- Diferencia entre `ExecuteReaderAsync`, `ExecuteScalarAsync` y `ExecuteNonQueryAsync`.
- Cómo funciona `SqlDataReader`.
- Para qué se usa `while (await rd.ReadAsync())`.
- Cómo una fila SQL se convierte en un objeto C#.
- Cómo se utilizan parámetros `@nombre`, `@id`, etc.
- Por qué se usa `DBNull.Value`.
- Cómo funcionan las transacciones y Rollback.
- Qué hace `ViewBag`.
- Qué significa `return View(...)`.
- Qué significa `RedirectToAction(...)`.
- Cómo funcionan `[HttpGet]` y `[HttpPost]`.
- Cómo funciona `@model` en Razor.
- Cómo funciona `asp-for`.
- Cómo funcionan `asp-controller` y `asp-action`.
- Cómo se usan `if` y `foreach` dentro de `.cshtml`.
- Cómo se muestran u ocultan botones según el rol.

## Ejemplo de lectura recomendada

Para entender el listado de reservas:

1. `Views/Reservas/Index.cshtml`
2. `Controllers/ReservasController.cs`
3. `Negocio/Servicios.cs`
4. `Datos/ReservaDatos.cs`
5. Consulta SQL de `ListarAsync()`

Para entender la creación de una reserva:

1. `Views/Reservas/Crear.cshtml`
2. `ReservasController -> Crear()`
3. `HotelNegocio.ValidarReserva()`
4. `ReservaDatos.CrearAsync()`
5. `sp_CrearReserva` en SQL Server

Para entender huéspedes:

1. `Views/Huespedes/Crear.cshtml`
2. `HuespedesController`
3. `HuespedDatos.CrearAsync()`
4. Tablas PERSONA y HUESPED

## Recomendación para defensa

No intentes memorizar cada línea.

Debes entender principalmente:

`Vista -> Controller -> Negocio -> Datos -> SQL Server`

La vista captura o muestra información.

El Controller recibe la acción del usuario.

La capa de negocio valida reglas.

La capa de datos se conecta con SQL Server.

SQL Server guarda o consulta la información.
