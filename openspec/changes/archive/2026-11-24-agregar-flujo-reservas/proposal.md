# Propuesta: flujo de solicitudes, aprobación, cancelación e historial

## Why

El catálogo y las cuentas ya existen, pero el sistema todavía no resuelve el problema que lo motiva: dos personas pueden reservar el mismo espacio a la misma hora. Este incremento implementa el ciclo completo de una reserva —solicitar, validar disponibilidad, aprobar o rechazar, cancelar y consultar el historial— y convierte la prevención de solapamientos en una garantía de la base de datos, no en una validación que se puede saltar con dos peticiones simultáneas.

Sin este incremento no hay nada que evaluar en la sustentación: el valor del proyecto está en este flujo.

## What Changes

- Se agrega la entidad `Reserva` con su máquina de estados: Pendiente, Aprobada, Rechazada y Cancelada.
- Se agrega la entidad `FranjaReservada`, que representa cada bloque de 30 minutos ocupado por una reserva activa.
- Se impone un índice único sobre `(espacio, fecha, índice de franja)`: la base rechaza el solapamiento aunque dos peticiones lo intenten al mismo tiempo.
- Se implementa la consulta de disponibilidad por espacio y fecha, que devuelve las 30 franjas del día con su estado.
- Se implementan las validaciones de horario: dentro de 07:00–22:00, misma fecha, múltiplos de 30 minutos, duración mínima de 30 minutos y comienzo en el futuro.
- Se implementan la aprobación y el rechazo con motivo, restringidos al rol Administrador.
- Se implementa la cancelación con motivo, por el dueño o por un administrador.
- Se implementa el historial propio y el historial global con filtros por estado, espacio y rango de fechas.
- Se agregan pruebas de integración del flujo completo y una prueba de concurrencia con diez peticiones simultáneas.
- Se agrega la tabla `reservas` y `franjas_reservadas` con su migración.

## Capabilities

### New Capabilities

- `solicitudes-de-reserva`: creación de solicitudes, validaciones de horario, consulta de disponibilidad y prevención de reservas simultáneas.
- `aprobacion-de-reservas`: decisión del administrador sobre una solicitud pendiente.
- `cancelacion-e-historial`: cancelación con motivo y consulta del historial con filtros.

### Modified Capabilities

Ninguna. Las capacidades anteriores no cambian su comportamiento. La única interacción es que un espacio inactivo no admite reservas, regla que ya estaba declarada en `espacios-academicos` y que ahora se hace efectiva.

## Impact

- **Código nuevo**: entidades `Reserva` y `FranjaReservada`, reglas de horario, `ServicioReservas`, repositorio de reservas, endpoints de reservas y disponibilidad, pruebas de flujo y de concurrencia.
- **Base de datos**: dos tablas nuevas, un índice único y dos claves foráneas con borrado restringido. Migración nueva.
- **API**: seis endpoints nuevos. El contrato existente no se modifica.
- **Riesgo principal**: la concurrencia. Se mitiga con el índice único y se verifica con una prueba real de diez peticiones paralelas.
- **Decisión que afecta al usuario**: una solicitud pendiente ocupa la franja. Se documenta en el diseño y en la especificación porque cambia la experiencia de uso.
- **Fuera de alcance**: notificaciones, reservas recurrentes y cobros. Sin cambios respecto del plan.
