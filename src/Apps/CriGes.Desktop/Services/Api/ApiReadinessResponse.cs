namespace CriGes.Desktop.Services.Api;

public sealed record ApiReadinessResponse(
    string Status,
    string? Detail,
    int PendingMigrations);
