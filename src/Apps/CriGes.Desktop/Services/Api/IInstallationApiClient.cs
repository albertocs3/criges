using CriGes.Modules.Platform.Contracts.Administration;
using CriGes.Modules.Platform.Contracts.Auth;
using CriGes.Modules.Platform.Contracts.Installation;

namespace CriGes.Desktop.Services.Api;

public interface IInstallationApiClient
{
    Task<ApiReadinessResponse> GetReadinessAsync(CancellationToken cancellationToken = default);

    Task<InstallationStatusResponse> GetStatusAsync(CancellationToken cancellationToken = default);

    Task<InitializePlatformResponse> InitializeAsync(
        InitializePlatformRequest request,
        CancellationToken cancellationToken = default);

    Task<SessionResponse> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default);

    Task<CurrentUserResponse> GetCurrentUserAsync(
        string accessToken,
        CancellationToken cancellationToken = default);

    Task<SessionResponse> RefreshAsync(
        RefreshSessionRequest request,
        CancellationToken cancellationToken = default);

    Task LogoutAsync(
        string accessToken,
        CancellationToken cancellationToken = default);

    Task<CloseActiveSessionsResponse> CloseDevelopmentActiveSessionsAsync(
        CloseActiveSessionsRequest request,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RoleSummaryResponse>> GetRolesAsync(
        string accessToken,
        CancellationToken cancellationToken = default);

    Task<RoleSummaryResponse> CreateRoleAsync(
        string accessToken,
        CreateRoleRequest request,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<UserSummaryResponse>> GetUsersAsync(
        string accessToken,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AuditEventResponse>> GetAuditEventsAsync(
        string accessToken,
        int take = 50,
        CancellationToken cancellationToken = default);

    Task<RolePermissionsResponse> GetRolePermissionsAsync(
        string accessToken,
        Guid roleId,
        CancellationToken cancellationToken = default);

    Task<RolePermissionsResponse> UpdateRolePermissionsAsync(
        string accessToken,
        Guid roleId,
        UpdateRolePermissionsRequest request,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PermissionResponse>> GetPermissionsAsync(
        string accessToken,
        CancellationToken cancellationToken = default);

    Task<UserSummaryResponse> CreateUserAsync(
        string accessToken,
        CreateUserRequest request,
        CancellationToken cancellationToken = default);
}
