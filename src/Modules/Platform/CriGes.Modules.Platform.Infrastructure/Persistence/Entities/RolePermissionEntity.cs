namespace CriGes.Modules.Platform.Infrastructure.Persistence.Entities;

public sealed class RolePermissionEntity
{
    public Guid RoleId { get; set; }

    public string Permission { get; set; } = string.Empty;

    public DateTime GrantedAtUtc { get; set; }

    public Guid? GrantedByUserId { get; set; }
}
