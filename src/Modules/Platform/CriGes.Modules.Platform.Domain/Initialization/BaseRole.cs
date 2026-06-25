namespace CriGes.Modules.Platform.Domain.Initialization;

public sealed record BaseRole(Guid RoleId, string Name, string NormalizedName, bool IsProtected)
{
    public static IReadOnlyList<string> RequiredNames { get; } =
    [
        "Administrador",
        "Facturacion",
        "Contabilidad",
        "Tecnico"
    ];

    public static BaseRole Create(Guid roleId, string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return new BaseRole(roleId, name, Normalize(name), IsProtected: true);
    }

    public static string Normalize(string value)
    {
        return value.Trim().ToUpperInvariant();
    }
}
