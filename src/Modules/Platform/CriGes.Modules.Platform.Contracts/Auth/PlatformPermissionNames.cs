namespace CriGes.Modules.Platform.Contracts.Auth;

public static class PlatformPermissionNames
{
    public const string ManageUsers = "Platform.ManageUsers";
    public const string ManageRoles = "Platform.ManageRoles";
    public const string ManageConfiguration = "Platform.ManageConfiguration";
    public const string ViewAudit = "Platform.ViewAudit";
    public const string ExportAudit = "Platform.ExportAudit";
    public const string ViewDiagnostics = "Platform.ViewDiagnostics";
    public const string ManageBackups = "Platform.ManageBackups";
    public const string RestoreBackups = "Platform.RestoreBackups";
    public const string ViewSessions = "Platform.ViewSessions";
    public const string CloseSessions = "Platform.CloseSessions";
    public const string UseAttachments = "Platform.UseAttachments";
    public const string ViewCustomers = "Customers.View";
    public const string ManageCustomers = "Customers.Manage";

    public static readonly IReadOnlyList<string> All =
    [
        ManageUsers,
        ManageRoles,
        ManageConfiguration,
        ViewAudit,
        ExportAudit,
        ViewDiagnostics,
        ManageBackups,
        RestoreBackups,
        ViewSessions,
        CloseSessions,
        UseAttachments,
        ViewCustomers,
        ManageCustomers
    ];
}
