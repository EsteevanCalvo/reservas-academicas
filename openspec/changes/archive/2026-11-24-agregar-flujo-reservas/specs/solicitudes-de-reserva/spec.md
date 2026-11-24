# Delta de especificación

## Purpose

Permitir que docentes y estudiantes soliciten un espacio académico en una fecha y hora determinadas, con reglas de horario explícitas y con la garantía de que dos reservas activas no puedan ocupar la misma franja del mismo espacio, incluso si las solicitudes llegan al mismo tiempo.

## ADDED Requirements

### Requirement: Creación de solicitudes de reserva

El sistema SHALL permitir que un usuario con rol Docente o Estudiante cree una solicitud de reserva indicando espacio, fecha y hora de inicio y de fin. La solicitud SHALL crearse en estado Pendiente y SHALL quedar asociada al usuario autenticado que la crea.

#### Scenario: Un docente crea una solicitud válida
- **WHEN** un docente solicita el "Laboratorio de Redes" para mañana de 08:00 a 10:00
- **THEN** el sistema devuelve la solicitud creada con estado Pendiente
- **AND** la solicitud queda asociada al docente que la creó
- **AND** la respuesta incluye el espacio, el usuario, el inicio y el fin

#### Scenario: Un estudiante crea una solicitud válida
- **WHEN** un estudiante solicita un espacio disponible para mañana de 14:00 a 15:30
- **THEN** el sistema crea la solicitud en estado Pendiente

#### Scenario: Un administrador intenta crear una solicitud
- **WHEN** un usuario con rol Administrador intenta crear una solicitud de reserva
- **THEN** el sistema responde que no tiene permiso
- **AND** el mensaje indica que las solicitudes las crean docentes y estudiantes

#### Scenario: Un usuario no autenticado intenta crear una solicitud
- **WHEN** una petición sin token intenta crear una solicitud
- **THEN** el sistema responde que no está autenticado

#### Scenario: Se solicita un espacio inexistente
- **WHEN** un docente solicita un espacio cuyo identificador no existe
- **THEN** el sistema responde que el espacio no fue encontrado

#### Scenario: Se solicita un espacio desactivado
- **WHEN** un docente solicita un espacio que está inactivo
- **THEN** el sistema rechaza la solicitud
- **AND** el mensaje indica que el espacio no admite reservas

#### Scenario: La solicitud no recibe datos del usuario que la crea
- **WHEN** un docente envía una solicitud indicando un identificador de usuario distinto del suyo
- **THEN** el sistema ignora ese dato
- **AND** la solicitud queda asociada al usuario autenticado

### Requirement: Validación del horario institucional

El sistema SHALL validar toda solicitud antes de guardarla. La reserva SHALL estar dentro del horario de 07:00 a 22:00, SHALL comenzar y terminar el mismo día, SHALL comenzar y terminar en múltiplos de 30 minutos, SHALL durar al menos 30 minutos y SHALL comenzar en el futuro. Una solicitud que incumpla cualquiera de estas reglas NO SHALL guardarse.

#### Scenario: La reserva está fuera del horario de atención
- **WHEN** un docente solicita un espacio de 06:00 a 08:00
- **THEN** el sistema rechaza la solicitud con un mensaje que indica el horario permitido

#### Scenario: La reserva termina después del cierre
- **WHEN** un docente solicita un espacio de 21:00 a 23:00
- **THEN** el sistema rechaza la solicitud con un mensaje que indica el horario permitido

#### Scenario: La hora de fin no es posterior a la de inicio
- **WHEN** un docente solicita un espacio de 10:00 a 10:00
- **THEN** el sistema rechaza la solicitud con un mensaje que indica que el fin debe ser posterior al inicio

#### Scenario: La reserva no está alineada a la cuadrícula
- **WHEN** un docente solicita un espacio de 08:15 a 09:15
- **THEN** el sistema rechaza la solicitud con un mensaje que indica que las horas deben ser múltiplos de 30 minutos

#### Scenario: La reserva dura menos de la duración mínima
- **WHEN** un docente solicita un espacio de 08:00 a 08:15
- **THEN** el sistema rechaza la solicitud

#### Scenario: La reserva comienza en el pasado
- **WHEN** un docente solicita un espacio para una hora que ya pasó
- **THEN** el sistema rechaza la solicitud con un mensaje que indica que debe comenzar en el futuro

#### Scenario: La reserva cruza la medianoche
- **WHEN** un docente solicita un espacio de 21:00 a 01:00 del día siguiente
- **THEN** el sistema rechaza la solicitud con un mensaje que indica que debe ser del mismo día

#### Scenario: Una solicitud válida en el límite del horario se acepta
- **WHEN** un docente solicita un espacio de 07:00 a 07:30
- **THEN** el sistema acepta la solicitud

### Requirement: Prevención de reservas simultáneas

El sistema SHALL impedir que dos reservas activas ocupen la misma franja del mismo espacio. Se consideran activas las reservas en estado Pendiente o Aprobada. La garantía SHALL sostenerse en una restricción de la base de datos y NO SHALL depender únicamente de una validación previa en la aplicación.

#### Scenario: Dos solicitudes cubren exactamente la misma franja
- **WHEN** un docente ya tiene una solicitud pendiente de 08:00 a 10:00 en un espacio
- **AND** otro docente solicita el mismo espacio de 08:00 a 10:00
- **THEN** el sistema rechaza la segunda solicitud con un conflicto
- **AND** la primera solicitud conserva su estado

#### Scenario: Dos solicitudes se solapan parcialmente
- **WHEN** existe una solicitud activa de 08:00 a 10:00 en un espacio
- **AND** un docente solicita ese espacio de 09:00 a 11:00
- **THEN** el sistema rechaza la segunda solicitud con un conflicto

#### Scenario: Una solicitud rodea a otra
- **WHEN** existe una reserva activa de 09:00 a 10:00 en un espacio
- **AND** un docente solicita ese espacio de 08:00 a 11:00
- **THEN** el sistema rechaza la solicitud con un conflicto

#### Scenario: Las franjas contiguas están permitidas
- **WHEN** existe una reserva activa de 08:00 a 09:00 en un espacio
- **AND** un docente solicita ese espacio de 09:00 a 10:00
- **THEN** el sistema acepta la solicitud
- **AND** ambas reservas quedan activas

#### Scenario: Dos espacios distintos admiten la misma franja
- **WHEN** un docente reserva un espacio de 08:00 a 09:00
- **AND** otro docente reserva otro espacio de 08:00 a 09:00
- **THEN** el sistema acepta ambas solicitudes

#### Scenario: Diez solicitudes simultáneas sobre la misma franja
- **WHEN** diez peticiones idénticas solicitan el mismo espacio y la misma franja al mismo tiempo
- **THEN** exactamente una solicitud se crea
- **AND** las nueve restantes reciben un conflicto
- **AND** la base contiene una sola reserva activa para esa franja

#### Scenario: Una solicitud rechazada libera la franja
- **WHEN** una solicitud pendiente es rechazada
- **THEN** otro usuario puede solicitar la misma franja del mismo espacio

#### Scenario: Una solicitud cancelada libera la franja
- **WHEN** una reserva aprobada es cancelada
- **THEN** otro usuario puede solicitar la misma franja del mismo espacio

### Requirement: Consulta de disponibilidad

El sistema SHALL permitir consultar, para un espacio y una fecha, cuáles de las treinta franjas de 30 minutos están disponibles. La consulta SHALL estar disponible para cualquier usuario autenticado.

#### Scenario: Se consulta un día sin reservas
- **WHEN** un usuario autenticado consulta la disponibilidad de un espacio para una fecha futura sin reservas
- **THEN** el sistema devuelve treinta franjas con la primera a las 07:00 y la última a las 21:30
- **AND** todas las franjas figuran disponibles

#### Scenario: Se consulta un día con reservas
- **WHEN** existe una reserva activa de 08:00 a 09:00 en un espacio
- **AND** un usuario consulta la disponibilidad de ese espacio para esa fecha
- **THEN** las franjas de 08:00 a 08:30 y de 08:30 a 09:00 figuran ocupadas
- **AND** las demás figuran disponibles

#### Scenario: Se consulta un espacio inexistente
- **WHEN** un usuario consulta la disponibilidad de un espacio que no existe
- **THEN** el sistema responde que el espacio no fue encontrado

#### Scenario: Se consultan franjas que ya pasaron
- **WHEN** un usuario consulta la disponibilidad del día de hoy
- **THEN** las franjas cuyo inicio ya pasó figuran como no disponibles
- **AND** las franjas futuras conservan su estado real

#### Scenario: La consulta no revela datos de otros usuarios
- **WHEN** un estudiante consulta la disponibilidad de un espacio
- **THEN** la respuesta indica únicamente qué franjas están ocupadas
- **AND** no incluye qué usuario reservó
