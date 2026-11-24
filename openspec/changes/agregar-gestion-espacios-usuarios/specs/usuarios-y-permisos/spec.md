# Delta de especificación

## Purpose

Registrar a las personas que usan el sistema, asignarles un rol y exigir una identidad verificable antes de permitir cualquier operación. Sin identidad ni roles, la aprobación de reservas no tendría sentido porque cualquiera podría actuar como solicitante o como autorizador.

## ADDED Requirements

### Requirement: Registro de usuarios

El sistema SHALL permitir que un administrador registre usuarios con nombre, correo electrónico, contraseña y rol. El correo electrónico SHALL ser único y SHALL normalizarse a minúsculas. El rol SHALL ser uno de los tres roles definidos.

#### Scenario: Se registra un docente
- **WHEN** un administrador registra un usuario con nombre "Ana Torres", correo "ana.torres@universidad.edu", contraseña válida y rol Docente
- **THEN** el sistema devuelve el usuario creado
- **AND** el usuario queda activo

#### Scenario: Se intenta repetir el correo
- **WHEN** un administrador intenta registrar un usuario con un correo que ya existe
- **THEN** el sistema rechaza la operación con un mensaje que indica que el correo ya está registrado

#### Scenario: El correo se normaliza
- **WHEN** un administrador registra un usuario con el correo "Ana.Torres@Universidad.edu"
- **THEN** el sistema lo almacena en minúsculas
- **AND** el inicio de sesión funciona con cualquier combinación de mayúsculas

#### Scenario: Se registra un rol desconocido
- **WHEN** un administrador intenta registrar un usuario con un rol que no existe
- **THEN** el sistema rechaza la operación

#### Scenario: Se listan los usuarios
- **WHEN** un administrador solicita la lista de usuarios
- **THEN** el sistema devuelve todos los usuarios con su rol y su estado
- **AND** ninguna respuesta incluye la contraseña ni su hash

### Requirement: Contraseñas protegidas

El sistema SHALL almacenar únicamente un hash de la contraseña, calculado con PBKDF2 y una sal distinta por usuario. La contraseña en claro NO SHALL almacenarse ni devolverse en ninguna respuesta.

#### Scenario: La contraseña no se devuelve
- **WHEN** se consulta un usuario por cualquier vía
- **THEN** la respuesta no contiene la contraseña ni el hash

#### Scenario: Dos usuarios con la misma contraseña tienen hashes distintos
- **WHEN** dos usuarios se registran con la misma contraseña
- **THEN** sus hashes almacenados son diferentes

#### Scenario: Se rechaza una contraseña demasiado corta
- **WHEN** un administrador intenta registrar un usuario con una contraseña de menos de ocho caracteres
- **THEN** el sistema rechaza la operación con un mensaje que indica la longitud mínima

### Requirement: Roles y permisos

El sistema SHALL definir tres roles fijos: Administrador, Docente y Estudiante. Los permisos SHALL aplicarse en el servidor y NO SHALL depender de lo que muestre el cliente.

| Acción | Administrador | Docente | Estudiante |
|---|---|---|---|
| Consultar espacios | Sí | Sí | Sí |
| Administrar espacios | Sí | No | No |
| Administrar usuarios | Sí | No | No |
| Consultar disponibilidad | Sí | Sí | Sí |
| Crear solicitudes de reserva | No | Sí | Sí |
| Ver todas las reservas | Sí | No | No |
| Ver las reservas propias | Sí | Sí | Sí |
| Aprobar o rechazar | Sí | No | No |
| Cancelar reservas propias | Sí | Sí | Sí |
| Cancelar cualquier reserva | Sí | No | No |

#### Scenario: Un estudiante intenta administrar usuarios
- **WHEN** un usuario con rol Estudiante intenta registrar un usuario
- **THEN** el sistema responde que no tiene permiso

#### Scenario: Un administrador no crea solicitudes de reserva
- **WHEN** un usuario con rol Administrador intenta crear una solicitud de reserva
- **THEN** el sistema responde que su rol no solicita reservas
- **AND** el mensaje indica que debe usar una cuenta de Docente o Estudiante

#### Scenario: Un docente consulta sus reservas
- **WHEN** un usuario con rol Docente solicita sus reservas
- **THEN** el sistema devuelve únicamente las reservas de ese usuario

### Requirement: Autenticación con token

El sistema SHALL autenticar a un usuario activo con correo y contraseña, y SHALL devolver un token con expiración y con la identidad y el rol del usuario. Las credenciales inválidas SHALL rechazarse sin revelar cuál de los dos datos es incorrecto.

#### Scenario: Inicio de sesión correcto
- **WHEN** un usuario activo envía su correo y su contraseña correctos
- **THEN** el sistema devuelve un token y los datos públicos del usuario
- **AND** el token permite acceder a los endpoints protegidos

#### Scenario: Contraseña incorrecta
- **WHEN** un usuario envía su correo correcto con una contraseña incorrecta
- **THEN** el sistema responde que las credenciales no son válidas
- **AND** no revela si el correo existe

#### Scenario: Correo inexistente
- **WHEN** una petición usa un correo que no está registrado
- **THEN** el sistema responde con el mismo mensaje que usa para una contraseña incorrecta

#### Scenario: Usuario inactivo
- **WHEN** un usuario desactivado intenta iniciar sesión con credenciales correctas
- **THEN** el sistema rechaza el acceso

#### Scenario: Petición sin token
- **WHEN** una petición sin token intenta acceder a un endpoint protegido
- **THEN** el sistema responde que no está autenticado

#### Scenario: Token inválido o expirado
- **WHEN** una petición presenta un token alterado o vencido
- **THEN** el sistema responde que no está autenticado

#### Scenario: Se consulta el perfil propio
- **WHEN** un usuario autenticado consulta su perfil
- **THEN** el sistema devuelve su nombre, correo y rol
- **AND** no devuelve datos de otros usuarios

### Requirement: Administración del estado del usuario

El sistema SHALL permitir que un administrador active o desactive usuarios y restablezca su contraseña. Un usuario desactivado NO SHALL poder autenticarse.

#### Scenario: Se desactiva un usuario
- **WHEN** un administrador desactiva a un usuario
- **THEN** ese usuario ya no puede iniciar sesión

#### Scenario: Se restablece una contraseña
- **WHEN** un administrador restablece la contraseña de un usuario
- **THEN** el usuario puede iniciar sesión con la nueva contraseña
- **AND** la anterior deja de funcionar
