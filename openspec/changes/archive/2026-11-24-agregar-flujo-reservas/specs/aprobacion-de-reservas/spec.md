# Delta de especificación

## Purpose

Cerrar el ciclo de una solicitud con una decisión explícita del administrador, dejando registro de quién decidió, cuándo y por qué, de modo que el uso de los espacios sea auditable y no dependa de la memoria de quien autorizó.

## ADDED Requirements

### Requirement: Aprobación de solicitudes

El sistema SHALL permitir que un usuario con rol Administrador apruebe una solicitud en estado Pendiente. Al aprobarla, el estado SHALL cambiar a Aprobada y SHALL registrarse el administrador responsable y el momento de la decisión. Las franjas ya ocupadas por la solicitud SHALL conservarse.

#### Scenario: Se aprueba una solicitud pendiente
- **WHEN** un administrador aprueba una solicitud pendiente
- **THEN** la reserva pasa a estado Aprobada
- **AND** la respuesta indica el administrador que decidió y el momento de la decisión
- **AND** las franjas siguen ocupadas

#### Scenario: Un docente intenta aprobar
- **WHEN** un usuario con rol Docente intenta aprobar una solicitud
- **THEN** el sistema responde que no tiene permiso

#### Scenario: Un estudiante intenta aprobar
- **WHEN** un usuario con rol Estudiante intenta aprobar una solicitud
- **THEN** el sistema responde que no tiene permiso

#### Scenario: Se aprueba una solicitud ya aprobada
- **WHEN** un administrador intenta aprobar una reserva que ya está aprobada
- **THEN** el sistema rechaza la operación
- **AND** el mensaje indica que la reserva no está pendiente

#### Scenario: Se aprueba una solicitud rechazada
- **WHEN** un administrador intenta aprobar una reserva rechazada
- **THEN** el sistema rechaza la operación

#### Scenario: Se aprueba una solicitud inexistente
- **WHEN** un administrador intenta aprobar una reserva cuyo identificador no existe
- **THEN** el sistema responde que la reserva no fue encontrada

### Requirement: Rechazo de solicitudes

El sistema SHALL permitir que un usuario con rol Administrador rechace una solicitud en estado Pendiente indicando un motivo obligatorio. Al rechazarla, el estado SHALL cambiar a Rechazada, SHALL registrarse el motivo y el responsable, y SHALL liberarse las franjas ocupadas.

#### Scenario: Se rechaza una solicitud con motivo
- **WHEN** un administrador rechaza una solicitud pendiente con el motivo "El laboratorio estará en mantenimiento"
- **THEN** la reserva pasa a estado Rechazada
- **AND** la respuesta incluye el motivo del rechazo
- **AND** las franjas quedan libres

#### Scenario: Se rechaza sin motivo
- **WHEN** un administrador intenta rechazar una solicitud sin indicar motivo
- **THEN** el sistema rechaza la operación con un mensaje que indica que el motivo es obligatorio

#### Scenario: Se rechaza con un motivo en blanco
- **WHEN** un administrador intenta rechazar una solicitud con un motivo compuesto solo de espacios
- **THEN** el sistema rechaza la operación

#### Scenario: Un docente intenta rechazar
- **WHEN** un usuario con rol Docente intenta rechazar una solicitud
- **THEN** el sistema responde que no tiene permiso

#### Scenario: Se rechaza una solicitud ya cancelada
- **WHEN** un administrador intenta rechazar una reserva cancelada
- **THEN** el sistema rechaza la operación
- **AND** el mensaje indica que la reserva no está pendiente
