using CriGes.Modules.Platform.Contracts.Auth;

namespace CriGes.Modules.Platform.Application.Auth;

public static class RolePermissionCatalog
{
    public static IReadOnlyList<string> GetPermissionsForRole(string roleName)
    {
        return roleName switch
        {
            "Administrador" => PlatformPermissionNames.All,
            "Facturacion" => BillingPermissions,
            "Contabilidad" => AccountingPermissions,
            "Tecnico" => TechnicianPermissions,
            _ => Array.Empty<string>()
        };
    }

    private static readonly IReadOnlyList<string> BillingPermissions =
    [
        PlatformPermissionNames.UseAttachments,
        PlatformPermissionNames.ViewCustomers,
        PlatformPermissionNames.ManageCustomers
    ];

    private static readonly IReadOnlyList<string> AccountingPermissions =
    [
        PlatformPermissionNames.ViewAudit
    ];

    private static readonly IReadOnlyList<string> TechnicianPermissions =
    [
        PlatformPermissionNames.ViewDiagnostics,
        PlatformPermissionNames.UseAttachments
    ];
}
