# Guia para agentes de desarrollo en CriGes

## Proposito

Este archivo orienta a cualquier agente o asistente que trabaje en este repositorio. Antes de proponer cambios de arquitectura, crear proyectos o implementar codigo, debe leer y respetar la documentacion vigente en `docs/`.

CriGes esta definido como un sistema de gestion empresarial con escritorio Windows, API central, Worker, SQL Server y modulos funcionales bien delimitados.

## Orden de lectura obligatorio

1. `docs/README.md`
2. `docs/00-vision-general.md`
3. `docs/04-alcance-mvp.md`
4. `docs/05-arquitectura-tecnica.md`
5. `docs/06-estructura-solucion-dotnet.md`
6. `docs/adr/README.md`
7. `docs/07-backlog-tecnico-primera-rebanada.md`
8. `docs/08-plan-creacion-fisica-solucion.md`

Para trabajar en Plataforma Fase 0, leer ademas:

1. `docs/plataforma/02-casos-de-uso.md`
2. `docs/plataforma/03-reglas-de-negocio.md`
3. `docs/plataforma/04-modelo-de-dominio.md`
4. `docs/plataforma/05-modelo-fisico-datos.md`
5. `docs/plataforma/06-contratos-api.md`
6. `docs/plataforma/07-diseno-pantallas.md`
7. `docs/plataforma/08-plan-de-pruebas.md`

## Arquitectura decidida

La arquitectura vigente es:

```text
Monolito modular + capas limpias por modulo
```

Decisiones base:

- `ADR-0001`: CriGes se implementa como monolito modular.
- `ADR-0002`: plataforma .NET 8.
- `ADR-0003`: cliente de escritorio WPF.
- `ADR-0004`: el escritorio opera siempre mediante API central.
- `ADR-0005`: SQL Server como base de datos central.
- `ADR-0006`: EF Core con SQL parametrizado cuando sea necesario.
- `ADR-0007`: cada modulo se separa, cuando lo justifique su tamano, en `Domain`, `Application`, `Infrastructure`, `Contracts` y `Api`.

No reabrir estas decisiones salvo que el usuario lo pida explicitamente o exista un cambio real de contexto. En ese caso, proponer un nuevo ADR en vez de modificar el sentido de uno aceptado.

## Estructura esperada

La solucion fisica debe seguir la estructura descrita en `docs/06-estructura-solucion-dotnet.md`:

```text
src/
  Apps/
    CriGes.Api/
    CriGes.Worker/
    CriGes.Desktop/
  Tools/
    CriGes.DbMigrator/
  BuildingBlocks/
    CriGes.SharedKernel/
    CriGes.Application.Abstractions/
    CriGes.Infrastructure/
    CriGes.Contracts/
  Modules/
    Platform/
      CriGes.Modules.Platform.Domain/
      CriGes.Modules.Platform.Application/
      CriGes.Modules.Platform.Infrastructure/
      CriGes.Modules.Platform.Contracts/
      CriGes.Modules.Platform.Api/
tests/
  Architecture/
  BuildingBlocks/
  Platform/
  EndToEnd/
```

Los modulos futuros deben repetir este patron solo cuando empiece su implementacion real.

## Reglas de dependencias

Respetar estas reglas:

- `Domain` no depende de tecnologia ni de otras capas.
- `Application` depende de `Domain`, contratos estables y abstracciones.
- `Infrastructure` implementa puertos definidos por `Application`.
- `Api` es un adaptador HTTP delgado; no contiene reglas de negocio ni accede directamente a EF Core.
- `Contracts` no expone entidades de dominio ni tipos de EF Core.
- `Desktop` solo consume contratos HTTP y servicios de presentacion; no referencia `Domain`, `Application` ni `Infrastructure` de los modulos.
- Un modulo no accede a la infraestructura interna de otro modulo.
- La comunicacion entre modulos se realiza mediante contratos, eventos o casos de uso autorizados.

Si una regla parece incomoda, no saltarla: revisar primero si falta una abstraccion, un contrato o un evento.

## Seguridad

La API es siempre la autoridad final para:

- Autenticacion.
- Autorizacion.
- Validacion final de reglas de negocio.
- Numeraciones.
- Persistencia.
- Auditoria.

Principios obligatorios:

- No registrar contrasenas, tokens, hashes, claves, cuerpos completos sensibles ni secretos.
- No devolver secretos por API.
- Proteger refresh tokens y sesiones segun `ADR-0009`.
- Validar permisos en servidor segun `ADR-0010`.
- Persistir instantes en UTC y presentar fechas en `Europe/Madrid`.
- Los adjuntos solo se gestionan mediante API y repositorio protegido.
- Las operaciones criticas deben ser auditables.
- Los endpoints deben devolver errores consistentes con `ProblemDetails` y codigo funcional estable cuando aplique.

Antes de implementar autenticacion, permisos, sesiones, adjuntos o auditoria, leer:

- `docs/seguridad/01-especificacion-funcional.md`
- `docs/adr/ADR-0009-autenticacion-sesiones.md`
- `docs/adr/ADR-0010-autorizacion-permisos.md`
- `docs/adr/ADR-0012-adjuntos-ficheros.md`
- `docs/adr/ADR-0013-auditoria-append-only.md`

## Persistencia

Reglas base:

- SQL Server es la base central.
- Cada modulo tiene esquema logico propio.
- EF Core es el ORM principal.
- SQL explicito esta permitido solo si es parametrizado, localizado en `Infrastructure`, probado y justificado por claridad, seguridad o rendimiento.
- Las migraciones se aplican mediante `CriGes.DbMigrator`, no automaticamente al arrancar API, Worker o Desktop.
- No usar atributos de persistencia en `Domain`; preferir configuracion Fluent API.
- No hacer que un modulo modifique directamente tablas propiedad de otro modulo.

## API y contratos

Reglas base:

- La API publica contratos HTTP versionados.
- No exponer entidades de dominio ni modelos EF Core.
- Los endpoints traducen contratos a casos de uso de `Application`.
- OpenAPI debe mantenerse sincronizado cuando cambie el contrato.
- Los errores deben ser estables y utiles para Desktop.
- La compatibilidad entre Desktop y API se controla por version.

## Trabajo recomendado para agentes

Antes de editar:

1. Identificar el modulo propietario.
2. Leer la especificacion funcional y tecnica correspondiente.
3. Verificar ADR relacionados.
4. Confirmar si el cambio afecta contratos, persistencia, seguridad, pruebas o documentacion.

Durante la implementacion:

1. Mantener cambios pequenos y trazables.
2. Seguir el patron existente del modulo.
3. No crear capas globales de negocio.
4. No duplicar reglas compartidas dentro de cada modulo.
5. Actualizar pruebas cerca del comportamiento modificado.
6. Actualizar documentacion si cambia una decision, contrato o regla funcional.

Al terminar:

1. Ejecutar build y pruebas relevantes.
2. Ejecutar pruebas de arquitectura si se tocaron referencias entre proyectos.
3. Indicar que se probo y que no se pudo probar.
4. No ocultar riesgos pendientes.

## Prioridad documental

En caso de contradiccion:

1. Normativa vigente.
2. Integraciones transversales.
3. Requisitos compartidos.
4. Especificacion modular.
5. ADR aceptados.
6. Arquitectura tecnica.
7. Estructura fisica de la solucion.
8. Backlog y plan de creacion.

Si la contradiccion afecta a comportamiento funcional, pedir confirmacion antes de implementar.

## Cosas que no debe hacer un agente

- Convertir el proyecto en microservicios sin un ADR nuevo aceptado.
- Crear una Clean Architecture global que mezcle todos los dominios.
- Permitir que Desktop acceda a SQL Server.
- Poner reglas de negocio en WPF, endpoints o repositorios.
- Compartir entidades de dominio entre modulos.
- Exponer modelos EF Core por API.
- Aplicar migraciones automaticamente desde API o Worker.
- Introducir dependencias tecnologicas en `Domain`.
- Reescribir ADR aceptados para cambiar su sentido.
- Hacer refactors amplios no pedidos.

## Primera rebanada vertical

La primera implementacion debe seguir `docs/07-backlog-tecnico-primera-rebanada.md` y `docs/08-plan-creacion-fisica-solucion.md`.

Objetivo inicial:

- Crear estructura fisica de solucion.
- Crear Building Blocks.
- Crear modulo Plataforma.
- Crear hosts API, Worker, Desktop y DbMigrator.
- Anadir pruebas de arquitectura.
- Empezar `PLT-CU-001` por dominio, persistencia, Application, API y Desktop.

## Criterio de buen cambio

Un cambio es aceptable si:

- Respeta el monolito modular.
- Mantiene las dependencias limpias.
- Conserva la API como autoridad central.
- No rompe la trazabilidad documental.
- Es verificable con build, pruebas o revision clara.
- Reduce ambiguedad sin sobredisenar.
