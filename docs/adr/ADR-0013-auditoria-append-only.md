# ADR-0013: Mantener auditoría append-only

## Estado

Aceptada.

## Contexto

El sistema debe registrar comunicaciones, cambios, accesos, operaciones económicas, configuración, errores relevantes y acciones administrativas. La auditoría debe conservar trazabilidad y no poder alterarse como una entidad ordinaria.

## Decisión

La auditoría se guardará en tablas append-only.

La aplicación ordinaria no tendrá operaciones de actualización ni eliminación sobre registros de auditoría.

## Alternativas consideradas

- Auditoría editable por administrador: facilita correcciones, pero destruye confianza.
- Logs solo en fichero: útiles técnicamente, insuficientes para consulta funcional.
- Auditoría distribuida por tabla sin modelo común: dificulta búsquedas y trazabilidad.

## Consecuencias

- Los errores de auditoría se corregirán con nuevos eventos, no editando los existentes.
- Se aplicarán permisos SQL más restrictivos.
- Se usará Correlation ID para unir acciones entre Desktop, API y Worker.
- La restauración deberá dejar evidencia externa de la operación realizada.

