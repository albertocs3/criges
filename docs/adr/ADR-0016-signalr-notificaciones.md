# ADR-0016: Usar SignalR con respaldo por consulta periódica para notificaciones

## Estado

Aceptada.

## Contexto

La aplicación debe mostrar notificaciones internas, algunas críticas, y mantener registro persistido. El cliente será WPF conectado a una API central.

## Decisión

Las notificaciones se persistirán en SQL Server.

La entrega inmediata al escritorio usará SignalR. Si SignalR no está disponible, el cliente podrá consultar periódicamente las notificaciones pendientes.

## Alternativas consideradas

- Solo polling: más simple, pero menos inmediato.
- Solo SignalR sin persistencia: rápido, pero pierde notificaciones ante desconexiones.
- Notificaciones del sistema operativo como fuente principal: útiles para UX, no para trazabilidad.

## Consecuencias

- La fuente de verdad será la base de datos.
- SignalR mejora experiencia, pero no define el estado funcional.
- El cliente validará permisos antes de mostrar detalle sensible.
- Las pruebas deben cubrir conexión, reconexión y consulta de respaldo.

