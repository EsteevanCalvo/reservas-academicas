# Propuesta: gestión de espacios académicos y usuarios

## Por qué

El sistema de reservas no puede existir sin dos catálogos previos: qué se puede reservar y quién puede reservar. Hoy la institución no tiene ninguno de los dos de forma confiable. Los espacios viven en hojas de cálculo que se desincronizan y los usuarios se identifican por el remitente de un correo, sin roles ni permisos.

Este incremento construye esa base y, además, establece la infraestructura transversal del proyecto: capas, persistencia, autenticación, autorización, manejo uniforme de errores y pruebas de integración. Todo lo que venga después se apoya en esta base, así que conviene cerrarla bien antes de agregar el flujo de reservas.

## Qué cambia

- Se crea la solución con cuatro proyectos: `Reservas.Core`, `Reservas.Infrastructure`, `Reservas.Api` y `Reservas.Tests`.
- Se implementa el catálogo de espacios académicos con tipo, capacidad, ubicación y estado activo.
- Se implementan los usuarios con tres roles fijos: Administrador, Docente y Estudiante.
- Se implementa la autenticación con correo y contraseña, y la emisión de un token JWT.
- Se implementa la autorización por rol sobre los endpoints de administración.
- Se agrega la semilla de desarrollo con un usuario de cada rol y seis espacios de ejemplo.
- Se documenta la API en Swagger con esquema de seguridad para el token.
- Se agregan pruebas unitarias de reglas y pruebas de integración contra la API real.

## Capacidades

### Capacidades nuevas

- `espacios-academicos`: catálogo de laboratorios, auditorios y salas de cómputo, con consulta pública autenticada y administración restringida.
- `usuarios-y-permisos`: registro y administración de usuarios, roles fijos, autenticación y autorización.

### Capacidades modificadas

Ninguna. Es el primer incremento del proyecto.

## Impacto

- **Código nuevo**: solución completa, entidades `Espacio` y `Usuario`, `ReservasDbContext`, migración inicial, servicios de espacios, usuarios y autenticación, endpoints y pruebas.
- **Dependencias nuevas**: EF Core con proveedor SQLite, `Microsoft.AspNetCore.Authentication.JwtBearer`, Swashbuckle y el SDK de pruebas de ASP.NET Core. Todas quedan fijadas por versión.
- **Configuración**: cadena de conexión, clave de firma de tokens y semilla de desarrollo en `appsettings.Development.json`.
- **Datos**: el esquema se crea con migraciones. La base local no se versiona.
- **Seguridad**: se introduce el manejo de contraseñas con PBKDF2 y la regla de no devolver nunca el hash en una respuesta.
- **Riesgo de alcance**: la autorización y la autenticación son transversales y podrían crecer sin límite (recuperación de contraseña, doble factor, refresco de tokens). Se excluyen explícitamente.
