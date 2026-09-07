# Sistema de Gestión Hotelera Gran Oriente

Prototipo académico en **ASP.NET Core MVC .NET 8 + SQL Server**, organizado en 3 capas:

1. **GranOriente.Web**: interfaz MVC.
2. **GranOriente.Negocio**: reglas de negocio.
3. **GranOriente.Datos**: acceso a SQL Server con ADO.NET.
4. **GranOriente.Entidades**: clases del dominio.

## Flujo implementado
Huésped -> Reserva -> Check-in -> Consumos -> Check-out -> Factura.

## Instalación
1. Abrir SQL Server Management Studio.
2. Ejecutar en este orden:
   - `Database/01_Estructura.sql`
   - `Database/02_Procedimientos_Triggers_Funciones.sql`
   - `Database/03_Datos_Prueba.sql`
   - opcional: `Database/04_Backup_Plan.sql`
3. Abrir `GranOriente.sln` en Visual Studio 2022.
4. Editar `GranOriente.Web/appsettings.json` si su SQL Server no es localhost.
5. Establecer `GranOriente.Web` como proyecto de inicio.
6. Ejecutar.

Usuario de prueba:
- usuario: `admin`
- contraseña: `Admin123`

## Qué se implementó para el caso
- Gestión de huéspedes.
- Consulta de habitaciones.
- Reserva con validación de disponibilidad y prevención de sobreventa.
- Tarifa dinámica por temporada mediante función SQL.
- Check-in.
- Registro de consumos.
- Check-out con factura automática.
- Reporte de ocupación por fecha.
- Roles y sesión.
- Bitácora mediante triggers.
- Índices.
- Transacciones.
- Procedimientos almacenados.
- Función SQL.
- Plan básico de backups.

## Reconocimiento facial
El caso exige autenticación facial. En este prototipo se deja el **punto de integración** en `/Account/Facial`, pero no se finge un reconocimiento biométrico real. Para producción se debe integrar un proveedor/librería biométrica real y aplicar consentimiento, seguridad de plantillas y protección de datos.

## Alcance académico
El proyecto está intencionalmente simplificado para que un estudiante pueda explicar claramente:
- qué hace cada capa;
- por qué se usan procedimientos almacenados;
- cómo se evita la sobreventa con transacción SERIALIZABLE;
- cómo se calcula la tarifa;
- cómo el check-out integra alojamiento + consumos;
- cómo se registran cambios en bitácora.

## Cambios incluidos en versión 2
- Dashboard funcional con indicadores reales.
- Accesos rápidos desde el inicio.
- Registro de huésped con fecha de nacimiento, sexo y nacionalidad.
- Listado de huéspedes mostrando esos campos.
- Alta de nuevas habitaciones desde la interfaz web.
- Selectores de tipo y estado de habitación desde la base de datos.
- Diseño visual simple con azul oscuro y dorado suave.


## Cambios incluidos en versión 3

- Módulo `Usuarios` visible únicamente para el rol ADMINISTRADOR.
- Alta de funcionarios RECEPCIONISTA, CAJERO o ADMINISTRADOR.
- Los datos personales del funcionario son los mismos utilizados en el registro de HUESPED:
  nombre, apellidos, tipo y número de documento, fecha de nacimiento, sexo, nacionalidad,
  teléfono y email.
- El alta se realiza en una sola transacción:
  `PERSONA -> EMPLEADOS -> USUARIO`.
- Se valida documento repetido y nombre de usuario repetido.
- El usuario recibe un rol de la tabla ROL.
- Contraseña almacenada con SHA-256 para mantener compatibilidad con el Login actual.
- No requiere cambios adicionales en la estructura de la base de datos si ya se ejecutó
  la versión anterior.


## Cambios incluidos en versión 4

### ADMINISTRADOR
- Acceso completo.
- Puede crear habitaciones.
- Puede crear funcionarios y usuarios.
- Puede crear reservas, hacer check-in, registrar consumos y realizar check-out.
- Acceso a reportes.

### RECEPCIONISTA
- Menú Huéspedes.
- Puede crear huéspedes.
- Menú Habitaciones solo para consulta.
- No puede crear habitaciones.
- Puede crear reservas.
- Puede realizar check-in.
- No puede registrar consumos.
- No puede realizar check-out ni facturación.
- No puede administrar usuarios.

### CAJERO
- Solo dispone del módulo Reservas como módulo operativo.
- No puede crear reservas ni hacer check-in.
- En reservas con estado CHECKIN puede:
  - Registrar consumos.
  - Realizar check-out/cobro.
- No tiene acceso a huéspedes, habitaciones, usuarios ni reportes.

Los permisos no solo se ocultan en la interfaz:
también son validados en los controladores.


## Versión 5 comentada

Esta versión mantiene las funciones de la versión 4 y agrega comentarios
explicativos en:

- Entidades y modelos.
- Capa de datos.
- Capa de negocio.
- Controllers.
- Program.cs.
- Vistas Razor `.cshtml`.
- Hoja de estilos CSS.

También incluye `GUIA_CODIGO_COMENTADO.md` con un orden recomendado
para estudiar el proyecto antes de la defensa.


## Versión 6 - comentarios a detalle

Esta versión amplía los comentarios de la versión anterior.

Ahora se explican bloques y líneas concretas dentro de los métodos,
incluyendo conexión SQL, consultas, parámetros, lectores de datos,
transacciones, validaciones, ViewBag, Razor y formularios HTML.

Consultar también `GUIA_COMENTARIOS_DETALLADOS.md`.


## Versión 7 - Factura detallada

Se mejoró el check-out y la impresión de factura.

Ahora se muestran por separado:

- Habitación.
- Tipo de habitación.
- Fechas de ingreso y salida.
- Cantidad de noches.
- Precio por noche.
- Total del alojamiento.
- Cada consumo con cantidad, precio e importe.
- Total consumos.
- Subtotal.
- Descuento.
- Total final.

Consultar `CAMBIOS_V7_FACTURA_DETALLADA.md`.


## Versión 8 - Factura corregida

Se corrigió el problema de Razor que mostraba `.ToString(...)`
como texto visible en la factura y en el check-out.

Las expresiones ahora usan la forma:

`@(Model.Propiedad.ToString("formato"))`

También se ampliaron los comentarios línea por línea
en las vistas de facturación.
