# Diseño: límite de cuatro horas consecutivas

## Context

La regla llega con el sistema ya en funcionamiento y con dos incrementos entregados. El flujo de creación ya valida horario, espacio activo y solapamiento; el nuevo límite se suma a esas validaciones. La decisión de diseño más importante no es técnica sino de interpretación: qué significa "durante más de cuatro horas consecutivas".

## Análisis de impacto

| Elemento | Impacto | Detalle |
|---|---|---|
| Modelo de datos | Ninguno | No se agregan tablas ni columnas. El cálculo usa `reservas` y sus estados. |
| Migraciones | Ninguna | El esquema no cambia. |
| Dominio | Alto | `ReglasDeReserva` incorpora la duración máxima y el cálculo de bloques. |
| Repositorio | Medio | Nueva consulta de reservas activas por usuario, espacio y fecha. |
| Servicio de reservas | Medio | La creación ejecuta una validación adicional antes de guardar. |
| Endpoints | Ninguno | Mismas rutas y cuerpos. Cambian las respuestas posibles (400). |
| Contrato Swagger | Bajo | Se documenta el nuevo 400 en la creación; no hay rutas nuevas. |
| Pruebas existentes | Ninguno esperado | Ninguna reserva de prueba supera las cuatro horas ni acumula más de cuatro. Se ejecuta la suite completa para confirmarlo. |
| Datos históricos | Ninguno | No se reescriben reservas existentes. |
| Documentación | Medio | README e informe final explican la interpretación. |

## Goals / Non-Goals

**Goals:**

- Rechazar una solicitud individual de más de cuatro horas con un mensaje claro.
- Rechazar la acumulación de reservas activas contiguas del mismo usuario en el mismo espacio cuando el bloque supera las cuatro horas.
- Mantener la regla aislada y probada sin base de datos.
- No romper ninguna validación ni prueba anterior.

**Non-Goals:**

- Limitar el total de horas por día.
- Limitar la ocupación del usuario en espacios distintos.
- Recalcular o recortar reservas ya existentes.
- Notificar al usuario cuando está por alcanzar el límite.

## Decisions

### D1. Se aplican las dos lecturas: por solicitud y por acumulación

**Decisión:** una solicitud no puede durar más de cuatro horas y, además, la cadena de reservas activas contiguas del mismo usuario en el mismo espacio no puede superar las cuatro horas.

**Por qué:** limitar solo la solicitud individual deja una evasión trivial: dos reservas de dos horas contiguas. La regla habla de "reservar durante más de cuatro horas consecutivas", no de "una reserva de más de cuatro horas".

**Alternativa descartada:** validar solo la duración de cada solicitud. Es más simple, pero no cumple el objetivo de la regla.

### D2. El bloque se calcula por usuario y por espacio

**Decisión:** solo cuentan las reservas activas del mismo usuario en el mismo espacio.

**Por qué:** el texto dice "un espacio académico". Además, la finalidad es evitar que una persona monopolice un espacio; ocupar dos espacios distintos a la misma hora ya está limitado por la lógica y no es el problema que la regla describe.

**Alternativa descartada:** sumar todas las reservas del usuario sin importar el espacio. Sería una regla más restrictiva que la enunciada y afectaría casos legítimos.

### D3. Las reservas contiguas se conectan; un hueco corta la cadena

**Decisión:** dos reservas se consideran consecutivas si el fin de una es igual o posterior al inicio de la otra. Un intervalo libre de cualquier duración corta la cadena.

**Por qué:** "consecutivas" significa sin interrupción. Un hueco de 30 minutos permite al usuario usar el espacio en otro momento y reinicia el conteo.

**Consecuencia:** el algoritmo calcula el componente conexo que contiene a la nueva solicitud y compara su extremo final con su extremo inicial. No se suman reservas desconectadas.

### D4. Solo cuentan las reservas activas

**Decisión:** Pendiente y Aprobada cuentan; Rechazada y Cancelada no.

**Por qué:** una reserva rechazada o cancelada ya liberó sus franjas y no representa ocupación. Contarla castigaría al usuario por una solicitud que el sistema no aceptó.

### D5. La validación ocurre al crear, no al aprobar

**Decisión:** el límite se evalúa en la creación de la solicitud. La aprobación no vuelve a calcularlo.

**Por qué:** una solicitud pendiente ya ocupa la franja (decisión D2 del incremento anterior). Si el límite se evaluara al aprobar, un usuario podría acumular solicitudes que después habría que rechazar, y el administrador cargaría con un problema que el sistema pudo detectar antes. Además, como las franjas ya están retenidas, la aprobación no puede aumentar la ocupación.

**Alternativa descartada:** evaluar solo al aprobar. Traslada el conflicto al final del flujo.

### D6. El algoritmo es una función pura sobre intervalos

**Decisión:** `ReglasDeReserva.ValidarLimiteConsecutivo` recibe una colección de pares (inicio, fin) y la nueva solicitud, y no conoce EF Core ni HTTP.

**Por qué:** permite probar la lógica de conexión de bloques con casos límite sin levantar la API. Es la parte más fácil de equivocar y la más barata de probar en aislamiento.

## Escenarios límite considerados

| Caso | Resultado |
|---|---|
| Una sola reserva de 4 h | Aceptada |
| Una sola reserva de 4 h 30 min | Rechazada por duración máxima |
| 2 h + 2 h contiguas | Aceptadas (bloque de 4 h) |
| 2 h + 2 h + 1 h contiguas | La tercera se rechaza (bloque de 5 h) |
| 2 h, hueco de 30 min, 2 h | Aceptadas (dos bloques) |
| 2 h del usuario A + 2 h del usuario B + 1 h del usuario A | La última se acepta: el bloque de A es de 1 h |
| 2 h canceladas + 2 h + 1 h | La última se acepta: el bloque real es de 3 h |
| 4 h en el espacio X + 4 h en el espacio Y | Aceptadas: el límite es por espacio |

## Riesgos / compromisos

- **Interpretación.** Si la institución quisiera limitar la ocupación total del usuario en cualquier espacio, habría que cambiar el filtro de la consulta y un escenario. El diseño lo permite sin tocar el algoritmo.
- **Solicitudes pendientes.** Un usuario puede retener cuatro horas con solicitudes que luego no se aprueben. Es consistente con la decisión de que lo pendiente ocupa la franja.
- **Sin aviso previo.** El usuario se entera al recibir el 400. Un aviso en la consulta de disponibilidad sería una mejora, no parte de este cambio.
- **Sin migración de datos.** Si existieran reservas históricas de más de cuatro horas, no se corrigen. En este repositorio no hay datos productivos; se documenta para un despliegue real.
