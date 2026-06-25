using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using CriGes.Modules.Platform.Contracts.Administration;
using CriGes.Modules.Platform.Contracts.Auth;
using CriGes.Modules.Platform.Contracts.Installation;

namespace CriGes.Desktop.Services.Api;

public sealed class InstallationApiClient : IInstallationApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _httpClient;

    public InstallationApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ApiReadinessResponse> GetReadinessAsync(CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient
            .GetAsync("health/ready", cancellationToken)
            .ConfigureAwait(false);

        var readiness = await response.Content
            .ReadFromJsonAsync<ApiReadinessResponse>(JsonOptions, cancellationToken)
            .ConfigureAwait(false);

        if (readiness is not null)
        {
            return readiness;
        }

        if (!response.IsSuccessStatusCode)
        {
            throw await InstallationApiException.FromResponseAsync(response, cancellationToken).ConfigureAwait(false);
        }

        return new ApiReadinessResponse("ready", null, PendingMigrations: 0);
    }

    public async Task<InstallationStatusResponse> GetStatusAsync(CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient
            .GetAsync("api/v1/platform/installation/", cancellationToken)
            .ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            throw await InstallationApiException.FromResponseAsync(response, cancellationToken).ConfigureAwait(false);
        }

        var status = await response.Content
            .ReadFromJsonAsync<InstallationStatusResponse>(JsonOptions, cancellationToken)
            .ConfigureAwait(false);

        return status ?? throw new InstallationApiException(
            HttpStatusCode.InternalServerError,
            "Respuesta vacia",
            "La API no devolvio el estado de inicializacion.");
    }

    public async Task<InitializePlatformResponse> InitializeAsync(
        InitializePlatformRequest request,
        CancellationToken cancellationToken = default)
    {
        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "api/v1/platform/installation/initialize")
        {
            Content = JsonContent.Create(request, options: JsonOptions)
        };

        httpRequest.Headers.Add("Idempotency-Key", Guid.NewGuid().ToString("N"));
        httpRequest.Headers.Add("X-Correlation-Id", Guid.NewGuid().ToString("N"));

        using var response = await _httpClient.SendAsync(httpRequest, cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
        {
            throw await InstallationApiException.FromResponseAsync(response, cancellationToken).ConfigureAwait(false);
        }

        var result = await response.Content
            .ReadFromJsonAsync<InitializePlatformResponse>(JsonOptions, cancellationToken)
            .ConfigureAwait(false);

        return result ?? throw new InstallationApiException(
            HttpStatusCode.InternalServerError,
            "Respuesta vacia",
            "La API no devolvio la confirmacion de inicializacion.");
    }

    public async Task<SessionResponse> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default)
    {
        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "api/v1/platform/auth/login")
        {
            Content = JsonContent.Create(request, options: JsonOptions)
        };

        httpRequest.Headers.Add("X-Correlation-Id", Guid.NewGuid().ToString("N"));

        using var response = await _httpClient.SendAsync(httpRequest, cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
        {
            throw await InstallationApiException.FromResponseAsync(response, cancellationToken).ConfigureAwait(false);
        }

        var result = await response.Content
            .ReadFromJsonAsync<SessionResponse>(JsonOptions, cancellationToken)
            .ConfigureAwait(false);

        return result ?? throw new InstallationApiException(
            HttpStatusCode.InternalServerError,
            "Respuesta vacia",
            "La API no devolvio la sesion.");
    }

    public async Task<CurrentUserResponse> GetCurrentUserAsync(
        string accessToken,
        CancellationToken cancellationToken = default)
    {
        using var httpRequest = new HttpRequestMessage(HttpMethod.Get, "api/v1/platform/auth/me");
        AddBearer(httpRequest, accessToken);

        using var response = await _httpClient.SendAsync(httpRequest, cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
        {
            throw await InstallationApiException.FromResponseAsync(response, cancellationToken).ConfigureAwait(false);
        }

        var result = await response.Content
            .ReadFromJsonAsync<CurrentUserResponse>(JsonOptions, cancellationToken)
            .ConfigureAwait(false);

        return result ?? throw new InstallationApiException(
            HttpStatusCode.InternalServerError,
            "Respuesta vacia",
            "La API no devolvio el usuario actual.");
    }

    public async Task<SessionResponse> RefreshAsync(
        RefreshSessionRequest request,
        CancellationToken cancellationToken = default)
    {
        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "api/v1/platform/auth/refresh")
        {
            Content = JsonContent.Create(request, options: JsonOptions)
        };

        httpRequest.Headers.Add("X-Correlation-Id", Guid.NewGuid().ToString("N"));

        using var response = await _httpClient.SendAsync(httpRequest, cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
        {
            throw await InstallationApiException.FromResponseAsync(response, cancellationToken).ConfigureAwait(false);
        }

        var result = await response.Content
            .ReadFromJsonAsync<SessionResponse>(JsonOptions, cancellationToken)
            .ConfigureAwait(false);

        return result ?? throw new InstallationApiException(
            HttpStatusCode.InternalServerError,
            "Respuesta vacia",
            "La API no devolvio la sesion refrescada.");
    }

    public async Task LogoutAsync(
        string accessToken,
        CancellationToken cancellationToken = default)
    {
        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "api/v1/platform/auth/logout");
        AddBearer(httpRequest, accessToken);

        using var response = await _httpClient.SendAsync(httpRequest, cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
        {
            throw await InstallationApiException.FromResponseAsync(response, cancellationToken).ConfigureAwait(false);
        }
    }

    public async Task<CloseActiveSessionsResponse> CloseDevelopmentActiveSessionsAsync(
        CloseActiveSessionsRequest request,
        CancellationToken cancellationToken = default)
    {
        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "api/v1/development/platform/auth/active-sessions/close")
        {
            Content = JsonContent.Create(request, options: JsonOptions)
        };

        httpRequest.Headers.Add("X-Correlation-Id", Guid.NewGuid().ToString("N"));

        using var response = await _httpClient.SendAsync(httpRequest, cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
        {
            throw await InstallationApiException.FromResponseAsync(response, cancellationToken).ConfigureAwait(false);
        }

        var result = await response.Content
            .ReadFromJsonAsync<CloseActiveSessionsResponse>(JsonOptions, cancellationToken)
            .ConfigureAwait(false);

        return result ?? throw new InstallationApiException(
            HttpStatusCode.InternalServerError,
            "Respuesta vacia",
            "La API no devolvio el resultado de cierre de sesiones.");
    }

    public async Task<IReadOnlyList<RoleSummaryResponse>> GetRolesAsync(
        string accessToken,
        CancellationToken cancellationToken = default)
    {
        using var httpRequest = new HttpRequestMessage(HttpMethod.Get, "api/v1/platform/roles");
        AddBearer(httpRequest, accessToken);

        using var response = await _httpClient.SendAsync(httpRequest, cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
        {
            throw await InstallationApiException.FromResponseAsync(response, cancellationToken).ConfigureAwait(false);
        }

        var result = await response.Content
            .ReadFromJsonAsync<IReadOnlyList<RoleSummaryResponse>>(JsonOptions, cancellationToken)
            .ConfigureAwait(false);

        return result ?? throw new InstallationApiException(
            HttpStatusCode.InternalServerError,
            "Respuesta vacia",
            "La API no devolvio roles.");
    }

    public async Task<IReadOnlyList<PermissionResponse>> GetPermissionsAsync(
        string accessToken,
        CancellationToken cancellationToken = default)
    {
        using var httpRequest = new HttpRequestMessage(HttpMethod.Get, "api/v1/platform/permissions");
        AddBearer(httpRequest, accessToken);

        using var response = await _httpClient.SendAsync(httpRequest, cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
        {
            throw await InstallationApiException.FromResponseAsync(response, cancellationToken).ConfigureAwait(false);
        }

        var result = await response.Content
            .ReadFromJsonAsync<IReadOnlyList<PermissionResponse>>(JsonOptions, cancellationToken)
            .ConfigureAwait(false);

        return result ?? throw new InstallationApiException(
            HttpStatusCode.InternalServerError,
            "Respuesta vacia",
            "La API no devolvio permisos.");
    }

    public async Task<RoleSummaryResponse> CreateRoleAsync(
        string accessToken,
        CreateRoleRequest request,
        CancellationToken cancellationToken = default)
    {
        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "api/v1/platform/roles")
        {
            Content = JsonContent.Create(request, options: JsonOptions)
        };
        AddBearer(httpRequest, accessToken);

        using var response = await _httpClient.SendAsync(httpRequest, cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
        {
            throw await InstallationApiException.FromResponseAsync(response, cancellationToken).ConfigureAwait(false);
        }

        var result = await response.Content
            .ReadFromJsonAsync<RoleSummaryResponse>(JsonOptions, cancellationToken)
            .ConfigureAwait(false);

        return result ?? throw new InstallationApiException(
            HttpStatusCode.InternalServerError,
            "Respuesta vacia",
            "La API no devolvio el rol creado.");
    }

    public async Task<IReadOnlyList<UserSummaryResponse>> GetUsersAsync(
        string accessToken,
        CancellationToken cancellationToken = default)
    {
        using var httpRequest = new HttpRequestMessage(HttpMethod.Get, "api/v1/platform/users");
        AddBearer(httpRequest, accessToken);

        using var response = await _httpClient.SendAsync(httpRequest, cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
        {
            throw await InstallationApiException.FromResponseAsync(response, cancellationToken).ConfigureAwait(false);
        }

        var result = await response.Content
            .ReadFromJsonAsync<IReadOnlyList<UserSummaryResponse>>(JsonOptions, cancellationToken)
            .ConfigureAwait(false);

        return result ?? throw new InstallationApiException(
            HttpStatusCode.InternalServerError,
            "Respuesta vacia",
            "La API no devolvio usuarios.");
    }

    public async Task<IReadOnlyList<AuditEventResponse>> GetAuditEventsAsync(
        string accessToken,
        int take = 50,
        CancellationToken cancellationToken = default)
    {
        using var httpRequest = new HttpRequestMessage(HttpMethod.Get, $"api/v1/platform/audit-events?take={take}");
        AddBearer(httpRequest, accessToken);

        using var response = await _httpClient.SendAsync(httpRequest, cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
        {
            throw await InstallationApiException.FromResponseAsync(response, cancellationToken).ConfigureAwait(false);
        }

        var result = await response.Content
            .ReadFromJsonAsync<IReadOnlyList<AuditEventResponse>>(JsonOptions, cancellationToken)
            .ConfigureAwait(false);

        return result ?? throw new InstallationApiException(
            HttpStatusCode.InternalServerError,
            "Respuesta vacia",
            "La API no devolvio auditoria.");
    }

    public async Task<RolePermissionsResponse> GetRolePermissionsAsync(
        string accessToken,
        Guid roleId,
        CancellationToken cancellationToken = default)
    {
        using var httpRequest = new HttpRequestMessage(HttpMethod.Get, $"api/v1/platform/roles/{roleId:D}/permissions");
        AddBearer(httpRequest, accessToken);

        using var response = await _httpClient.SendAsync(httpRequest, cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
        {
            throw await InstallationApiException.FromResponseAsync(response, cancellationToken).ConfigureAwait(false);
        }

        var result = await response.Content
            .ReadFromJsonAsync<RolePermissionsResponse>(JsonOptions, cancellationToken)
            .ConfigureAwait(false);

        return result ?? throw new InstallationApiException(
            HttpStatusCode.InternalServerError,
            "Respuesta vacia",
            "La API no devolvio los permisos del rol.");
    }

    public async Task<RolePermissionsResponse> UpdateRolePermissionsAsync(
        string accessToken,
        Guid roleId,
        UpdateRolePermissionsRequest request,
        CancellationToken cancellationToken = default)
    {
        using var httpRequest = new HttpRequestMessage(HttpMethod.Put, $"api/v1/platform/roles/{roleId:D}/permissions")
        {
            Content = JsonContent.Create(request, options: JsonOptions)
        };
        AddBearer(httpRequest, accessToken);

        using var response = await _httpClient.SendAsync(httpRequest, cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
        {
            throw await InstallationApiException.FromResponseAsync(response, cancellationToken).ConfigureAwait(false);
        }

        var result = await response.Content
            .ReadFromJsonAsync<RolePermissionsResponse>(JsonOptions, cancellationToken)
            .ConfigureAwait(false);

        return result ?? throw new InstallationApiException(
            HttpStatusCode.InternalServerError,
            "Respuesta vacia",
            "La API no devolvio los permisos actualizados del rol.");
    }

    public async Task<UserSummaryResponse> CreateUserAsync(
        string accessToken,
        CreateUserRequest request,
        CancellationToken cancellationToken = default)
    {
        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "api/v1/platform/users")
        {
            Content = JsonContent.Create(request, options: JsonOptions)
        };
        AddBearer(httpRequest, accessToken);

        using var response = await _httpClient.SendAsync(httpRequest, cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
        {
            throw await InstallationApiException.FromResponseAsync(response, cancellationToken).ConfigureAwait(false);
        }

        var result = await response.Content
            .ReadFromJsonAsync<UserSummaryResponse>(JsonOptions, cancellationToken)
            .ConfigureAwait(false);

        return result ?? throw new InstallationApiException(
            HttpStatusCode.InternalServerError,
            "Respuesta vacia",
            "La API no devolvio el usuario creado.");
    }

    private static void AddBearer(HttpRequestMessage request, string accessToken)
    {
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        request.Headers.Add("X-Correlation-Id", Guid.NewGuid().ToString("N"));
    }
}
