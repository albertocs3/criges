# Registro de decisiones arquitectónicas

## Propósito

Este directorio contiene las decisiones arquitectónicas relevantes de CriGes.

Cada ADR describe:

- La decisión tomada.
- El contexto que la motiva.
- Las alternativas consideradas.
- Las consecuencias aceptadas.
- El impacto sobre implementación, pruebas y operación.

Los ADR no sustituyen a la especificación funcional ni a la arquitectura técnica. Sirven para conservar la razón de las decisiones y evitar reabrirlas sin un cambio real de contexto.

## Estado de una decisión

Estados posibles:

- `Propuesta`: decisión pendiente de validar.
- `Aceptada`: decisión vigente.
- `Reemplazada`: decisión sustituida por otra posterior.
- `Obsoleta`: decisión ya no aplica.

## Convención

```text
ADR-0001-titulo-corto.md
```

Cada decisión debe mantener su número aunque cambie el título.

## Índice inicial

| ADR | Estado | Decisión |
|---|---|---|
| [ADR-0001](ADR-0001-monolito-modular.md) | Aceptada | Usar monolito modular |
| [ADR-0002](ADR-0002-dotnet-8.md) | Aceptada | Usar .NET 8 como plataforma de desarrollo |
| [ADR-0003](ADR-0003-wpf-desktop.md) | Aceptada | Usar WPF para la aplicación de escritorio |
| [ADR-0004](ADR-0004-api-central.md) | Aceptada | Obligar al escritorio a operar siempre mediante API central |
| [ADR-0005](ADR-0005-sql-server.md) | Aceptada | Usar SQL Server como base de datos central |
| [ADR-0006](ADR-0006-ef-core.md) | Aceptada | Usar Entity Framework Core con SQL parametrizado cuando sea necesario |
| [ADR-0007](ADR-0007-capas-por-modulo.md) | Aceptada | Separar cada módulo en Domain, Application, Infrastructure, Contracts y Api |
| [ADR-0008](ADR-0008-outbox.md) | Aceptada | Usar Outbox transaccional para trabajo diferido e integraciones |
| [ADR-0009](ADR-0009-autenticacion-sesiones.md) | Aceptada | Usar access token breve, refresh token opaco y sesión persistida |
| [ADR-0010](ADR-0010-autorizacion-permisos.md) | Aceptada | Autorizar mediante permisos y políticas validadas en servidor |
| [ADR-0011](ADR-0011-utc-europe-madrid.md) | Aceptada | Persistir instantes en UTC y presentar fechas en Europe/Madrid |
| [ADR-0012](ADR-0012-adjuntos-ficheros.md) | Aceptada | Guardar adjuntos en repositorio protegido y metadatos en SQL Server |
| [ADR-0013](ADR-0013-auditoria-append-only.md) | Aceptada | Mantener auditoría append-only |
| [ADR-0014](ADR-0014-worker-service.md) | Aceptada | Usar un Worker Service para trabajos de fondo |
| [ADR-0015](ADR-0015-db-migrator.md) | Aceptada | Aplicar migraciones mediante herramienta dedicada |
| [ADR-0016](ADR-0016-signalr-notificaciones.md) | Aceptada | Usar SignalR con respaldo por consulta periódica para notificaciones |
| [ADR-0017](ADR-0017-copias-restauracion.md) | Aceptada | Gestionar copias completas y restauración en modo controlado |
| [ADR-0018](ADR-0018-verifactu-adaptador.md) | Aceptada | Aislar VeriFactu detrás de un adaptador versionado |

## Reglas de mantenimiento

1. Una decisión aceptada no se edita para cambiar su sentido; se crea un ADR nuevo que la reemplace.
2. Las correcciones de redacción o enlaces sí pueden hacerse sobre el ADR existente.
3. Si una decisión afecta a reglas funcionales, debe actualizarse también la documentación funcional correspondiente.
4. Si una decisión afecta a estructura de proyectos, debe actualizarse [Estructura inicial de la solución .NET](../06-estructura-solucion-dotnet.md).
5. Si una decisión afecta al despliegue u operación, debe actualizarse [Arquitectura técnica general](../05-arquitectura-tecnica.md).

