# Delta de especificación

## Purpose

Esta capacidad existía antes de la regla de las cuatro horas. El cambio agrega un requisito nuevo que restringe la duración de las solicitudes y la acumulación de reservas consecutivas de un mismo usuario en un mismo espacio. Ningún requisito anterior se elimina ni se relaja.

## ADDED Requirements

### Requirement: Límite de cuatro horas consecutivas

El sistema SHALL impedir que un usuario reserve un espacio académico durante más de cuatro horas consecutivas. El límite SHALL aplicarse tanto a la duración de una solicitud individual como a la acumulación de solicitudes activas contiguas del mismo usuario en el mismo espacio. El sistema SHALL considerar consecutivas dos reservas cuando el fin de una coincide con el inicio de la otra o se superpone con ella. Un intervalo libre SHALL cortar la cadena y reiniciar el conteo. Las reservas en estado Pendiente o Aprobada SHALL contar para el límite; las rechazadas y las canceladas NO SHALL contar. El límite SHALL evaluarse al crear la solicitud.

#### Scenario: Una solicitud de exactamente cuatro horas se acepta
- **WHEN** un docente solicita un espacio de 08:00 a 12:00
- **THEN** el sistema acepta la solicitud
- **AND** la reserva queda en estado Pendiente

#### Scenario: Una solicitud de más de cuatro horas se rechaza
- **WHEN** un docente solicita un espacio de 08:00 a 12:30
- **THEN** el sistema rechaza la solicitud
- **AND** el mensaje indica que no se pueden superar las cuatro horas consecutivas

#### Scenario: Una solicitud de cinco horas se rechaza aunque el espacio esté libre
- **WHEN** un docente solicita un espacio sin ninguna otra reserva de 09:00 a 14:00
- **THEN** el sistema rechaza la solicitud
- **AND** la franja no queda ocupada

#### Scenario: Dos solicitudes contiguas de dos horas se aceptan
- **WHEN** un docente reserva un espacio de 08:00 a 10:00
- **AND** luego reserva el mismo espacio de 10:00 a 12:00
- **THEN** el sistema acepta ambas solicitudes
- **AND** el bloque acumulado es de exactamente cuatro horas

#### Scenario: Una tercera solicitud contigua que supera el límite se rechaza
- **WHEN** un docente tiene reservas contiguas de 08:00 a 10:00 y de 10:00 a 12:00 en un espacio
- **AND** solicita el mismo espacio de 12:00 a 13:00
- **THEN** el sistema rechaza la tercera solicitud
- **AND** el mensaje indica que el bloque supera las cuatro horas consecutivas
- **AND** las dos reservas anteriores conservan su estado

#### Scenario: Un hueco reinicia el conteo
- **WHEN** un docente tiene una reserva de 08:00 a 10:00 en un espacio
- **AND** solicita el mismo espacio de 10:30 a 12:30
- **THEN** el sistema acepta la solicitud
- **AND** el bloque se calcula desde las 10:30 y no desde las 08:00

#### Scenario: Las reservas de otro usuario no cuentan
- **WHEN** un docente tiene una reserva de 08:00 a 10:00 en un espacio
- **AND** otro docente tiene una reserva de 10:00 a 12:00 en ese espacio
- **AND** el primer docente solicita el espacio de 12:00 a 13:00
- **THEN** el sistema acepta la solicitud
- **AND** el bloque del primer docente es de una hora

#### Scenario: El límite es por espacio
- **WHEN** un docente tiene cuatro horas consecutivas reservadas en un espacio
- **AND** solicita otro espacio para una franja distinta
- **THEN** el sistema acepta la solicitud
- **AND** las horas del primer espacio no se suman

#### Scenario: Una reserva cancelada no cuenta para el límite
- **WHEN** un docente tiene reservas de 08:00 a 10:00 y de 10:00 a 12:00 en un espacio
- **AND** cancela la reserva de 10:00 a 12:00
- **AND** solicita el mismo espacio de 12:00 a 13:00
- **THEN** el sistema acepta la solicitud
- **AND** el bloque activo es de dos horas, no de cinco

#### Scenario: Una solicitud pendiente cuenta para el límite
- **WHEN** un docente tiene una solicitud pendiente de 08:00 a 10:00 en un espacio
- **AND** solicita el mismo espacio de 10:00 a 12:30
- **THEN** el sistema rechaza la segunda solicitud
- **AND** el mensaje indica que el bloque supera las cuatro horas consecutivas

#### Scenario: Una reserva rechazada no ocupa el cupo de la cadena
- **WHEN** una solicitud de 08:00 a 10:00 fue rechazada
- **AND** el docente solicita el mismo espacio de 08:00 a 10:00
- **AND** luego solicita ese espacio de 10:00 a 12:30
- **THEN** el sistema acepta la primera y rechaza la segunda por acumulación
- **AND** la solicitud rechazada anteriormente no se cuenta
