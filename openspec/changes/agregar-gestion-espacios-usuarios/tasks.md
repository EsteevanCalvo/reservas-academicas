# Tareas: gestión de espacios académicos y usuarios

## 1. Base de la solución

- [ ] 1.1 Crear la solución y los cuatro proyectos con sus referencias. Verificación: `dotnet build` compila y `Core` no referencia ningún otro proyecto
- [ ] 1.2 Fijar versiones de paquetes: EF Core SQLite, JwtBearer, Swashbuckle y SDK de pruebas. Verificación: `dotnet restore` sin errores y `dotnet build` sin advertencias
- [ ] 1.3 Configurar `Directory.Build.props` con `Nullable`, `ImplicitUsings` y tratamiento de advertencias como errores. Verificación: una variable sin usar rompe la compilación

## 2. Dominio

- [ ] 2.1 Crear los enums `Rol`, `TipoEspacio` y `EstadoReserva`. Verificación: compilan y se usan desde las entidades
- [ ] 2.2 Crear `Espacio` con nombre, tipo, capacidad, ubicación y estado, con su validación. Verificación: un nombre vacío o una capacidad no positiva lanzan excepción de negocio
- [ ] 2.3 Crear `Usuario` con nombre, correo normalizado, hash, rol y estado. Verificación: el correo se guarda en minúsculas
- [ ] 2.4 Crear las excepciones de negocio y sus mensajes en español. Verificación: cada tipo se distingue en las pruebas
- [ ] 2.5 Crear los DTO de entrada y salida de espacios y usuarios. Verificación: ningún DTO de salida contiene hash ni contraseña

## 3. Infraestructura

- [ ] 3.1 Crear `ReservasDbContext` con las configuraciones de `Espacio` y `Usuario`. Verificación: el modelo se construye sin errores
- [ ] 3.2 Configurar índices únicos de nombre de espacio y correo de usuario. Verificación: la migración los incluye
- [ ] 3.3 Implementar el hash PBKDF2 con formato versionado y su verificación. Verificación: dos hashes de la misma contraseña difieren y ambos verifican
- [ ] 3.4 Implementar el emisor de tokens JWT con identidad, rol y expiración. Verificación: el token se decodifica y contiene el rol
- [ ] 3.5 Configurar el proveedor SQLite, WAL y el tiempo de espera. Verificación: la base se crea al arrancar en un archivo local
- [ ] 3.6 Generar la migración inicial y aplicarla al arrancar. Verificación: la base contiene las tablas y los índices
- [ ] 3.7 Implementar la semilla idempotente con tres usuarios y seis espacios. Verificación: dos arranques seguidos no duplican datos

## 4. Aplicación

- [ ] 4.1 Implementar `ServicioEspacios` con crear, listar, obtener, actualizar y desactivar. Verificación: las reglas de unicidad y capacidad se cumplen
- [ ] 4.2 Implementar `ServicioUsuarios` con registrar, listar, actualizar y restablecer contraseña. Verificación: el correo duplicado produce conflicto
- [ ] 4.3 Implementar `ServicioAutenticacion` con verificación de credenciales y usuario activo. Verificación: credenciales incorrectas producen el mismo mensaje que un correo inexistente

## 5. API

- [ ] 5.1 Configurar autenticación JWT y las políticas `Administrador` y `UsuarioAutenticado`. Verificación: sin token se recibe 401 y con rol insuficiente se recibe 403
- [ ] 5.2 Implementar los endpoints de autenticación. Verificación: el login devuelve un token usable y el perfil devuelve solo datos propios
- [ ] 5.3 Implementar los endpoints de espacios. Verificación: el CRUD responde con los códigos acordados
- [ ] 5.4 Implementar los endpoints de usuarios. Verificación: ninguna respuesta incluye hash
- [ ] 5.5 Implementar el middleware de errores con ProblemDetails. Verificación: cada excepción de negocio produce su código HTTP
- [ ] 5.6 Configurar Swagger con esquema de seguridad de portador. Verificación: se puede autenticar y ejecutar una petición desde la interfaz

## 6. Pruebas

- [ ] 6.1 Crear el escenario de prueba de integración con base SQLite temporal aislada por prueba. Verificación: dos pruebas no comparten datos
- [ ] 6.2 Probar el hash y su verificación. Verificación: hashes distintos y verificación correcta
- [ ] 6.3 Probar el login correcto, la contraseña incorrecta, el correo inexistente y el usuario inactivo. Verificación: los cuatro casos responden lo esperado
- [ ] 6.4 Probar los 401 y 403 en los endpoints administrativos. Verificación: docente y estudiante reciben 403
- [ ] 6.5 Probar el CRUD de espacios y sus validaciones. Verificación: nombre vacío, nombre duplicado y capacidad inválida son rechazados
- [ ] 6.6 Probar el alta de usuarios, el correo duplicado y la ausencia de hash en las respuestas. Verificación: las tres pruebas pasan
- [ ] 6.7 Probar que un usuario desactivado no puede iniciar sesión ni usar un token emitido antes. Verificación: el segundo caso devuelve 401

## 7. Cierre

- [ ] 7.1 Ejecutar la suite completa. Verificación: todas las pruebas pasan
- [ ] 7.2 Verificar la compilación sin advertencias. Verificación: 0 errores y 0 advertencias
- [ ] 7.3 Validar el cambio con `openspec validate --strict`. Verificación: sin errores
- [ ] 7.4 Archivar el cambio y comprobar que `openspec/specs/` contiene las dos capacidades. Verificación: las especificaciones archivadas reflejan lo implementado
- [ ] 7.5 Registrar el commit del incremento. Verificación: el historial muestra la especificación antes que el código
