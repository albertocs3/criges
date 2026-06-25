using CriGes.Modules.Platform.Contracts.Auth;

namespace CriGes.Modules.Platform.Api.Auth;

internal static class PlatformPermissionDescriptions
{
    public static readonly IReadOnlyList<PermissionResponse> All =
    [
        new(PlatformPermissionNames.ManageUsers, "Crear y modificar usuarios"),
        new(PlatformPermissionNames.ManageRoles, "Gestionar roles y permisos"),
        new(PlatformPermissionNames.ManageConfiguration, "Modificar configuracion"),
        new(PlatformPermissionNames.ViewAudit, "Consultar auditoria"),
        new(PlatformPermissionNames.ExportAudit, "Exportar auditoria"),
        new(PlatformPermissionNames.ViewDiagnostics, "Consultar diagnostico"),
        new(PlatformPermissionNames.ManageBackups, "Crear copias"),
        new(PlatformPermissionNames.RestoreBackups, "Restaurar copias"),
        new(PlatformPermissionNames.ViewSessions, "Consultar sesiones"),
        new(PlatformPermissionNames.CloseSessions, "Cerrar sesiones"),
        new(PlatformPermissionNames.UseAttachments, "Usar adjuntos")
    ];
}
