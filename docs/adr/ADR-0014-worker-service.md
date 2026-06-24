# ADR-0014: Usar un Worker Service para trabajos de fondo

## Estado

Aceptada.

## Contexto

CriGes requiere tareas que no deben depender de una petición HTTP activa: Outbox, notificaciones, limpieza, caducidad de sesiones, reintentos, copias, avisos de certificados y procesos largos.

## Decisión

Se desplegará `CriGes.Worker` como proceso .NET Worker Service separado de la API.

## Alternativas consideradas

- Ejecutar trabajos dentro de la API: simple, pero mezcla responsabilidades y complica escalado/control.
- Programador externo sin código propio: útil para lanzar procesos, insuficiente para trabajos funcionales persistidos.
- Solo tareas manuales: no cubre reintentos ni mantenimiento continuo.

## Consecuencias

- Habrá que monitorizar API y Worker por separado.
- Los trabajos serán persistidos, bloqueados e idempotentes.
- El Worker compartirá módulos, pero no duplicará casos de uso.
- El despliegue tendrá un servicio adicional.

