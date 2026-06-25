using System.Net;
using System.Net.Http;
using System.Text.Json;
using CriGes.Desktop.Services.Api;
using CriGes.Modules.Platform.Contracts.Administration;
using CriGes.Modules.Platform.Contracts.Auth;
using CriGes.Modules.Platform.Contracts.Customers;
using CriGes.Modules.Platform.Contracts.Installation;

namespace CriGes.Desktop.EndToEndTests;

public sealed class InstallationApiClientTests
{
    [Fact]
    public async Task GetReadinessAsyncReturnsReadyStatus()
    {
        using var httpClient = new HttpClient(new StubHttpMessageHandler(request =>
        {
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal("/health/ready", request.RequestUri?.AbsolutePath);

            return JsonResponse(new ApiReadinessResponse("ready", null, 0));
        }))
        {
            BaseAddress = new Uri("http://localhost:5099/")
        };

        var client = new InstallationApiClient(httpClient);

        var response = await client.GetReadinessAsync();

        Assert.Equal("ready", response.Status);
        Assert.Equal(0, response.PendingMigrations);
    }

    [Fact]
    public async Task GetReadinessAsyncReturnsDatabaseNotMigratedStatus()
    {
        using var httpClient = new HttpClient(new StubHttpMessageHandler(request =>
            JsonResponse(
                new ApiReadinessResponse("databaseNotMigrated", "Hay migraciones pendientes.", 2),
                HttpStatusCode.ServiceUnavailable)))
        {
            BaseAddress = new Uri("http://localhost:5099/")
        };

        var client = new InstallationApiClient(httpClient);

        var response = await client.GetReadinessAsync();

        Assert.Equal("databaseNotMigrated", response.Status);
        Assert.Equal(2, response.PendingMigrations);
    }

    [Fact]
    public async Task GetStatusAsyncReturnsNotInitializedStatus()
    {
        using var httpClient = new HttpClient(new StubHttpMessageHandler(request =>
        {
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal("/api/v1/platform/installation/", request.RequestUri?.AbsolutePath);

            return JsonResponse(new InstallationStatusResponse("notInitialized", "0.1.0", true));
        }))
        {
            BaseAddress = new Uri("http://localhost:5099/")
        };

        var client = new InstallationApiClient(httpClient);

        var response = await client.GetStatusAsync();

        Assert.True(response.RequiresInitialization);
        Assert.Equal("notInitialized", response.Status);
    }

    [Fact]
    public async Task InitializeAsyncSendsIdempotencyAndCorrelationHeaders()
    {
        using var httpClient = new HttpClient(new StubHttpMessageHandler(request =>
        {
            Assert.Equal(HttpMethod.Post, request.Method);
            Assert.Equal("/api/v1/platform/installation/initialize", request.RequestUri?.AbsolutePath);
            Assert.True(request.Headers.Contains("Idempotency-Key"));
            Assert.True(request.Headers.Contains("X-Correlation-Id"));

            return JsonResponse(new InitializePlatformResponse(Guid.NewGuid(), "initialized", Guid.NewGuid(), false), HttpStatusCode.Created);
        }))
        {
            BaseAddress = new Uri("http://localhost:5099/")
        };

        var client = new InstallationApiClient(httpClient);
        var request = new InitializePlatformRequest(
            new CompanyRequest(
                "CriGes SL",
                "CriGes",
                "B00000000",
                new AddressRequest("Calle Mayor 1", "29000", "Malaga", "Malaga", "ES"),
                "600000000",
                "admin@criges.local"),
            new AdministratorRequest("Admin CriGes", "admin", "Password123!"));

        var response = await client.InitializeAsync(request);

        Assert.Equal("initialized", response.Status);
    }

    [Fact]
    public async Task LoginAsyncSendsCredentialsAndReturnsSession()
    {
        using var httpClient = new HttpClient(new StubHttpMessageHandler(request =>
        {
            Assert.Equal(HttpMethod.Post, request.Method);
            Assert.Equal("/api/v1/platform/auth/login", request.RequestUri?.AbsolutePath);
            Assert.True(request.Headers.Contains("X-Correlation-Id"));

            return JsonResponse(CreateSessionResponse());
        }))
        {
            BaseAddress = new Uri("http://localhost:5099/")
        };

        var client = new InstallationApiClient(httpClient);

        var response = await client.LoginAsync(new LoginRequest("admin", "Password123!", "WIN-CLIENT-01", "1.0.0"));

        Assert.Equal("Administrador", response.User.DisplayName);
        Assert.False(string.IsNullOrWhiteSpace(response.AccessToken));
    }

    [Fact]
    public async Task GetCurrentUserAsyncSendsBearerToken()
    {
        using var httpClient = new HttpClient(new StubHttpMessageHandler(request =>
        {
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal("/api/v1/platform/auth/me", request.RequestUri?.AbsolutePath);
            Assert.Equal("Bearer", request.Headers.Authorization?.Scheme);
            Assert.Equal("access-token", request.Headers.Authorization?.Parameter);

            return JsonResponse(new CurrentUserResponse(
                Guid.NewGuid(),
                "Administrador",
                "admin",
                new CurrentUserRoleResponse(Guid.NewGuid(), "Administrador"),
                PlatformPermissionNames.All,
                new CurrentUserSessionResponse(Guid.NewGuid(), DateTimeOffset.UtcNow.AddHours(5))));
        }))
        {
            BaseAddress = new Uri("http://localhost:5099/")
        };

        var client = new InstallationApiClient(httpClient);

        var response = await client.GetCurrentUserAsync("access-token");

        Assert.Equal("Administrador", response.DisplayName);
    }

    [Fact]
    public async Task RefreshAsyncReturnsRotatedSession()
    {
        using var httpClient = new HttpClient(new StubHttpMessageHandler(request =>
        {
            Assert.Equal(HttpMethod.Post, request.Method);
            Assert.Equal("/api/v1/platform/auth/refresh", request.RequestUri?.AbsolutePath);

            return JsonResponse(CreateSessionResponse());
        }))
        {
            BaseAddress = new Uri("http://localhost:5099/")
        };

        var client = new InstallationApiClient(httpClient);

        var response = await client.RefreshAsync(new RefreshSessionRequest(Guid.NewGuid(), "refresh-token"));

        Assert.False(string.IsNullOrWhiteSpace(response.RefreshToken));
    }

    [Fact]
    public async Task LogoutAsyncSendsBearerToken()
    {
        using var httpClient = new HttpClient(new StubHttpMessageHandler(request =>
        {
            Assert.Equal(HttpMethod.Post, request.Method);
            Assert.Equal("/api/v1/platform/auth/logout", request.RequestUri?.AbsolutePath);
            Assert.Equal("access-token", request.Headers.Authorization?.Parameter);

            return new HttpResponseMessage(HttpStatusCode.NoContent);
        }))
        {
            BaseAddress = new Uri("http://localhost:5099/")
        };

        var client = new InstallationApiClient(httpClient);

        await client.LogoutAsync("access-token");
    }

    [Fact]
    public async Task CloseDevelopmentActiveSessionsAsyncSendsUserName()
    {
        using var httpClient = new HttpClient(new StubHttpMessageHandler(request =>
        {
            Assert.Equal(HttpMethod.Post, request.Method);
            Assert.Equal("/api/v1/development/platform/auth/active-sessions/close", request.RequestUri?.AbsolutePath);
            Assert.True(request.Headers.Contains("X-Correlation-Id"));

            return JsonResponse(new CloseActiveSessionsResponse(1));
        }))
        {
            BaseAddress = new Uri("http://localhost:5099/")
        };

        var client = new InstallationApiClient(httpClient);

        var response = await client.CloseDevelopmentActiveSessionsAsync(new CloseActiveSessionsRequest("admin"));

        Assert.Equal(1, response.ClosedSessions);
    }

    [Fact]
    public async Task GetRolesAsyncSendsBearerToken()
    {
        using var httpClient = new HttpClient(new StubHttpMessageHandler(request =>
        {
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal("/api/v1/platform/roles", request.RequestUri?.AbsolutePath);
            Assert.Equal("Bearer", request.Headers.Authorization?.Scheme);
            Assert.Equal("access-token", request.Headers.Authorization?.Parameter);

            return JsonResponse<IReadOnlyList<RoleSummaryResponse>>(
            [
                new RoleSummaryResponse(Guid.NewGuid(), "Administrador", "system", "active", PlatformPermissionNames.All)
            ]);
        }))
        {
            BaseAddress = new Uri("http://localhost:5099/")
        };

        var client = new InstallationApiClient(httpClient);

        var response = await client.GetRolesAsync("access-token");

        Assert.Single(response);
        Assert.Equal("Administrador", response[0].Name);
    }

    [Fact]
    public async Task CreateRoleAsyncSendsBearerTokenAndReturnsRole()
    {
        var roleId = Guid.NewGuid();
        using var httpClient = new HttpClient(new StubHttpMessageHandler(request =>
        {
            Assert.Equal(HttpMethod.Post, request.Method);
            Assert.Equal("/api/v1/platform/roles", request.RequestUri?.AbsolutePath);
            Assert.Equal("access-token", request.Headers.Authorization?.Parameter);

            return JsonResponse(
                new RoleSummaryResponse(roleId, "Soporte", "custom", "active", []),
                HttpStatusCode.Created);
        }))
        {
            BaseAddress = new Uri("http://localhost:5099/")
        };

        var client = new InstallationApiClient(httpClient);

        var response = await client.CreateRoleAsync("access-token", new CreateRoleRequest("Soporte"));

        Assert.Equal(roleId, response.Id);
        Assert.Equal("Soporte", response.Name);
        Assert.Equal("custom", response.Type);
    }

    [Fact]
    public async Task GetPermissionsAsyncSendsBearerToken()
    {
        using var httpClient = new HttpClient(new StubHttpMessageHandler(request =>
        {
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal("/api/v1/platform/permissions", request.RequestUri?.AbsolutePath);
            Assert.Equal("access-token", request.Headers.Authorization?.Parameter);

            return JsonResponse<IReadOnlyList<PermissionResponse>>(
            [
                new PermissionResponse(PlatformPermissionNames.ManageUsers, "Crear y modificar usuarios")
            ]);
        }))
        {
            BaseAddress = new Uri("http://localhost:5099/")
        };

        var client = new InstallationApiClient(httpClient);

        var response = await client.GetPermissionsAsync("access-token");

        Assert.Single(response);
        Assert.Equal(PlatformPermissionNames.ManageUsers, response[0].Name);
    }

    [Fact]
    public async Task GetUsersAsyncSendsBearerToken()
    {
        using var httpClient = new HttpClient(new StubHttpMessageHandler(request =>
        {
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal("/api/v1/platform/users", request.RequestUri?.AbsolutePath);
            Assert.Equal("access-token", request.Headers.Authorization?.Parameter);

            return JsonResponse<IReadOnlyList<UserSummaryResponse>>(
            [
                new UserSummaryResponse(
                    Guid.NewGuid(),
                    "Administrador",
                    "admin",
                    null,
                    new RoleReferenceResponse(Guid.NewGuid(), "Administrador"),
                    "active",
                    null,
                    null)
            ]);
        }))
        {
            BaseAddress = new Uri("http://localhost:5099/")
        };

        var client = new InstallationApiClient(httpClient);

        var response = await client.GetUsersAsync("access-token");

        Assert.Single(response);
        Assert.Equal("admin", response[0].UserName);
    }

    [Fact]
    public async Task GetAuditEventsAsyncSendsBearerToken()
    {
        using var httpClient = new HttpClient(new StubHttpMessageHandler(request =>
        {
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal("/api/v1/platform/audit-events", request.RequestUri?.AbsolutePath);
            Assert.Equal("take=25", request.RequestUri?.Query.TrimStart('?'));
            Assert.Equal("access-token", request.Headers.Authorization?.Parameter);

            return JsonResponse<IReadOnlyList<AuditEventResponse>>(
            [
                new AuditEventResponse(
                    1,
                    DateTimeOffset.UtcNow,
                    Guid.NewGuid(),
                    "Administrador",
                    "Platform",
                    "UserCreated",
                    "User",
                    Guid.NewGuid().ToString("D"),
                    "User created.",
                    Guid.NewGuid(),
                    "success")
            ]);
        }))
        {
            BaseAddress = new Uri("http://localhost:5099/")
        };

        var client = new InstallationApiClient(httpClient);

        var response = await client.GetAuditEventsAsync("access-token", take: 25);

        Assert.Single(response);
        Assert.Equal("UserCreated", response[0].Action);
    }

    [Fact]
    public async Task GetRolePermissionsAsyncSendsBearerToken()
    {
        var roleId = Guid.NewGuid();
        using var httpClient = new HttpClient(new StubHttpMessageHandler(request =>
        {
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal($"/api/v1/platform/roles/{roleId:D}/permissions", request.RequestUri?.AbsolutePath);
            Assert.Equal("access-token", request.Headers.Authorization?.Parameter);

            return JsonResponse(new RolePermissionsResponse(roleId, [PlatformPermissionNames.ManageUsers]));
        }))
        {
            BaseAddress = new Uri("http://localhost:5099/")
        };

        var client = new InstallationApiClient(httpClient);

        var response = await client.GetRolePermissionsAsync("access-token", roleId);

        Assert.Equal(roleId, response.RoleId);
        Assert.Contains(PlatformPermissionNames.ManageUsers, response.Permissions);
    }

    [Fact]
    public async Task UpdateRolePermissionsAsyncSendsBearerTokenAndBody()
    {
        var roleId = Guid.NewGuid();
        using var httpClient = new HttpClient(new StubHttpMessageHandler(request =>
        {
            Assert.Equal(HttpMethod.Put, request.Method);
            Assert.Equal($"/api/v1/platform/roles/{roleId:D}/permissions", request.RequestUri?.AbsolutePath);
            Assert.Equal("access-token", request.Headers.Authorization?.Parameter);

            return JsonResponse(new RolePermissionsResponse(roleId, [PlatformPermissionNames.ViewAudit]));
        }))
        {
            BaseAddress = new Uri("http://localhost:5099/")
        };

        var client = new InstallationApiClient(httpClient);

        var response = await client.UpdateRolePermissionsAsync(
            "access-token",
            roleId,
            new UpdateRolePermissionsRequest([PlatformPermissionNames.ViewAudit]));

        Assert.Equal(roleId, response.RoleId);
        Assert.Single(response.Permissions);
    }

    [Fact]
    public async Task CreateUserAsyncSendsBearerTokenAndReturnsUser()
    {
        var roleId = Guid.NewGuid();
        using var httpClient = new HttpClient(new StubHttpMessageHandler(request =>
        {
            Assert.Equal(HttpMethod.Post, request.Method);
            Assert.Equal("/api/v1/platform/users", request.RequestUri?.AbsolutePath);
            Assert.Equal("access-token", request.Headers.Authorization?.Parameter);

            return JsonResponse(
                new UserSummaryResponse(
                    Guid.NewGuid(),
                    "Usuario Nuevo",
                    "usuario",
                    "+34600000000",
                    new RoleReferenceResponse(roleId, "Administrador"),
                    "active",
                    null,
                    null),
                HttpStatusCode.Created);
        }))
        {
            BaseAddress = new Uri("http://localhost:5099/")
        };

        var client = new InstallationApiClient(httpClient);

        var response = await client.CreateUserAsync(
            "access-token",
            new CreateUserRequest("Usuario Nuevo", "usuario", "+34600000000", roleId, "Password123!"));

        Assert.Equal("usuario", response.UserName);
        Assert.Equal("Administrador", response.Role.Name);
    }

    [Fact]
    public async Task GetCustomersAsyncSendsBearerToken()
    {
        using var httpClient = new HttpClient(new StubHttpMessageHandler(request =>
        {
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal("/api/v1/customers/", request.RequestUri?.AbsolutePath);
            Assert.Equal("access-token", request.Headers.Authorization?.Parameter);

            return JsonResponse<IReadOnlyList<CustomerSummaryResponse>>(
            [
                new CustomerSummaryResponse(
                    Guid.NewGuid(),
                    "Cliente",
                    "B11111111",
                    "cliente@example.com",
                    "+34910000000",
                    "active",
                    DateTimeOffset.UtcNow)
            ]);
        }))
        {
            BaseAddress = new Uri("http://localhost:5099/")
        };

        var client = new InstallationApiClient(httpClient);

        var response = await client.GetCustomersAsync("access-token");

        Assert.Single(response);
        Assert.Equal("Cliente", response[0].Name);
    }

    [Fact]
    public async Task CreateCustomerAsyncSendsBearerTokenAndReturnsCustomer()
    {
        using var httpClient = new HttpClient(new StubHttpMessageHandler(request =>
        {
            Assert.Equal(HttpMethod.Post, request.Method);
            Assert.Equal("/api/v1/customers/", request.RequestUri?.AbsolutePath);
            Assert.Equal("access-token", request.Headers.Authorization?.Parameter);

            return JsonResponse(
                new CustomerSummaryResponse(
                    Guid.NewGuid(),
                    "Cliente Nuevo",
                    "B11111111",
                    "cliente@example.com",
                    "+34910000000",
                    "active",
                    DateTimeOffset.UtcNow),
                HttpStatusCode.Created);
        }))
        {
            BaseAddress = new Uri("http://localhost:5099/")
        };

        var client = new InstallationApiClient(httpClient);

        var response = await client.CreateCustomerAsync(
            "access-token",
            new CreateCustomerRequest("Cliente Nuevo", "B11111111", "cliente@example.com", "+34910000000"));

        Assert.Equal("Cliente Nuevo", response.Name);
    }

    private static HttpResponseMessage JsonResponse<T>(T body, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        return new HttpResponseMessage(statusCode)
        {
            Content = new StringContent(JsonSerializer.Serialize(body), System.Text.Encoding.UTF8, "application/json")
        };
    }

    private static SessionResponse CreateSessionResponse()
    {
        var now = DateTimeOffset.UtcNow;
        return new SessionResponse(
            "access-token",
            now.AddMinutes(15),
            "refresh-token",
            now.AddDays(30),
            new AuthSessionResponse(Guid.NewGuid(), now, now.AddHours(5)),
            new AuthUserResponse(Guid.NewGuid(), "Administrador", "admin", "Administrador", PlatformPermissionNames.All));
    }

    private sealed class StubHttpMessageHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _handler;

        public StubHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> handler)
        {
            _handler = handler;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(_handler(request));
        }
    }
}
