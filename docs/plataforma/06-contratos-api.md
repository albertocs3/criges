# Contratos HTTP: Fase 0 - Plataforma

## 1. Propósito

Este documento define el contrato HTTP entre `CriGes.Desktop` y `CriGes.Api` para la Fase 0.

Incluye:

- Convenciones generales.
- Autenticación y sesiones.
- Usuarios, roles y permisos.
- Configuración.
- Auditoría.
- Notificaciones.
- Adjuntos.
- Diagnóstico.
- Copias y restauración.
- Errores.
- Concurrencia.
- Idempotencia.

La API no expone entidades de dominio ni modelos EF Core.

## 2. Base y formato

- Base: `/api/v1`.
- Transporte: HTTPS obligatorio.
- Formato principal: `application/json; charset=utf-8`.
- Fechas y horas: ISO 8601 UTC, por ejemplo `2026-06-23T15:30:45.123Z`.
- Fechas puras: `yyyy-MM-dd`.
- Identificadores: UUID.
- Importes: números JSON decimales, nunca cadenas formateadas.
- OpenAPI: `/openapi/v1.json`.

## 3. Cabeceras

### Petición

| Cabecera | Uso |
|---|---|
| `Authorization: Bearer <token>` | Acceso protegido |
| `X-Correlation-Id` | Correlación opcional del cliente |
| `Idempotency-Key` | Comandos repetibles de riesgo |
| `If-Match` | Versión ETag para modificación |
| `Accept-Language` | Reservado; inicialmente `es-ES` |
| `X-Client-Version` | Versión de Desktop |
| `X-Device-Id` | Identificador local del puesto |

### Respuesta

| Cabecera | Uso |
|---|---|
| `X-Correlation-Id` | Correlación efectiva |
| `ETag` | Versión del recurso |
| `Location` | URI de recurso creado o proceso |
| `Retry-After` | Espera recomendada |
| `X-Minimum-Client-Version` | Cliente mínimo compatible |

## 4. Autenticación

### Tokens

- Access token JWT de corta duración.
- Refresh token opaco y rotatorio.
- El refresh token solo aparece en respuestas de autenticación.
- Desktop lo protege con DPAPI.

### Respuesta de sesión

```json
{
  "accessToken": "eyJ...",
  "accessTokenExpiresAtUtc": "2026-06-23T15:45:00Z",
  "refreshToken": "opaque-token",
  "refreshTokenExpiresAtUtc": "2026-07-23T15:30:00Z",
  "session": {
    "id": "8c91ad97-5246-4a3e-9f68-cfb755e08953",
    "startedAtUtc": "2026-06-23T15:30:00Z",
    "idleExpiresAtUtc": "2026-06-23T20:30:00Z"
  },
  "user": {
    "id": "a1979dc3-60c0-43e3-920d-4df88a204568",
    "displayName": "Administrador",
    "role": "Administrador",
    "permissions": [
      "Platform.ManageUsers",
      "Platform.ManageConfiguration"
    ]
  }
}
```

## 5. Errores

Se utilizará `application/problem+json`.

```json
{
  "type": "https://docs.criges.local/errors/concurrency-conflict",
  "title": "El registro ha cambiado",
  "status": 409,
  "detail": "Recarga el registro antes de volver a modificarlo.",
  "instance": "/api/v1/platform/users/a1979dc3-60c0-43e3-920d-4df88a204568",
  "code": "PLATFORM.CONCURRENCY_CONFLICT",
  "correlationId": "665035d1-5382-4e6e-bdfa-7442b9ba394e",
  "errors": {
    "etag": ["La versión enviada ya no es vigente."]
  }
}
```

### Estados comunes

| HTTP | Uso |
|---|---|
| 200 | Consulta o acción síncrona correcta |
| 201 | Recurso creado |
| 202 | Proceso asíncrono aceptado |
| 204 | Acción correcta sin cuerpo |
| 400 | Petición mal formada |
| 401 | No autenticado o token inválido |
| 403 | Sin permiso |
| 404 | Recurso no encontrado o no visible |
| 409 | Conflicto de negocio, concurrencia o duplicidad |
| 412 | `If-Match` ausente o no válido cuando sea obligatorio |
| 413 | Archivo demasiado grande |
| 415 | Tipo de contenido no admitido |
| 422 | Validación semántica |
| 423 | Recurso bloqueado |
| 429 | Demasiados intentos |
| 503 | Dependencia o mantenimiento no disponible |

### Códigos comunes

- `PLATFORM.VALIDATION_FAILED`.
- `PLATFORM.NOT_FOUND`.
- `PLATFORM.FORBIDDEN`.
- `PLATFORM.CONCURRENCY_CONFLICT`.
- `PLATFORM.IDEMPOTENCY_CONFLICT`.
- `PLATFORM.MAINTENANCE_MODE`.
- `PLATFORM.CLIENT_VERSION_UNSUPPORTED`.

## 6. Paginación y filtros

### Petición

- `pageSize`: 1 a 200, predeterminado 50.
- `continuationToken`: paginación por clave.
- `sort`: campo permitido.
- `direction`: `asc` o `desc`.

### Respuesta

```json
{
  "items": [],
  "pageSize": 50,
  "nextContinuationToken": null,
  "totalCount": 0
}
```

`totalCount` podrá omitirse en listados de gran volumen.

## 7. Concurrencia

Los recursos editables devuelven un `ETag`.

Ejemplo:

```http
ETag: "AAAAAAAAB9E="
```

Las operaciones de modificación exigen:

```http
If-Match: "AAAAAAAAB9E="
```

Ausencia:

- HTTP 412.

Versión obsoleta:

- HTTP 409.
- Código `PLATFORM.CONCURRENCY_CONFLICT`.

## 8. Idempotencia

`Idempotency-Key` será obligatorio en:

- Inicialización.
- Creación de usuario.
- Creación de ejercicio.
- Exportaciones.
- Carga y reemplazo de adjuntos.
- Creación de copia.
- Solicitud de restauración.

Reglas:

- Máximo 200 caracteres.
- Mismo usuario, ruta y clave con mismo cuerpo devuelve el resultado original.
- Misma clave con cuerpo diferente devuelve 409.
- El servidor conserva la clave durante el horizonte definido.

## 9. Permisos de Plataforma

| Permiso | Descripción |
|---|---|
| `Platform.ManageUsers` | Crear y modificar usuarios |
| `Platform.ManageRoles` | Gestionar roles y permisos |
| `Platform.ManageConfiguration` | Modificar configuración |
| `Platform.ViewAudit` | Consultar auditoría |
| `Platform.ExportAudit` | Exportar auditoría |
| `Platform.ViewDiagnostics` | Consultar diagnóstico |
| `Platform.ManageBackups` | Crear copias |
| `Platform.RestoreBackups` | Restaurar copias |
| `Platform.ViewSessions` | Consultar sesiones |
| `Platform.CloseSessions` | Cerrar sesiones |
| `Platform.UseAttachments` | Usar adjuntos, combinado con permiso contextual |

Los roles base se sembrarán con sus permisos.

## 10. Estado e inicialización

### `GET /platform/installation`

**Autenticación:** pública solo hasta inicializar; después Administrador.

**Respuesta:**

```json
{
  "status": "notInitialized",
  "productVersion": "1.0.0",
  "requiresInitialization": true
}
```

No devuelve datos sensibles.

### `POST /platform/installation/initialize`

**Autenticación:** pública únicamente si no está inicializado.

**Idempotencia:** obligatoria.

**Petición:**

```json
{
  "company": {
    "legalName": "Empresa Ejemplo SL",
    "tradeName": "Empresa Ejemplo",
    "taxId": "B12345678",
    "address": {
      "line": "Calle Mayor 1",
      "postalCode": "28001",
      "city": "Madrid",
      "region": "Madrid",
      "countryCode": "ES"
    },
    "phone": "+34910000000",
    "email": "administracion@example.com"
  },
  "administrator": {
    "fullName": "Administrador",
    "userName": "admin",
    "password": "StrongPassword1!"
  }
}
```

**Respuesta:** 201.

```json
{
  "installationId": "6692f09c-87ca-4922-b462-ec24ad397b82",
  "status": "initialized",
  "administratorUserId": "a1979dc3-60c0-43e3-920d-4df88a204568",
  "requiresRestart": false
}
```

**Errores:**

- `PLATFORM.ALREADY_INITIALIZED` - 409.
- `PLATFORM.INITIALIZATION_FAILED` - 500.
- `PLATFORM.INVALID_TAX_ID` - 422.
- `SECURITY.USERNAME_ALREADY_RESERVED` - 409.
- `SECURITY.PASSWORD_POLICY_FAILED` - 422.

**Caso:** PLT-CU-001.

## 11. Autenticación y perfil

### `POST /auth/login`

**Autenticación:** pública.

**Petición:**

```json
{
  "userName": "admin",
  "password": "StrongPassword1!",
  "deviceId": "WIN-CLIENT-01",
  "clientVersion": "1.0.0"
}
```

**Respuesta:** 200, `SessionResponse`.

**Errores:**

- `AUTH.INVALID_CREDENTIALS` - 401.
- `AUTH.ACCOUNT_LOCKED` - 423.
- `AUTH.ACCOUNT_DISABLED` - 403.
- `AUTH.ROLE_DISABLED` - 403.
- `AUTH.ACTIVE_SESSION_EXISTS` - 409.
- `PLATFORM.CLIENT_VERSION_UNSUPPORTED` - 426.
- `AUTH.TOO_MANY_ATTEMPTS` - 429.

Los errores públicos no revelan si el usuario existe.

### `POST /auth/refresh`

**Autenticación:** refresh token.

**Petición:**

```json
{
  "sessionId": "8c91ad97-5246-4a3e-9f68-cfb755e08953",
  "refreshToken": "opaque-token"
}
```

**Respuesta:** 200 con tokens rotados.

El token anterior queda invalidado.

### `POST /auth/logout`

**Autenticación:** sesión activa.

**Respuesta:** 204.

### `POST /auth/change-password`

**Autenticación:** sesión activa.

**Petición:**

```json
{
  "currentPassword": "OldPassword1!",
  "newPassword": "NewPassword2!"
}
```

**Respuesta:** 204 y revocación inmediata.

### `GET /auth/me`

**Autenticación:** sesión activa.

**Respuesta:**

```json
{
  "id": "a1979dc3-60c0-43e3-920d-4df88a204568",
  "displayName": "Administrador",
  "userName": "admin",
  "role": {
    "id": "bc80b014-1a35-48be-905d-921f5328130c",
    "name": "Administrador"
  },
  "permissions": [],
  "session": {
    "id": "8c91ad97-5246-4a3e-9f68-cfb755e08953",
    "idleExpiresAtUtc": "2026-06-23T20:30:00Z"
  }
}
```

**Casos:** PLT-CU-002 a 007.

## 12. Sesiones

### `GET /platform/sessions`

**Permiso:** `Platform.ViewSessions`.

**Filtros:**

- `userId`.
- `status`.
- `startedFromUtc`.
- `startedToUtc`.

**Respuesta:** listado paginado.

### `POST /platform/sessions/{sessionId}/close`

**Permiso:** `Platform.CloseSessions`.

**Petición:**

```json
{
  "reason": "Cierre administrativo solicitado"
}
```

**Respuesta:** 204.

**Errores:**

- `SESSION.ALREADY_CLOSED` - 409.
- `SESSION.CLOSE_REASON_REQUIRED` - 422 cuando aplique.

**Caso:** PLT-CU-008.

La caducidad automática no expone endpoint público.

## 13. Usuarios

### Modelo `UserSummary`

```json
{
  "id": "a1979dc3-60c0-43e3-920d-4df88a204568",
  "fullName": "Usuario Ejemplo",
  "userName": "usuario",
  "phone": "+34910000000",
  "role": {
    "id": "bc80b014-1a35-48be-905d-921f5328130c",
    "name": "Facturación"
  },
  "status": "active",
  "lastSuccessfulLoginUtc": null,
  "blockedUntilUtc": null,
  "etag": "AAAAAAAAB9E="
}
```

### `GET /platform/users`

**Permiso:** `Platform.ManageUsers`.

**Filtros:**

- `query`.
- `roleId`.
- `status`.

### `GET /platform/users/{userId}`

**Permiso:** `Platform.ManageUsers`.

**Respuesta:** detalle y ETag.

### `POST /platform/users`

**Permiso:** `Platform.ManageUsers`.

**Idempotencia:** obligatoria.

**Petición:**

```json
{
  "fullName": "Usuario Ejemplo",
  "userName": "usuario",
  "phone": "+34910000000",
  "roleId": "bc80b014-1a35-48be-905d-921f5328130c",
  "password": "StrongPassword1!"
}
```

**Respuesta:** 201 y `Location`.

### `PATCH /platform/users/{userId}`

**Permiso:** `Platform.ManageUsers`.

**Concurrencia:** `If-Match` obligatorio.

**Petición:**

```json
{
  "fullName": "Nuevo Nombre",
  "phone": "+34910000001",
  "roleId": "b600d87b-c917-4f61-845e-f9910e02fdac"
}
```

Cambiar el rol revoca la sesión.

### `POST /platform/users/{userId}/deactivate`

**Permiso:** `Platform.ManageUsers`.

**Concurrencia:** `If-Match`.

```json
{
  "reason": "Baja laboral"
}
```

### `POST /platform/users/{userId}/reactivate`

**Permiso:** `Platform.ManageUsers`.

**Concurrencia:** `If-Match`.

```json
{
  "roleId": "b600d87b-c917-4f61-845e-f9910e02fdac"
}
```

### `POST /platform/users/{userId}/unlock`

**Permiso:** `Platform.ManageUsers`.

```json
{
  "reason": "Identidad verificada"
}
```

### `POST /platform/users/{userId}/reset-password`

**Permiso:** `Platform.ManageUsers`.

```json
{
  "newPassword": "NewPassword2!"
}
```

**Errores principales:**

- `SECURITY.USERNAME_ALREADY_RESERVED` - 409.
- `SECURITY.ROLE_INACTIVE` - 422.
- `SECURITY.PASSWORD_POLICY_FAILED` - 422.
- `SECURITY.USER_ALREADY_IN_STATE` - 409.

**Casos:** PLT-CU-009 a 012.

## 14. Roles y permisos

### `GET /platform/permissions`

**Permiso:** `Platform.ManageRoles`.

Devuelve el catálogo agrupado por módulo.

### `GET /platform/roles`

**Permiso:** `Platform.ManageRoles`.

Filtros:

- `status`.
- `type`.
- `query`.

### `GET /platform/roles/{roleId}`

**Permiso:** `Platform.ManageRoles`.

Devuelve:

- Datos.
- Permisos.
- Número de usuarios.
- ETag.

### `POST /platform/roles`

**Permiso:** `Platform.ManageRoles`.

**Idempotencia:** recomendada.

```json
{
  "name": "Soporte avanzado",
  "copyFromRoleId": null,
  "permissionCodes": [
    "Support.Read",
    "Support.Update"
  ]
}
```

### `PATCH /platform/roles/{roleId}`

**Permiso:** `Platform.ManageRoles`.

**Concurrencia:** `If-Match`.

```json
{
  "name": "Soporte especializado",
  "permissionCodes": [
    "Support.Read",
    "Support.Update",
    "Support.Assign"
  ]
}
```

### `POST /platform/roles/{roleId}/deactivate`

**Permiso:** `Platform.ManageRoles`.

**Concurrencia:** `If-Match`.

```json
{
  "reason": "Rol sustituido"
}
```

Si tiene usuarios, la respuesta previa de detalle permite conocerlos. La acción revoca sus sesiones.

### `POST /platform/roles/{roleId}/reactivate`

**Permiso:** `Platform.ManageRoles`.

**Concurrencia:** `If-Match`.

**Errores:**

- `ROLE.BASE_ROLE_PROTECTED` - 409.
- `ROLE.NAME_ALREADY_EXISTS` - 409.
- `ROLE.REQUIRES_PERMISSION` - 422.
- `ROLE.ADMIN_ONLY_PERMISSION` - 422.

**Casos:** PLT-CU-013 a 016.

## 15. Empresa

### `GET /platform/company`

**Permiso:** usuario autenticado para datos públicos; los campos sensibles se filtran según permiso.

**Respuesta:** detalle y ETag.

Los roles no autorizados no reciben:

- NIF completo.
- IBAN.
- Correo o teléfono cifrado cuando no les corresponda.

### `PATCH /platform/company`

**Permiso:** `Platform.ManageConfiguration`.

**Concurrencia:** `If-Match`.

```json
{
  "legalName": "Empresa Ejemplo SL",
  "tradeName": "Empresa Ejemplo",
  "taxId": "B12345678",
  "address": {
    "line": "Calle Mayor 1",
    "postalCode": "28001",
    "city": "Madrid",
    "region": "Madrid",
    "countryCode": "ES"
  },
  "phone": "+34910000000",
  "email": "administracion@example.com",
  "website": "https://example.com",
  "commercialRegistryText": "Registro Mercantil...",
  "bankAccount": {
    "alias": "Cuenta principal",
    "iban": "ES...",
    "bic": "..."
  }
}
```

Los campos omitidos no se modifican.

### `POST /platform/company/logo`

**Permiso:** `Platform.ManageConfiguration`.

**Contenido:** `multipart/form-data`.

Usa el flujo común de adjuntos con política `company-logo`.

**Respuesta:** 202 con recurso de adjunto.

### `POST /platform/company/logo/remove`

**Permiso:** `Platform.ManageConfiguration`.

**Concurrencia:** `If-Match` de empresa.

No existe una ruta `DELETE`; la acción retira la referencia del logotipo y aplica las reglas de conservación del adjunto.

**Casos:** PLT-CU-017 y PLT-CU-028 a 030.

## 16. Ejercicios

### `GET /platform/fiscal-years`

**Permiso:** `Platform.ManageConfiguration`.

### `GET /platform/fiscal-years/{fiscalYearId}`

**Permiso:** `Platform.ManageConfiguration`.

### `POST /platform/fiscal-years`

**Permiso:** `Platform.ManageConfiguration`.

**Idempotencia:** obligatoria.

```json
{
  "year": 2026,
  "startDate": "2026-01-01",
  "endDate": "2026-12-31"
}
```

**Respuesta:** 201.

La creación incluye contadores anuales.

**Errores:**

- `FISCAL_YEAR.ALREADY_EXISTS` - 409.
- `FISCAL_YEAR.OVERLAPS` - 409.
- `FISCAL_YEAR.INVALID_RANGE` - 422.

**Caso:** PLT-CU-018.

## 17. Numeraciones

### `GET /platform/number-counters`

**Permiso:** `Platform.ManageConfiguration`.

Filtros:

- `scope`.
- `fiscalYearId`.
- `counterCode`.

Respuesta:

```json
{
  "items": [
    {
      "id": "b4780076-b646-4a3a-9893-cd8165f16bba",
      "counterCode": "Invoice",
      "scope": "annual",
      "fiscalYear": 2026,
      "formatPattern": "F{YY}{00000}",
      "lastValue": 0,
      "nextValue": 1,
      "lastAssignedAtUtc": null
    }
  ]
}
```

No existe endpoint para modificar o reservar manualmente.

**Caso:** PLT-CU-020.

## 18. Fiscalidad

### `GET /platform/tax-rates`

**Permiso:** usuario autenticado; edición solo configuración.

Filtros:

- `status`.
- `validOn`.
- `code`.

### `POST /platform/tax-rates`

**Permiso:** `Platform.ManageConfiguration`.

```json
{
  "code": "IVA_GENERAL",
  "name": "IVA general",
  "percentage": 21.0000,
  "validFrom": "2026-01-01",
  "validTo": null
}
```

### `POST /platform/tax-rates/{taxRateId}/deactivate`

**Permiso:** `Platform.ManageConfiguration`.

**Concurrencia:** `If-Match`.

### `GET /platform/fiscal-configuration`

**Permiso:** usuario autenticado.

### `PATCH /platform/fiscal-configuration`

**Permiso:** `Platform.ManageConfiguration`.

**Concurrencia:** `If-Match`.

```json
{
  "withholdingPercentage": 15.0000
}
```

### `GET /platform/fiscal-reasons`

**Permiso:** usuario autenticado.

### `POST /platform/fiscal-reasons`

**Permiso:** `Platform.ManageConfiguration`.

```json
{
  "code": "EXPORT",
  "description": "Operación no sujeta por exportación",
  "type": "notSubject"
}
```

### `PATCH /platform/fiscal-reasons/{reasonId}`

**Permiso:** `Platform.ManageConfiguration`.

**Concurrencia:** `If-Match`.

Solo permite descripción y estado.

**Errores:**

- `TAX_RATE.OVERLAPPING_VALIDITY` - 409.
- `TAX_RATE.USED_CANNOT_CHANGE` - 409.
- `FISCAL_REASON.USED_CANNOT_DELETE` - 409.

**Caso:** PLT-CU-019.

## 19. SMTP

### `GET /platform/smtp`

**Permiso:** `Platform.ManageConfiguration`.

No devuelve contraseña ni secreto.

### `PUT /platform/smtp`

**Permiso:** `Platform.ManageConfiguration`.

**Concurrencia:** `If-Match` cuando ya existe.

```json
{
  "server": "smtp.example.com",
  "port": 587,
  "securityMode": "startTls",
  "userName": "mailer@example.com",
  "password": "secret",
  "senderAddress": "mailer@example.com",
  "senderDisplayName": "Empresa Ejemplo",
  "enabled": false
}
```

Si `password` es nulo o se omite, se conserva el secreto existente.

### `POST /platform/smtp/test`

**Permiso:** `Platform.ManageConfiguration`.

```json
{
  "destinationAddress": "admin@example.com",
  "draftConfiguration": {
    "server": "smtp.example.com",
    "port": 587,
    "securityMode": "startTls",
    "userName": "mailer@example.com",
    "password": "secret",
    "senderAddress": "mailer@example.com",
    "senderDisplayName": "Empresa Ejemplo"
  }
}
```

La configuración de prueba puede no estar guardada.

**Respuesta:** 200.

```json
{
  "success": true,
  "testedAtUtc": "2026-06-23T15:30:00Z",
  "resultCode": "SMTP.OK",
  "safeDiagnostic": "Conexión, autenticación y envío correctos."
}
```

### `POST /platform/smtp/disable`

**Permiso:** `Platform.ManageConfiguration`.

**Concurrencia:** `If-Match`.

**Caso:** PLT-CU-021.

## 20. Validación y versión de configuración

### `POST /platform/configuration/validate`

**Permiso:** `Platform.ManageConfiguration`.

**Respuesta:** 200.

```json
{
  "id": "0cdcf930-922b-4501-b9f8-6dabec6ff177",
  "executedAtUtc": "2026-06-23T15:30:00Z",
  "overallResult": "warning",
  "issues": [
    {
      "severity": "warning",
      "code": "SMTP.NOT_CONFIGURED",
      "description": "El correo no está configurado.",
      "module": "Platform",
      "resolutionTarget": "platform/settings/smtp"
    }
  ]
}
```

### `GET /platform/configuration/validation-runs`

**Permiso:** `Platform.ManageConfiguration`.

### `GET /platform/configuration/version`

**Permiso:** usuario autenticado.

```json
{
  "currentVersion": 12,
  "pendingVersion": 13,
  "restartRequired": true,
  "currentHash": "base64..."
}
```

### `POST /platform/configuration/apply-on-restart`

No reinicia remotamente la aplicación. Confirma que la versión pendiente está preparada para aplicarse en el siguiente arranque.

**Permiso:** `Platform.ManageConfiguration`.

**Respuesta:** 204.

La aplicación del servidor se realiza durante el arranque o despliegue, no por una petición mantenida abierta.

**Casos:** PLT-CU-022 y 023.

## 21. Auditoría

### `GET /platform/audit-events`

**Permiso:** `Platform.ViewAudit`.

Filtros:

- `occurredFromUtc`.
- `occurredToUtc`.
- `actorUserId`.
- `module`.
- `action`.
- `entityType`.
- `entityId`.
- `result`.
- `originIp`.
- `correlationId`.

Paginación por `AuditEventId`.

**Respuesta:**

```json
{
  "items": [
    {
      "id": 1001,
      "occurredAtUtc": "2026-06-23T15:30:00Z",
      "actor": {
        "type": "user",
        "userId": "a1979dc3-60c0-43e3-920d-4df88a204568",
        "displayName": "Administrador"
      },
      "module": "Platform",
      "action": "User.Created",
      "entity": {
        "type": "User",
        "id": "..."
      },
      "result": "success",
      "description": "Usuario creado",
      "correlationId": "665035d1-5382-4e6e-bdfa-7442b9ba394e"
    }
  ],
  "pageSize": 50,
  "nextContinuationToken": "..."
}
```

Los valores anteriores y nuevos podrán devolverse en detalle si el permiso y la política lo permiten.

### `POST /platform/audit-events/export`

**Permiso:** `Platform.ExportAudit`.

**Idempotencia:** obligatoria.

**Petición:** mismos filtros y formato `xlsx` o `csv`.

**Respuesta:** 202.

```json
{
  "operationId": "76d7d433-4867-4647-905c-ccae6663d7c7",
  "status": "accepted",
  "statusUrl": "/api/v1/platform/exports/76d7d433-4867-4647-905c-ccae6663d7c7"
}
```

### `GET /platform/exports/{operationId}`

**Permiso:** propietario de la exportación o Administrador.

Estados:

- `pending`.
- `processing`.
- `completed`.
- `failed`.
- `expired`.

### `GET /platform/exports/{operationId}/download`

**Permiso:** propietario o Administrador.

El archivo caduca a las 24 horas.

No existen endpoints de edición o eliminación de auditoría.

**Casos:** PLT-CU-024 y 025.

## 22. Notificaciones

### `GET /platform/notifications`

**Permiso:** sesión activa.

Siempre restringido al usuario actual.

Filtros:

- `status`.
- `severity`.
- `createdFromUtc`.
- `createdToUtc`.

### `GET /platform/notifications/unread-count`

**Permiso:** sesión activa.

```json
{
  "count": 4
}
```

### `POST /platform/notifications/{notificationId}/read`

**Permiso:** destinatario.

**Concurrencia:** `If-Match`.

### `POST /platform/notifications/{notificationId}/unread`

**Permiso:** destinatario.

**Concurrencia:** `If-Match`.

### `POST /platform/notifications/{notificationId}/archive`

**Permiso:** destinatario.

**Concurrencia:** `If-Match`.

### `POST /platform/notifications/batch/read`

**Permiso:** destinatario de todas.

```json
{
  "notificationIds": [
    "1e3772f9-a59a-4dba-b8e2-a579de8d0796"
  ]
}
```

### `POST /platform/notifications/batch/archive`

Mismo patrón.

No existe endpoint público para crear notificaciones críticas. Las crean los procesos internos mediante eventos.

**Casos:** PLT-CU-026 y 027.

## 23. Canal SignalR

Hub:

`/hubs/notifications`

Eventos servidor a cliente:

- `notificationCreated`.
- `notificationUpdated`.
- `sessionWarning`.
- `sessionRevoked`.
- `configurationRestartRequired`.

El payload contiene identificadores y datos mínimos. Desktop consulta el recurso completo por API.

SignalR no es fuente de verdad.

## 24. Adjuntos

### `POST /platform/attachments`

**Permiso:** `Platform.UseAttachments` más permiso contextual.

**Contenido:** `multipart/form-data`.

Campos:

- `ownerModule`.
- `ownerEntityType`.
- `ownerEntityId`.
- `policyCode`.
- `description`.
- `file`.

**Idempotencia:** obligatoria.

**Respuesta:** 202.

```json
{
  "id": "8ee6cebd-253c-44b2-8e2e-079399772e12",
  "status": "pendingValidation",
  "statusUrl": "/api/v1/platform/attachments/8ee6cebd-253c-44b2-8e2e-079399772e12"
}
```

### `GET /platform/attachments/{attachmentId}`

**Permiso:** permiso contextual sobre propietario.

Devuelve metadatos y ETag.

### `GET /platform/attachments`

Filtros obligatorios:

- `ownerModule`.
- `ownerEntityType`.
- `ownerEntityId`.

### `GET /platform/attachments/{attachmentId}/download`

**Permiso:** contextual.

Solo estado `available`.

Respuestas:

- 200 archivo.
- 409 si aún está procesándose.
- 422 si fue rechazado.
- 410 si fue eliminado físicamente.

### `POST /platform/attachments/{attachmentId}/replace`

**Contenido:** `multipart/form-data`.

**Concurrencia:** `If-Match`.

**Idempotencia:** obligatoria.

Respuesta 202 con el identificador del nuevo adjunto.

### Estados expuestos

- `pendingValidation`.
- `scanning`.
- `available`.
- `rejected`.
- `replaced`.
- `retentionBlocked`.
- `physicallyDeleted`.

### Errores

- `ATTACHMENT.POLICY_NOT_FOUND`.
- `ATTACHMENT.EXTENSION_NOT_ALLOWED`.
- `ATTACHMENT.SIZE_EXCEEDED`.
- `ATTACHMENT.TYPE_MISMATCH`.
- `ATTACHMENT.MALWARE_DETECTED`.
- `ATTACHMENT.SCAN_INCONCLUSIVE`.
- `ATTACHMENT.NOT_AVAILABLE`.

**Casos:** PLT-CU-028 a 030.

## 25. Diagnóstico

### `GET /platform/technical-incidents`

**Permiso:** `Platform.ViewDiagnostics`.

Filtros:

- `occurredFromUtc`.
- `occurredToUtc`.
- `severity`.
- `module`.
- `process`.
- `status`.
- `correlationId`.

### `GET /platform/technical-incidents/{incidentId}`

**Permiso:** `Platform.ViewDiagnostics`.

El detalle protegido solo se descifra para Administrador y nunca incluye secretos no filtrados.

### `POST /platform/technical-incidents/{incidentId}/review`

**Permiso:** `Platform.ViewDiagnostics`.

**Concurrencia:** `If-Match`.

```json
{
  "notes": "Revisado; problema externo resuelto."
}
```

### `POST /platform/technical-incidents/export`

**Permiso:** `Platform.ViewDiagnostics`.

**Respuesta:** 202 con recurso de exportación.

**Caso:** PLT-CU-031.

## 26. Copias de seguridad

### `GET /platform/backups`

**Permiso:** `Platform.ManageBackups`.

Filtros:

- `status`.
- `requestedFromUtc`.
- `requestedToUtc`.

### `GET /platform/backups/{backupId}`

**Permiso:** `Platform.ManageBackups`.

### `POST /platform/backups`

**Permiso:** `Platform.ManageBackups`.

**Idempotencia:** obligatoria.

```json
{
  "reason": "Copia manual previa a actualización"
}
```

**Respuesta:** 202.

```json
{
  "id": "d07f5c30-423c-424a-a0d0-76222a77066a",
  "status": "requested",
  "statusUrl": "/api/v1/platform/backups/d07f5c30-423c-424a-a0d0-76222a77066a"
}
```

### `POST /platform/backups/{backupId}/verify`

**Permiso:** `Platform.ManageBackups`.

Normalmente la verificación es automática. Este endpoint permite repetirla.

**Respuesta:** 202.

### `POST /platform/backups/{backupId}/restore`

**Permiso:** `Platform.RestoreBackups`.

**Idempotencia:** obligatoria.

```json
{
  "reason": "Restauración aprobada tras fallo de actualización",
  "confirmation": "RESTORE"
}
```

**Respuesta:** 202.

```json
{
  "restoreOperationId": "3aaf71ed-efb0-4c6a-8c8c-61b509110158",
  "status": "requested",
  "maintenanceWillStart": true,
  "statusUrl": "/api/v1/platform/restore-operations/3aaf71ed-efb0-4c6a-8c8c-61b509110158"
}
```

### `GET /platform/restore-operations/{restoreOperationId}`

**Permiso:** `Platform.RestoreBackups`.

Durante mantenimiento la API normal puede dejar de estar disponible. El estado definitivo deberá consultarse tras reinicio o mediante el canal de mantenimiento.

### Errores

- `BACKUP.OPERATION_IN_PROGRESS` - 423.
- `BACKUP.NOT_VERIFIED` - 409.
- `BACKUP.INCOMPATIBLE_VERSION` - 409.
- `BACKUP.INTEGRITY_FAILED` - 422.
- `BACKUP.REPOSITORY_UNAVAILABLE` - 503.
- `RESTORE.CONFIRMATION_INVALID` - 422.

**Casos:** PLT-CU-032 y 033.

## 27. Operaciones y salud

### `GET /platform/operations/{operationId}`

Recurso genérico para procesos largos:

```json
{
  "id": "76d7d433-4867-4647-905c-ccae6663d7c7",
  "type": "auditExport",
  "status": "processing",
  "progress": 45,
  "startedAtUtc": "2026-06-23T15:30:00Z",
  "completedAtUtc": null,
  "result": null,
  "error": null
}
```

Permiso:

- Solicitante.
- Administrador.
- Permiso específico del proceso.

### `GET /health/live`

Sin datos sensibles. Indica que el proceso vive.

### `GET /health/ready`

Acceso restringido o de infraestructura.

Comprueba dependencias esenciales.

## 28. Endpoints internos del Worker

El Worker no consumirá endpoints administrativos públicos para procesar:

- Outbox.
- Caducidad.
- Retenciones.
- Antivirus.
- Copias.

Compartirá la capa Application o utilizará contratos internos autenticados.

No se expondrán endpoints públicos como:

- Crear notificación arbitraria.
- Insertar auditoría arbitraria.
- Marcar Outbox procesada.
- Cambiar directamente estados de copias.

## 29. DTO comunes

### `EntityReference`

```json
{
  "module": "Billing",
  "type": "Invoice",
  "id": "..."
}
```

### `OperationResult`

```json
{
  "id": "...",
  "status": "completed",
  "resultCode": "OK",
  "message": "Operación completada."
}
```

### `LookupItem`

```json
{
  "id": "...",
  "code": "Administrator",
  "name": "Administrador",
  "active": true
}
```

## 30. Validaciones de entrada

### Texto

- Se recorta espacio exterior.
- Se rechazan valores obligatorios vacíos.
- Se aplican longitudes del modelo físico.
- Se rechazan caracteres de control.

### Identificadores

- UUID válido.
- No se aceptan identificadores vacíos.

### Fechas

- UTC para instantes.
- Fechas puras para ejercicios e impuestos.
- Rangos coherentes.

### Archivos

- La validación inicial no sustituye al análisis asíncrono.

## 31. Protección frente a enumeración

En autenticación:

- Mismo mensaje para usuario inexistente y contraseña incorrecta.
- Tiempos de respuesta aproximados.

En recursos protegidos:

- Se puede devolver 404 en lugar de 403 cuando revelar existencia sea sensible.

## 32. Rate limiting

Políticas iniciales:

- Login: por usuario normalizado y origen.
- Refresh: por sesión.
- Inicialización: muy restrictiva.
- Carga de archivos: por usuario y tamaño.
- Exportaciones: por usuario.
- Copias y restauraciones: exclusión global.

Una respuesta limitada devolverá 429 y `Retry-After`.

## 33. Compatibilidad de cliente

Cada petición Desktop envía:

`X-Client-Version`.

La API puede responder 426 si es incompatible:

```json
{
  "code": "PLATFORM.CLIENT_VERSION_UNSUPPORTED",
  "minimumVersion": "1.2.0",
  "downloadRequired": true
}
```

## 34. Trazabilidad con casos de uso

| Caso de uso | Endpoints |
|---|---|
| PLT-CU-001 | `GET /platform/installation`, `POST /platform/installation/initialize` |
| PLT-CU-002 | `POST /auth/login`, `GET /auth/me` |
| PLT-CU-003 | `POST /auth/login` |
| PLT-CU-004 | `POST /auth/logout` |
| PLT-CU-005 | `POST /auth/refresh`, SignalR `sessionWarning` |
| PLT-CU-006 | `POST /auth/change-password` |
| PLT-CU-007 | `POST /platform/users/{id}/reset-password` |
| PLT-CU-008 | `GET /platform/sessions`, `POST /platform/sessions/{id}/close` |
| PLT-CU-009 | `POST /platform/users` |
| PLT-CU-010 | `PATCH /platform/users/{id}` |
| PLT-CU-011 | Acciones `deactivate` y `reactivate` |
| PLT-CU-012 | Acción `unlock` |
| PLT-CU-013 | `GET /platform/roles`, `GET /platform/permissions` |
| PLT-CU-014 | `POST /platform/roles` |
| PLT-CU-015 | `PATCH /platform/roles/{id}` |
| PLT-CU-016 | Acciones de estado de rol |
| PLT-CU-017 | `GET/PATCH /platform/company`, logo |
| PLT-CU-018 | `POST /platform/fiscal-years` |
| PLT-CU-019 | Recursos fiscales |
| PLT-CU-020 | `GET /platform/number-counters` |
| PLT-CU-021 | Recursos SMTP |
| PLT-CU-022 | `POST /platform/configuration/validate` |
| PLT-CU-023 | Recursos de versión de configuración |
| PLT-CU-024 | `GET /platform/audit-events` |
| PLT-CU-025 | Exportación de auditoría |
| PLT-CU-026 | Recursos de notificación |
| PLT-CU-027 | Sin endpoint público; eventos internos y SignalR |
| PLT-CU-028 | `POST /platform/attachments` |
| PLT-CU-029 | Descarga de adjunto |
| PLT-CU-030 | Reemplazo de adjunto |
| PLT-CU-031 | Recursos de incidencias técnicas |
| PLT-CU-032 | Recursos de copia |
| PLT-CU-033 | Recursos de restauración |

## 35. Criterios de aceptación del contrato

1. Los 33 casos de uso tienen contrato o mecanismo interno.
2. Ningún endpoint expone entidades EF Core.
3. Las modificaciones editables exigen ETag.
4. Los comandos críticos admiten idempotencia.
5. Los procesos largos responden 202.
6. Las contraseñas y secretos nunca se devuelven.
7. Los datos sensibles se filtran por permiso.
8. Los errores usan códigos estables y Correlation ID.
9. No existen endpoints para borrar usuarios, auditoría o documentos conservables.
10. Los adjuntos se descargan solo tras estar disponibles.
11. Las notificaciones solo son visibles por su destinatario.
12. La restauración exige confirmación, motivo y permiso específico.
13. Los endpoints administrativos están versionados.
14. La compatibilidad del cliente se valida.
15. OpenAPI documenta permisos, errores y ejemplos.

## 36. Pendientes para implementación

- Definir todos los DTO como contratos C# versionados.
- Completar catálogo de permisos de módulos futuros.
- Determinar duración exacta de refresh token.
- Elegir librería de versionado de API.
- Definir almacenamiento de idempotencia HTTP.
- Concretar endpoint de mantenimiento para restauración.
- Definir formatos de exportación definitivos.
- Establecer límites de rate limiting.
- Diseñar autorización polimórfica de adjuntos.
- Diseñar eventos SignalR y reconexión Desktop.

La experiencia de escritorio que consume estos contratos se define en [Diseño de pantallas de Plataforma](07-diseno-pantallas.md).

Las pruebas de contrato, autorización, concurrencia e idempotencia se definen en [Plan de pruebas de Plataforma](08-plan-de-pruebas.md).
