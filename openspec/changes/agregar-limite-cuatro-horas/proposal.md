# Propuesta: límite de cuatro horas consecutivas por usuario y espacio

## Why

La institución incorporó una regla nueva después de la primera entrega: *"Ningún usuario podrá reservar un espacio académico durante más de cuatro horas consecutivas."* El sistema actual permite reservas de cualquier duración dentro del horario 07:00–22:00, de modo que un usuario podría ocupar un laboratorio durante quince horas seguidas. La regla no estaba en el plan original: llega como cambio funcional y debe entrar por el mismo camino que el resto, con la especificación actualizada antes que el código.

La regla admite dos lecturas y hay que elegir una:

- **Por solicitud:** una reserva individual no puede durar más de cuatro horas.
- **Por acumulación:** un usuario no puede ocupar el mismo espacio más de cuatro horas seguidas, aunque reparta el tiempo en varias solicitudes contiguas.

Se implementan **las dos**. Limitar solo la solicitud individual se evade creando dos reservas de dos horas seguidas, que es exactamente el comportamiento que la regla quiere impedir. El límite se aplica por usuario y por espacio porque el texto dice "un espacio académico".

## What Changes

- Se agrega una validación de duración máxima de cuatro horas a cada solicitud.
- Se agrega una validación de acumulación: se calcula el bloque continuo de reservas activas del mismo usuario en el mismo espacio que toca o solapa la nueva solicitud, y se rechaza si el bloque supera las cuatro horas.
- Se agrega al repositorio una consulta de reservas activas de un usuario en un espacio y una fecha.
- Se agregan escenarios a la capacidad `solicitudes-de-reserva`.
- Se agregan pruebas unitarias del algoritmo de bloques y pruebas de integración del límite.
- No hay cambios de esquema: la regla se evalúa con los datos existentes.

## Capabilities

### New Capabilities

Ninguna.

### Modified Capabilities

- `solicitudes-de-reserva`: se agrega el requisito "Límite de cuatro horas consecutivas" con sus escenarios. Los requisitos existentes no cambian.

## Impact

- **Código afectado**: `ReglasDeReserva` (dominio), `IRepositorioReservas` y su implementación, `ServicioReservas` (creación).
- **Contrato de la API**: sin cambios de rutas ni de cuerpos. Una solicitud que antes se aceptaba ahora puede devolver 400. Es un cambio de comportamiento intencional.
- **Base de datos**: sin migración.
- **Datos existentes**: no se reescriben. Las reservas ya creadas conservan su duración; la regla se aplica a las solicitudes nuevas.
- **Pruebas**: se agregan casos nuevos y se ejecuta la suite completa para detectar regresiones. En particular, las reservas de dos y tres horas que ya se usaban en las pruebas deben seguir funcionando.
- **Documentación**: se actualizan el README y el informe final con la interpretación adoptada y sus límites.
- **Riesgo**: si la interpretación correcta fuera "por usuario en cualquier espacio", el alcance sería distinto. Se documenta la decisión y se deja el algoritmo aislado para poder extenderlo sin tocar el resto.
