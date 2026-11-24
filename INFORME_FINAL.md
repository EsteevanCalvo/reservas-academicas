# Informe final

## 1. Resumen ejecutivo

Se construyó un **sistema de gestión de reservas de espacios académicos** para laboratorios, auditorios y salas de cómputo, entregado como API REST con autenticación por roles, persistencia en SQLite y una suite de **108 pruebas automatizadas**.

El trabajo siguió las cuatro etapas pedidas:

1. **Plan de ingeniería** escrito y registrado antes de instalar OpenSpec o escribir código.
2. **Especificaciones OpenSpec** para las ocho funcionalidades mínimas, revisadas y validadas antes de implementar.
3. **Implementación incremental** en dos entregas, cada una precedida por su commit de especificación.
4. **Cambio funcional de la fase 4**: la regla de las cuatro horas consecutivas, analizada, especificada, implementada y probada sin romper el resto del sistema.

El historial de Git demuestra la secuencia: el primer commit es el plan, y cada commit de especificación aparece antes del commit de código que la implementa. El repositorio contiene además el informe y las especificaciones vigentes en `openspec/specs/`.

| Indicador | Resultado |
|---|---|
| Capacidades de OpenSpec | 5 |
| Requisitos especificados | 18 |
| Escenarios verificables | 102 |
| Cambios de OpenSpec archivados | 3 |
| Proyectos .NET | 4 |
| Archivos C# versionados | 61 |
| Pruebas automatizadas | 108, todas en verde |
| Advertencias de compilación | 0 (`TreatWarningsAsErrors`) |
| Migraciones de EF Core | 2 |

---

## 2. Contexto y objetivo

### Problemática

La reserva de laboratorios, auditorios y salas de cómputo se gestionaba de forma manual. Eso producía:

- Doble asignación de un mismo espacio en el mismo horario.
- Pérdida del historial de quién reservó, quién aprobó y por qué se rechazó.
- Falta de visibilidad sobre la disponibilidad real.
- Ausencia de reglas uniformes de horario y duración.

### Objetivo

Construir un sistema que permita administrar espacios y usuarios, recibir solicitudes de reserva, evitar solapamientos incluso con peticiones concurrentes, gestionar la aprobación, el rechazo y la cancelación, y consultar disponibilidad e historial, con reglas explícitas y verificables.

### Alcance

- API REST documentada con Swagger; sin panel web.
- Roles fijos: Administrador, Docente y Estudiante.
- Horario institucional de 07:00 a 22:00 en franjas de 30 minutos.
- Una zona horaria única.
- Persistencia local en SQLite.
- Regla adicional incorporada al final: máximo cuatro horas consecutivas por usuario y espacio.

---

## 3. Entregables

| Entregable | Ubicación | Estado |
|---|---|---|
| Repositorio publicado | <https://github.com/EsteevanCalvo/reservas-academicas> | Público, rama `main` |
| Plan de ingeniería | `PLAN_INGENIERIA.md` | Commit inicial `cad1c5a`, 414 líneas |
| Especificaciones OpenSpec | `openspec/specs/` y `openspec/changes/archive/` | 5 capacidades, 18 requisitos, 102 escenarios, 3 cambios archivados |
| Código funcional | `src/Reservas.Core`, `src/Reservas.Infrastructure`, `src/Reservas.Api` | Compila sin advertencias |
| Pruebas automatizadas | `src/Reservas.Tests` | 108 pruebas en verde |
| README | `README.md` | Instrucciones de uso, endpoints, reglas y decisiones |
| Informe final | `INFORME_FINAL.md` | Este documento |

---

## 4. Proceso de desarrollo

### 4.1 Etapa 1: plan de ingeniería

**Commit:** `cad1c5a` — `docs(plan): agregar planificacion de ingenieria previa al desarrollo` (2026-11-23).

El plan se escribió antes de inicializar OpenSpec y antes de crear cualquier proyecto. Contiene:

- Problemática, necesidades, actores, alcance y limitaciones.
- Solución propuesta, funcionalidades prioritarias, módulos y estrategia progresiva.
- Arquitectura, stack, persistencia, organización de componentes y **decisiones técnicas comparadas con alternativas descartadas**.
- Metodología, etapas, orden, dependencias y criterios de terminación.
- **Riesgos** con mitigaciones y forma de verificación.
- Uso previsto de SDD y de inteligencia artificial.

Verificación:

```powershell
git show --stat cad1c5a
# Solo incluye PLAN_INGENIERIA.md
```

### 4.2 Etapa 2: especificaciones con OpenSpec

**Commit de inicialización:** `d7c4ef1` — OpenSpec con esquema `spec-driven`, herramientas de OpenCode y lenguaje español.

Se definieron dos incrementos, cada uno con propuesta, diseño, tareas y deltas de especificación:

| Cambio | Commit de spec | Capacidades afectadas |
|---|---|---|
| `agregar-gestion-espacios-usuarios` | `0f80e86` | `espacios-academicos`, `usuarios-y-permisos` |
| `agregar-flujo-reservas` | `d49f736` | `solicitudes-de-reserva`, `aprobacion-de-reservas`, `cancelacion-e-historial` |
| `agregar-limite-cuatro-horas` | `9093832` | `solicitudes-de-reserva` (nuevo requisito) |

Cada especificación se validó con:

```powershell
$env:OPENSPEC_TELEMETRY='0'
openspec validate <cambio> --type change --strict
```

Regla de trabajo aplicada: **ningún desarrollo sin una especificación revisada y validada**. Las tareas de cada cambio incluyen su propio criterio de verificación.

### 4.3 Etapa 3: implementación

| Incremento | Commit de código | Contenido | Pruebas |
|---|---|---|---|
| 1 | `ce0ecda` | Dominio de espacios y usuarios, autenticación JWT, PBKDF2, endpoints, migración `Inicial`, semilla, Swagger | 33 |
| 2 | `50e7572` | Reservas, franjas, disponibilidad, aprobación, rechazo, cancelación, historial, migración `FlujoDeReservas`, prueba de concurrencia | 57 nuevas, 90 totales |
| 3 | `b535482` | Regla de cuatro horas por solicitud y por cadena | 18 nuevas, 108 totales |

Después de cada incremento se ejecutó la suite completa y una verificación manual con la API en ejecución.

### 4.4 Etapa 4: cambio funcional

La regla nueva se trató como un cambio de OpenSpec independiente:

1. Análisis de impacto en `design.md`.
2. Actualización de la especificación y once escenarios nuevos.
3. Validación estricta y commit `9093832`.
4. Implementación en dominio, repositorio y servicio.
5. Pruebas nuevas y ejecución de toda la suite.
6. Detección y corrección de una prueba previa incompatible.
7. Archivo del cambio y commit `b535482`.

El detalle está en la sección 10.

---

## 5. Evidencia del orden plan → especificaciones → código

### 5.1 Historial de commits

```text
cad1c5a  2026-11-23  docs(plan): agregar planificacion de ingenieria previa al desarrollo
d7c4ef1  2026-11-23  chore(openspec): inicializar OpenSpec y preparar el repositorio
0f80e86  2026-11-23  chore(openspec): especificar gestion de espacios y usuarios
ce0ecda  2026-11-23  feat(espacios,usuarios): implementar catalogo y autenticacion
d49f736  2026-11-24  chore(openspec): especificar el flujo de reservas
50e7572  2026-11-24  feat(reservas): implementar solicitudes, aprobacion y cancelacion
9093832  2026-11-24  chore(openspec): especificar el limite de cuatro horas consecutivas
b535482  2026-11-24  feat(reservas): limitar a cuatro horas consecutivas por usuario y espacio
```

Cadena de dependencia:

```text
plan (cad1c5a)
  └─ inicialización OpenSpec (d7c4ef1)
       └─ specs incremento 1 (0f80e86) ──► código incremento 1 (ce0ecda)
                                              └─ specs incremento 2 (d49f736) ──► código incremento 2 (50e7572)
                                                                                    └─ specs fase 4 (9093832) ──► código fase 4 (b535482)
```

### 5.2 Cómo comprobarlo

```powershell
# Orden cronológico de commits
git log --oneline --reverse

# El primer commit solo contiene el plan
git show --stat cad1c5a

# Cada commit de especificación precede a su implementación
git show --stat 0f80e86
git show --stat ce0ecda
git show --stat d49f736
git show --stat 50e7572
git show --stat 9093832
git show --stat b535482
```

Los cambios archivados conservan fecha y contenido:

```text
openspec/changes/archive/2026-11-24-agregar-gestion-espacios-usuarios/
openspec/changes/archive/2026-11-24-agregar-flujo-reservas/
openspec/changes/archive/2026-11-24-agregar-limite-cuatro-horas/
```

Cada archivo incluye `proposal.md`, `design.md`, `tasks.md` con las tareas marcadas y los deltas aplicados.

---

## 6. Uso de SDD con OpenSpec

### 6.1 Estructura

```text
openspec/
├── config.yaml
├── specs/                          # verdad vigente
│   ├── espacios-academicos/
│   ├── usuarios-y-permisos/
│   ├── solicitudes-de-reserva/
│   ├── aprobacion-de-reservas/
│   └── cancelacion-e-historial/
└── changes/
    ├── agregar-.../                # cambios en curso (ninguno al cerrar)
    └── archive/                    # cambios aplicados
```

### 6.2 Convenciones

- Encabezados estructurales en inglés (`## Why`, `## ADDED Requirements`, `### Requirement:`, `WHEN`/`THEN`, `SHALL`), contenido en español, como exige la herramienta.
- Requisitos redactados con `SHALL` y comportamientos negativos con `NO SHALL`.
- Cada requisito con escenarios observables, no con detalles de implementación.
- Validación `--strict` antes de programar y después de archivar.

### 6.3 Aporte del enfoque

- Los escenarios se convirtieron en criterios de aceptación y luego en pruebas. Por ejemplo, el escenario "una tercera solicitud contigua que supera el límite se rechaza" es exactamente el caso `Una_tercera_reserva_contigua_que_supera_el_limite_se_rechaza`.
- El análisis de impacto obligó a decidir entre dos interpretaciones de la regla de cuatro horas antes de escribir código, en lugar de descubrirlo a mitad de la implementación.
- El archivo del cambio actualiza la especificación vigente, de modo que `openspec/specs/` siempre describe el sistema real.
- La validación estricta detectó y evitó deltas mal formados.

### 6.4 Revisión antes de implementar

Cada `design.md` incluye:

- Contexto y objetivos / no objetivos.
- Decisiones con la alternativa descartada y el motivo.
- Riesgos y compromisos.
- En el cambio de la fase 4, una tabla explícita de análisis de impacto.

Las tareas de `tasks.md` se marcaron al completarlas y se archivaron junto con el cambio.

---

## 7. Arquitectura y diseño técnico

### 7.1 Capas

```text
Reservas.Api  ──►  Reservas.Infrastructure  ──►  Reservas.Core
      │                     │
      └──────────┬──────────┘
                 ▼
           Reservas.Tests
```

| Proyecto | Contenido | Regla de dependencia |
|---|---|---|
| `Reservas.Core` | Entidades, reglas, estados, DTOs, interfaces y servicios de aplicación | No referencia ASP.NET Core ni EF Core |
| `Reservas.Infrastructure` | `DbContext`, configuraciones, repositorios, unidad de trabajo, migraciones, semilla, hash y JWT | Solo depende de Core |
| `Reservas.Api` | Endpoints, políticas, middleware, ProblemDetails, Swagger y composición | Depende de Infrastructure y Core |
| `Reservas.Tests` | Pruebas unitarias e integración | Depende de Api y Core |

Esta separación permitió probar las reglas de horario y de cuatro horas sin base de datos, y sustituir la persistencia en las pruebas sin tocar el dominio.

### 7.2 Persistencia y concurrencia

- EF Core 10 con SQLite.
- `Reserva` se descompone en `FranjaReservada` (una fila por franja de 30 minutos).
- Índice único `IX_franjas_reservadas_unicas` sobre `(EspacioId, Fecha, Indice)`.
- La violación del índice se captura en `UnidadDeTrabajo` y se traduce a `ConflictoException` (HTTP 409).
- La consulta de solapamiento en el servicio da un mensaje claro en el caso normal; el índice cubre la carrera.
- Las franjas se cargan con `Include` antes de liberarlas: cancelar o rechazar las elimina y la relación en cascada las borra.

### 7.3 Seguridad

- Contraseñas PBKDF2-HMAC-SHA256, 120 000 iteraciones, sal de 16 bytes, comparación en tiempo constante.
- JWT HS256 de 8 horas, con emisor, audiencia y firma validados.
- Middleware que relee el usuario en cada petición: un usuario desactivado pierde acceso de inmediato.
- Políticas `Administrador` y `Solicitante`; política de respaldo que exige autenticación.
- El Administrador no puede crear reservas: se evita la autoaprobación.

### 7.4 Errores

- `ProblemDetails` para todas las respuestas de error.
- Excepciones tipadas: `NoEncontradoException`, `ConflictoException`, `ReglaNegocioException`, `NoAutorizadoException`.
- JSON malformado y valores de enum desconocidos se traducen a 400, no a 500.
- Los errores no controlados devuelven un mensaje genérico sin detalles internos.

### 7.5 Arranque y pruebas

- Las migraciones y la semilla se ejecutan en un `IHostedService`, no entre `Build` y `Run`. Así el servidor no atiende peticiones antes de que el esquema esté listo, algo crítico en las pruebas de integración.
- Cada prueba crea una base SQLite temporal y un `WebApplicationFactory<Program>` propio. La configuración se inyecta con `ConfigureAppConfiguration` y se refuerza con `ConfigureTestServices`.
- Los clientes de prueba se autentican con tokens reales emitidos por la API.

---

## 8. Decisiones técnicas

| # | Decisión | Alternativa descartada | Motivo | Costo asumido |
|---|---|---|---|---|
| 1 | ASP.NET Core 10 + C# | Node.js, Spring Boot, Django | Tipado fuerte, rendimiento, integración con EF Core y xUnit; el entorno ya dispone del SDK. | Menos portabilidad que una solución multiplataforma de scripting. |
| 2 | API REST + Swagger UI | Panel web MVC o Blazor | Concentra el esfuerzo en el dominio, deja un contrato verificable y permite probar todo con HTTP. | No hay interfaz gráfica para usuarios finales. |
| 3 | EF Core + SQLite | Dapper con SQL manual; PostgreSQL | Migraciones, consultas tipadas y cero configuración. El índice único de SQLite garantiza la exclusión mutua. | SQLite limita la concurrencia de escritura; PostgreSQL queda como ruta de producción. |
| 4 | Franjas de 30 minutos con índice único | Validar solo con una consulta de solapamiento en la aplicación | Una consulta previa es vulnerable a carreras; la restricción de base es la única garantía real. | Más filas y una traducción específica del error de unicidad. |
| 5 | La reserva `Pendiente` bloquea la franja | Bloquear solo las aprobadas | Evita que dos pendientes se aprueben sobre la misma franja y permite mostrar disponibilidad real. | Un usuario puede retener una franja con una solicitud que luego no se apruebe. |
| 6 | JWT + middleware que relee el usuario | Sesión en servidor; JWT sin consulta a la base | Mantiene el token sin estado y permite revocar a un usuario desactivado sin esperar la expiración. | Una consulta adicional por petición autenticada. |
| 7 | PBKDF2 integrado en .NET | BCrypt; ASP.NET Core Identity | Sin dependencias extra, formato explícito y parametrizable, fácil de probar. | Hay que gestionar el formato y la verificación a mano. |
| 8 | Minimal API | Controladores MVC | Menos ceremonia para el tamaño del proyecto; políticas y filtros suficientes. | Menos estructura impuesta para equipos grandes. |
| 9 | Monolito en capas | Microservicios | El problema y el plazo no justifican la complejidad operativa. | Las fronteras deben cuidarse para no degradar el diseño. |
| 10 | Cuatro horas por solicitud **y** por cadena | Limitar solo la solicitud individual | Limitar solo la solicitud se evade con dos reservas contiguas; el enunciado habla de "durante más de cuatro horas consecutivas". | Más lógica y una consulta adicional al crear. |
| 11 | Límite por usuario y espacio | Límite global por usuario | El texto dice "un espacio académico"; un límite global sería más restrictivo que la regla. | La interpretación queda documentada y es extensible. |
| 12 | Reloj detrás de `IProveedorFechaHora` | `DateTime.Now` disperso | Permite sustituir el tiempo en pruebas y concentra la dependencia en un punto. | Una abstracción adicional. |
| 13 | Pruebas de integración con SQLite temporal | Mockear repositorios | Verifican el comportamiento real, incluidos índices, filtros y JWT. | Pruebas algo más lentas (8 s para 108). |

---

## 9. Riesgos y verificación

| Riesgo | Probabilidad / impacto | Mitigación | Verificación | Riesgo residual |
|---|---|---|---|---|
| Dos reservas simultáneas sobre la misma franja | Media / alto | Índice único + traducción a 409 | `ConcurrenciaTests`: 10 peticiones, exactamente 1 creada y 9 conflictos | Ninguno en una sola instancia; en varias instancias la base sigue siendo el árbitro. |
| Errores en la máquina de estados | Media / alto | Transiciones centralizadas en `Reserva` y pruebas por transición | `AprobacionTests`, `CancelacionTests` | Bajo. |
| Deriva entre especificación y código | Media / alto | SDD, validación estricta, archivo de cambios y commits ordenados | `openspec validate --all --strict`, matriz de trazabilidad | Bajo, si se mantiene la disciplina. |
| SQLite como cuello de botella en producción | Media / medio | Abstracción de persistencia y migración documentada a PostgreSQL | Pruebas de concurrencia y nota en limitaciones | Medio si el volumen crece. |
| Robo o filtración de credenciales | Baja / alto | PBKDF2 con sal, JWT con expiración, revocación por estado, clave fuera de producción | `HashPbkdf2Tests`, `AutenticacionTests` | La clave de desarrollo está en `appsettings.json`. |
| Cambio de alcance tardío | Alta / medio | Proceso de cambio de OpenSpec con análisis de impacto | Commit de spec anterior al código y suite de regresión | Bajo. |
| Errores de fecha u hora | Media / medio | Una sola zona, franjas discretas, pruebas con fechas futuras | Pruebas unitarias y de integración | Limitación documentada para múltiples zonas. |
| Entrada malformada | Media / bajo | ProblemDetails y traducción de errores de deserialización | Pruebas de 400 en varios endpoints | Bajo. |

---

## 10. Cambio funcional de la fase 4: límite de cuatro horas

### 10.1 Regla

> "Ningún usuario podrá reservar un espacio académico durante más de cuatro horas consecutivas."

### 10.2 Análisis de impacto

Documentado en `openspec/changes/archive/2026-11-24-agregar-limite-cuatro-horas/design.md`:

| Elemento | Impacto |
|---|---|
| Modelo de datos | Ninguno: no hay tablas ni columnas nuevas. |
| Migraciones | Ninguna. |
| Dominio | Alto: nueva validación de duración y cálculo de bloques. |
| Repositorio | Medio: consulta de reservas activas por usuario, espacio y fecha. |
| Servicio | Medio: validación adicional al crear. |
| Endpoints | Ninguno: mismas rutas y cuerpos; se documenta el 400. |
| Pruebas | 18 nuevas y revisión de regresión. |
| Datos históricos | No se reescriben. |
| Documentación | README e informe explican la interpretación. |

### 10.3 Interpretación adoptada

La regla admite dos lecturas:

- **Por solicitud:** una reserva individual no puede durar más de cuatro horas.
- **Por acumulación:** la ocupación continua del mismo usuario en el mismo espacio no puede superar cuatro horas, aunque se reparta en varias solicitudes contiguas.

Se implementaron **ambas**. La primera es literal; la segunda evita la evasión obvia de crear dos reservas de dos horas seguidas. El límite se aplica **por usuario y por espacio**, porque el enunciado dice "un espacio académico" y la finalidad es evitar el monopolio de un espacio concreto.

### 10.4 Algoritmo

1. Validar que la solicitud individual no supere las cuatro horas.
2. Cargar las reservas activas del mismo usuario en el mismo espacio y fecha.
3. Calcular el componente conexo que contiene a la nueva solicitud:
   - Dos intervalos se conectan si el fin de uno es igual o posterior al inicio del otro.
   - El bloque se expande por ambos extremos hasta que ninguna reserva lo amplíe.
4. Rechazar si la duración del bloque supera las cuatro horas.

Un hueco de cualquier duración corta la cadena. Las reservas rechazadas y canceladas no se cargan. Las de otros usuarios y otros espacios no se consultan.

La función `ReglasDeReserva.ValidarLimiteConsecutivo` es pura: recibe intervalos y no conoce EF Core ni HTTP. Eso permitió probar la conexión de bloques, la transitividad, los huecos y el desorden de entrada sin levantar la API.

### 10.5 Implementación

| Archivo | Cambio |
|---|---|
| `src/Reservas.Core/Domain/ReglasDeReserva.cs` | Constante `MaximoHorasConsecutivas = 4`, validación de duración y `ValidarLimiteConsecutivo`. |
| `src/Reservas.Core/Abstractions/Reservas.cs` | `ListarActivasDeUsuarioAsync`. |
| `src/Reservas.Infrastructure/Persistencia/RepositorioReservas.cs` | Consulta filtrada por usuario, espacio, fecha y estados activos. |
| `src/Reservas.Core/Servicios/ServicioReservas.cs` | Validación de la cadena antes de construir la reserva. |
| `src/Reservas.Api/Endpoints/EndpointsReservas.cs` | Descripción del endpoint con el límite. |

### 10.6 Especificación y escenarios

El cambio agregó a `solicitudes-de-reserva` el requisito **"Límite de cuatro horas consecutivas"** con once escenarios:

1. Una solicitud de exactamente cuatro horas se acepta.
2. Una solicitud de más de cuatro horas se rechaza.
3. Una solicitud de cinco horas se rechaza aunque el espacio esté libre.
4. Dos solicitudes contiguas de dos horas se aceptan.
5. Una tercera solicitud contigua que supera el límite se rechaza.
6. Un hueco reinicia el conteo.
7. Las reservas de otro usuario no cuentan.
8. El límite es por espacio.
9. Una reserva cancelada no cuenta.
10. Una solicitud pendiente cuenta.
11. Una reserva rechazada no ocupa el cupo.

El commit `9093832` contiene la especificación; el commit `b535482`, la implementación. Entre ambos no se modificó código de producción.

### 10.7 Pruebas y no regresión

Se agregaron **8 pruebas unitarias** y **10 de integración**. La suite pasó de **90 a 108 pruebas**.

La primera ejecución posterior al cambio encontró **una prueba previa incompatible**: `El_horario_completo_es_valido` consideraba válida una reserva de 07:00 a 22:00 (quince horas). Con la regla nueva ya no lo es. La prueba se reemplazó por `Los_extremos_del_horario_son_validos`, que comprueba 07:00–11:00 y 18:00–22:00, ambas de cuatro horas. No fue un ajuste para ocultar el fallo: la prueba anterior codificaba una regla que el negocio acaba de cambiar, y el reemplazo conserva su intención original (verificar los extremos del horario) dentro del nuevo límite.

Resultado final:

```text
Correctas! - Con error: 0, Superado: 108, Omitido: 0, Total: 108
```

Verificación manual con la API en ejecución:

```text
08:00-10:00 -> Pendiente
10:00-12:00 -> Pendiente
12:00-13:00 -> 400 / Un usuario no puede reservar el mismo espacio durante más de 4 horas consecutivas.
```

### 10.8 Compatibilidad

- No hay migración ni cambio de esquema.
- Las reservas existentes se conservan.
- Los clientes mantienen las mismas rutas y cuerpos; solo aparece un 400 adicional, documentado en Swagger.
- Las reservas de dos y tres horas que usaban las pruebas anteriores siguen funcionando.

---

## 11. Pruebas y verificación

### 11.1 Estrategia

| Nivel | Qué prueba | Cómo |
|---|---|---|
| Unitarias | Reglas de horario, franjas, límite de cuatro horas, hash PBKDF2 | xUnit, sin base de datos ni HTTP |
| Integración | Endpoints, autenticación, autorización, persistencia, errores, historial, disponibilidad | `WebApplicationFactory<Program>` + SQLite temporal + JWT real |
| Concurrencia | Exclusión mutua ante peticiones simultáneas | 10 peticiones en paralelo, 1 creada y 9 conflictos |
| Manual | Flujo completo y regla de cuatro horas | Swagger/HTTP con la API en ejecución |

### 11.2 Detalle por suite

| Suite | Pruebas | Cobertura principal |
|---|---:|---|
| `ReglasDeReservaTests` | 20 | Franjas, alineación, horario, duración mínima, máximo de cuatro horas, bloques conexos, huecos y desorden. |
| `ReservasTests` | 13 | Creación, permisos, espacio inexistente o inactivo, solapamiento exacto/parcial/envolvente, franjas contiguas y espacios distintos. |
| `AprobacionTests` | 10 | Aprobación, doble aprobación, rechazo con y sin motivo, permisos, liberación de franja y rechazo de canceladas. |
| `CancelacionTests` | 7 | Cancelación propia y administrativa, motivo obligatorio, reserva ajena, doble cancelación y liberación. |
| `HistorialTests` | 9 | Historial propio y global, filtros por estado/espacio/fecha, detalle ajeno, motivo y orden. |
| `DisponibilidadTests` | 5 | 30 franjas, ocupación, pasado, espacio inexistente y ausencia de datos personales. |
| `LimiteCuatroHorasTests` | 10 | Cuatro horas exactas, exceso, acumulación, hueco, otro usuario, otro espacio, cancelada, pendiente y rechazada. |
| `ConcurrenciaTests` | 1 | Diez peticiones simultáneas. |
| `EspaciosTests` | 12 | CRUD, validaciones, duplicados y desactivación con historial. |
| `UsuariosTests` | 8 | Registro, roles, duplicados, estado y restablecimiento. |
| `AutenticacionTests` | 8 | Login, token, perfil, credenciales inválidas y usuario desactivado. |
| `HashPbkdf2Tests` | 5 | Formato, sal, verificación y cambios en el hash. |

### 11.3 Determinismo

- Cada prueba usa una base SQLite temporal propia.
- Las fechas son futuras (`DateTime.Today.AddDays(1)` o más), para que la suite funcione a cualquier hora.
- La prueba de franjas pasadas captura la hora antes de la consulta, evitando depender del instante exacto.
- La suite se ejecutó dos veces seguidas para confirmar el aislamiento.

### 11.4 Verificación manual realizada

- Swagger responde 200 y documenta todos los endpoints.
- Login del administrador, del docente y del estudiante.
- 401 sin token; 403 con rol insuficiente.
- Seis espacios sembrados.
- Creación, aprobación, disponibilidad (4 franjas ocupadas), conflicto 409, cancelación y liberación (0 franjas ocupadas).
- Límite de cuatro horas: dos reservas contiguas aceptadas y la tercera rechazada con el mensaje correcto.

---

## 12. Matriz de trazabilidad

| Requisito de la consigna | Capacidad y requisito de OpenSpec | Implementación | Pruebas |
|---|---|---|---|
| Gestión de espacios | `espacios-academicos`: Catálogo, Validación de datos, Administración restringida, Desactivación sin pérdida de historial | `EndpointsEspacios`, `ServicioEspacios`, `RepositorioEspacios`, `Espacio` | `EspaciosTests` (12) |
| Gestión de usuarios | `usuarios-y-permisos`: Registro de usuarios, Administración del estado | `EndpointsUsuarios`, `ServicioUsuarios`, `RepositorioUsuarios`, `Usuario` | `UsuariosTests` (8) |
| Roles y permisos | `usuarios-y-permisos`: Roles y permisos, Autenticación con token | `Politicas`, `Program`, `VerificarUsuarioActivoMiddleware`, `EmisorTokens` | `AutenticacionTests` (8) y verificaciones 403 en las suites de reservas, usuarios y espacios |
| Solicitudes de reserva | `solicitudes-de-reserva`: Creación de solicitudes, Validación del horario institucional | `EndpointsReservas` (POST), `ServicioReservas`, `ReglasDeReserva`, `Reserva` | `ReservasTests` (13), `ReglasDeReservaTests` (20) |
| Disponibilidad | `solicitudes-de-reserva`: Consulta de disponibilidad | `EndpointsDisponibilidad`, `ServicioReservas`, `RepositorioReservas` | `DisponibilidadTests` (5) |
| Prevención de reservas simultáneas | `solicitudes-de-reserva`: Prevención de reservas simultáneas | `FranjaReservada`, índice `IX_franjas_reservadas_unicas`, `UnidadDeTrabajo`, consulta de solapamiento | `ReservasTests` (solapamientos), `ConcurrenciaTests` (1) |
| Aprobación y rechazo | `aprobacion-de-reservas`: Aprobación de solicitudes, Rechazo de solicitudes | `EndpointsReservas`, `ServicioReservas`, `Reserva.Aprobar/Rechazar` | `AprobacionTests` (10) |
| Cancelación e historial | `cancelacion-e-historial`: Cancelación de reservas, Historial de reservas | `EndpointsReservas`, `ServicioReservas`, `Reserva.Cancelar`, filtros del repositorio | `CancelacionTests` (7), `HistorialTests` (9) |
| Regla de cuatro horas | `solicitudes-de-reserva`: Límite de cuatro horas consecutivas | `ReglasDeReserva.ValidarLimiteConsecutivo`, `ServicioReservas`, `RepositorioReservas.ListarActivasDeUsuarioAsync` | `LimiteCuatroHorasTests` (10), `ReglasDeReservaTests` (8 casos del límite) |
| Contraseñas protegidas | `usuarios-y-permisos`: Contraseñas protegidas | `HashPbkdf2` | `HashPbkdf2Tests` (5), `AutenticacionTests` (8) |
| Errores uniformes y contrato | Transversal, documentado en el diseño de cada cambio | `ManejadorDeExcepciones`, `ProblemDetails`, Swagger | Verificaciones de 400/401/403/404/409 en las suites |
| Documentación y proceso | Plan, propuestas, diseños, tareas y deltas archivados | `PLAN_INGENIERIA.md`, `openspec/`, `README.md`, este informe | `openspec validate --all --strict` |

Cada requisito de OpenSpec incluye sus escenarios; los 102 escenarios cubren los caminos felices, los errores de validación, los permisos y los casos límite.

---

## 13. Dificultades y aprendizajes

1. **Versiones de paquetes:** `dotnet add package` sin versión intentaba instalar la 11.0.0 (no disponible para el SDK 10). Se fijaron versiones explícitas (EF Core, JwtBearer y Mvc.Testing 10.0.15; `System.IdentityModel.Tokens.Jwt` 8.19.2; Swashbuckle 9.0.6).
2. **Nombre del índice único:** la traducción del error de SQLite depende del nombre exacto. Se fijó `IX_franjas_reservadas_unicas` tanto en el `DbContext` como en la unidad de trabajo.
3. **Migraciones y pruebas:** ejecutar migraciones entre `Build` y `Run` dejaba el host a medio construir. Se movieron a un `IHostedService`, lo que además hizo que las pruebas de integración esperaran el esquema.
4. **Errores de deserialización:** un JSON inválido o un enum desconocido producían 500. Se interceptaron en el manejador y se devuelven como 400 con `ProblemDetails`.
5. **Aislamiento de pruebas:** compartir el archivo `reservas.db` hacía que las pruebas se contaminaran. Cada prueba usa ahora un archivo temporal y su propia configuración.
6. **Codificación en la consola:** PowerShell mostró mal algunos acentos al imprimir respuestas UTF-8. El contenido de los archivos y las respuestas HTTP es correcto; fue un problema de visualización, no del producto.
7. **Regresión de la fase 4:** la nueva regla invalidó una prueba que consideraba válida una reserva de quince horas. La suite lo detectó antes de cerrar el cambio; la prueba se reemplazó por una verificación de los extremos dentro del límite.
8. **Archivo de OpenSpec:** al archivar el tercer cambio se emitió una advertencia porque el delta traía un `Purpose` y la capacidad ya tenía uno. La herramienta conservó el existente, que sigue siendo correcto.

---

## 14. Limitaciones y trabajo futuro

### Limitaciones actuales

- Zona horaria única; sin soporte para sedes en husos distintos.
- Sin notificaciones por correo ni webhooks.
- Sin límites de anticipación, cupos diarios ni listas de espera.
- SQLite con un solo escritor; adecuado para la demostración.
- Clave JWT de desarrollo en `appsettings.json`; sin refresh tokens.
- Swagger habilitado siempre.
- Listados sin paginación.
- Auditoría limitada a los campos de la reserva.
- La regla de cuatro horas no reescribe reservas históricas.

### Trabajo futuro

- Migrar a PostgreSQL y añadir índices de cobertura para consultas de disponibilidad a gran escala.
- Notificaciones y recordatorios.
- Paginación y proyecciones para el historial.
- Refresh tokens, rotación de claves y rate limiting.
- Gestión de feriados y horarios especiales.
- Listas de espera y liberación automática de solicitudes no aprobadas.
- Panel web para administración y reserva.
- CI con `dotnet test`, `openspec validate --all --strict` y análisis estático.
- Contenerización y despliegue con variables de entorno para secretos.

---

## 15. Conclusiones

El sistema cumple los requisitos funcionales de las cuatro etapas:

- El **plan** existe como primer commit y contiene problemática, alcance, arquitectura, decisiones comparadas, riesgos y metodología.
- Las **especificaciones** cubren las ocho funcionalidades mínimas, con 18 requisitos y 102 escenarios verificables, y cada una se validó antes de implementar.
- El **código** implementa el catálogo, los usuarios, los roles, las solicitudes, la disponibilidad, la exclusión de solapamientos, la aprobación, el rechazo, la cancelación y el historial.
- La **regla de cuatro horas** se incorporó por el mismo camino: análisis de impacto, especificación, escenarios, implementación, pruebas y verificación de no regresión.

La fortaleza principal del resultado no es solo que las 108 pruebas pasen, sino que el historial de Git y los cambios archivados permiten reconstruir **por qué** existe cada pieza, **qué** alternativa se descartó y **cómo** se verificó. Esa trazabilidad es la diferencia entre un conjunto de archivos y un proyecto de ingeniería.

---

## Anexo A: comandos para reproducir

```powershell
# Restaurar y compilar
dotnet restore
dotnet build ReservasAcademicas.slnx

# Ejecutar la API
dotnet run --project src/Reservas.Api
# Swagger: http://localhost:5080/swagger

# Ejecutar todas las pruebas
dotnet test

# Validar todas las especificaciones
$env:OPENSPEC_TELEMETRY='0'
openspec validate --all --strict

# Revisar el orden de los commits
git log --oneline --reverse

# Herramientas de EF Core (si se agregan migraciones)
dotnet tool restore
dotnet ef migrations add NombreDelCambio `
  --project src/Reservas.Infrastructure `
  --startup-project src/Reservas.Api `
  --output-dir Persistencia\Migraciones
```

## Anexo B: verificación rápida de la secuencia

```powershell
# 1. El plan es el primer commit
git log --reverse --pretty=format:'%h %s' | Select-Object -First 1

# 2. Cada especificación tiene su commit anterior al código
git log --oneline --reverse | Select-String 'openspec|feat'

# 3. Las capacidades vigentes están validadas
openspec list
openspec validate --all --strict

# 4. La suite está en verde
dotnet test --nologo
```
