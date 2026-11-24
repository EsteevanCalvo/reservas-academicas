# Diseño: flujo de solicitudes, aprobación, cancelación e historial

## Context

El incremento anterior dejó una base estable: capas separadas, autenticación con roles, manejo uniforme de errores y pruebas de integración contra SQLite real. Sobre esa base hay que resolver el problema central del proyecto, que tiene dos partes distintas:

1. **Reglas de negocio**: qué es una reserva válida.
2. **Integridad bajo concurrencia**: cómo se impide que dos reservas válidas ocupen la misma franja.

La segunda parte es la que no se resuelve con un `if`. Si la validación vive solo en el servicio, dos peticiones simultáneas pueden leer "libre" al mismo tiempo y escribir ambas.

## Goals / Non-Goals

**Goals:**

- Que una franja de un espacio no pueda estar ocupada por dos reservas activas, ni siquiera con peticiones concurrentes.
- Que la regla sea verificable desde fuera: la API responde 409 y la base contiene una sola reserva.
- Que la disponibilidad se pueda consultar antes de solicitar.
- Que cada decisión (aprobar, rechazar, cancelar) quede registrada con su responsable y su motivo.
- Que las pruebas del flujo completo se puedan leer como la especificación.

**Non-Goals:**

- Notificaciones al usuario.
- Reservas recurrentes o periódicas.
- Listas de espera.
- Paginación del historial.
- Zona horaria configurable.

## Decisions

### D1. Cuadrícula de franjas con índice único

Cada reserva se descompone en franjas de 30 minutos, que se guardan en `franjas_reservadas`. La combinación `(espacio_id, fecha, indice_franja)` es única.

**Por qué:** el solapamiento de intervalos no se puede expresar como una restricción de unicidad sobre `(inicio, fin)`. La cuadrícula sí, porque convierte el problema en "una fila por bloque ocupado". Dos transacciones concurrentes que intenten ocupar el mismo bloque chocan contra el índice, y SQLite rechaza la segunda.

**Alternativas descartadas:**

- *Validar solo en el servicio.* Dos peticiones pueden pasar la validación antes de que cualquiera escriba. No previene, solo reduce la probabilidad.
- *Bloquear la tabla durante la validación.* Correcto pero serializa toda la base y depende de una API de bloqueo que SQLite no ofrece por filas.
- *Guardar solo `inicio` y `fin` y consultar solapamientos al aprobar.* Deja ventanas de carrera y hace que el resultado dependa del orden de llegada.

**Consecuencia aceptada:** las reservas deben comenzar y terminar en múltiplos de 30 minutos. Se documenta como regla del negocio.

### D2. Una solicitud pendiente ocupa la franja

Se considera activa toda reserva en estado Pendiente o Aprobada. Ambas escriben sus franjas.

**Por qué:** si solo la aprobación bloqueara, se podrían acumular solicitudes idénticas condenadas a rechazarse, y el administrador tendría que resolver un conflicto que el sistema ya conocía. Con esta regla, el solicitante recibe la respuesta en el momento en que pide.

**Alternativa descartada:** permitir varias solicitudes pendientes sobre la misma franja y decidir al aprobar. Se descartó porque traslada el problema al final del flujo y empeora la experiencia.

### D3. Las franjas se liberan al rechazar o cancelar

Rechazar o cancelar una reserva elimina sus franjas. El índice único solo cubre reservas activas.

**Por qué:** si las franjas se conservaran, una franja rechazada quedaría bloqueada para siempre. La alternativa —conservarlas y filtrar por estado en las consultas— haría que el índice único impidiera reutilizar la franja, que es justo lo que no se quiere.

**Consecuencia:** la cancelación y el rechazo son operaciones que escriben en dos tablas dentro de la misma transacción. Si falla una, no se aplica ninguna.

### D4. Máquina de estados explícita

| Estado actual | Acción | Estado nuevo | Quién |
|---|---|---|---|
| Pendiente | Aprobar | Aprobada | Administrador |
| Pendiente | Rechazar | Rechazada | Administrador |
| Pendiente | Cancelar | Cancelada | Dueño o Administrador |
| Aprobada | Cancelar | Cancelada | Dueño o Administrador |
| Rechazada | Cualquiera | — | Nadie: es un estado final |
| Cancelada | Cualquiera | — | Nadie: es un estado final |

Las transiciones inválidas devuelven 400 con un mensaje que indica el estado actual. La entidad `Reserva` las valida; el servicio no repite la regla.

### D5. Hora local del servidor

Las reservas usan `DateTime` sin zona horaria y se comparan contra `DateTime.Now`. El sistema asume una única zona horaria institucional.

**Por qué:** el problema es de una institución en una ciudad. Introducir zonas horarias obliga a decidir qué pasa con el horario de verano y agrega un eje de error que nadie pidió. Se documenta como limitación y como trabajo futuro.

### D6. El conflicto de concurrencia se traduce a un mensaje de negocio

La unidad de trabajo inspecciona la excepción de SQLite. Si la violación corresponde al índice único de franjas, lanza `ConflictoException` con el mensaje "El espacio ya está reservado en ese horario". Cualquier otra violación de integridad produce un mensaje genérico.

**Por qué:** el usuario necesita saber qué pasó, no ver un error 500. La traducción vive en infraestructura porque es el único lugar que conoce SQLite.

### D7. El historial no se pagina

Se devuelve la lista filtrada y ordenada por fecha de inicio descendente. Con el volumen de una institución pequeña es suficiente. El contrato admite agregar paginación sin romper a los clientes actuales, porque una lista y una página se distinguen por el cuerpo de la respuesta, no por la ruta.

**Alternativa descartada:** paginar desde el inicio. Agrega parámetros, índices y pruebas que no aportan al objetivo académico.

## Revisión de la especificación

Antes de implementar se revisó el conjunto contra el plan de ingeniería y se encontraron tres puntos que se corrigieron en la especificación, no en el código:

1. **Ambigüedad sobre las solicitudes pendientes.** El plan no decía si bloqueaban la franja. Se resolvió en D2 y se agregó un escenario explícito ("una solicitud pendiente impide otra solicitud sobre la misma franja"). Sin esa decisión, la implementación habría sido arbitraria.
2. **La cancelación debía liberar la franja.** La primera redacción no lo decía. Se agregó el escenario "cancelar una reserva permite volver a reservar esa franja". Es un comportamiento observable y, sin él, el índice único habría dejado franjas bloqueadas para siempre.
3. **El rechazo sin motivo.** El plan decía "rechazo motivado" pero no definía el efecto de un motivo vacío. Se fijó como error 400 y se agregó su escenario.

También se verificó que cada requisito del plan (RF06–RF12) tenga al menos un escenario y que ningún escenario dependa de detalles internos. Las tareas de `tasks.md` cubren la implementación y su verificación.

## Riesgos / compromisos

- **La cuadrícula impone granularidad de 30 minutos.** Es una decisión de negocio, no una limitación técnica accidental. Si la institución necesitara reservas de 15 minutos, habría que migrar las franjas existentes.
- **Una solicitud pendiente bloquea.** Un usuario podría acaparar franjas solicitando de más. No se implementa un límite de solicitudes activas por usuario porque nadie lo pidió; se registra como trabajo futuro.
- **El rechazo y la cancelación borran filas.** El historial conserva la reserva y su motivo, pero no las franjas. Es suficiente para auditar qué pasó; no se puede reconstruir la cuadrícula histórica. Se documenta.
- **La prueba de concurrencia depende de SQLite.** Con diez peticiones y WAL el comportamiento es estable, pero no es una prueba de carga. Su objetivo es demostrar que la restricción existe, no medir rendimiento.
