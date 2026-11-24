# Diseño: gestión de espacios académicos y usuarios

## Contexto

Es el primer incremento y por eso carga con dos responsabilidades a la vez: resolver el catálogo de espacios y usuarios, y montar la infraestructura transversal que sostendrá los incrementos siguientes. La tentación es empezar por el flujo de reservas, que es lo interesante; el riesgo de hacerlo es descubrir tarde que faltan autenticación, autorización o manejo de errores y tener que reescribir medio sistema.

Restricciones que moldean el diseño:

- **Un solo proceso y una sola base.** El volumen institucional no justifica distribuirlos.
- **La base debe imponer restricciones.** Si la unicidad del correo o del nombre del espacio se valida solo en C#, dos peticiones simultáneas pueden colarla.
- **Sin dependencias innecesarias.** No se usan bibliotecas de mapeo, validación ni mediación.
- **El rol Administrador no solicita reservas.** Se decide aquí porque es una regla de permisos, aunque su consecuencia se vea en el incremento siguiente.

## Objetivos y no objetivos

**Objetivos:**

- Que un administrador pueda preparar el catálogo y las cuentas sin tocar la base a mano.
- Que la autorización viva en el servidor y no dependa del cliente.
- Que las pruebas de integración puedan autenticarse como cualquiera de los tres roles con una línea.
- Que un error de validación se distinga de un error de permisos y de un conflicto.

**No objetivos:**

- Recuperación de contraseña, doble factor, refresco de tokens ni cierre de sesión del servidor.
- CRUD de roles: los tres roles son fijos.
- Historial de auditoría de cambios administrativos.
- Interfaz web. La operación se hace desde Swagger.

## Decisiones

### D1. Cuatro proyectos con dependencias en una sola dirección

`Core` no referencia nada; `Infrastructure` referencia `Core`; `Api` referencia ambos; `Tests` referencia `Core` e `Infrastructure` y usa `Api` como host.

La alternativa era un proyecto único con carpetas. Se descartó porque la regla de dependencia dejaría de ser verificable por el compilador y porque probar las reglas de negocio obligaría a levantar el servidor. Con cuatro proyectos, si alguien intenta usar EF Core desde el dominio, la compilación falla.

### D2. Los roles son un enum, no una tabla

Tres roles fijos, sin operaciones de alta ni baja. Una tabla `Roles` con un CRUD sería más flexible y también más superficie: habría que decidir qué pasa al borrar un rol en uso, quién puede crear roles y cómo se protege el rol Administrador de quedarse sin usuarios.

Se elige el enum y se documenta la consecuencia: agregar un rol nuevo requiere un cambio de código y una migración, que es exactamente el nivel de fricción que se quiere para una decisión de permisos.

### D3. JWT sin estado, con expiración de ocho horas

El token lleva el identificador del usuario y su rol. El servidor no guarda sesiones.

La alternativa era una sesión en servidor con cookie. Se descartó porque obliga a mantener estado, agrega CSRF y complica las pruebas concurrentes. La desventaja aceptada es que un usuario desactivado conserva su token hasta que expire; se mitiga validando el estado del usuario en cada petición autenticada, no solo al emitir el token.

**Consecuencia de diseño:** el middleware de autenticación no basta. Cada endpoint que actúa en nombre del usuario relee el usuario desde la base y verifica que siga activo. Es una consulta extra por petición y es el precio de poder revocar el acceso sin lista negra.

### D4. PBKDF2 con formato versionado

Se usa `Rfc2898DeriveBytes.Pbkdf2` con SHA-256, 120 000 iteraciones y sal de 16 bytes. El hash se guarda como `pbkdf2-sha256$iteraciones$sal$hash`, todo en Base64.

El formato versionado permite subir las iteraciones en el futuro sin invalidar los hashes existentes: al verificar se leen las iteraciones del propio registro. La alternativa era BCrypt, que es una buena elección pero agrega una dependencia externa para algo que la plataforma ya resuelve.

### D5. La desactivación de espacios es lógica

`DELETE /api/espacios/{id}` no borra: marca `Activo = false`. Borrar físicamente rompería las claves foráneas de las reservas históricas y destruiría la trazabilidad, que es uno de los objetivos del sistema.

Se acepta la consecuencia: los administradores ven espacios inactivos en la lista. Se resuelve con el filtro `activos` del endpoint de consulta.

### D6. Los errores son ProblemDetails

Un middleware traduce las excepciones de negocio a un cuerpo uniforme con `status`, `title` y `detail`. Las reglas de negocio lanzan excepciones tipadas (`ReglaNegocioException`, `ConflictoException`, `NoEncontradoException`, `AccesoDenegadoException`) y la capa de presentación decide el código HTTP.

La alternativa era devolver `Results.BadRequest("texto")` en cada endpoint. Se descartó porque dispersa la decisión del código de estado por decenas de lugares y hace que dos endpoints devuelvan formas distintas para el mismo error.

### D7. El correo es el identificador de acceso, normalizado

Se normaliza a minúsculas y se recorta antes de guardar, con índice único en la base. La comparación es ordinal, sin distinguir mayúsculas. La alternativa era un nombre de usuario separado del correo; se descartó por agregar un dato que nadie pidió.

### D8. Semilla idempotente en desarrollo

Al arrancar, si no hay usuarios, se crean un administrador, un docente y un estudiante con contraseñas documentadas, y seis espacios. Si ya hay datos, no se toca nada.

La alternativa era un script SQL separado. Se descartó porque el esquema lo controla EF y un script se desincroniza en la primera migración.

**Riesgo aceptado:** las credenciales de demostración están en el repositorio. Se documenta que son de desarrollo y que en producción se desactivaría la semilla.

### D9. El administrador no crea reservas

Es una decisión de permisos, no una limitación técnica. Si el administrador pudiera solicitar y además aprobar, podría aprobar su propia solicitud y la separación de funciones —el motivo por el que existe el rol— quedaría anulada.

La alternativa era permitirlo y prohibir la autoaprobación. Se descartó porque, con un único administrador sembrado, dejaría solicitudes imposibles de aprobar.

## Endpoints del incremento

| Método | Ruta | Rol | Resultado |
|---|---|---|---|
| POST | `/api/auth/login` | Anónimo | 200 con token; 401 si las credenciales fallan |
| GET | `/api/auth/perfil` | Autenticado | 200 con los datos públicos del usuario |
| GET | `/api/espacios` | Autenticado | 200 con la lista, filtrable por tipo y estado |
| GET | `/api/espacios/{id}` | Autenticado | 200 o 404 |
| POST | `/api/espacios` | Administrador | 201 con el espacio creado; 400 si es inválido |
| PUT | `/api/espacios/{id}` | Administrador | 200 con el espacio actualizado; 404 si no existe |
| DELETE | `/api/espacios/{id}` | Administrador | 204; desactiva lógicamente |
| GET | `/api/usuarios` | Administrador | 200 con la lista, sin contraseñas |
| POST | `/api/usuarios` | Administrador | 201; 409 si el correo ya existe |
| PUT | `/api/usuarios/{id}` | Administrador | 200; permite cambiar nombre, rol y estado |
| POST | `/api/usuarios/{id}/contrasena` | Administrador | 204; restablece la contraseña |

## Riesgos y compromisos

- **La clave de firma vive en la configuración.** En un repositorio público eso es un secreto expuesto. Se documenta como configuración de desarrollo y se deja la advertencia de reemplazarla por variable de entorno en producción. No se implementa rotación de claves porque está fuera del alcance.

- **Ocho horas de validez es mucho para un token.** Se acepta porque el sistema se demuestra en una sesión de trabajo y porque la revocación se resuelve releyendo el usuario. Un token de quince minutos exigiría refresco, que está fuera del alcance.

- **Un administrador puede desactivarse a sí mismo.** No se agrega una salvaguarda: sería una regla más que nadie pidió y hay varios administradores posibles. Se documenta como limitación conocida.

- **El catálogo no tiene paginación.** Con decenas de espacios es innecesaria. Si la institución creciera, el contrato ya admite agregar `pagina` y `tamano` sin romper a los clientes actuales.

- **La semilla crea datos con contraseñas conocidas.** Es deliberado para la demostración y para las pruebas manuales. Se aísla en `Development`.
