# Documentación funcional de CriGes

## 1. Propósito

Este directorio contiene la especificación funcional del software de gestión CriGes.

La documentación describe:

- Qué debe hacer el sistema.
- Qué módulo es responsable de cada función.
- Qué reglas de negocio deben cumplirse.
- Cómo se integran los módulos.
- Qué decisiones quedan pendientes antes del diseño técnico.

No constituye todavía una especificación técnica ni sustituye la validación fiscal, contable, bancaria o de protección de datos que corresponda.

## 2. Orden de lectura

1. [Visión general](00-vision-general.md)
2. [Requisitos compartidos](00-requisitos-compartidos.md)
3. [Integraciones transversales](01-integraciones-transversales.md)
4. [Mapa de módulos](02-mapa-modulos.md)
5. [Glosario común](03-glosario.md)
6. [Alcance y fases del MVP](04-alcance-mvp.md)
7. [Arquitectura técnica](05-arquitectura-tecnica.md)
8. [Estructura de la solución .NET](06-estructura-solucion-dotnet.md)
9. [Registro de decisiones arquitectónicas](adr/README.md)
10. [Backlog técnico de la primera rebanada vertical](07-backlog-tecnico-primera-rebanada.md)
11. [Plan de creación física de la solución](08-plan-creacion-fisica-solucion.md)
12. Especificaciones funcionales y técnicas de cada módulo.

## 3. Documentos transversales

| Documento | Contenido |
|---|---|
| [Visión general](00-vision-general.md) | Objetivos, alcance global, usuarios y principios del producto. |
| [Requisitos compartidos](00-requisitos-compartidos.md) | Auditoría, notificaciones, adjuntos, fechas, importes, conservación y protección de datos. |
| [Integraciones transversales](01-integraciones-transversales.md) | Motor único de facturación, contabilidad, anticipos, rectificaciones, VeriFactu e inmutabilidad. |
| [Mapa de módulos](02-mapa-modulos.md) | Responsabilidades, propietarios de datos y dependencias. |
| [Glosario común](03-glosario.md) | Definiciones compartidas y términos que no deben confundirse. |
| [Alcance y fases del MVP](04-alcance-mvp.md) | Priorización, entregas, exclusiones y criterios de finalización. |
| [Arquitectura técnica](05-arquitectura-tecnica.md) | Aplicaciones, módulos, persistencia, seguridad, despliegue e integraciones. |
| [Estructura de la solución .NET](06-estructura-solucion-dotnet.md) | Proyectos, dependencias, composición, pruebas y orden inicial de creación. |
| [Registro de decisiones arquitectónicas](adr/README.md) | ADR iniciales, alternativas consideradas y consecuencias aceptadas. |
| [Backlog técnico de la primera rebanada vertical](07-backlog-tecnico-primera-rebanada.md) | Tareas técnicas trazables para implementar `PLT-CU-001`. |
| [Plan de creación física de la solución](08-plan-creacion-fisica-solucion.md) | Secuencia de comandos, proyectos, referencias y validaciones para crear la solución real. |

## 4. Especificaciones por módulo

| Módulo | Especificación |
|---|---|
| Clientes y Tiendas | [Especificación funcional](clientes/01-especificacion-funcional.md) |
| Catálogo e Inventario | [Especificación funcional](catalogo/01-especificacion-funcional.md) |
| Suscripciones | [Especificación funcional](suscripciones/01-especificacion-funcional.md) |
| Facturación | [Especificación funcional](facturacion/01-especificacion-funcional.md) |
| Atención al Cliente | [Especificación funcional](atencion-cliente/01-especificacion-funcional.md) |
| Contabilidad, Compras y Proveedores | [Especificación funcional](contabilidad/01-especificacion-funcional.md) |
| Tesorería y SEPA | [Especificación funcional](tesoreria/01-especificacion-funcional.md) |
| Usuarios, Roles y Seguridad | [Especificación funcional](seguridad/01-especificacion-funcional.md) |
| Configuración General | [Especificación funcional](configuracion/01-especificacion-funcional.md) |
| Plataforma - Fase 0 | [Casos de uso](plataforma/02-casos-de-uso.md) |
| Plataforma - Fase 0 | [Reglas de negocio](plataforma/03-reglas-de-negocio.md) |
| Plataforma - Fase 0 | [Modelo de dominio](plataforma/04-modelo-de-dominio.md) |
| Plataforma - Fase 0 | [Modelo físico de datos](plataforma/05-modelo-fisico-datos.md) |
| Plataforma - Fase 0 | [Contratos HTTP](plataforma/06-contratos-api.md) |
| Plataforma - Fase 0 | [Diseño de pantallas](plataforma/07-diseno-pantallas.md) |
| Plataforma - Fase 0 | [Plan de pruebas](plataforma/08-plan-de-pruebas.md) |

## 5. Estado documental

| Área | Estado |
|---|---|
| Recopilación funcional por módulos | Completada inicialmente |
| Requisitos compartidos | Completados inicialmente |
| Cruces funcionales principales | Revisados inicialmente |
| Validación fiscal y legal | Pendiente |
| Glosario | Completado inicialmente |
| Alcance y priorización del MVP | Completado inicialmente |
| Casos de uso | Fase 0 completada inicialmente |
| Reglas de negocio normalizadas | Fase 0 completada inicialmente |
| Modelo de dominio | Fase 0 completada inicialmente |
| Modelo físico de datos | Fase 0 completada inicialmente |
| Arquitectura técnica | Completada inicialmente |
| Estructura de la solución .NET | Completada inicialmente |
| Registro de decisiones arquitectónicas | Completado inicialmente |
| Backlog técnico primera rebanada vertical | Completado inicialmente |
| Plan de creación física de la solución | Completado inicialmente |
| Diseño de pantallas | Fase 0 completada inicialmente |
| Plan de pruebas | Fase 0 completada inicialmente |

## 6. Convención documental

Cada módulo podrá incorporar:

```text
modulo/
├── 01-especificacion-funcional.md
├── 02-casos-de-uso.md
├── 03-reglas-de-negocio.md
├── 04-modelo-de-dominio.md
├── 05-pantallas.md
└── 06-plan-de-pruebas.md
```

Los requisitos comunes no deben copiarse en cada módulo. Las especificaciones modulares deben enlazar con el documento transversal y declarar únicamente sus excepciones.

## 7. Precedencia

En caso de contradicción:

1. La normativa vigente prevalece sobre la documentación.
2. Las integraciones transversales prevalecen en operaciones entre módulos.
3. Los requisitos compartidos prevalecen en reglas generales.
4. La especificación modular prevalece para reglas propias de su ámbito.
5. Las decisiones técnicas posteriores no pueden modificar una regla funcional sin actualizar esta documentación.

## 8. Módulos futuros o técnicos

Quedan identificados, pero todavía no especificados:

- Copias de seguridad y restauración.
- Monitorización y diagnóstico técnico.
- Importación inicial de datos.
- Mantenimiento y actualización de la aplicación.
- Integraciones automáticas con WhatsApp.
- Portal del cliente.
- Factura electrónica B2B, cuando se concrete su marco definitivo.
