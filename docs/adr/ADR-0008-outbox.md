# ADR-0008: Usar Outbox transaccional para trabajo diferido e integraciones

## Estado

Aceptada.

## Contexto

Varias operaciones deben persistir cambios y después ejecutar tareas externas: correo, VeriFactu, notificaciones, copias, procesos bancarios o integraciones futuras. No debe mantenerse una transacción SQL abierta mientras se llama a sistemas externos.

## Decisión

Se usará patrón Outbox transaccional.

Los mensajes se guardarán en SQL Server dentro de la misma transacción que el cambio funcional. El Worker los procesará después con reintentos, trazabilidad e idempotencia.

## Alternativas consideradas

- Ejecutar llamadas externas dentro de la transacción: arriesga bloqueos y estados inciertos.
- Publicar directamente a una cola externa: válido, pero añade infraestructura no necesaria al inicio.
- Procesos en memoria: frágiles ante reinicios.

## Consecuencias

- Los efectos diferidos sobreviven a reinicios.
- Los consumidores deben ser idempotentes.
- Se necesita monitorización de mensajes pendientes y fallidos.
- La consistencia con sistemas externos será controlada por estados, no por transacción distribuida.

