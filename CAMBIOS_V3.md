# Gran Oriente - Versión 3

## Nuevo módulo: Usuarios y funcionarios

El módulo solo puede ser utilizado por un usuario con rol `ADMINISTRADOR`.

Al registrar un funcionario se solicitan los mismos datos personales utilizados
para registrar un huésped:

- Nombre
- Apellido paterno
- Apellido materno
- Tipo de documento
- Número de documento
- Fecha de nacimiento
- Sexo
- Nacionalidad
- Teléfono
- Email

Además se solicitan:

- Cargo
- Fecha de ingreso
- Nombre de usuario
- Contraseña
- Rol

## Guardado

Todo se guarda dentro de una sola transacción:

1. `PERSONA`
2. `EMPLEADOS`
3. `USUARIO`

Si una de las operaciones falla, se revierte todo el registro.

## Roles

El combo de roles se carga directamente desde la tabla `ROL`.

Los datos de prueba existentes incluyen:

- ADMINISTRADOR
- RECEPCIONISTA
- CAJERO
