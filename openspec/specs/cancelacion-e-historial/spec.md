# cancelacion-e-historial Specification

## Purpose
Permitir que una reserva se cancele cuando deja de ser necesaria, dejando constancia del motivo y del responsable, y ofrecer un historial consultable que permita reconstruir el uso de los espacios y las decisiones tomadas sobre cada solicitud.

## Requirements

### Requirement: Cancelación de reservas

El sistema SHALL permitir cancelar una reserva en estado Pendiente o Aprobada indicando un motivo obligatorio. El dueño de la reserva SHALL poder cancelar la suya; el Administrador SHALL poder cancelar cualquiera. Cualquier otro usuario NO SHALL poder cancelarla. Al cancelar, el estado SHALL cambiar a Cancelada, SHALL registrarse el motivo y el momento, y SHALL liberarse las franjas ocupadas.

#### Scenario: El dueño cancela su reserva aprobada
- **WHEN** un docente cancela su propia reserva aprobada con el motivo "Cambio de horario de la clase"
- **THEN** la reserva pasa a estado Cancelada
- **AND** la respuesta incluye el motivo de la cancelación
- **AND** las franjas quedan libres

#### Scenario: El dueño cancela su solicitud pendiente
- **WHEN** un estudiante cancela su propia solicitud pendiente con un motivo
- **THEN** la reserva pasa a estado Cancelada

#### Scenario: Se cancela sin motivo
- **WHEN** un usuario intenta cancelar su reserva sin indicar motivo
- **THEN** el sistema rechaza la operación con un mensaje que indica que el motivo es obligatorio

#### Scenario: Un usuario cancela la reserva de otro
- **WHEN** un docente intenta cancelar una reserva de otro docente
- **THEN** el sistema responde que no tiene permiso
- **AND** la reserva conserva su estado

#### Scenario: Un administrador cancela la reserva de otro
- **WHEN** un administrador cancela la reserva de un docente con un motivo
- **THEN** la reserva pasa a estado Cancelada

#### Scenario: Se cancela una reserva rechazada
- **WHEN** un usuario intenta cancelar una reserva rechazada
- **THEN** el sistema rechaza la operación
- **AND** el mensaje indica el estado actual de la reserva

#### Scenario: Se cancela una reserva ya cancelada
- **WHEN** un usuario intenta cancelar una reserva cancelada
- **THEN** el sistema rechaza la operación

#### Scenario: Una reserva cancelada ya no aparece como activa
- **WHEN** una reserva es cancelada
- **THEN** deja de considerarse activa para la prevención de solapamientos
- **AND** la franja puede volver a solicitarse

### Requirement: Historial de reservas

El sistema SHALL permitir que un usuario consulte sus propias reservas y que un Administrador consulte todas. El historial SHALL incluir las reservas en cualquier estado y SHALL poder filtrarse por estado, espacio y rango de fechas. El resultado SHALL ordenarse por fecha de inicio descendente.

#### Scenario: Un docente consulta sus reservas
- **WHEN** un docente solicita su historial
- **THEN** el sistema devuelve únicamente sus reservas
- **AND** no incluye las de otros usuarios

#### Scenario: Un estudiante consulta sus reservas
- **WHEN** un estudiante solicita su historial
- **THEN** el sistema devuelve únicamente sus reservas

#### Scenario: Un administrador consulta todas las reservas
- **WHEN** un administrador solicita el historial global
- **THEN** el sistema devuelve las reservas de todos los usuarios

#### Scenario: Un docente intenta consultar el historial global
- **WHEN** un docente solicita el historial global
- **THEN** el sistema responde que no tiene permiso

#### Scenario: Se filtra por estado
- **WHEN** un administrador filtra el historial por estado Cancelada
- **THEN** el sistema devuelve únicamente las reservas canceladas

#### Scenario: Se filtra por espacio
- **WHEN** un administrador filtra el historial por un espacio
- **THEN** el sistema devuelve únicamente las reservas de ese espacio

#### Scenario: Se filtra por rango de fechas
- **WHEN** un administrador filtra el historial entre dos fechas
- **THEN** el sistema devuelve únicamente las reservas cuyo inicio está dentro del rango

#### Scenario: El historial incluye el motivo y el responsable
- **WHEN** un administrador consulta una reserva rechazada
- **THEN** la respuesta incluye el motivo del rechazo
- **AND** incluye el identificador del administrador que decidió y el momento de la decisión

#### Scenario: Un usuario consulta una reserva ajena
- **WHEN** un docente intenta consultar por identificador una reserva de otro usuario
- **THEN** el sistema responde que no tiene permiso
