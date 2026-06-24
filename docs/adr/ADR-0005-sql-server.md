# ADR-0005: Usar SQL Server como base de datos central

## Estado

Aceptada.

## Contexto

El sistema necesita consistencia transaccional, numeraciones, contabilidad, registros de IVA, remesas, conciliación, auditoría y consultas administrativas. El despliegue previsto es Windows con servidor central.

## Decisión

Se usará SQL Server como base de datos central.

Cada módulo tendrá un esquema lógico propio dentro de la misma base de datos.

## Alternativas consideradas

- SQLite: sencillo, pero insuficiente para concurrencia multiusuario centralizada.
- PostgreSQL: técnicamente viable, pero menos alineado con el entorno Windows previsto.
- Bases separadas por módulo: refuerzan aislamiento, pero complican transacciones económicas críticas.

## Consecuencias

- Las operaciones críticas pueden usar transacciones ACID.
- Se podrán usar restricciones, índices, `rowversion` y procedimientos puntuales cuando estén justificados.
- Habrá que definir estrategia de backup, restauración y administración.
- El coste/licencia dependerá de la edición elegida y del volumen final.

