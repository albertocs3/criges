# CriGes

CriGes es un software de gestion empresarial orientado a escritorio Windows con API central, SQL Server y modulos funcionales para clientes, catalogo, suscripciones, facturacion, contabilidad, tesoreria, atencion al cliente, seguridad y configuracion.

El repositorio esta actualmente en fase de definicion documental y preparacion de la primera rebanada vertical.

## Arquitectura

La arquitectura decidida es:

```text
Monolito modular + capas limpias por modulo
```

Decisiones principales:

- Cliente de escritorio WPF sobre .NET 8.
- API central ASP.NET Core.
- Worker Service para trabajos de fondo.
- SQL Server como base de datos central.
- Entity Framework Core como ORM principal.
- Migraciones mediante herramienta dedicada.
- Autenticacion, autorizacion y validacion final siempre en servidor.
- Modulos separados por limites funcionales claros.

Cada modulo relevante puede organizarse en:

```text
Domain
Application
Infrastructure
Contracts
Api
```

## Documentacion

La documentacion principal esta en [`docs/`](docs/README.md).

Orden recomendado de lectura:

1. [`docs/README.md`](docs/README.md)
2. [`docs/00-vision-general.md`](docs/00-vision-general.md)
3. [`docs/04-alcance-mvp.md`](docs/04-alcance-mvp.md)
4. [`docs/05-arquitectura-tecnica.md`](docs/05-arquitectura-tecnica.md)
5. [`docs/06-estructura-solucion-dotnet.md`](docs/06-estructura-solucion-dotnet.md)
6. [`docs/adr/README.md`](docs/adr/README.md)
7. [`docs/07-backlog-tecnico-primera-rebanada.md`](docs/07-backlog-tecnico-primera-rebanada.md)
8. [`docs/08-plan-creacion-fisica-solucion.md`](docs/08-plan-creacion-fisica-solucion.md)

## Archivos de trabajo para agentes

- [`agente.md`](agente.md): reglas para asistentes o agentes que trabajen en el proyecto.
- [`review.md`](review.md): guia para revisiones de codigo con subagentes especializados.

## Primera rebanada vertical

La primera implementacion se centra en Plataforma Fase 0 y debe seguir:

- [`docs/07-backlog-tecnico-primera-rebanada.md`](docs/07-backlog-tecnico-primera-rebanada.md)
- [`docs/08-plan-creacion-fisica-solucion.md`](docs/08-plan-creacion-fisica-solucion.md)
- [`docs/plataforma/02-casos-de-uso.md`](docs/plataforma/02-casos-de-uso.md)
- [`docs/plataforma/03-reglas-de-negocio.md`](docs/plataforma/03-reglas-de-negocio.md)
- [`docs/plataforma/04-modelo-de-dominio.md`](docs/plataforma/04-modelo-de-dominio.md)
- [`docs/plataforma/05-modelo-fisico-datos.md`](docs/plataforma/05-modelo-fisico-datos.md)
- [`docs/plataforma/06-contratos-api.md`](docs/plataforma/06-contratos-api.md)
- [`docs/plataforma/07-diseno-pantallas.md`](docs/plataforma/07-diseno-pantallas.md)
- [`docs/plataforma/08-plan-de-pruebas.md`](docs/plataforma/08-plan-de-pruebas.md)

## Estado actual

- Documentacion funcional inicial completada.
- Arquitectura tecnica inicial definida.
- ADR iniciales aceptados.
- Plan de creacion fisica de solucion definido.
- Solucion .NET pendiente de creacion fisica.

## Proximo paso recomendado

Crear la solucion .NET siguiendo [`docs/08-plan-creacion-fisica-solucion.md`](docs/08-plan-creacion-fisica-solucion.md), empezando por:

1. `CriGes.sln`
2. configuracion comun de build
3. Building Blocks
4. modulo `Platform`
5. hosts `CriGes.Api`, `CriGes.Worker`, `CriGes.Desktop` y `CriGes.DbMigrator`
6. primeras pruebas de arquitectura
