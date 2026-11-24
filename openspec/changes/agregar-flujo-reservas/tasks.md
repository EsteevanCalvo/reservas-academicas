# Tareas: flujo de solicitudes, aprobación, cancelación e historial

## 1. Dominio

- [ ] 1.1 Crear el enum `EstadoReserva` con Pendiente, Aprobada, Rechazada y Cancelada, y la noción de reserva activa. Verificación: una prueba unitaria confirma que solo Pendiente y Aprobada son activas
- [ ] 1.2 Crear `HorarioInstitucional` con apertura, cierre, duración de franja y total de franjas. Verificación: el día tiene 30 franjas y la última termina a las 22:00
- [ ] 1.3 Crear `ReglasDeReserva` con la validación de horario y el cálculo de franjas. Verificación: pruebas unitarias de cada regla y de los límites
- [ ] 1.4 Crear `Reserva` con su máquina de estados: aprobar, rechazar y cancelar. Verificación: una transición inválida lanza excepción de negocio
- [ ] 1.5 Crear `FranjaReservada` y la operación de agregar franjas a una reserva. Verificación: una reserva de dos horas ocupa cuatro franjas

## 2. Persistencia

- [ ] 2.1 Configurar `Reserva` y `FranjaReservada` en el contexto, con claves foráneas y borrado restringido hacia espacio y usuario. Verificación: la migración crea las claves
- [ ] 2.2 Crear el índice único `(espacio_id, fecha, indice_franja)`. Verificación: un insert duplicado directo contra la base falla
- [ ] 2.3 Implementar `RepositorioReservas` con consulta de solapamiento, franjas ocupadas e historial filtrado. Verificación: las consultas devuelven lo esperado con datos de prueba
- [ ] 2.4 Traducir la violación del índice único a un conflicto de negocio con mensaje útil. Verificación: dos inserts concurrentes producen un 409, no un 500
- [ ] 2.5 Generar y aplicar la migración del incremento. Verificación: la base contiene `reservas` y `franjas_reservadas`

## 3. Aplicación

- [ ] 3.1 Implementar la creación de solicitudes con validación de espacio activo y horario. Verificación: una solicitud fuera de horario no se guarda
- [ ] 3.2 Implementar la consulta de disponibilidad con las 30 franjas del día. Verificación: una reserva de una hora ocupa dos franjas
- [ ] 3.3 Implementar la aprobación con registro de responsable y momento. Verificación: el estado y los campos de decisión cambian juntos
- [ ] 3.4 Implementar el rechazo con motivo obligatorio y liberación de franjas. Verificación: después del rechazo la franja vuelve a estar disponible
- [ ] 3.5 Implementar la cancelación con motivo, permiso de dueño o administrador, y liberación de franjas. Verificación: un tercero recibe 403 y la reserva no cambia
- [ ] 3.6 Implementar el historial propio y el global con filtros y orden descendente. Verificación: un docente no ve reservas ajenas
- [ ] 3.7 Implementar la consulta por identificador con verificación de propiedad. Verificación: consultar una reserva ajena devuelve 403

## 4. API

- [ ] 4.1 Agregar la política `Solicitante` con los roles Docente y Estudiante. Verificación: un administrador recibe 403 al crear una solicitud
- [ ] 4.2 Implementar los endpoints de reservas: crear, mis reservas, historial global y consulta por identificador. Verificación: cada uno responde con los códigos acordados
- [ ] 4.3 Implementar los endpoints de aprobación, rechazo y cancelación. Verificación: las transiciones válidas e inválidas responden lo especificado
- [ ] 4.4 Implementar el endpoint de disponibilidad. Verificación: devuelve 30 franjas con su estado
- [ ] 4.5 Documentar todos los endpoints en Swagger con sus códigos de error. Verificación: el contrato generado incluye las rutas nuevas

## 5. Pruebas

- [ ] 5.1 Probar las reglas de horario en unitarias: límites, alineación, duración, pasado y cambio de día. Verificación: cada regla tiene al menos una prueba
- [ ] 5.2 Probar el ciclo completo: solicitud, aprobación y consulta. Verificación: el estado final es Aprobada y el historial lo refleja
- [ ] 5.3 Probar el rechazo con y sin motivo, y la liberación de la franja. Verificación: la franja vuelve a estar disponible
- [ ] 5.4 Probar la cancelación propia, la ajena y la del administrador. Verificación: permisos y estados correctos
- [ ] 5.5 Probar los solapamientos: exacto, parcial, envolvente, contiguo y en espacios distintos. Verificación: cinco pruebas diferenciadas
- [ ] 5.6 Probar diez altas simultáneas sobre la misma franja. Verificación: exactamente un 201 y nueve conflictos
- [ ] 5.7 Probar la disponibilidad con y sin reservas, y con franjas pasadas. Verificación: los estados de las franjas son correctos
- [ ] 5.8 Probar el historial con filtros por estado, espacio y fechas. Verificación: cada filtro devuelve solo lo que corresponde
- [ ] 5.9 Probar que los 401 y 403 se mantienen en los endpoints nuevos. Verificación: sin token y con rol insuficiente

## 6. Cierre

- [ ] 6.1 Ejecutar la suite completa. Verificación: todas las pruebas pasan, incluidas las del incremento anterior
- [ ] 6.2 Verificar la compilación sin advertencias. Verificación: 0 errores y 0 advertencias
- [ ] 6.3 Validar el cambio con `openspec validate --strict`. Verificación: sin errores
- [ ] 6.4 Archivar el cambio y comprobar que `openspec/specs/` contiene las tres capacidades nuevas. Verificación: las especificaciones archivadas reflejan lo implementado
- [ ] 6.5 Registrar el commit del incremento. Verificación: el historial muestra la especificación antes que el código
