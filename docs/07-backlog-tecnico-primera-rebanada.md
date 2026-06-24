# Backlog técnico: primera rebanada vertical

## 1. Propósito

Este documento convierte la primera rebanada vertical de Plataforma en trabajo técnico planificable.

La rebanada inicial implementa de extremo a extremo:

`PLT-CU-001 - Inicializar el sistema`

Incluye:

- Base de solución y proyectos mínimos.
- Migración inicial de Plataforma.
- Dominio y aplicación de inicialización.
- Persistencia transaccional.
- Contratos HTTP.
- Endpoint de estado e inicialización.
- Asistente WPF de inicialización.
- Auditoría mínima.
- Idempotencia mínima.
- Pruebas unitarias, integración, contrato y E2E necesarias.

No pretende completar toda la Fase 0. Debe dejar una instalación nueva inicializada y preparada para continuar con login y sesiones.

## 2. Referencias

- [Casos de uso de Plataforma](plataforma/02-casos-de-uso.md).
- [Reglas de negocio de Plataforma](plataforma/03-reglas-de-negocio.md).
- [Modelo de dominio de Plataforma](plataforma/04-modelo-de-dominio.md).
- [Modelo físico de datos de Plataforma](plataforma/05-modelo-fisico-datos.md).
- [Contratos HTTP de Plataforma](plataforma/06-contratos-api.md).
- [Diseño de pantallas de Plataforma](plataforma/07-diseno-pantallas.md).
- [Plan de pruebas de Plataforma](plataforma/08-plan-de-pruebas.md).
- [Arquitectura técnica general](05-arquitectura-tecnica.md).
- [Estructura inicial de la solución .NET](06-estructura-solucion-dotnet.md).
- [Registro de decisiones arquitectónicas](adr/README.md).

## 3. Objetivo de la rebanada

Al terminar esta rebanada:

1. La solución .NET compila.
2. La base de datos se crea mediante `CriGes.DbMigrator`.
3. `GET /api/v1/platform/installation` informa si el sistema requiere inicialización.
4. `POST /api/v1/platform/installation/initialize` inicializa una instalación nueva.
5. El escritorio detecta una instalación no inicializada y muestra el asistente.
6. La inicialización crea roles base, permisos, primer administrador, identidad `Sistema`, empresa mínima, configuración regional, contadores globales y auditoría.
7. La contraseña inicial nunca queda en texto legible.
8. La operación es transaccional e idempotente.
9. Una segunda inicialización se rechaza.
10. Las pruebas `PLT-TP-001` a `PLT-TP-005` tienen cobertura automatizada inicial.

## 4. Alcance funcional

### Incluido

- `PLT-CU-001`.
- `INI-RN-001` a `INI-RN-015`.
- Contratos:
  - `GET /platform/installation`.
  - `POST /platform/installation/initialize`.
- Pantalla:
  - Conexión inicial.
  - Asistente de inicialización.
- Tablas mínimas necesarias:
  - `Installations`.
  - `Roles`.
  - `Permissions`.
  - `RolePermissions`.
  - `Users`.
  - `ReservedUserNames`.
  - `Companies`.
  - `NumberCounters`.
  - `ConfigurationVersions`.
  - `AuditEvents`.
  - `OutboxMessages`.
  - tablas auxiliares mínimas si la implementación de idempotencia lo requiere.

### Excluido

- Login real y emisión de tokens.
- Gestión completa de usuarios.
- Gestión completa de roles personalizados.
- Configuración fiscal completa.
- SMTP.
- Notificaciones.
- Adjuntos.
- Copias y restauración.
- Worker operativo completo.
- Autorización por permisos en endpoints autenticados.

La rebanada puede crear datos que usarán los siguientes casos de uso, pero no implementa sus pantallas ni operaciones.

## 5. Decisiones aplicadas

| ADR | Impacto en la rebanada |
|---|---|
| [ADR-0001](adr/ADR-0001-monolito-modular.md) | Se implementa dentro del módulo Plataforma del monolito modular. |
| [ADR-0002](adr/ADR-0002-dotnet-8.md) | Todos los proyectos usan .NET 8. |
| [ADR-0003](adr/ADR-0003-wpf-desktop.md) | El asistente se implementa en WPF/MVVM. |
| [ADR-0004](adr/ADR-0004-api-central.md) | Desktop inicializa llamando a la API, nunca a SQL Server. |
| [ADR-0005](adr/ADR-0005-sql-server.md) | La persistencia usa SQL Server. |
| [ADR-0006](adr/ADR-0006-ef-core.md) | EF Core gestiona la persistencia principal. |
| [ADR-0007](adr/ADR-0007-capas-por-modulo.md) | Plataforma se separa en Domain, Application, Infrastructure, Contracts y Api. |
| [ADR-0008](adr/ADR-0008-outbox.md) | Se deja preparada la Outbox para eventos de inicialización. |
| [ADR-0009](adr/ADR-0009-autenticacion-sesiones.md) | Se crea el hash de contraseña compatible con autenticación posterior. |
| [ADR-0013](adr/ADR-0013-auditoria-append-only.md) | La inicialización registra auditoría append-only. |
| [ADR-0015](adr/ADR-0015-db-migrator.md) | La base se crea mediante migrador dedicado. |

## 6. Definición de preparado

Una tarea está preparada para implementarse cuando:

1. Tiene identificador estable.
2. Tiene objetivo técnico claro.
3. Declara entradas y salidas.
4. Declara reglas `RN` afectadas.
5. Declara pruebas esperadas.
6. No depende de una decisión abierta no registrada.
7. Puede verificarse en local o integración.

## 7. Definición de terminado

Una tarea se considera terminada cuando:

1. Compila sin advertencias no justificadas.
2. Tiene pruebas proporcionales a su riesgo.
3. No introduce secretos en configuración, logs, auditoría o datos de prueba.
4. Respeta las dependencias permitidas.
5. Actualiza OpenAPI o documentación si cambia el contrato.
6. Los errores devuelven códigos funcionales estables cuando salen por API.
7. La trazabilidad con `PLT-CU`, `RN` y `PLT-TP` queda reflejada en pruebas o metadatos.

## 8. Backlog

### EPIC-PVS-001 - Base de solución ejecutable

**Objetivo:** crear la estructura mínima para poder implementar una rebanada vertical real.

**Trazabilidad:** arquitectura, estructura .NET, ADR-0001 a ADR-0007.

| ID | Tarea | Resultado | Pruebas |
|---|---|---|---|
| PVS-001 | Crear `CriGes.sln` y archivos raíz | Solución con `global.json`, `.editorconfig`, `Directory.Build.props`, `Directory.Packages.props` y `NuGet.config` | Compilación de solución vacía |
| PVS-002 | Crear proyectos BuildingBlocks | `SharedKernel`, `Application.Abstractions`, `Infrastructure`, `Contracts` | Prueba de referencias permitidas |
| PVS-003 | Crear proyectos Plataforma | `Platform.Domain`, `Platform.Application`, `Platform.Infrastructure`, `Platform.Contracts`, `Platform.Api` | Prueba de arquitectura básica |
| PVS-004 | Crear hosts | `CriGes.Api`, `CriGes.Worker`, `CriGes.Desktop`, `CriGes.DbMigrator` | Arranque básico de API, Worker y migrador |
| PVS-005 | Configurar referencias permitidas | Referencias alineadas con el documento de estructura | Prueba automática de dependencias prohibidas |
| PVS-006 | Crear proyectos de prueba iniciales | Unitarias, integración, contrato y arquitectura | `dotnet test` ejecuta suites vacías o mínimas |

**Criterio de salida:** la solución compila y las pruebas de arquitectura impiden dependencias básicas prohibidas.

### EPIC-PVS-002 - Infraestructura transversal mínima

**Objetivo:** disponer de servicios técnicos comunes necesarios para inicializar.

**Trazabilidad:** INI-RN-013 a 015, AUD-RN-001 a 018, contrato HTTP.

| ID | Tarea | Resultado | Pruebas |
|---|---|---|---|
| PVS-007 | Implementar reloj UTC | Puerto de reloj y servicio de infraestructura | Unitarias de uso de UTC |
| PVS-008 | Implementar generación de identificadores | Puerto para UUID y generador por defecto | Unitarias deterministas con doble |
| PVS-009 | Implementar resultado y errores comunes | `Result`, `Error`, códigos funcionales | Unitarias de mapeo de error |
| PVS-010 | Implementar Correlation ID | Middleware API y propagación a Application | Contrato de cabecera `X-Correlation-Id` |
| PVS-011 | Implementar `ProblemDetails` común | Errores API con código y correlación | Contrato de validación y conflicto |
| PVS-012 | Implementar hash de contraseña | Servicio compatible con PBKDF2 versionado | Seguridad: no reversible, no texto claro |
| PVS-013 | Implementar sanitizado de auditoría | Filtro de secretos para auditoría y logs | Seguridad: contraseña ausente en auditoría |
| PVS-014 | Implementar idempotencia HTTP mínima | Almacenamiento y comportamiento para inicialización | API: misma clave/cuerpo devuelve resultado; distinto cuerpo rechaza |

**Criterio de salida:** la API puede aceptar comandos idempotentes con correlación y errores uniformes.

### EPIC-PVS-003 - Persistencia inicial de Plataforma

**Objetivo:** crear el esquema físico mínimo para soportar inicialización transaccional.

**Trazabilidad:** modelo físico, INI-RN-001 a 015, PLT-TP-003 y PLT-TP-004.

| ID | Tarea | Resultado | Pruebas |
|---|---|---|---|
| PVS-015 | Configurar `PlatformDbContext` | DbContext con esquema `platform` | Integración con SQL Server |
| PVS-016 | Mapear `Installations` | Singleton de instalación con `rowversion` | Restricción de única instalación |
| PVS-017 | Mapear roles y permisos | `Roles`, `Permissions`, `RolePermissions` | Roles protegidos y permisos únicos |
| PVS-018 | Mapear usuarios y nombres reservados | `Users`, `ReservedUserNames` | Usuario único y no reutilización |
| PVS-019 | Mapear empresa mínima | `Companies` con singleton, NIF protegido si aplica | Única empresa |
| PVS-020 | Mapear contadores globales | `NumberCounters` para ámbito global | Unicidad por código |
| PVS-021 | Mapear versión de configuración | `ConfigurationVersions` vigente inicial | Única versión vigente |
| PVS-022 | Mapear auditoría append-only | `AuditEvents` y mecanismo de inserción | No actualización ni borrado ordinario |
| PVS-023 | Mapear Outbox mínima | `OutboxMessages` preparada | Inserción dentro de transacción |
| PVS-024 | Crear primera migración | `CreatePlatformSchema` | Migración desde cero |
| PVS-025 | Implementar `DbMigrator` mínimo | Aplica migraciones y devuelve código de salida | Integración: base creada desde cero |

**Criterio de salida:** una base vacía puede migrarse y contiene las restricciones mínimas para evitar estados imposibles.

### EPIC-PVS-004 - Dominio de inicialización

**Objetivo:** modelar las invariantes de `Instalacion` y los valores necesarios para el primer arranque.

**Trazabilidad:** PLT-CU-001, INI-RN-001 a 015.

| ID | Tarea | Resultado | Pruebas |
|---|---|---|---|
| PVS-026 | Crear agregado `Instalacion` | Estados `NoInicializada`, `Inicializando`, `Inicializada`, `Fallida` | Unitarias de transiciones válidas |
| PVS-027 | Crear entidades/VO de rol base | Roles `Administrador`, `Facturación`, `Contabilidad`, `Técnico` protegidos | Unitarias de protección |
| PVS-028 | Crear `NombreUsuario` | Normalización y validación de usuario | Unitarias de vacío, espacios y normalización |
| PVS-029 | Crear `PoliticaClave` | Validación de longitud y complejidad inicial | Unitarias `SEG-RN-018` a `SEG-RN-022` usadas por inicialización |
| PVS-030 | Crear modelo de administrador inicial | Usuario activo con rol Administrador | Unitarias de usuario válido |
| PVS-031 | Crear identidad técnica `Sistema` | Usuario/proceso técnico no interactivo según modelo elegido | Unitarias de existencia requerida |
| PVS-032 | Crear `Empresa` mínima | Razón social, NIF, dirección, contacto, idioma, moneda y zona | Unitarias de configuración regional |
| PVS-033 | Crear eventos de dominio | `InicializacionIniciada`, `PlataformaInicializada`, `InicializacionFallida` | Unitarias de eventos emitidos |

**Criterio de salida:** el dominio impide marcar una instalación como inicializada si faltan piezas obligatorias.

### EPIC-PVS-005 - Caso de uso de inicialización

**Objetivo:** coordinar la operación transaccional que crea el estado inicial.

**Trazabilidad:** PLT-CU-001, INI-RN-001 a 015, PLT-TP-001 a 005.

| ID | Tarea | Resultado | Pruebas |
|---|---|---|---|
| PVS-034 | Definir `InitializePlatformCommand` | Comando de Application con empresa y administrador | Unitarias de construcción |
| PVS-035 | Definir validador del comando | Validación de obligatorios, NIF, usuario y contraseña | Unitarias/API de errores 422 |
| PVS-036 | Definir puertos de persistencia | Repositorios/Unit of Work necesarios | Prueba de Application con dobles |
| PVS-037 | Implementar `InitializePlatformHandler` | Orquesta creación de todos los datos | Unitarias con dobles |
| PVS-038 | Implementar transacción | Todo se confirma o revierte | Integración `PLT-TP-003` |
| PVS-039 | Crear roles base y permisos | Roles protegidos con permisos iniciales | Integración `PLT-TP-004` |
| PVS-040 | Crear primer administrador | Usuario activo, nombre reservado y hash de contraseña | Seguridad `PLT-TP-005` |
| PVS-041 | Crear identidad `Sistema` | Actor técnico disponible para procesos | Integración `PLT-TP-004` |
| PVS-042 | Crear empresa y configuración regional | Español, euro y `Europe/Madrid` | Integración `INI-RN-008` a 010 |
| PVS-043 | Crear contadores globales | Contadores iniciales según catálogo acordado | Integración `INI-RN-011` |
| PVS-044 | Activar auditoría y registrar evento | Evento de inicialización sin secretos | Integración y seguridad |
| PVS-045 | Marcar instalación inicializada | Estado final con administrador inicial | Integración `INI-RN-006` |
| PVS-046 | Rechazar segunda inicialización | Error funcional `PLATFORM.ALREADY_INITIALIZED` | API `PLT-TP-002` |

**Criterio de salida:** `InitializePlatformHandler` cumple el flujo principal y los alternativos de `PLT-CU-001`.

### EPIC-PVS-006 - Contratos y API

**Objetivo:** exponer la inicialización por HTTP con contratos estables.

**Trazabilidad:** contratos HTTP, PLT-CU-001.

| ID | Tarea | Resultado | Pruebas |
|---|---|---|---|
| PVS-047 | Crear contratos C# de instalación | Request/Response de estado e inicialización | Contract tests de serialización |
| PVS-048 | Implementar `GET /api/v1/platform/installation` | Devuelve `notInitialized` o `initialized` | API: instalación vacía e inicializada |
| PVS-049 | Implementar `POST /api/v1/platform/installation/initialize` | Crea instalación y devuelve 201 | API `PLT-TP-001` |
| PVS-050 | Aplicar idempotencia obligatoria | Requiere `Idempotency-Key` | API `PLT-TP-083` y `PLT-TP-084` parcial |
| PVS-051 | Aplicar rate limit inicial | Política restrictiva para inicialización | API/seguridad básica |
| PVS-052 | Mapear errores funcionales | 409, 422 y 500 seguro según contrato | Contract tests de `ProblemDetails` |
| PVS-053 | Generar OpenAPI | Contratos documentados en `/openapi/v1.json` | Comparación básica de OpenAPI |

**Criterio de salida:** la API permite inicializar desde un cliente externo y documenta el contrato.

### EPIC-PVS-007 - Desktop: conexión y asistente inicial

**Objetivo:** permitir que un usuario inicialice CriGes desde la aplicación de escritorio.

**Trazabilidad:** diseño de pantallas, PLT-CU-001, PLT-TP-001.

| ID | Tarea | Resultado | Pruebas |
|---|---|---|---|
| PVS-054 | Crear shell de arranque mínimo | Pantalla de conexión y comprobación de API | E2E básico de arranque |
| PVS-055 | Crear cliente HTTP de instalación | Métodos `GetInstallationStatus` e `Initialize` | Unitarias con handler HTTP falso |
| PVS-056 | Detectar estado no inicializado | Navega al asistente cuando corresponda | E2E de navegación |
| PVS-057 | Implementar pasos de empresa | Razón social, nombre comercial, NIF | UI validation |
| PVS-058 | Implementar pasos de dirección/contacto | Dirección, CP, localidad, provincia, país, teléfono y correo | UI validation |
| PVS-059 | Implementar paso de administrador | Nombre, usuario, contraseña y confirmación | UI validation de política |
| PVS-060 | Implementar revisión sin contraseña | Resumen seguro y advertencia de operación única | E2E visual/funcional |
| PVS-061 | Implementar progreso y resultado | Bloqueo de navegación durante inicialización | E2E `PLT-TP-001` |
| PVS-062 | Gestionar errores de inicialización | Validación, ya inicializado, conexión y error técnico | E2E/API error handling |
| PVS-063 | Redirigir a login pendiente | Tras inicializar, muestra que el siguiente flujo será login | E2E final de rebanada |

**Criterio de salida:** un usuario puede completar el asistente y dejar la instalación lista para login.

### EPIC-PVS-008 - Seguridad y privacidad de la rebanada

**Objetivo:** evitar que el primer flujo introduzca deuda peligrosa en credenciales, logs o auditoría.

**Trazabilidad:** INI-RN-015, SEG-RN-023 a 025, AUD-RN-018, PLT-TP-005.

| ID | Tarea | Resultado | Pruebas |
|---|---|---|---|
| PVS-064 | Revisar almacenamiento de contraseña | Solo hash versionado en `Users` | Seguridad `PLT-TP-005` |
| PVS-065 | Revisar logs de API | No incluyen contraseña ni request completo sensible | Seguridad con capturador de logs |
| PVS-066 | Revisar auditoría | No contiene contraseña ni secretos | Seguridad `PLT-TP-005` |
| PVS-067 | Revisar errores de validación | No devuelven datos sensibles innecesarios | Contract tests |
| PVS-068 | Revisar Desktop | No guarda contraseña en disco ni logs | Prueba manual/automatizada razonable |

**Criterio de salida:** la contraseña inicial no aparece fuera del hash bajo búsquedas automatizadas razonables en base, auditoría y logs de prueba.

### EPIC-PVS-009 - Pruebas de aceptación de la rebanada

**Objetivo:** materializar `PLT-TP-001` a `PLT-TP-005`.

**Trazabilidad:** plan de pruebas.

| ID | Tarea | Resultado | Pruebas |
|---|---|---|---|
| PVS-069 | Automatizar `PLT-TP-001` | Inicialización E2E válida | E2E Desktop/API/SQL |
| PVS-070 | Automatizar `PLT-TP-002` | Segunda inicialización rechazada | API |
| PVS-071 | Automatizar `PLT-TP-003` | Fallo intermedio revierte todo | Integración con fallo inyectado |
| PVS-072 | Automatizar `PLT-TP-004` | Datos iniciales obligatorios creados | Integración |
| PVS-073 | Automatizar `PLT-TP-005` | Contraseña no legible | Seguridad |
| PVS-074 | Añadir trazabilidad en pruebas | Traits `PLT-CU`, `RN`, `PLT-TP` | Revisión de metadata |
| PVS-075 | Crear datos de prueba seguros | Fixtures sin secretos reales | Revisión de repositorio |

**Criterio de salida:** las cinco pruebas P0 de inicialización pueden ejecutarse de forma repetible.

## 9. Orden recomendado de implementación

1. `EPIC-PVS-001` - Base de solución ejecutable.
2. `EPIC-PVS-002` - Infraestructura transversal mínima.
3. `EPIC-PVS-003` - Persistencia inicial de Plataforma.
4. `EPIC-PVS-004` - Dominio de inicialización.
5. `EPIC-PVS-005` - Caso de uso de inicialización.
6. `EPIC-PVS-006` - Contratos y API.
7. `EPIC-PVS-007` - Desktop: conexión y asistente inicial.
8. `EPIC-PVS-008` - Seguridad y privacidad de la rebanada.
9. `EPIC-PVS-009` - Pruebas de aceptación.

Aunque el orden sea secuencial, conviene cerrar pequeños cortes verticales:

1. API devuelve estado `notInitialized`.
2. Migrador crea esquema vacío.
3. Handler inicializa sin Desktop.
4. API inicializa desde prueba de contrato.
5. Desktop inicializa usando la API.

## 10. Matriz de trazabilidad

| Elemento | Cobertura en backlog |
|---|---|
| PLT-CU-001 | EPIC-PVS-004 a EPIC-PVS-009 |
| INI-RN-001 | PVS-046, PVS-070 |
| INI-RN-002 | PVS-046, PVS-070 |
| INI-RN-003 | PVS-027, PVS-039, PVS-072 |
| INI-RN-004 | PVS-027, PVS-039, PVS-072 |
| INI-RN-005 | PVS-030, PVS-040, PVS-072 |
| INI-RN-006 | PVS-026, PVS-045, PVS-072 |
| INI-RN-007 | PVS-031, PVS-041, PVS-072 |
| INI-RN-008 | PVS-032, PVS-042, PVS-072 |
| INI-RN-009 | PVS-032, PVS-042, PVS-072 |
| INI-RN-010 | PVS-032, PVS-042, PVS-072 |
| INI-RN-011 | PVS-020, PVS-043, PVS-072 |
| INI-RN-012 | PVS-022, PVS-044, PVS-072 |
| INI-RN-013 | PVS-038, PVS-071 |
| INI-RN-014 | PVS-038, PVS-071 |
| INI-RN-015 | PVS-012, PVS-040, PVS-064 a 068, PVS-073 |
| PLT-TP-001 | PVS-069 |
| PLT-TP-002 | PVS-070 |
| PLT-TP-003 | PVS-071 |
| PLT-TP-004 | PVS-072 |
| PLT-TP-005 | PVS-073 |

## 11. Riesgos técnicos

| Riesgo | Impacto | Mitigación |
|---|---|---|
| Sembrar roles/permisos en migración y también en inicialización | Duplicidad o estados incoherentes | Separar catálogo estable de permisos y activación funcional de roles durante inicialización |
| Auditoría dentro de la misma transacción | Puede bloquear la inicialización si falla auditoría | Considerar auditoría como parte obligatoria de la transacción para `PLT-CU-001`; si falla, revierte |
| Idempotencia de inicialización mal definida | Reintentos pueden crear duplicados | Guardar respuesta asociada a `Idempotency-Key` y cuerpo normalizado |
| Hash de contraseña dependiente de librería concreta | Dificulta pruebas y evolución | Encapsular detrás de puerto `PasswordHasher` |
| Desktop implementa validaciones distintas a API | Errores inconsistentes | Validación local solo orientativa; API decide |
| Migración inicial demasiado grande | Difícil de revisar | Mantener primera migración en Plataforma mínima, pero consistente |
| Prueba E2E WPF costosa al inicio | Puede retrasar el corte vertical | Automatizar al menos el flujo principal; dejar accesibilidad completa para fase posterior |

## 12. Datos semilla mínimos

La rebanada necesita definir explícitamente:

### Permisos de Plataforma iniciales

- `Platform.ManageUsers`.
- `Platform.ManageRoles`.
- `Platform.ManageConfiguration`.
- `Platform.ViewAudit`.
- `Platform.ExportAudit`.
- `Platform.ViewDiagnostics`.
- `Platform.ManageBackups`.
- `Platform.RestoreBackups`.
- `Platform.ViewSessions`.
- `Platform.CloseSessions`.
- `Platform.UseAttachments`.

### Roles base

| Rol | Tipo | Protegido | Permisos iniciales |
|---|---|---|---|
| Administrador | Base | Sí | Todos los permisos iniciales |
| Facturación | Base | Sí | Sin permisos efectivos de Plataforma salvo los que se definan en fases posteriores |
| Contabilidad | Base | Sí | Sin permisos efectivos de Plataforma salvo los que se definan en fases posteriores |
| Técnico | Base | Sí | Sin permisos efectivos de Plataforma salvo los que se definan en fases posteriores |

La asignación exacta de permisos de módulos futuros se completará al implementar cada módulo. Para esta rebanada, solo el rol `Administrador` necesita acceso completo a Plataforma.

### Contadores globales mínimos

Se crearán los contadores globales técnicamente necesarios para Plataforma. Si todavía no hay documentos económicos, se recomienda incluir al menos:

- `AuditExport`.
- `BackupOperation`.
- `TechnicalIncident`.

Los contadores económicos anuales se crearán al implementar ejercicios y facturación.

## 13. Criterios de salida de la rebanada

La rebanada se considera cerrada cuando:

1. `dotnet build` de la solución completa finaliza correctamente.
2. `CriGes.DbMigrator` crea una base vacía funcional.
3. `CriGes.Api` arranca y expone health, OpenAPI y endpoints de instalación.
4. `CriGes.Desktop` arranca, detecta instalación no inicializada y muestra el asistente.
5. Una inicialización válida desde Desktop termina correctamente.
6. La misma base rechaza una segunda inicialización.
7. Una inicialización fallida no deja datos parciales.
8. Los roles base quedan protegidos.
9. El primer administrador queda activo y con rol `Administrador`.
10. La identidad `Sistema` existe.
11. La empresa mínima queda creada con idioma español, moneda euro y zona `Europe/Madrid`.
12. La auditoría contiene el evento de inicialización y no contiene la contraseña.
13. La base no contiene la contraseña en texto claro.
14. Las pruebas `PLT-TP-001` a `PLT-TP-005` están automatizadas o justificadas si alguna parte E2E queda temporalmente manual.
15. La documentación de contrato queda sincronizada con OpenAPI.

## 14. Continuación natural

Tras cerrar esta rebanada, la siguiente será:

`PLT-CU-002 - Iniciar sesión`

Esa rebanada reutilizará:

- Primer administrador.
- Hash de contraseña.
- Roles y permisos base.
- Sesión única.
- Auditoría.
- Contratos de error y correlación.
- Shell de escritorio ya capaz de navegar desde conexión a login.

El plan operativo para crear físicamente la solución antes de implementar estas tareas está definido en [Plan de creación física de la solución](08-plan-creacion-fisica-solucion.md).
