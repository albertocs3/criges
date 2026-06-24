# Plan de pruebas: Fase 0 - Plataforma

## 1. Propósito

Este documento define cómo se verificará la Fase 0 de Plataforma antes de considerarla terminada.

El plan cubre:

- Los 33 casos de uso de Plataforma.
- Las 253 reglas de negocio.
- El modelo de dominio y el modelo físico.
- Los contratos HTTP.
- La aplicación de escritorio.
- Seguridad, concurrencia, auditoría y recuperación.
- Servicios externos simulados o controlados.

No sustituye los casos de prueba automatizados ni sus datos ejecutables. Define su alcance, prioridad, trazabilidad y criterios de aceptación.

## 2. Documentos de referencia

1. [Casos de uso](02-casos-de-uso.md).
2. [Reglas de negocio](03-reglas-de-negocio.md).
3. [Modelo de dominio](04-modelo-de-dominio.md).
4. [Modelo físico de datos](05-modelo-fisico-datos.md).
5. [Contratos HTTP](06-contratos-api.md).
6. [Diseño de pantallas](07-diseno-pantallas.md).
7. [Arquitectura técnica](../05-arquitectura-tecnica.md).
8. [Requisitos compartidos](../00-requisitos-compartidos.md).

## 3. Objetivos de calidad

La Fase 0 debe demostrar que:

1. Una instalación nueva puede inicializarse de forma atómica.
2. Solo los usuarios autorizados acceden a cada operación.
3. Las sesiones, bloqueos y cambios de permisos se aplican inmediatamente.
4. La configuración queda validada, versionada y protegida.
5. Toda acción sensible deja una auditoría suficiente y no manipulable.
6. Las notificaciones se entregan sin duplicidades funcionales.
7. Los adjuntos no se publican antes de superar los controles de seguridad.
8. Los errores pueden correlacionarse sin exponer secretos.
9. Las copias son completas, verificables y restaurables.
10. Los fallos no dejan estados parciales ni datos incoherentes.
11. Desktop y API respetan el contrato y gestionan errores de forma consistente.
12. El sistema mantiene su comportamiento bajo concurrencia y reintentos.

## 4. Alcance

### Incluido

- Inicialización.
- Autenticación, contraseñas, bloqueo y sesiones.
- Usuarios, roles y permisos.
- Empresa, cuenta bancaria, ejercicios, impuestos y numeraciones.
- SMTP y aplicación de configuración.
- Auditoría.
- Notificaciones internas y SignalR.
- Adjuntos.
- Diagnóstico, operaciones y salud.
- Copias y restauración.
- API y shell de escritorio de Plataforma.
- Persistencia, migraciones y restricciones de la base de datos.

### Excluido

- Funcionalidad propia de los módulos posteriores.
- Pruebas fiscales o legales de facturación.
- Integraciones reales con VeriFactu, WhatsApp o banca.
- Rendimiento de procesos masivos de módulos aún no implementados.
- Pruebas completas de instalación y actualización del producto, salvo lo necesario para Fase 0.

## 5. Identificadores y prioridades

Los escenarios usan el prefijo `PLT-TP`.

| Prioridad | Significado |
|---|---|
| P0 | Bloquea la salida; protege acceso, integridad o recuperación |
| P1 | Función principal obligatoria de la fase |
| P2 | Comportamiento relevante que no impide el uso básico |

Un escenario puede materializarse en varias pruebas para cubrir datos válidos, límites y errores.

## 6. Niveles de prueba

### 6.1 Unitarias

Verifican sin infraestructura externa:

- Entidades, objetos de valor y políticas.
- Validadores.
- Transiciones de estado.
- Cálculo de caducidades.
- Evaluación de permisos.
- Normalización de nombres e identificadores.
- Construcción de eventos y resultados de dominio.

### 6.2 Integración

Usan SQL Server y los adaptadores reales que corresponda:

- Mapeos de EF Core.
- Restricciones, índices y concurrencia.
- Transacciones.
- Repositorios.
- Idempotencia.
- Outbox.
- Cifrado y almacenamiento de secretos.
- Repositorio de archivos.
- Creación y restauración de copias.

Cada prueba debe aislar sus datos y restaurar el entorno conocido.

### 6.3 Contrato HTTP

Verifican:

- Rutas, verbos y códigos HTTP.
- DTO, validaciones y `application/problem+json`.
- Autenticación y autorización.
- Paginación, filtros y ordenación.
- `ETag`, `If-Match` e idempotencia.
- Cabeceras de correlación y compatibilidad.
- Compatibilidad del OpenAPI publicado.

### 6.4 Componentes externos

Se probarán mediante dobles controlables:

- SMTP válido, rechazado, no disponible y con tiempo de espera.
- Antivirus limpio, infectado, no disponible y con tiempo de espera.
- Repositorio de archivos disponible, lleno o inaccesible.
- Repositorio de copias disponible, alterado o sin espacio.
- SignalR conectado, desconectado y reconectado.

### 6.5 Extremo a extremo

Ejecutan `CriGes.Desktop` contra API y base de datos de pruebas:

- Navegación.
- Visibilidad por permisos.
- Formularios y validaciones.
- Confirmaciones y motivos.
- Gestión de conflictos.
- Operaciones asíncronas.
- Accesibilidad básica y escalado.

### 6.6 No funcionales

- Seguridad.
- Rendimiento.
- Resiliencia.
- Recuperación.
- Compatibilidad.
- Accesibilidad.
- Privacidad de logs y pantallas.

## 7. Entornos

| Entorno | Uso | Datos |
|---|---|---|
| Local | Unitarias y desarrollo | Generados por prueba |
| Integración | API, SQL Server y Worker | Reiniciables |
| E2E | Desktop, API, Worker y servicios simulados | Conjunto versionado |
| Preproducción | Seguridad, rendimiento y recuperación | Sintéticos, nunca reales |

Todos los entornos usarán UTC para persistencia y `Europe/Madrid` como zona funcional.

## 8. Datos base

El conjunto mínimo contendrá:

- Instalación no inicializada.
- Instalación inicializada.
- Administrador activo.
- Usuarios de Facturación, Contabilidad y Técnico.
- Usuario desactivado.
- Usuario bloqueado temporalmente.
- Usuario con contraseña provisional.
- Cuatro roles base protegidos.
- Rol personalizado activo e inactivo.
- Empresa con datos completos.
- Ejercicio abierto y ejercicio cerrado.
- Impuestos activos e inactivos.
- Configuración SMTP válida e inválida.
- Notificaciones leídas, no leídas y críticas.
- Adjuntos limpios, pendientes, rechazados y reemplazados.
- Operaciones completadas, fallidas y en curso.
- Copia válida, alterada e incompatible.

Las contraseñas, tokens, certificados y claves usados serán exclusivos del entorno de pruebas.

## 9. Criterios de entrada

Para comenzar una campaña:

1. La versión está identificada.
2. Las migraciones se aplican desde cero.
3. Los servicios simulados están disponibles.
4. Los datos base pueden recrearse automáticamente.
5. El OpenAPI corresponde a la versión desplegada.
6. No existen defectos P0 abiertos de una campaña anterior.
7. Los casos incluidos tienen reglas y criterios de aceptación definidos.

## 10. Criterios de suspensión

La campaña se suspenderá si:

- La base no puede recrearse de forma fiable.
- El entorno produce fallos ajenos al producto de manera repetida.
- La versión desplegada no coincide con la declarada.
- Hay pérdida o contaminación de datos entre pruebas.
- Un defecto P0 invalida una parte sustancial de los resultados.

## 11. Catálogo de pruebas funcionales

### 11.1 Inicialización

| ID | Escenario | Nivel | Prioridad | Trazabilidad |
|---|---|---|---|---|
| PLT-TP-001 | Inicializar una instalación nueva con datos válidos | E2E | P0 | PLT-CU-001; INI-RN-001 a 013 |
| PLT-TP-002 | Rechazar una segunda inicialización | API | P0 | PLT-CU-001; INI-RN-001 y 002 |
| PLT-TP-003 | Revertir completamente la inicialización ante un fallo intermedio | Integración | P0 | PLT-CU-001; INI-RN-013 y 014 |
| PLT-TP-004 | Crear roles base, administrador, identidad Sistema y contadores | Integración | P0 | PLT-CU-001; INI-RN-003 a 012 |
| PLT-TP-005 | Comprobar que la contraseña inicial no queda legible en datos, auditoría o logs | Seguridad | P0 | PLT-CU-001; INI-RN-015 |

### 11.2 Autenticación, contraseñas y sesiones

| ID | Escenario | Nivel | Prioridad | Trazabilidad |
|---|---|---|---|---|
| PLT-TP-006 | Iniciar sesión con credenciales válidas y crear una única sesión | E2E | P0 | PLT-CU-002; SEG-RN-035, 036, 046 y 047 |
| PLT-TP-007 | Rechazar credenciales, usuario, rol o instalación no válidos sin facilitar enumeración | API/Seguridad | P0 | PLT-CU-002; SEG-RN-006, 013, 035 a 041 y 076 |
| PLT-TP-008 | Bloquear temporalmente tras cinco intentos fallidos | Integración | P0 | PLT-CU-003; SEG-RN-037 a 042 |
| PLT-TP-009 | Desbloquear automáticamente al vencer el bloqueo | Integración | P1 | PLT-CU-003; SEG-RN-043 a 045 |
| PLT-TP-010 | Aplicar todos los requisitos y límites de contraseña | Unitarias/API | P0 | PLT-CU-006 y 007; SEG-RN-018 a 031 |
| PLT-TP-011 | Cambiar la contraseña propia e invalidar las sesiones correspondientes | E2E | P0 | PLT-CU-006; SEG-RN-026, 027, 030 y 031 |
| PLT-TP-012 | Restablecer una contraseña sin revelar ni registrar su contenido | E2E/Seguridad | P0 | PLT-CU-007; SEG-RN-025 y 028 a 031 |
| PLT-TP-013 | Cerrar sesión e invalidar refresh y access token | API | P0 | PLT-CU-004; SEG-RN-048 y 049 |
| PLT-TP-014 | Cerrar una sesión desde la administración | E2E | P1 | PLT-CU-008; SEG-RN-050, 051 y 059 |
| PLT-TP-015 | Caducar la sesión tras cinco horas de inactividad | Integración/E2E | P0 | PLT-CU-005; SEG-RN-052 a 056 |
| PLT-TP-016 | Rotar refresh tokens y rechazar su reutilización | API/Seguridad | P0 | PLT-CU-002; SEG-RN-046 a 059 |
| PLT-TP-017 | Rechazar una segunda sesión cuando ya existe una activa | API | P1 | PLT-CU-002; SEG-RN-046 y 047 |

### 11.3 Usuarios

| ID | Escenario | Nivel | Prioridad | Trazabilidad |
|---|---|---|---|---|
| PLT-TP-018 | Crear un usuario activo con un único rol y contraseña válida | E2E | P0 | PLT-CU-009; SEG-RN-001 a 011 |
| PLT-TP-019 | Rechazar nombres duplicados o utilizados históricamente | Integración/API | P0 | PLT-CU-009; SEG-RN-002 y 003 |
| PLT-TP-020 | Modificar datos y rol invalidando la sesión afectada | E2E | P0 | PLT-CU-010; SEG-RN-005 a 008 y 017 |
| PLT-TP-021 | Desactivar con motivo, conservar historial e invalidar sesión | E2E | P0 | PLT-CU-011; SEG-RN-004, 009 y 012 a 016 |
| PLT-TP-022 | Reactivar únicamente con un rol activo | API | P1 | PLT-CU-011; SEG-RN-015 |
| PLT-TP-023 | Desbloquear manualmente a un usuario | E2E | P1 | PLT-CU-012; SEG-RN-043 a 045 |
| PLT-TP-024 | Impedir todas las operaciones de usuario a quien no sea administrador | Seguridad | P0 | PLT-CU-009 a 012; SEG-RN-007 a 009 |

### 11.4 Roles y permisos

| ID | Escenario | Nivel | Prioridad | Trazabilidad |
|---|---|---|---|---|
| PLT-TP-025 | Consultar roles y permisos efectivos | E2E | P1 | PLT-CU-013; SEG-RN-060 a 084 |
| PLT-TP-026 | Crear y copiar un rol personalizado | E2E | P1 | PLT-CU-014; SEG-RN-063 a 068 |
| PLT-TP-027 | Modificar permisos e invalidar sesiones de usuarios afectados | Integración/E2E | P0 | PLT-CU-015; SEG-RN-069 a 071 |
| PLT-TP-028 | Proteger los roles base frente a modificación o eliminación indebida | API | P0 | PLT-CU-015 y 016; SEG-RN-060 a 075 |
| PLT-TP-029 | Desactivar y reactivar roles respetando usuarios asignados | E2E | P1 | PLT-CU-016; SEG-RN-072 a 075 |
| PLT-TP-030 | Ocultar módulos y acciones sin permiso y rechazarlos también en servidor | E2E/Seguridad | P0 | PLT-CU-013; SEG-RN-076 a 084 |
| PLT-TP-031 | Restringir costes y márgenes a Administrador | Seguridad | P0 | PLT-CU-013; SEG-RN-080 a 084 |

### 11.5 Configuración general

| ID | Escenario | Nivel | Prioridad | Trazabilidad |
|---|---|---|---|---|
| PLT-TP-032 | Guardar y modificar datos de empresa y cuenta bancaria válidos | E2E | P0 | PLT-CU-017; CFG-RN-001 a 014 |
| PLT-TP-033 | Validar NIF, IBAN, BIC y campos obligatorios | Unitarias/API | P0 | PLT-CU-017; CFG-RN-001 a 014 |
| PLT-TP-034 | Proteger y enmascarar datos sensibles de configuración | Seguridad | P0 | PLT-CU-017 y 021; CFG-RN-001 a 014 y 039 a 049 |
| PLT-TP-035 | Crear un ejercicio y todos sus contadores | E2E/Integración | P0 | PLT-CU-018; CFG-RN-015 a 027 |
| PLT-TP-036 | Impedir duplicados y usos inválidos de ejercicio o contador | Integración | P0 | PLT-CU-018 y 020; CFG-RN-015 a 027 |
| PLT-TP-037 | Consultar numeraciones sin permitir edición directa | E2E/API | P1 | PLT-CU-020; CFG-RN-020 a 027 |
| PLT-TP-038 | Crear y modificar tipos fiscales válidos | E2E | P0 | PLT-CU-019; CFG-RN-028 a 038 |
| PLT-TP-039 | Impedir modificaciones fiscales incompatibles con su uso histórico | Integración | P0 | PLT-CU-019; CFG-RN-028 a 038 |
| PLT-TP-040 | Guardar SMTP sin devolver la contraseña almacenada | API/Seguridad | P0 | PLT-CU-021; CFG-RN-039 a 049 |
| PLT-TP-041 | Probar SMTP con éxito, rechazo, timeout y servidor caído | Integración/E2E | P1 | PLT-CU-021; CFG-RN-039 a 049 |
| PLT-TP-042 | Validar configuración y presentar todos los errores accionables | E2E | P0 | PLT-CU-022; CFG-RN-054 a 061 |
| PLT-TP-043 | Aplicar configuración y marcar correctamente el reinicio pendiente | E2E | P1 | PLT-CU-023; CFG-RN-050 a 053 |
| PLT-TP-044 | Resolver una edición concurrente mediante ETag sin sobrescritura | API/E2E | P0 | PLT-CU-017 a 023; contrato de concurrencia |

### 11.6 Auditoría

| ID | Escenario | Nivel | Prioridad | Trazabilidad |
|---|---|---|---|---|
| PLT-TP-045 | Registrar altas, cambios, estados y acciones sensibles | Integración | P0 | PLT-CU-024; AUD-RN-001 a 013 |
| PLT-TP-046 | Conservar actor, fecha UTC, entidad, motivo y correlación | Integración | P0 | PLT-CU-024; AUD-RN-001 a 013 |
| PLT-TP-047 | Impedir modificación y eliminación de eventos de auditoría | Seguridad/Integración | P0 | PLT-CU-024; AUD-RN-001 a 020 |
| PLT-TP-048 | Consultar con filtros, paginación y orden estable | API/E2E | P1 | PLT-CU-024; AUD-RN-014 a 019 |
| PLT-TP-049 | Exportar exactamente el conjunto filtrado | E2E | P2 | PLT-CU-025; AUD-RN-014 a 020 |
| PLT-TP-050 | Comprobar que auditoría y exportación no contienen secretos | Seguridad | P0 | PLT-CU-024 y 025; AUD-RN-001 a 020 |

### 11.7 Notificaciones

| ID | Escenario | Nivel | Prioridad | Trazabilidad |
|---|---|---|---|---|
| PLT-TP-051 | Listar, filtrar y marcar notificaciones como leídas | E2E | P1 | PLT-CU-026; NOT-RN-001 a 009 |
| PLT-TP-052 | Archivar notificaciones conservando su trazabilidad | API/E2E | P1 | PLT-CU-026; NOT-RN-001 a 009 |
| PLT-TP-053 | Entregar una notificación crítica por SignalR y mostrarla de forma destacada | E2E | P0 | PLT-CU-027; NOT-RN-010 a 015 |
| PLT-TP-054 | Recuperar notificaciones tras una desconexión sin perder ni duplicar efectos | Integración/E2E | P0 | PLT-CU-026 y 027; NOT-RN-010 a 015 |
| PLT-TP-055 | Aplicar conservación y archivado tras un año | Integración | P2 | PLT-CU-026; NOT-RN-013 a 015 |

### 11.8 Adjuntos

| ID | Escenario | Nivel | Prioridad | Trazabilidad |
|---|---|---|---|---|
| PLT-TP-056 | Cargar un JPG o PDF válido de hasta 16 MB | E2E | P0 | PLT-CU-028; ADJ-RN-001 a 013 |
| PLT-TP-057 | Rechazar extensión, MIME, firma o tamaño no permitido | Seguridad/API | P0 | PLT-CU-028; ADJ-RN-001 a 013 |
| PLT-TP-058 | Mantener el archivo en cuarentena hasta resultado limpio | Integración | P0 | PLT-CU-028; ADJ-RN-001 a 013 |
| PLT-TP-059 | Rechazar y aislar un archivo infectado | Integración/Seguridad | P0 | PLT-CU-028; ADJ-RN-001 a 013 |
| PLT-TP-060 | Descargar solo un archivo limpio y autorizado | API/Seguridad | P0 | PLT-CU-029; ADJ-RN-014 a 017 |
| PLT-TP-061 | Reemplazar conservando metadatos e historial exigido | E2E/Integración | P1 | PLT-CU-030; ADJ-RN-018 a 021 |
| PLT-TP-062 | No publicar el archivo si falla almacenamiento, metadatos o antivirus | Resiliencia | P0 | PLT-CU-028; ADJ-RN-001 a 022 |
| PLT-TP-063 | Evitar acceso cruzado mediante identificadores conocidos | Seguridad | P0 | PLT-CU-029; ADJ-RN-014 a 017 |

### 11.9 Diagnóstico y operaciones

| ID | Escenario | Nivel | Prioridad | Trazabilidad |
|---|---|---|---|---|
| PLT-TP-064 | Registrar un error técnico con Correlation ID y contexto permitido | Integración | P0 | PLT-CU-031; OPS-RN-001 a 007 |
| PLT-TP-065 | Consultar y filtrar errores solo con permiso administrativo | E2E/Seguridad | P1 | PLT-CU-031; OPS-RN-001 a 007 |
| PLT-TP-066 | Eliminar contraseñas, tokens y secretos de logs y respuestas | Seguridad | P0 | PLT-CU-031; OPS-RN-001 a 012 |
| PLT-TP-067 | Mostrar el estado y progreso de una operación asíncrona | E2E | P1 | PLT-CU-031 a 033; OPS-RN-008 a 012 |
| PLT-TP-068 | Mantener una operación idempotente ante reintento del cliente | Integración/API | P0 | PLT-CU-031 a 033; contrato de idempotencia |
| PLT-TP-069 | Verificar endpoints de salud, dependencias y compatibilidad de cliente | API | P0 | PLT-CU-031; contrato HTTP |

### 11.10 Copias y restauración

| ID | Escenario | Nivel | Prioridad | Trazabilidad |
|---|---|---|---|---|
| PLT-TP-070 | Crear una copia completa con base, archivos, configuración y claves necesarias | Integración/E2E | P0 | PLT-CU-032; BKP-RN-001 y 003 a 011 |
| PLT-TP-071 | Cifrar, firmar o verificar la integridad de la copia | Seguridad | P0 | PLT-CU-032; BKP-RN-003 a 011 |
| PLT-TP-072 | Detectar una copia incompleta, alterada o indescifrable | Integración | P0 | PLT-CU-032 y 033; BKP-RN-008 a 011 y 020 a 023 |
| PLT-TP-073 | Restaurar una copia válida en mantenimiento exclusivo | Recuperación/E2E | P0 | PLT-CU-033; BKP-RN-002 y 012 a 024 |
| PLT-TP-074 | Impedir accesos y escrituras durante la restauración | Seguridad/E2E | P0 | PLT-CU-033; BKP-RN-012 a 019 |
| PLT-TP-075 | Recuperar el estado anterior o activar procedimiento controlado si falla la restauración | Recuperación | P0 | PLT-CU-033; BKP-RN-019 y 024 |
| PLT-TP-076 | Auditar inicio, resultado y actor de copia y restauración | Integración | P0 | PLT-CU-032 y 033; BKP-RN-001 a 024 |

## 12. Pruebas transversales de API

| ID | Escenario | Prioridad |
|---|---|---|
| PLT-TP-077 | Todas las rutas protegidas rechazan ausencia, manipulación o caducidad del token | P0 |
| PLT-TP-078 | Cada endpoint exige su permiso aunque Desktop oculte la acción | P0 |
| PLT-TP-079 | Los errores siguen `application/problem+json` e incluyen Correlation ID | P0 |
| PLT-TP-080 | Los listados respetan paginación, filtros, orden y límites máximos | P1 |
| PLT-TP-081 | Las fechas UTC, fechas puras, UUID y decimales usan el formato acordado | P1 |
| PLT-TP-082 | `If-Match` rechaza versiones obsoletas sin perder cambios | P0 |
| PLT-TP-083 | Una misma `Idempotency-Key` y carga devuelve el mismo resultado | P0 |
| PLT-TP-084 | Reutilizar la clave con otra carga se rechaza | P0 |
| PLT-TP-085 | Rate limiting limita abuso sin bloquear el tráfico legítimo | P1 |
| PLT-TP-086 | Un cliente inferior a la versión mínima recibe una respuesta accionable | P0 |
| PLT-TP-087 | El OpenAPI no presenta cambios incompatibles no aprobados | P1 |
| PLT-TP-088 | Las respuestas no exponen entidades EF, hashes, claves ni secretos | P0 |

## 13. Pruebas de base de datos

| ID | Escenario | Prioridad |
|---|---|---|
| PLT-TP-089 | Crear la base desde cero mediante todas las migraciones | P0 |
| PLT-TP-090 | Actualizar desde cada versión de Fase 0 soportada | P0 |
| PLT-TP-091 | Verificar claves, índices únicos y restricciones del modelo físico | P0 |
| PLT-TP-092 | Verificar precisión de fechas, versiones y campos cifrados | P0 |
| PLT-TP-093 | Resolver concurrencia optimista sin actualización perdida | P0 |
| PLT-TP-094 | Confirmar que las transacciones críticas revierten todos sus cambios | P0 |
| PLT-TP-095 | Publicar Outbox una sola vez a nivel funcional tras reintentos | P0 |
| PLT-TP-096 | Aplicar conservación sin eliminar identidades ni documentos protegidos | P1 |

## 14. Pruebas de escritorio

| ID | Escenario | Prioridad |
|---|---|---|
| PLT-TP-097 | Navegar con teclado por shell, listados, formularios y diálogos | P1 |
| PLT-TP-098 | Mostrar foco, etiquetas y errores sin depender únicamente del color | P1 |
| PLT-TP-099 | Mantener filtros y contexto al volver a un listado | P2 |
| PLT-TP-100 | Avisar antes de descartar cambios sin guardar | P1 |
| PLT-TP-101 | Gestionar carga, vacío, error, desconexión y reintento | P1 |
| PLT-TP-102 | Mostrar conflictos de concurrencia sin sobrescribir silenciosamente | P0 |
| PLT-TP-103 | Ocultar datos y acciones no permitidos | P0 |
| PLT-TP-104 | Funcionar a 1366x768 y con escalado de Windows del 100 %, 125 % y 150 % | P1 |
| PLT-TP-105 | No bloquear la interfaz durante operaciones asíncronas | P1 |
| PLT-TP-106 | Mostrar errores técnicos con referencia útil y sin detalles sensibles | P0 |

## 15. Pruebas de seguridad

Se aplicarán como mínimo:

1. Autorización horizontal y vertical en todos los endpoints.
2. Enumeración de usuarios y recursos.
3. Fuerza bruta y bloqueo.
4. Reutilización, robo y rotación de tokens.
5. Inyección en filtros, ordenaciones, textos y nombres de archivo.
6. Manipulación de MIME, extensión y firma de archivos.
7. Acceso directo a adjuntos.
8. Exposición de secretos en API, logs, auditoría y UI.
9. Modificación de auditoría.
10. Manipulación y restauración de copias.
11. Idempotencia y repetición de comandos.
12. Elevación de privilegios tras cambios de rol o sesión.

Los hallazgos se clasificarán por impacto y explotabilidad. Ningún hallazgo crítico o alto podrá permanecer abierto al cerrar la fase.

## 16. Pruebas de rendimiento

Los objetivos iniciales, sujetos a medición en el entorno acordado, son:

| Operación | Objetivo |
|---|---|
| Login válido | p95 inferior a 1 segundo sin contar latencia externa |
| Consulta paginada ordinaria | p95 inferior a 1 segundo |
| Comando ordinario | p95 inferior a 1,5 segundos |
| Notificación conectada | entrega p95 inferior a 2 segundos |
| Inicio del shell | interactivo en menos de 3 segundos tras autenticar |
| Operación larga | confirmación de aceptación en menos de 2 segundos |

Las copias, restauraciones, análisis antivirus y exportaciones se medirán por volumen y no compartirán el límite de una operación ordinaria.

Se probarán:

- 25 usuarios internos concurrentes.
- Ráfagas de login fallido.
- Paginación sobre auditoría de gran volumen.
- Entrega simultánea de notificaciones.
- Carga concurrente de adjuntos.
- Ejecución de Worker sin degradar las operaciones interactivas.

## 17. Pruebas de resiliencia

Se introducirán fallos controlados en:

- SQL Server antes y durante una transacción.
- Worker antes y después de confirmar un mensaje.
- SMTP.
- Antivirus.
- Almacenamiento de adjuntos.
- Repositorio de copias.
- SignalR.
- Red entre Desktop y API.

Se comprobará que:

1. No se confirman estados parciales.
2. Los reintentos son limitados y observables.
3. Las operaciones idempotentes no duplican efectos.
4. El usuario recibe un estado accionable.
5. La recuperación posterior no requiere manipulación manual de datos.

## 18. Pruebas de recuperación

La prueba de restauración no se considerará superada por el mero hecho de que el comando finalice.

Después de restaurar se verificará:

- Inicio correcto de API y Worker.
- Acceso del administrador esperado.
- Integridad referencial.
- Usuarios, roles y permisos.
- Configuración y secretos utilizables.
- Auditoría.
- Adjuntos y sus hashes.
- Notificaciones.
- Operaciones.
- Numeraciones y contadores.
- Capacidad de crear una nueva copia.

Se conservará evidencia del tiempo de recuperación y del punto temporal restaurado.

## 19. Automatización

### Ejecución por cambio

- Unitarias.
- Arquitectura.
- Validadores.
- Contratos y OpenAPI.
- Integración afectada.

### Ejecución diaria

- Integración completa.
- API.
- Escenarios E2E P0.
- Escaneo de dependencias y secretos.

### Ejecución previa a versión

- Catálogo completo P0 y P1.
- E2E.
- Seguridad.
- Rendimiento.
- Copia y restauración real.
- Migración desde versiones soportadas.

Las pruebas inestables se tratarán como defectos; no se aceptará ocultarlas mediante reintentos indefinidos.

## 20. Trazabilidad de cobertura

| Área | Casos de uso | Reglas | Pruebas principales |
|---|---|---|---|
| Inicialización | PLT-CU-001 | INI-RN-001 a 015 | PLT-TP-001 a 005 |
| Autenticación y sesiones | PLT-CU-002 a 008 | SEG-RN-018 a 059 | PLT-TP-006 a 017 |
| Usuarios | PLT-CU-009 a 012 | SEG-RN-001 a 017 y 043 a 045 | PLT-TP-018 a 024 |
| Roles y permisos | PLT-CU-013 a 016 | SEG-RN-060 a 084 | PLT-TP-025 a 031 |
| Configuración | PLT-CU-017 a 023 | CFG-RN-001 a 061 | PLT-TP-032 a 044 |
| Auditoría | PLT-CU-024 y 025 | AUD-RN-001 a 020 | PLT-TP-045 a 050 |
| Notificaciones | PLT-CU-026 y 027 | NOT-RN-001 a 015 | PLT-TP-051 a 055 |
| Adjuntos | PLT-CU-028 a 030 | ADJ-RN-001 a 022 | PLT-TP-056 a 063 |
| Diagnóstico | PLT-CU-031 | OPS-RN-001 a 012 | PLT-TP-064 a 069 |
| Copias | PLT-CU-032 y 033 | BKP-RN-001 a 024 | PLT-TP-070 a 076 |
| Transversal | Todos | Todas | PLT-TP-077 a 106 |

La cobertura detallada se mantendrá en el código mediante rasgos, categorías o metadatos con los identificadores `PLT-CU`, `RN` y `PLT-TP`.

## 21. Cobertura mínima

- 100 % de casos de uso con al menos una prueba automatizada.
- 100 % de reglas P0 cubiertas.
- 100 % de endpoints con pruebas de autenticación y autorización.
- 100 % de comandos sensibles con prueba de auditoría.
- 100 % de comandos idempotentes con repetición y conflicto de clave.
- 100 % de recursos con versión con prueba de concurrencia.
- Cobertura de líneas como indicador, no como criterio único.
- Objetivo inicial del 80 % en Domain y Application para código con lógica.

No se exigirá cobertura artificial de DTO triviales, migraciones generadas o código de arranque sin lógica.

## 22. Gestión de defectos

| Severidad | Criterio |
|---|---|
| Crítica | Pérdida de datos, acceso no autorizado, restauración imposible o sistema inutilizable |
| Alta | Flujo P0/P1 incumplido sin alternativa segura |
| Media | Función degradada con alternativa aceptable |
| Baja | Defecto visual, textual o de usabilidad menor |

Cada defecto incluirá:

- Versión y entorno.
- Precondiciones.
- Pasos reproducibles.
- Resultado esperado y observado.
- Correlation ID.
- Evidencias sin datos sensibles.
- Caso de uso, regla y prueba relacionados.

Todo defecto corregido requiere una prueba de regresión automatizada cuando sea técnicamente razonable.

## 23. Evidencias

La campaña conservará:

- Resultado de cada suite.
- Versión de aplicación, base y contratos.
- Logs de prueba depurados.
- Informes de cobertura.
- Informe de seguridad.
- Medidas de rendimiento.
- Resultado de migraciones.
- Informe de copia y restauración.
- Relación de defectos abiertos y aceptados.

No se almacenarán contraseñas, tokens, claves privadas ni datos personales reales como evidencia.

## 24. Criterios de salida

La Fase 0 podrá cerrarse cuando:

1. Los 106 escenarios del catálogo estén implementados o justificados.
2. Todos los P0 y P1 aplicables hayan sido ejecutados.
3. No existan defectos críticos o altos abiertos.
4. Los 33 casos de uso y las 253 reglas tengan cobertura trazable.
5. Todos los endpoints hayan superado autorización y contrato.
6. Las migraciones desde cero y desde versiones soportadas funcionen.
7. La copia y restauración completa se hayan demostrado.
8. La revisión de secretos, logs y auditoría sea satisfactoria.
9. Los objetivos de rendimiento se cumplan o tengan una aceptación documentada.
10. Las desviaciones conocidas estén aprobadas y vinculadas a una tarea posterior.

## 25. Responsabilidades

| Responsabilidad | Rol |
|---|---|
| Mantener reglas y criterios funcionales | Responsable funcional |
| Implementar pruebas unitarias e integración | Desarrollo |
| Mantener entorno, dobles y datos | Desarrollo/QA |
| Ejecutar campañas y registrar defectos | QA |
| Revisar autorización y exposición de datos | Seguridad/Desarrollo |
| Aprobar recuperación y restauración | Administrador técnico |
| Aceptar la salida de fase | Responsable del producto |

Una misma persona puede asumir varios roles, pero la restauración y los controles de seguridad deberán revisarse explícitamente.

## 26. Pendientes para implementación

- Elegir framework concreto de automatización E2E para WPF.
- Definir herramienta de comparación OpenAPI.
- Concretar el motor antivirus de pruebas.
- Fijar volúmenes de referencia para auditoría y adjuntos.
- Establecer RTO y RPO definitivos.
- Definir el almacén de informes de campaña.
- Convertir `PLT-TP-001` a `PLT-TP-106` en suites ejecutables.

La distribución de estas suites y de los proyectos productivos está definida en [Estructura inicial de la solución .NET](../06-estructura-solucion-dotnet.md).
