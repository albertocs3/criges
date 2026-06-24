# Resumen del proyecto CriGes

Documento de revisión general del alcance, fases, arquitectura y primer plan de desarrollo.

Fecha: 23/06/2026

## 1. Visión

CriGes será un software de gestión empresarial integrado para una única empresa.

Su objetivo es concentrar en una sola aplicación:

- Clientes, tiendas y contactos.
- Catálogo de productos, servicios, software y licencias.
- Suscripciones periódicas.
- Presupuestos, facturas ordinarias y facturas rectificativas.
- Atención al cliente e incidencias.
- Compras, gastos, proveedores y contabilidad.
- Tesorería, cobros, pagos, remesas SEPA y conciliación.
- Configuración, usuarios, roles, permisos, auditoría y copias.

El principio central es evitar registros paralelos: cada operación debe introducirse una sola vez y producir sus efectos comerciales, fiscales, contables, bancarios y de trazabilidad de forma coherente.

## 2. Objetivos principales

### Operativos

- Mantener una ficha unificada del cliente.
- Automatizar la facturación periódica de suscripciones.
- Registrar comunicaciones, incidencias y actuaciones.
- Emitir facturas, registrar cobros y generar asientos.
- Controlar compras, gastos, pagos y stock básico.
- Preparar remesas SEPA y conciliar movimientos bancarios.

### Control y seguridad

- Aplicar permisos por rol.
- Auditar operaciones relevantes.
- Evitar modificaciones no autorizadas.
- Conservar documentos emitidos.
- Impedir duplicidades en facturas, renovaciones y numeraciones.
- Proteger datos personales, bancarios y secretos.

### Cumplimiento

- Plan General de Contabilidad español.
- Registros de IVA.
- Preparación para VeriFactu.
- SEPA CORE.
- Conservación documental.
- Protección de datos.

## 3. Roles de usuario

| Rol | Responsabilidades principales |
|---|---|
| Administrador | Acceso completo, configuración, usuarios, permisos, auditoría, costes y operaciones excepcionales. |
| Facturación | Clientes, catálogo, suscripciones, presupuestos, facturas, vencimientos y cobros. |
| Contabilidad | Compras, proveedores, asientos, informes, pagos, tesorería, remesas y conciliación. |
| Técnico | Clientes básicos, tiendas, comunicaciones e incidencias. Sin acceso económico sensible. |

## 4. Principios funcionales

1. Dato maestro único por módulo propietario.
2. Motor único de facturación para facturas manuales, suscripciones, presupuestos y anticipos.
3. Automatización con supervisión en procesos económicos.
4. Inmutabilidad de documentos emitidos.
5. Correcciones mediante rectificaciones o nuevas operaciones trazables.
6. Auditoría de acciones relevantes.
7. Seguridad por defecto en servidor e interfaz.
8. Evolución modular sin duplicar responsabilidades.

## 5. Módulos funcionales

### Clientes y tiendas

Gestiona empresas, autónomos y particulares, con NIF/VAT, direcciones, tiendas, contactos, condiciones de pago, IBAN, mandato SEPA, estado activo/inactivo e historial relacionado.

### Catálogo

Gestiona productos, servicios, software y licencias, con categorías, código automático, precio, coste, cuentas contables, impuestos aplicables, estado e inventario básico para productos físicos.

### Suscripciones

Gestiona planes y conceptos periódicos, periodicidades mensual/trimestral/semestral/anual, importes fijos o por licencias, cambios programados, cancelación, reactivación y facturación anticipada agrupada por cliente y forma de pago.

### Facturación

Incluye presupuestos, facturas ordinarias, rectificativas, vencimientos, cobros, impuestos, redondeos, PDF, correo, VeriFactu, asientos automáticos, registros de IVA e inmutabilidad.

### Atención al cliente e incidencias

Registra comunicaciones por teléfono y WhatsApp manual. Solo las comunicaciones con seguimiento generan incidencia. Permite responsables, colaboradores, estados, prioridades, categorías, actuaciones, adjuntos y seguimiento.

### Contabilidad, compras y proveedores

Incluye Plan General Contable español, subcuentas, asientos manuales y automáticos, compras, gastos, IVA soportado, pagos, diario, mayor, balances, registros de IVA, ejercicios y cierres.

### Tesorería y SEPA

Gestiona una cuenta bancaria empresarial, remesas SEPA CORE, mandatos, devoluciones, extractos Norma 43, conciliación manual y previsiones mensuales/anuales.

### Plataforma

Base común de usuarios, roles, permisos, configuración, auditoría, notificaciones, adjuntos, diagnóstico, copias y restauración.

## 6. Fases del MVP

| Fase | Nombre | Resultado esperado |
|---|---|---|
| Fase 0 | Plataforma | Aplicación segura, configurable, auditable y preparada para módulos. |
| Fase 1 | Maestros operativos | Clientes, tiendas, catálogo, stock inicial y cuentas base. |
| Fase 2 | Facturación y atención al cliente | Facturación manual, presupuestos, rectificativas, cobros básicos e incidencias. |
| Fase 3 | Suscripciones | Renovaciones periódicas, vista previa, agrupación y facturación automática supervisada. |
| Fase 4 | Compras, contabilidad y tesorería | Compras, pagos, asientos, informes, remesas, devoluciones y conciliación. |
| Fase 5 | Consolidación posterior | Mejoras, automatizaciones, informes avanzados e integraciones futuras. |

## 7. Alcance de Fase 0

La Fase 0 crea la base técnica y funcional:

- Inicialización del sistema.
- Datos de empresa y cuenta bancaria.
- Ejercicio abierto, impuestos y numeraciones.
- Usuarios, roles y permisos.
- Inicio/cierre de sesión y sesión única.
- Auditoría central.
- Notificaciones internas.
- Repositorio seguro de adjuntos.
- Configuración SMTP básica.
- Diagnóstico técnico.
- Copia de seguridad manual completa.

La primera rebanada vertical será `PLT-CU-001 - Inicializar el sistema`.

## 8. Arquitectura técnica

### Decisiones principales

| Área | Decisión |
|---|---|
| Cliente | WPF sobre .NET 8 |
| API | ASP.NET Core Web API .NET 8 |
| Arquitectura | Monolito modular |
| Procesos de fondo | .NET Worker Service |
| Base de datos | SQL Server |
| Persistencia | Entity Framework Core 8 |
| Seguridad | Usuario/contraseña, access token breve, refresh token opaco |
| Autorización | Permisos y políticas validadas en servidor |
| Auditoría | Append-only |
| Eventos diferidos | Outbox transaccional |
| Adjuntos | Ficheros protegidos en servidor y metadatos en SQL Server |
| Tiempo | UTC en servidor y `Europe/Madrid` en presentación |

### Aplicaciones

- `CriGes.Desktop`: aplicación WPF instalada en cada puesto.
- `CriGes.Api`: proceso central que aplica reglas, transacciones, seguridad y persistencia.
- `CriGes.Worker`: procesos de fondo, Outbox, caducidades, notificaciones, copias y reintentos.
- `CriGes.DbMigrator`: herramienta dedicada a aplicar migraciones.

### Estructura modular

Cada módulo relevante se organizará en:

- `Domain`.
- `Application`.
- `Infrastructure`.
- `Contracts`.
- `Api`.

El escritorio solo consume contratos HTTP. No accede a SQL Server ni a capas internas.

## 9. Primera rebanada vertical

### Caso de uso

`PLT-CU-001 - Inicializar el sistema`

Objetivo: dejar una instalación nueva en condiciones de acceso administrativo.

### Debe crear

- Registro de instalación inicializada.
- Roles base protegidos:
  - Administrador.
  - Facturación.
  - Contabilidad.
  - Técnico.
- Primer usuario administrador.
- Identidad técnica `Sistema`.
- Empresa mínima.
- Idioma español.
- Moneda euro.
- Zona horaria `Europe/Madrid`.
- Contadores globales.
- Auditoría activa.
- Evento de inicialización.

### Reglas clave

- La inicialización solo se ejecuta una vez.
- No puede existir instalación inicializada sin administrador activo.
- La contraseña nunca se almacena en texto legible.
- La operación es transaccional.
- Un fallo no puede dejar datos parciales.

## 10. Backlog técnico inicial

La primera rebanada se ha dividido en 9 épicas y 75 tareas:

| Épica | Contenido |
|---|---|
| EPIC-PVS-001 | Base de solución ejecutable |
| EPIC-PVS-002 | Infraestructura transversal mínima |
| EPIC-PVS-003 | Persistencia inicial de Plataforma |
| EPIC-PVS-004 | Dominio de inicialización |
| EPIC-PVS-005 | Caso de uso de inicialización |
| EPIC-PVS-006 | Contratos y API |
| EPIC-PVS-007 | Desktop: conexión y asistente inicial |
| EPIC-PVS-008 | Seguridad y privacidad |
| EPIC-PVS-009 | Pruebas de aceptación |

Las primeras tareas físicas son:

- Crear `CriGes.sln`.
- Crear archivos raíz.
- Crear Building Blocks.
- Crear proyectos de Plataforma.
- Crear API, Worker, Desktop y DbMigrator.
- Crear proyectos de prueba.
- Configurar referencias permitidas.
- Validar con `dotnet restore`, `dotnet build` y `dotnet test`.

## 11. Plan de creación física

La solución física prevista contiene:

```text
src/
├── Apps/
│   ├── CriGes.Api/
│   ├── CriGes.Worker/
│   └── CriGes.Desktop/
├── Tools/
│   └── CriGes.DbMigrator/
├── BuildingBlocks/
│   ├── CriGes.SharedKernel/
│   ├── CriGes.Application.Abstractions/
│   ├── CriGes.Infrastructure/
│   └── CriGes.Contracts/
└── Modules/
    └── Platform/
        ├── CriGes.Modules.Platform.Domain/
        ├── CriGes.Modules.Platform.Application/
        ├── CriGes.Modules.Platform.Infrastructure/
        ├── CriGes.Modules.Platform.Contracts/
        └── CriGes.Modules.Platform.Api/
```

Y pruebas:

```text
tests/
├── Architecture/
├── BuildingBlocks/
├── Platform/
└── EndToEnd/
```

## 12. Pruebas y criterios de calidad

La Fase 0 tiene un plan de 106 escenarios `PLT-TP`.

Para la primera rebanada son críticos:

| Prueba | Escenario |
|---|---|
| PLT-TP-001 | Inicializar una instalación nueva con datos válidos. |
| PLT-TP-002 | Rechazar una segunda inicialización. |
| PLT-TP-003 | Revertir completamente ante un fallo intermedio. |
| PLT-TP-004 | Crear roles base, administrador, Sistema y contadores. |
| PLT-TP-005 | Confirmar que la contraseña no queda legible en datos, auditoría o logs. |

La salida de cada fase exige:

- Compilación correcta.
- Pruebas automatizadas.
- Auditoría activa.
- Seguridad en servidor.
- Sin secretos en logs ni configuración.
- Sin estados parciales.
- Documentación sincronizada.

## 13. Riesgos principales

| Riesgo | Mitigación |
|---|---|
| Cambios normativos en VeriFactu o factura electrónica | Validación legal antes de producción. |
| Complejidad transversal de facturación, contabilidad y tesorería | Motor común, transacciones e idempotencia. |
| Duplicidad de datos entre módulos | Propietario único de dato y contratos entre módulos. |
| Exposición de datos sensibles | Cifrado, HMAC de búsqueda, permisos y auditoría. |
| Estados parciales en procesos críticos | Transacciones ACID y Outbox. |
| Crecimiento de auditoría y adjuntos | Diseño de retención, índices y copias completas. |

## 14. Próximos pasos

1. Ejecutar el plan de creación física de la solución.
2. Crear proyectos y referencias.
3. Añadir primera prueba de arquitectura.
4. Compilar solución vacía.
5. Implementar infraestructura transversal mínima.
6. Empezar `PLT-CU-001` por dominio y persistencia.
7. Exponer endpoints de instalación.
8. Crear asistente WPF.
9. Automatizar pruebas `PLT-TP-001` a `PLT-TP-005`.

## 15. Documentación generada

Documentos principales:

- `00-vision-general.md`.
- `00-requisitos-compartidos.md`.
- `01-integraciones-transversales.md`.
- `02-mapa-modulos.md`.
- `03-glosario.md`.
- `04-alcance-mvp.md`.
- `05-arquitectura-tecnica.md`.
- `06-estructura-solucion-dotnet.md`.
- `07-backlog-tecnico-primera-rebanada.md`.
- `08-plan-creacion-fisica-solucion.md`.
- `adr/README.md`.
- Documentos de Plataforma:
  - casos de uso.
  - reglas de negocio.
  - modelo de dominio.
  - modelo físico.
  - contratos API.
  - diseño de pantallas.
  - plan de pruebas.

