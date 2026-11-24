# Tareas: límite de cuatro horas consecutivas

## 1. Especificación y análisis

- [ ] 1.1 Documentar las dos lecturas de la regla y la decisión adoptada. Verificación: el diseño explica por qué se aplican ambas
- [ ] 1.2 Analizar el impacto sobre modelo, API, pruebas y datos existentes. Verificación: la tabla de impacto cubre todos los elementos
- [ ] 1.3 Agregar el requisito y sus escenarios a `solicitudes-de-reserva`. Verificación: `openspec validate --strict` sin errores
- [ ] 1.4 Registrar la especificación en un commit anterior al código. Verificación: el historial de Git lo muestra

## 2. Dominio

- [ ] 2.1 Agregar la constante de cuatro horas y la validación de duración máxima a `ReglasDeReserva`. Verificación: una solicitud de 4 h 30 min se rechaza
- [ ] 2.2 Implementar el cálculo del bloque conexo de reservas contiguas. Verificación: pruebas unitarias de contigüidad, huecos y transitividad
- [ ] 2.3 Verificar que una reserva de exactamente cuatro horas siga siendo válida. Verificación: el límite es "más de cuatro", no "cuatro o más"

## 3. Persistencia y aplicación

- [ ] 3.1 Agregar la consulta de reservas activas por usuario, espacio y fecha. Verificación: excluye rechazadas y canceladas
- [ ] 3.2 Integrar la validación en la creación antes de guardar. Verificación: el mensaje distingue duración máxima de acumulación
- [ ] 3.3 Confirmar que no se requiere migración. Verificación: el esquema no cambia y las pruebas existentes siguen pasando

## 4. Pruebas

- [ ] 4.1 Probar la duración máxima en unitarias y en integración. Verificación: 4 h aceptada, 4 h 30 min rechazada
- [ ] 4.2 Probar la acumulación: 2 h + 2 h aceptadas, 2 h + 2 h + 1 h rechazada. Verificación: los tres casos en integración
- [ ] 4.3 Probar que un hueco corta la cadena. Verificación: dos bloques separados se aceptan
- [ ] 4.4 Probar que el límite es por usuario. Verificación: las reservas de otro usuario entre las propias no suman
- [ ] 4.5 Probar que el límite es por espacio. Verificación: el mismo usuario reserva cuatro horas en otro espacio
- [ ] 4.6 Probar que una reserva cancelada no cuenta. Verificación: al cancelar se libera también el cupo de la cadena
- [ ] 4.7 Ejecutar la suite completa y comparar con la línea base de 90 pruebas. Verificación: todas pasan, ninguna se rompió

## 5. Cierre

- [ ] 5.1 Actualizar Swagger y la documentación de errores. Verificación: el 400 de creación menciona el límite
- [ ] 5.2 Actualizar README e informe final con la interpretación. Verificación: la regla y sus límites quedan explícitos
- [ ] 5.3 Validar y archivar el cambio de OpenSpec. Verificación: la capacidad actualizada queda en `openspec/specs/`
- [ ] 5.4 Registrar el commit del cambio funcional. Verificación: el historial muestra especificación, código y pruebas
