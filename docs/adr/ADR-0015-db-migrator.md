# ADR-0015: Aplicar migraciones mediante herramienta dedicada

## Estado

Aceptada.

## Contexto

La base de datos contendrá datos económicos y de configuración críticos. Las migraciones deben aplicarse de forma ordenada, registrada y con validaciones previas/posteriores.

## Decisión

Se creará `CriGes.DbMigrator` como herramienta de consola para aplicar migraciones.

API, Worker y Desktop no aplicarán migraciones automáticamente.

## Alternativas consideradas

- Migrar al arrancar la API: cómodo, pero arriesgado en producción y difícil de controlar.
- Scripts SQL manuales únicamente: aportan control, pero separan modelo y migración de EF Core.
- Migrar desde Desktop: descartado por seguridad y concurrencia.

## Consecuencias

- El despliegue tendrá un paso explícito de migración.
- Las migraciones podrán registrarse con versión, fecha y resultado.
- API y Worker comprobarán compatibilidad de esquema al arrancar.
- El proceso de actualización podrá exigir copia previa.

