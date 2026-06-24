# ADR-0002: Usar .NET 8 como plataforma de desarrollo

## Estado

Aceptada.

## Contexto

CriGes necesita API, procesos de fondo, aplicación de escritorio Windows, acceso a SQL Server, seguridad, observabilidad y pruebas automatizadas. La solución debe mantenerse durante varios años con una base tecnológica estable.

## Decisión

La plataforma principal será .NET 8.

Se usará:

- ASP.NET Core para la API.
- .NET Worker Service para procesos de fondo.
- WPF sobre `net8.0-windows` para escritorio.
- Entity Framework Core 8 para persistencia.

## Alternativas consideradas

- .NET Framework: mayor compatibilidad histórica con WPF, pero menor evolución y peor encaje con arquitectura moderna.
- .NET 9 o superior: podría aportar mejoras, pero reduce estabilidad si se inicia antes de consolidar soporte y dependencias.
- Stack web completo: no responde a la decisión inicial de aplicación de escritorio Windows.

## Consecuencias

- Se fija `global.json` para controlar el SDK.
- Los paquetes se gestionan centralmente con `Directory.Packages.props`.
- La solución podrá compartir lenguaje, herramientas y patrones entre API, Worker y Desktop.
- Las futuras actualizaciones de versión mayor requerirán ADR propio o actualización explícita de este.

