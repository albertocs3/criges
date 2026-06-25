using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using CriGes.Modules.Platform.Application.Initialization;
using CriGes.Modules.Platform.Contracts.Auth;
using CriGes.Modules.Platform.Contracts.Installation;
using CriGes.Modules.Platform.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace CriGes.Modules.Platform.ContractTests.Auth;

public sealed class PlatformAuthEndpointTests
{
    [Fact]
    public async Task LoginReturnsConflictBeforeInitialization()
    {
        await using var factory = CreateFactory();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/v1/platform/auth/login", CreateLoginRequest());
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal("PLATFORM.NOT_INITIALIZED", problem.GetProperty("code").GetString());
    }

    [Fact]
    public async Task LoginReturnsSessionAfterInitialization()
    {
        await using var factory = CreateFactory();
        using var client = factory.CreateClient();
        await InitializeAsync(client);

        var response = await client.PostAsJsonAsync("/api/v1/platform/auth/login", CreateLoginRequest());
        var session = await response.Content.ReadFromJsonAsync<SessionResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(session);
        Assert.False(string.IsNullOrWhiteSpace(session.AccessToken));
        Assert.False(string.IsNullOrWhiteSpace(session.RefreshToken));
        Assert.Equal("Administrador", session.User.DisplayName);
        Assert.Equal("admin", session.User.UserName);
        Assert.Equal("Administrador", session.User.Role);
        Assert.Contains(PlatformPermissionNames.ManageUsers, session.User.Permissions);
        Assert.Contains(PlatformPermissionNames.ManageConfiguration, session.User.Permissions);
    }

    [Fact]
    public async Task LoginReturnsUnauthorizedForInvalidCredentials()
    {
        await using var factory = CreateFactory();
        using var client = factory.CreateClient();
        await InitializeAsync(client);

        var response = await client.PostAsJsonAsync(
            "/api/v1/platform/auth/login",
            CreateLoginRequest() with { Password = "WrongPassword123!" });
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal("AUTH.INVALID_CREDENTIALS", problem.GetProperty("code").GetString());
    }

    [Fact]
    public async Task LoginReturnsConflictWhenActiveSessionExists()
    {
        await using var factory = CreateFactory();
        using var client = factory.CreateClient();
        await InitializeAsync(client);

        var first = await client.PostAsJsonAsync("/api/v1/platform/auth/login", CreateLoginRequest());
        var second = await client.PostAsJsonAsync("/api/v1/platform/auth/login", CreateLoginRequest() with { DeviceId = "WIN-CLIENT-02" });
        var problem = await second.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
        Assert.Equal("AUTH.ACTIVE_SESSION_EXISTS", problem.GetProperty("code").GetString());
    }

    [Fact]
    public async Task GetMeReturnsUnauthorizedWithoutBearerToken()
    {
        await using var factory = CreateFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/v1/platform/auth/me");
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal("AUTH.INVALID_TOKEN", problem.GetProperty("code").GetString());
    }

    [Fact]
    public async Task GetMeReturnsCurrentUserWithBearerToken()
    {
        await using var factory = CreateFactory();
        using var client = factory.CreateClient();
        await InitializeAsync(client);
        var session = await LoginAsync(client);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", session.AccessToken);
        var response = await client.GetFromJsonAsync<CurrentUserResponse>("/api/v1/platform/auth/me");

        Assert.NotNull(response);
        Assert.Equal("Administrador", response.DisplayName);
        Assert.Equal("admin", response.UserName);
        Assert.Equal(session.Session.Id, response.Session.Id);
        Assert.Contains(PlatformPermissionNames.ManageRoles, response.Permissions);
    }

    [Fact]
    public async Task RefreshRotatesTokensAndKeepsSessionActive()
    {
        await using var factory = CreateFactory();
        using var client = factory.CreateClient();
        await InitializeAsync(client);
        var session = await LoginAsync(client);

        var refresh = await client.PostAsJsonAsync(
            "/api/v1/platform/auth/refresh",
            new RefreshSessionRequest(session.Session.Id, session.RefreshToken));
        var refreshed = await refresh.Content.ReadFromJsonAsync<SessionResponse>();

        Assert.Equal(HttpStatusCode.OK, refresh.StatusCode);
        Assert.NotNull(refreshed);
        Assert.Equal(session.Session.Id, refreshed.Session.Id);
        Assert.NotEqual(session.AccessToken, refreshed.AccessToken);
        Assert.NotEqual(session.RefreshToken, refreshed.RefreshToken);
        Assert.Contains(PlatformPermissionNames.ViewDiagnostics, refreshed.User.Permissions);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", refreshed.AccessToken);
        var me = await client.GetAsync("/api/v1/platform/auth/me");
        Assert.Equal(HttpStatusCode.OK, me.StatusCode);
    }

    [Fact]
    public async Task LogoutClosesSessionAndInvalidatesBearerToken()
    {
        await using var factory = CreateFactory();
        using var client = factory.CreateClient();
        await InitializeAsync(client);
        var session = await LoginAsync(client);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", session.AccessToken);
        var logout = await client.PostAsync("/api/v1/platform/auth/logout", null);
        var meAfterLogout = await client.GetAsync("/api/v1/platform/auth/me");

        Assert.Equal(HttpStatusCode.NoContent, logout.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, meAfterLogout.StatusCode);
    }

    [Fact]
    public async Task GetPermissionsRequiresManageRolesPermission()
    {
        await using var factory = CreateFactory();
        using var client = factory.CreateClient();

        var anonymous = await client.GetAsync("/api/v1/platform/permissions");
        var anonymousProblem = await anonymous.Content.ReadFromJsonAsync<JsonElement>();

        await InitializeAsync(client);
        var session = await LoginAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", session.AccessToken);
        var authorized = await client.GetFromJsonAsync<IReadOnlyList<PermissionResponse>>("/api/v1/platform/permissions");

        Assert.Equal(HttpStatusCode.Unauthorized, anonymous.StatusCode);
        Assert.Equal("AUTH.INVALID_TOKEN", anonymousProblem.GetProperty("code").GetString());
        Assert.NotNull(authorized);
        Assert.Contains(authorized, permission => permission.Name == PlatformPermissionNames.ManageRoles);
    }

    private static LoginRequest CreateLoginRequest()
    {
        return new LoginRequest("admin", "StrongPassword1!", "WIN-CLIENT-01", "1.0.0");
    }

    private static async Task InitializeAsync(HttpClient client)
    {
        var message = JsonContent.Create(new InitializePlatformRequest(
            new CompanyRequest(
                "Empresa Ejemplo SL",
                "Empresa Ejemplo",
                "B12345678",
                new AddressRequest("Calle Mayor 1", "28001", "Madrid", "Madrid", "ES"),
                "+34910000000",
                "administracion@example.com"),
            new AdministratorRequest("Administrador", "admin", "StrongPassword1!")));
        message.Headers.Add("Idempotency-Key", Guid.NewGuid().ToString("N"));

        var response = await client.PostAsync("/api/v1/platform/installation/initialize", message);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    private static async Task<SessionResponse> LoginAsync(HttpClient client)
    {
        var response = await client.PostAsJsonAsync("/api/v1/platform/auth/login", CreateLoginRequest());
        var session = await response.Content.ReadFromJsonAsync<SessionResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(session);
        return session;
    }

    private static WebApplicationFactory<Program> CreateFactory()
    {
        return new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.ConfigureLogging(logging => logging.ClearProviders());
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IPlatformInitializationStore>();
                services.AddPlatformInMemoryInfrastructureForTests();
            });
        });
    }
}
