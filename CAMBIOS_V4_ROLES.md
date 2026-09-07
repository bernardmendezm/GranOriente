# Control de acceso por roles - Versión 4

## ADMINISTRADOR
Tiene acceso completo al sistema.

## RECEPCIONISTA
Puede:
- Registrar y consultar huéspedes.
- Consultar habitaciones.
- Crear reservas.
- Realizar check-in.

No puede:
- Crear habitaciones.
- Registrar consumos.
- Realizar check-out/cobro.
- Administrar usuarios.
- Ver reportes administrativos.

## CAJERO
Puede:
- Acceder a Reservas.
- Registrar consumos después del check-in.
- Realizar check-out y cobro.

No puede:
- Crear reservas.
- Realizar check-in.
- Gestionar huéspedes.
- Gestionar habitaciones.
- Administrar usuarios.
- Ver reportes administrativos.

## Seguridad
Las opciones se ocultan visualmente según rol y,
además, cada Controller valida el rol antes de ejecutar la acción.
