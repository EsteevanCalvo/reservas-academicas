# Delta de especificación

## Purpose

Mantener un catálogo confiable de los espacios académicos de la institución —laboratorios, auditorios y salas de cómputo— con su tipo, capacidad, ubicación y estado, de modo que las reservas se hagan siempre sobre un espacio que existe, está identificado y puede desactivarse cuando deja de estar disponible.

## ADDED Requirements

### Requirement: Catálogo de espacios académicos

El sistema SHALL mantener un catálogo de espacios académicos, cada uno con nombre, tipo, capacidad, ubicación y estado activo. Los tipos permitidos SHALL ser Laboratorio, Auditorio y SalaDeComputo.

#### Scenario: Se crea un laboratorio
- **WHEN** un administrador registra un espacio llamado "Laboratorio de Redes" de tipo Laboratorio, capacidad 30 y ubicación "Bloque B"
- **THEN** el sistema devuelve el espacio creado con su identificador
- **AND** el espacio queda activo

#### Scenario: Se consulta el catálogo
- **WHEN** un usuario autenticado solicita la lista de espacios
- **THEN** el sistema devuelve todos los espacios con su tipo, capacidad y estado

#### Scenario: Se filtra por tipo
- **WHEN** un usuario autenticado solicita únicamente los espacios de tipo Auditorio
- **THEN** el sistema devuelve solo los espacios de ese tipo

#### Scenario: Se consulta un espacio inexistente
- **WHEN** un usuario autenticado solicita un espacio cuyo identificador no existe
- **THEN** el sistema responde que el espacio no fue encontrado

#### Scenario: Se actualizan los datos de un espacio
- **WHEN** un administrador cambia la capacidad de un espacio
- **THEN** el sistema guarda el nuevo valor
- **AND** las consultas posteriores devuelven la capacidad actualizada

#### Scenario: Se desactiva un espacio
- **WHEN** un administrador desactiva un espacio
- **THEN** el espacio deja de estar activo
- **AND** el espacio sigue existiendo en el catálogo para no perder el historial

### Requirement: Validación de los datos del espacio

El sistema SHALL validar los datos del espacio antes de guardarlo. El nombre SHALL ser obligatorio y único, la capacidad SHALL ser mayor que cero y el tipo SHALL ser uno de los permitidos.

#### Scenario: Se intenta crear un espacio sin nombre
- **WHEN** un administrador envía un espacio con el nombre vacío
- **THEN** el sistema rechaza la operación con un mensaje que indica que el nombre es obligatorio

#### Scenario: Se intenta repetir el nombre de un espacio
- **WHEN** un administrador intenta crear un espacio con un nombre que ya existe
- **THEN** el sistema rechaza la operación con un mensaje que indica que el nombre ya está en uso

#### Scenario: Se intenta crear un espacio con capacidad inválida
- **WHEN** un administrador envía un espacio con capacidad cero o negativa
- **THEN** el sistema rechaza la operación con un mensaje que indica que la capacidad debe ser mayor que cero

#### Scenario: Se intenta crear un espacio con un tipo desconocido
- **WHEN** un administrador envía un tipo que no es Laboratorio, Auditorio ni SalaDeComputo
- **THEN** el sistema rechaza la operación

### Requirement: Administración restringida de espacios

La consulta del catálogo SHALL estar disponible para cualquier usuario autenticado. La creación, la actualización y la desactivación SHALL estar restringidas al rol Administrador.

#### Scenario: Un docente intenta crear un espacio
- **WHEN** un usuario con rol Docente intenta registrar un espacio
- **THEN** el sistema responde que no tiene permiso

#### Scenario: Un estudiante consulta el catálogo
- **WHEN** un usuario con rol Estudiante solicita la lista de espacios
- **THEN** el sistema devuelve el catálogo sin restricción

#### Scenario: Un usuario no autenticado consulta el catálogo
- **WHEN** una petición sin token solicita la lista de espacios
- **THEN** el sistema responde que no está autenticado

### Requirement: Desactivación sin pérdida de historial

La desactivación de un espacio SHALL ser lógica: el registro NO SHALL eliminarse. Un espacio inactivo NO SHALL aceptar nuevas reservas. Las reservas ya existentes NO SHALL cancelarse automáticamente al desactivar el espacio.

#### Scenario: Desactivar un espacio no borra sus datos
- **WHEN** un administrador desactiva un espacio que tiene reservas asociadas
- **THEN** el espacio sigue siendo consultable
- **AND** su estado indica que está inactivo

#### Scenario: Las reservas existentes sobreviven a la desactivación
- **WHEN** se desactiva un espacio con reservas registradas
- **THEN** las reservas conservan su estado y su historial
- **AND** el administrador debe cancelarlas explícitamente si corresponde
