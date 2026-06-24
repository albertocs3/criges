# ADR-0003: Usar WPF para la aplicación de escritorio

## Estado

Aceptada.

## Contexto

El usuario ha definido que la aplicación será de escritorio. El sistema se usará en puestos Windows y debe ofrecer pantallas administrativas, formularios, listados, adjuntos y notificaciones internas.

## Decisión

La aplicación cliente será WPF sobre .NET 8, organizada con patrón MVVM.

## Alternativas consideradas

- WinForms: válido para formularios simples, pero menos adecuado para una interfaz modular moderna.
- MAUI: aporta multiplataforma, pero añade complejidad no necesaria para el alcance actual.
- Web SPA: facilita despliegue central, pero contradice la preferencia inicial de escritorio.

## Consecuencias

- El producto inicial queda orientado a Windows.
- El cliente podrá integrarse bien con DPAPI, sistema de archivos local y experiencia de escritorio.
- Las reglas de negocio definitivas no estarán en el cliente.
- Las pruebas E2E de interfaz necesitarán agente Windows.

