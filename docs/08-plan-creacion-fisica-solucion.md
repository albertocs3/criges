# Plan de creación física de la solución y proyectos

## 1. Propósito

Este documento define el plan operativo para crear físicamente la solución .NET de CriGes.

Convierte la arquitectura, la estructura de proyectos y el backlog de la primera rebanada vertical en una secuencia ejecutable de:

- Carpetas.
- Archivos raíz.
- Proyectos .NET.
- Referencias.
- Paquetes iniciales.
- Configuración común.
- Validaciones.

El objetivo no es implementar todavía toda la lógica de `PLT-CU-001`, sino dejar una solución real, compilable y preparada para empezar la primera rebanada vertical con límites correctos.

## 2. Referencias

- [Arquitectura técnica general](05-arquitectura-tecnica.md).
- [Estructura inicial de la solución .NET](06-estructura-solucion-dotnet.md).
- [Backlog técnico de la primera rebanada vertical](07-backlog-tecnico-primera-rebanada.md).
- [Registro de decisiones arquitectónicas](adr/README.md).
- [Contratos HTTP de Plataforma](plataforma/06-contratos-api.md).
- [Plan de pruebas de Plataforma](plataforma/08-plan-de-pruebas.md).

## 3. Alcance

### Incluido

- Crear la solución `CriGes.sln`.
- Crear los proyectos productivos iniciales.
- Crear los proyectos de prueba iniciales.
- Configurar referencias entre proyectos.
- Crear archivos raíz de compilación y estilo.
- Preparar carpetas `deploy` y `scripts`.
- Dejar API, Worker, Desktop y DbMigrator con arranque mínimo.
- Dejar una primera prueba de arquitectura.
- Verificar compilación y ejecución básica.

### Excluido

- Crear migraciones EF reales.
- Implementar el modelo de dominio.
- Implementar endpoints funcionales.
- Implementar UI del asistente.
- Configurar SQL Server real.
- Añadir integración con SMTP, antivirus, SignalR o copias.
- Crear instaladores.

Esas tareas pertenecen a las épicas posteriores del backlog técnico.

## 4. Precondiciones

Antes de ejecutar este plan debe existir:

1. SDK .NET 8 instalado.
2. PowerShell disponible.
3. Repositorio o carpeta de trabajo en `F:\CriGes`.
4. Acceso a NuGet.org o caché local de paquetes.
5. Windows para crear y compilar el proyecto WPF.

Comprobaciones recomendadas:

```powershell
dotnet --info
dotnet --list-sdks
```

La versión exacta del SDK se fijará en `global.json`.

## 5. Principios de ejecución

1. Crear primero estructura y referencias; implementar lógica después.
2. Compilar tras cada bloque de proyectos.
3. No añadir paquetes hasta que exista una responsabilidad clara.
4. Mantener `Domain` sin dependencias tecnológicas.
5. Mantener `Desktop` sin referencias a `Application`, `Infrastructure` o `Domain`.
6. Mantener hosts sin lógica de negocio.
7. Preferir comandos repetibles a pasos manuales opacos.
8. No versionar secretos ni cadenas de conexión reales.

## 6. Resultado esperado

Al completar el plan existirá:

```text
CriGes.sln
├── src/
│   ├── Apps/
│   │   ├── CriGes.Api/
│   │   ├── CriGes.Worker/
│   │   └── CriGes.Desktop/
│   ├── Tools/
│   │   └── CriGes.DbMigrator/
│   ├── BuildingBlocks/
│   │   ├── CriGes.SharedKernel/
│   │   ├── CriGes.Application.Abstractions/
│   │   ├── CriGes.Infrastructure/
│   │   └── CriGes.Contracts/
│   └── Modules/
│       └── Platform/
│           ├── CriGes.Modules.Platform.Domain/
│           ├── CriGes.Modules.Platform.Application/
│           ├── CriGes.Modules.Platform.Infrastructure/
│           ├── CriGes.Modules.Platform.Contracts/
│           └── CriGes.Modules.Platform.Api/
├── tests/
│   ├── Architecture/
│   │   └── CriGes.ArchitectureTests/
│   ├── BuildingBlocks/
│   │   └── CriGes.SharedKernel.Tests/
│   ├── Platform/
│   │   ├── CriGes.Modules.Platform.Domain.Tests/
│   │   ├── CriGes.Modules.Platform.Application.Tests/
│   │   ├── CriGes.Modules.Platform.IntegrationTests/
│   │   └── CriGes.Modules.Platform.ContractTests/
│   └── EndToEnd/
│       └── CriGes.Desktop.EndToEndTests/
├── deploy/
├── scripts/
└── docs/
```

## 7. Fase 1: archivos raíz

### 7.1 Crear solución

```powershell
dotnet new sln -n CriGes
```

### 7.2 Crear `global.json`

Objetivo:

- Fijar .NET 8.
- Evitar saltos accidentales a otra familia de SDK.

Contenido inicial recomendado:

```json
{
  "sdk": {
    "version": "8.0.000",
    "rollForward": "latestFeature"
  }
}
```

La versión concreta debe ajustarse al SDK instalado en el equipo.

### 7.3 Crear `Directory.Build.props`

Contenido base:

```xml
<Project>
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
    <EnforceCodeStyleInBuild>true</EnforceCodeStyleInBuild>
    <Deterministic>true</Deterministic>
    <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
    <AnalysisLevel>latest-recommended</AnalysisLevel>
  </PropertyGroup>
</Project>
```

Excepción:

- `CriGes.Desktop` sobrescribirá `TargetFramework` a `net8.0-windows` y activará WPF.

### 7.4 Crear `Directory.Packages.props`

Inicialmente debe contener solo paquetes necesarios para compilar y probar.

Familias previstas:

- `Microsoft.EntityFrameworkCore`.
- `Microsoft.EntityFrameworkCore.SqlServer`.
- `Microsoft.EntityFrameworkCore.Design`.
- `Microsoft.Extensions.Hosting`.
- `Microsoft.AspNetCore.OpenApi`.
- `Swashbuckle.AspNetCore` si se decide usar Swagger UI.
- `xunit`.
- `xunit.runner.visualstudio`.
- `Microsoft.NET.Test.Sdk`.
- `FluentAssertions`.
- `NetArchTest.Rules` o alternativa para arquitectura.
- `Microsoft.AspNetCore.Mvc.Testing`.

Las versiones se fijarán al ejecutar la creación real, preferiblemente en bloque.

### 7.5 Crear `NuGet.config`

Objetivo:

- Declarar fuentes aprobadas.
- No incluir credenciales.

Contenido inicial:

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <clear />
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
  </packageSources>
</configuration>
```

### 7.6 Crear `.editorconfig`

Debe fijar:

- UTF-8.
- Final de línea.
- Sangría.
- Convenciones C#.
- Severidades básicas.

### 7.7 Crear `.gitignore`

Debe excluir:

- `bin/`.
- `obj/`.
- `.vs/`.
- `TestResults/`.
- secretos locales.
- bases de datos locales.
- copias, adjuntos y temporales.

### 7.8 Validación

```powershell
dotnet --info
dotnet new sln --help
```

**Backlog cubierto:** `PVS-001`.

## 8. Fase 2: Building Blocks

### 8.1 Crear proyectos

```powershell
dotnet new classlib -n CriGes.SharedKernel -o src/BuildingBlocks/CriGes.SharedKernel
dotnet new classlib -n CriGes.Application.Abstractions -o src/BuildingBlocks/CriGes.Application.Abstractions
dotnet new classlib -n CriGes.Infrastructure -o src/BuildingBlocks/CriGes.Infrastructure
dotnet new classlib -n CriGes.Contracts -o src/BuildingBlocks/CriGes.Contracts
```

### 8.2 Añadir a solución

```powershell
dotnet sln CriGes.sln add src/BuildingBlocks/CriGes.SharedKernel/CriGes.SharedKernel.csproj
dotnet sln CriGes.sln add src/BuildingBlocks/CriGes.Application.Abstractions/CriGes.Application.Abstractions.csproj
dotnet sln CriGes.sln add src/BuildingBlocks/CriGes.Infrastructure/CriGes.Infrastructure.csproj
dotnet sln CriGes.sln add src/BuildingBlocks/CriGes.Contracts/CriGes.Contracts.csproj
```

### 8.3 Referencias

```powershell
dotnet add src/BuildingBlocks/CriGes.Application.Abstractions/CriGes.Application.Abstractions.csproj reference src/BuildingBlocks/CriGes.SharedKernel/CriGes.SharedKernel.csproj
dotnet add src/BuildingBlocks/CriGes.Infrastructure/CriGes.Infrastructure.csproj reference src/BuildingBlocks/CriGes.Application.Abstractions/CriGes.Application.Abstractions.csproj
dotnet add src/BuildingBlocks/CriGes.Infrastructure/CriGes.Infrastructure.csproj reference src/BuildingBlocks/CriGes.SharedKernel/CriGes.SharedKernel.csproj
```

`CriGes.Contracts` no debe referenciar capas internas.

### 8.4 Carpetas iniciales

```text
CriGes.SharedKernel/
├── Domain/
├── Results/
└── Time/

CriGes.Application.Abstractions/
├── Authorization/
├── Behaviors/
├── Events/
├── Idempotency/
├── Messaging/
├── Time/
└── Transactions/

CriGes.Infrastructure/
├── Clock/
├── Encryption/
├── Files/
├── Messaging/
├── Observability/
├── Persistence/
└── Security/

CriGes.Contracts/
├── Api/
├── Operations/
└── Versioning/
```

### 8.5 Validación

```powershell
dotnet build CriGes.sln
```

**Backlog cubierto:** `PVS-002`.

## 9. Fase 3: módulo Plataforma

### 9.1 Crear proyectos

```powershell
dotnet new classlib -n CriGes.Modules.Platform.Domain -o src/Modules/Platform/CriGes.Modules.Platform.Domain
dotnet new classlib -n CriGes.Modules.Platform.Application -o src/Modules/Platform/CriGes.Modules.Platform.Application
dotnet new classlib -n CriGes.Modules.Platform.Infrastructure -o src/Modules/Platform/CriGes.Modules.Platform.Infrastructure
dotnet new classlib -n CriGes.Modules.Platform.Contracts -o src/Modules/Platform/CriGes.Modules.Platform.Contracts
dotnet new classlib -n CriGes.Modules.Platform.Api -o src/Modules/Platform/CriGes.Modules.Platform.Api
```

### 9.2 Añadir a solución

```powershell
dotnet sln CriGes.sln add src/Modules/Platform/CriGes.Modules.Platform.Domain/CriGes.Modules.Platform.Domain.csproj
dotnet sln CriGes.sln add src/Modules/Platform/CriGes.Modules.Platform.Application/CriGes.Modules.Platform.Application.csproj
dotnet sln CriGes.sln add src/Modules/Platform/CriGes.Modules.Platform.Infrastructure/CriGes.Modules.Platform.Infrastructure.csproj
dotnet sln CriGes.sln add src/Modules/Platform/CriGes.Modules.Platform.Contracts/CriGes.Modules.Platform.Contracts.csproj
dotnet sln CriGes.sln add src/Modules/Platform/CriGes.Modules.Platform.Api/CriGes.Modules.Platform.Api.csproj
```

### 9.3 Referencias permitidas

```powershell
dotnet add src/Modules/Platform/CriGes.Modules.Platform.Domain/CriGes.Modules.Platform.Domain.csproj reference src/BuildingBlocks/CriGes.SharedKernel/CriGes.SharedKernel.csproj

dotnet add src/Modules/Platform/CriGes.Modules.Platform.Application/CriGes.Modules.Platform.Application.csproj reference src/Modules/Platform/CriGes.Modules.Platform.Domain/CriGes.Modules.Platform.Domain.csproj
dotnet add src/Modules/Platform/CriGes.Modules.Platform.Application/CriGes.Modules.Platform.Application.csproj reference src/Modules/Platform/CriGes.Modules.Platform.Contracts/CriGes.Modules.Platform.Contracts.csproj
dotnet add src/Modules/Platform/CriGes.Modules.Platform.Application/CriGes.Modules.Platform.Application.csproj reference src/BuildingBlocks/CriGes.Application.Abstractions/CriGes.Application.Abstractions.csproj
dotnet add src/Modules/Platform/CriGes.Modules.Platform.Application/CriGes.Modules.Platform.Application.csproj reference src/BuildingBlocks/CriGes.SharedKernel/CriGes.SharedKernel.csproj

dotnet add src/Modules/Platform/CriGes.Modules.Platform.Infrastructure/CriGes.Modules.Platform.Infrastructure.csproj reference src/Modules/Platform/CriGes.Modules.Platform.Application/CriGes.Modules.Platform.Application.csproj
dotnet add src/Modules/Platform/CriGes.Modules.Platform.Infrastructure/CriGes.Modules.Platform.Infrastructure.csproj reference src/Modules/Platform/CriGes.Modules.Platform.Domain/CriGes.Modules.Platform.Domain.csproj
dotnet add src/Modules/Platform/CriGes.Modules.Platform.Infrastructure/CriGes.Modules.Platform.Infrastructure.csproj reference src/Modules/Platform/CriGes.Modules.Platform.Contracts/CriGes.Modules.Platform.Contracts.csproj
dotnet add src/Modules/Platform/CriGes.Modules.Platform.Infrastructure/CriGes.Modules.Platform.Infrastructure.csproj reference src/BuildingBlocks/CriGes.Infrastructure/CriGes.Infrastructure.csproj

dotnet add src/Modules/Platform/CriGes.Modules.Platform.Api/CriGes.Modules.Platform.Api.csproj reference src/Modules/Platform/CriGes.Modules.Platform.Application/CriGes.Modules.Platform.Application.csproj
dotnet add src/Modules/Platform/CriGes.Modules.Platform.Api/CriGes.Modules.Platform.Api.csproj reference src/Modules/Platform/CriGes.Modules.Platform.Contracts/CriGes.Modules.Platform.Contracts.csproj
dotnet add src/Modules/Platform/CriGes.Modules.Platform.Api/CriGes.Modules.Platform.Api.csproj reference src/BuildingBlocks/CriGes.Contracts/CriGes.Contracts.csproj
```

### 9.4 Carpetas iniciales

```text
CriGes.Modules.Platform.Domain/
├── Installation/
├── Users/
├── Roles/
├── Company/
├── Configuration/
└── Auditing/

CriGes.Modules.Platform.Application/
├── Abstractions/
│   ├── Persistence/
│   ├── Security/
│   └── Auditing/
├── Installation/
│   ├── GetInstallationStatus/
│   └── InitializePlatform/
└── DependencyInjection.cs

CriGes.Modules.Platform.Infrastructure/
├── Persistence/
│   ├── Configurations/
│   ├── Migrations/
│   ├── Repositories/
│   └── PlatformDbContext.cs
├── Security/
├── Auditing/
├── Idempotency/
└── DependencyInjection.cs

CriGes.Modules.Platform.Contracts/
├── Installation/
└── Events/

CriGes.Modules.Platform.Api/
├── Endpoints/
│   └── Installation/
├── OpenApi/
├── PlatformEndpoints.cs
└── PlatformModule.cs
```

### 9.5 Validación

```powershell
dotnet build CriGes.sln
```

**Backlog cubierto:** `PVS-003`, preparación para `PVS-026` a `PVS-053`.

## 10. Fase 4: hosts desplegables y herramienta de migración

### 10.1 Crear API

```powershell
dotnet new webapi -n CriGes.Api -o src/Apps/CriGes.Api
dotnet sln CriGes.sln add src/Apps/CriGes.Api/CriGes.Api.csproj
```

Referencias:

```powershell
dotnet add src/Apps/CriGes.Api/CriGes.Api.csproj reference src/Modules/Platform/CriGes.Modules.Platform.Api/CriGes.Modules.Platform.Api.csproj
dotnet add src/Apps/CriGes.Api/CriGes.Api.csproj reference src/Modules/Platform/CriGes.Modules.Platform.Infrastructure/CriGes.Modules.Platform.Infrastructure.csproj
dotnet add src/Apps/CriGes.Api/CriGes.Api.csproj reference src/BuildingBlocks/CriGes.Infrastructure/CriGes.Infrastructure.csproj
```

Estado mínimo:

- `GET /health/live`.
- Registro vacío de Plataforma:
  - `services.AddPlatformApplication()`.
  - `services.AddPlatformInfrastructure(configuration)`.
  - `services.AddPlatformApi()`.
  - `app.MapPlatformEndpoints()`.

### 10.2 Crear Worker

```powershell
dotnet new worker -n CriGes.Worker -o src/Apps/CriGes.Worker
dotnet sln CriGes.sln add src/Apps/CriGes.Worker/CriGes.Worker.csproj
```

Referencias:

```powershell
dotnet add src/Apps/CriGes.Worker/CriGes.Worker.csproj reference src/Modules/Platform/CriGes.Modules.Platform.Infrastructure/CriGes.Modules.Platform.Infrastructure.csproj
dotnet add src/Apps/CriGes.Worker/CriGes.Worker.csproj reference src/BuildingBlocks/CriGes.Infrastructure/CriGes.Infrastructure.csproj
```

Estado mínimo:

- Arranque y parada limpios.
- Sin trabajos funcionales todavía.

### 10.3 Crear Desktop WPF

```powershell
dotnet new wpf -n CriGes.Desktop -o src/Apps/CriGes.Desktop
dotnet sln CriGes.sln add src/Apps/CriGes.Desktop/CriGes.Desktop.csproj
```

Referencias:

```powershell
dotnet add src/Apps/CriGes.Desktop/CriGes.Desktop.csproj reference src/Modules/Platform/CriGes.Modules.Platform.Contracts/CriGes.Modules.Platform.Contracts.csproj
dotnet add src/Apps/CriGes.Desktop/CriGes.Desktop.csproj reference src/BuildingBlocks/CriGes.Contracts/CriGes.Contracts.csproj
```

Estado mínimo:

- Ventana principal.
- Pantalla de conexión vacía o placeholder.
- Sin referencia a `Platform.Application`, `Platform.Infrastructure` ni `Platform.Domain`.

### 10.4 Crear DbMigrator

```powershell
dotnet new console -n CriGes.DbMigrator -o src/Tools/CriGes.DbMigrator
dotnet sln CriGes.sln add src/Tools/CriGes.DbMigrator/CriGes.DbMigrator.csproj
```

Referencias:

```powershell
dotnet add src/Tools/CriGes.DbMigrator/CriGes.DbMigrator.csproj reference src/Modules/Platform/CriGes.Modules.Platform.Infrastructure/CriGes.Modules.Platform.Infrastructure.csproj
```

Estado mínimo:

- Lee configuración.
- Muestra versión.
- Devuelve código de salida `0` si no hay trabajo.

### 10.5 Validación

```powershell
dotnet build CriGes.sln
dotnet run --project src/Apps/CriGes.Api/CriGes.Api.csproj
dotnet run --project src/Apps/CriGes.Worker/CriGes.Worker.csproj
dotnet run --project src/Tools/CriGes.DbMigrator/CriGes.DbMigrator.csproj
```

**Backlog cubierto:** `PVS-004`.

## 11. Fase 5: proyectos de prueba

### 11.1 Crear proyectos

```powershell
dotnet new xunit -n CriGes.ArchitectureTests -o tests/Architecture/CriGes.ArchitectureTests
dotnet new xunit -n CriGes.SharedKernel.Tests -o tests/BuildingBlocks/CriGes.SharedKernel.Tests
dotnet new xunit -n CriGes.Modules.Platform.Domain.Tests -o tests/Platform/CriGes.Modules.Platform.Domain.Tests
dotnet new xunit -n CriGes.Modules.Platform.Application.Tests -o tests/Platform/CriGes.Modules.Platform.Application.Tests
dotnet new xunit -n CriGes.Modules.Platform.IntegrationTests -o tests/Platform/CriGes.Modules.Platform.IntegrationTests
dotnet new xunit -n CriGes.Modules.Platform.ContractTests -o tests/Platform/CriGes.Modules.Platform.ContractTests
dotnet new xunit -n CriGes.Desktop.EndToEndTests -o tests/EndToEnd/CriGes.Desktop.EndToEndTests
```

### 11.2 Añadir a solución

```powershell
dotnet sln CriGes.sln add tests/Architecture/CriGes.ArchitectureTests/CriGes.ArchitectureTests.csproj
dotnet sln CriGes.sln add tests/BuildingBlocks/CriGes.SharedKernel.Tests/CriGes.SharedKernel.Tests.csproj
dotnet sln CriGes.sln add tests/Platform/CriGes.Modules.Platform.Domain.Tests/CriGes.Modules.Platform.Domain.Tests.csproj
dotnet sln CriGes.sln add tests/Platform/CriGes.Modules.Platform.Application.Tests/CriGes.Modules.Platform.Application.Tests.csproj
dotnet sln CriGes.sln add tests/Platform/CriGes.Modules.Platform.IntegrationTests/CriGes.Modules.Platform.IntegrationTests.csproj
dotnet sln CriGes.sln add tests/Platform/CriGes.Modules.Platform.ContractTests/CriGes.Modules.Platform.ContractTests.csproj
dotnet sln CriGes.sln add tests/EndToEnd/CriGes.Desktop.EndToEndTests/CriGes.Desktop.EndToEndTests.csproj
```

### 11.3 Referencias de prueba

```powershell
dotnet add tests/BuildingBlocks/CriGes.SharedKernel.Tests/CriGes.SharedKernel.Tests.csproj reference src/BuildingBlocks/CriGes.SharedKernel/CriGes.SharedKernel.csproj

dotnet add tests/Platform/CriGes.Modules.Platform.Domain.Tests/CriGes.Modules.Platform.Domain.Tests.csproj reference src/Modules/Platform/CriGes.Modules.Platform.Domain/CriGes.Modules.Platform.Domain.csproj

dotnet add tests/Platform/CriGes.Modules.Platform.Application.Tests/CriGes.Modules.Platform.Application.Tests.csproj reference src/Modules/Platform/CriGes.Modules.Platform.Application/CriGes.Modules.Platform.Application.csproj

dotnet add tests/Platform/CriGes.Modules.Platform.IntegrationTests/CriGes.Modules.Platform.IntegrationTests.csproj reference src/Modules/Platform/CriGes.Modules.Platform.Infrastructure/CriGes.Modules.Platform.Infrastructure.csproj
dotnet add tests/Platform/CriGes.Modules.Platform.IntegrationTests/CriGes.Modules.Platform.IntegrationTests.csproj reference src/Modules/Platform/CriGes.Modules.Platform.Application/CriGes.Modules.Platform.Application.csproj

dotnet add tests/Platform/CriGes.Modules.Platform.ContractTests/CriGes.Modules.Platform.ContractTests.csproj reference src/Apps/CriGes.Api/CriGes.Api.csproj
dotnet add tests/Platform/CriGes.Modules.Platform.ContractTests/CriGes.Modules.Platform.ContractTests.csproj reference src/Modules/Platform/CriGes.Modules.Platform.Contracts/CriGes.Modules.Platform.Contracts.csproj
```

`CriGes.Desktop.EndToEndTests` no debe referenciar capas internas salvo que el framework E2E lo requiera. Preferiblemente automatizará la aplicación como proceso externo.

### 11.4 Primera prueba de arquitectura

La primera prueba debe fallar si:

- `Platform.Domain` referencia EF Core.
- `Platform.Application` referencia `Platform.Infrastructure`.
- `Platform.Contracts` referencia `Platform.Domain`.
- `Desktop` referencia `Platform.Application`, `Platform.Infrastructure` o `Platform.Domain`.
- `Platform.Api` referencia `Platform.Infrastructure`.

### 11.5 Validación

```powershell
dotnet test CriGes.sln
```

**Backlog cubierto:** `PVS-005`, `PVS-006`.

## 12. Fase 6: paquetes iniciales

### 12.1 Paquetes productivos mínimos

| Proyecto | Paquetes previstos | Motivo |
|---|---|---|
| `CriGes.Api` | OpenAPI/Swagger si se decide | Documentar contrato HTTP |
| `CriGes.Infrastructure` | `Microsoft.Extensions.*` según necesidad | Infraestructura transversal |
| `Platform.Infrastructure` | EF Core SQL Server, EF Core Design | Persistencia y migraciones |
| `Platform.Api` | ASP.NET Core abstractions si hace falta | Minimal APIs del módulo |
| `CriGes.Desktop` | MVVM Toolkit si se decide | MVVM y comandos |

No añadir en esta fase:

- MailKit.
- SignalR Client.
- OpenTelemetry.
- Serilog.
- Librería de antivirus.
- Librería de instalador.

Esas dependencias entrarán cuando su épica las necesite.

### 12.2 Paquetes de prueba mínimos

| Proyecto | Paquetes previstos |
|---|---|
| Todos los test | `xunit`, `xunit.runner.visualstudio`, `Microsoft.NET.Test.Sdk`, `FluentAssertions` |
| ArchitectureTests | `NetArchTest.Rules` o alternativa |
| ContractTests | `Microsoft.AspNetCore.Mvc.Testing` |
| IntegrationTests | EF Core SQL Server, herramientas de fixture |

### 12.3 Regla de centralización

Todas las versiones se declaran en:

```text
Directory.Packages.props
```

Los `.csproj` solo declaran:

```xml
<PackageReference Include="Nombre.Paquete" />
```

## 13. Fase 7: configuración mínima de hosts

### 13.1 API

Debe quedar con:

- `Program.cs` mínimo.
- `appsettings.json`.
- `appsettings.Development.json`.
- endpoint `/health/live`.
- endpoint `/openapi/v1.json` cuando OpenAPI esté configurado.
- registro de `CorrelationIdMiddleware`, aunque sea mínimo.

### 13.2 Worker

Debe quedar con:

- `Program.cs`.
- logging básico.
- `Worker` o `HostedService` placeholder.
- configuración validable.

### 13.3 Desktop

Debe quedar con:

- `App.xaml`.
- `MainWindow.xaml`.
- carpeta `Bootstrap`.
- carpeta `Services/Api`.
- pantalla de conexión placeholder.

### 13.4 DbMigrator

Debe quedar con:

- `Program.cs`.
- lectura de `appsettings.json`.
- estructura preparada para registrar módulos de migración.
- salida clara por consola.

## 14. Fase 8: scripts iniciales

Crear carpeta:

```text
scripts/
```

Scripts recomendados:

```text
scripts/
├── build.ps1
├── test.ps1
├── run-api.ps1
├── run-worker.ps1
├── run-migrator.ps1
└── format.ps1
```

Reglas:

1. No incluir secretos.
2. No usar rutas personales fijas.
3. Fallar con código distinto de cero ante error.
4. Imprimir comandos principales antes de ejecutarlos.
5. Aceptar parámetros para configuración local.

Ejemplo de intención:

```powershell
dotnet build CriGes.sln
dotnet test CriGes.sln
```

La implementación concreta de scripts puede hacerse justo después de crear la solución.

## 15. Fase 9: carpetas de despliegue

Crear:

```text
deploy/
├── README.md
├── windows/
├── sql/
└── configuration/
```

Contenido inicial:

- `deploy/README.md` explicando que no se guardan certificados, contraseñas ni copias reales.
- `.gitkeep` opcional en carpetas vacías si se desea conservar estructura.

## 16. Validaciones globales

### 16.1 Restauración

```powershell
dotnet restore CriGes.sln
```

### 16.2 Compilación

```powershell
dotnet build CriGes.sln
```

### 16.3 Pruebas

```powershell
dotnet test CriGes.sln
```

### 16.4 Arranque de hosts

```powershell
dotnet run --project src/Apps/CriGes.Api/CriGes.Api.csproj
dotnet run --project src/Apps/CriGes.Worker/CriGes.Worker.csproj
dotnet run --project src/Tools/CriGes.DbMigrator/CriGes.DbMigrator.csproj
```

Desktop:

```powershell
dotnet build src/Apps/CriGes.Desktop/CriGes.Desktop.csproj
```

El arranque visual de WPF se comprobará manualmente al crear el placeholder.

## 17. Orden operativo recomendado

1. Crear archivos raíz.
2. Crear Building Blocks.
3. Crear módulo Plataforma.
4. Crear API, Worker, Desktop y DbMigrator.
5. Crear proyectos de prueba.
6. Añadir referencias.
7. Añadir paquetes mínimos.
8. Crear carpetas internas.
9. Crear arranques mínimos.
10. Crear primera prueba de arquitectura.
11. Ejecutar `dotnet restore`.
12. Ejecutar `dotnet build`.
13. Ejecutar `dotnet test`.
14. Arrancar API.
15. Arrancar Worker.
16. Arrancar DbMigrator.
17. Confirmar Desktop compilable.

## 18. Relación con el backlog

| Backlog | Estado esperado al completar este plan |
|---|---|
| PVS-001 | Completado |
| PVS-002 | Completado |
| PVS-003 | Completado |
| PVS-004 | Completado con arranque mínimo |
| PVS-005 | Completado con primera prueba de arquitectura |
| PVS-006 | Completado con proyectos de prueba creados |
| PVS-007 a PVS-014 | Preparados, no implementados |
| PVS-015 a PVS-025 | Preparados, no implementados |
| PVS-026 a PVS-075 | Preparados, no implementados |

## 19. Riesgos de ejecución

| Riesgo | Señal | Respuesta |
|---|---|---|
| SDK .NET no coincide con `global.json` | `dotnet` no puede resolver SDK | Ajustar `global.json` al SDK 8 instalado |
| WPF no compila | Error de target Windows | Revisar `TargetFramework=net8.0-windows` y `UseWPF=true` |
| Paquetes no restauran | Error de red o fuente NuGet | Verificar `NuGet.config` y conectividad |
| Warnings como errores bloquean plantillas generadas | `TreatWarningsAsErrors` falla al crear plantilla | Corregir código generado o documentar excepción mínima |
| Referencias circulares | `dotnet build` falla | Revisar matriz de dependencias antes de añadir más referencias |
| Test E2E de Desktop requiere entorno interactivo | Falla en CI o sesión no interactiva | Mantenerlo creado pero no obligatorio hasta definir runner |

## 20. Criterios de salida

El plan se considera ejecutado correctamente cuando:

1. La estructura de carpetas coincide con el documento.
2. Todos los proyectos están incluidos en `CriGes.sln`.
3. Las referencias coinciden con la matriz permitida.
4. `dotnet restore` finaliza correctamente.
5. `dotnet build CriGes.sln` finaliza correctamente.
6. `dotnet test CriGes.sln` ejecuta al menos la prueba de arquitectura inicial.
7. API arranca y responde a `/health/live`.
8. Worker arranca y se detiene limpiamente.
9. DbMigrator arranca y se detiene limpiamente.
10. Desktop compila como WPF.
11. No hay secretos en archivos versionados.
12. Queda claro el siguiente bloque: implementar `PVS-007` a `PVS-014`.

## 21. Siguiente paso

Una vez ejecutado este plan, el desarrollo continuará con:

`EPIC-PVS-002 - Infraestructura transversal mínima`

Primeras piezas:

- Reloj UTC.
- Generador de identificadores.
- `Result` y errores comunes.
- Correlation ID.
- `ProblemDetails`.
- Hash de contraseña.
- Sanitizado de auditoría.
- Idempotencia HTTP mínima.

