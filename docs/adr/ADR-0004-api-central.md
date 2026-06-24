# ADR-0004: Obligar al escritorio a operar siempre mediante API central

## Estado

Aceptada.

## Contexto

CriGes tendrá varios puestos concurrentes. Las operaciones deben validar permisos, sesión única, reglas de negocio, auditoría, numeraciones e integridad contable de forma uniforme.

## Decisión

El escritorio no se conectará directamente a SQL Server.

Toda operación pasará por `CriGes.Api` mediante HTTP, contratos JSON y autorización en servidor.

## Alternativas consideradas

- Acceso directo del escritorio a SQL Server: reduce una capa, pero dispersa reglas, credenciales y transacciones.
- Cliente rico con lógica de negocio local: mejora respuesta local, pero compromete coherencia y seguridad.
- API local por puesto: añade despliegue y sincronización sin necesidad en el alcance inicial.

## Consecuencias

- La API es la única puerta de entrada funcional.
- Las credenciales de base de datos no se distribuyen a puestos.
- La autorización no depende de ocultar botones en la interfaz.
- Las operaciones pueden auditarse con un identificador de correlación extremo a extremo.

