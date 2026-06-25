namespace CriGes.Modules.Platform.Infrastructure.Persistence.Entities;

public sealed class RoleEntity
{
    public Guid RoleId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string NormalizedName { get; set; } = string.Empty;

    public byte RoleType { get; set; }

    public byte Status { get; set; }

    public bool IsProtected { get; set; }

    public long PermissionVersion { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public DateTime ModifiedAtUtc { get; set; }

    public Guid? CreatedByUserId { get; set; }

    public Guid? ModifiedByUserId { get; set; }

    public byte[] RowVersion { get; set; } = [];
}
