using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using CriGes.Modules.Platform.Contracts.Administration;
using CriGes.Modules.Platform.Contracts.Auth;
using CriGes.Modules.Platform.Contracts.Installation;
using CriGes.Modules.Platform.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CriGes.Modules.Platform.IntegrationTests.Initialization;

public sealed class PlatformInitializationSqlIntegrationTests
{
    [Fact]
    public void PlatformDbContextExposesAllPlatformMigrations()
    {
        using var dbContext = CreateDbContext("Server=(localdb)\\MSSQLLocalDB;Database=CriGes_MigrationMetadata;Trusted_Connection=True;TrustServerCertificate=True");

        var migrations = dbContext.Database.GetMigrations().ToArray();

        Assert.Equal(
            [
                "202606240001_CreatePlatformSchema",
                "202606240002_AddHttpIdempotencyRecords",
                "202606240003_AddUserSessions",
                "202606240004_AddRolePermissions",
                "202606240005_AddUserPhone",
                "202606240006_SeedCurrentBaseRolePermissions"
            ],
            migrations);
    }

    [Fact]
    public async Task InitializeEndpointPersistsRequiredPlatformDataInSqlServer()
    {
        var databaseName = $"CriGes_Integration_{Guid.NewGuid():N}";
        var connectionString = $"Server=(localdb)\\MSSQLLocalDB;Database={databaseName};Trusted_Connection=True;TrustServerCertificate=True";

        await using var dbContext = CreateDbContext(connectionString);
        if (!await CanOpenSqlServerAsync(connectionString))
        {
            return;
        }

        await dbContext.Database.MigrateAsync();

        try
        {
            await using var factory = CreateFactory(connectionString);
            using var client = factory.CreateClient();

            var response = await PostInitializeAsync(client, CreateRequest(), "sql-integration-key-1");
            var body = await response.Content.ReadFromJsonAsync<InitializePlatformResponse>();

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.NotNull(body);
            Assert.Equal("initialized", body.Status);

            Assert.Equal(1, await dbContext.Installations.CountAsync());
            Assert.Equal(4, await dbContext.Roles.CountAsync(role => role.IsProtected));
            Assert.Equal(1, await dbContext.Users.CountAsync(user => user.NormalizedUserName == "ADMIN"));
            Assert.Equal(1, await dbContext.Users.CountAsync(user => user.NormalizedUserName == "SISTEMA"));
            Assert.Equal(15, await dbContext.RolePermissions.CountAsync());
            Assert.True(await dbContext.RolePermissions.AnyAsync(permission => permission.Permission == PlatformPermissionNames.ManageUsers));
            Assert.Equal(1, await dbContext.Companies.CountAsync());
            Assert.True(await dbContext.NumberCounters.AnyAsync());
            Assert.Equal(1, await dbContext.AuditEvents.CountAsync(audit => audit.Action == "PlatformInitialized"));
            Assert.Equal(1, await dbContext.OutboxMessages.CountAsync(message => message.MessageType == "PlatformInitialized"));
            Assert.Equal(1, await dbContext.HttpIdempotencyRecords.CountAsync(record => record.Key == "sql-integration-key-1"));

            var administrator = await dbContext.Users.SingleAsync(user => user.NormalizedUserName == "ADMIN");
            Assert.NotEqual("StrongPassword1!", administrator.PasswordHash);
            Assert.StartsWith("PBKDF2-SHA256.", administrator.PasswordHash, StringComparison.Ordinal);

            var auditTexts = await dbContext.AuditEvents
                .Select(audit => string.Concat(audit.Description, audit.NewValuesJson, audit.PreviousValuesJson))
                .ToListAsync();
            Assert.DoesNotContain(auditTexts, value => value.Contains("StrongPassword1!", StringComparison.Ordinal));
        }
        finally
        {
            await dbContext.Database.EnsureDeletedAsync();
        }
    }

    [Fact]
    public async Task InitializeEndpointReturns422AndDoesNotPersistDataWhenPasswordIsInvalid()
    {
        var databaseName = $"CriGes_Integration_{Guid.NewGuid():N}";
        var connectionString = $"Server=(localdb)\\MSSQLLocalDB;Database={databaseName};Trusted_Connection=True;TrustServerCertificate=True";

        await using var dbContext = CreateDbContext(connectionString);
        if (!await CanOpenSqlServerAsync(connectionString))
        {
            return;
        }

        await dbContext.Database.MigrateAsync();

        try
        {
            await using var factory = CreateFactory(connectionString);
            using var client = factory.CreateClient();

            var response = await PostInitializeAsync(
                client,
                CreateRequest() with
                {
                    Administrator = new AdministratorRequest("Administrador", "admin", "weak")
                },
                "sql-integration-invalid-password");
            var problem = await response.Content.ReadFromJsonAsync<JsonElement>();

            Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
            Assert.Equal("SECURITY.PASSWORD_POLICY_FAILED", problem.GetProperty("code").GetString());
            Assert.True(problem.TryGetProperty("correlationId", out _));
            Assert.Equal(0, await dbContext.Installations.CountAsync());
            Assert.Equal(0, await dbContext.Users.CountAsync());
            Assert.Equal(0, await dbContext.Roles.CountAsync());
        }
        finally
        {
            await dbContext.Database.EnsureDeletedAsync();
        }
    }

    [Fact]
    public async Task InitializeEndpointReturns409ForSecondInitializationAndForDifferentBodyWithSameIdempotencyKey()
    {
        var databaseName = $"CriGes_Integration_{Guid.NewGuid():N}";
        var connectionString = $"Server=(localdb)\\MSSQLLocalDB;Database={databaseName};Trusted_Connection=True;TrustServerCertificate=True";

        await using var dbContext = CreateDbContext(connectionString);
        if (!await CanOpenSqlServerAsync(connectionString))
        {
            return;
        }

        await dbContext.Database.MigrateAsync();

        try
        {
            await using var factory = CreateFactory(connectionString);
            using var client = factory.CreateClient();

            var created = await PostInitializeAsync(client, CreateRequest(), "sql-integration-conflict-1");
            var secondInitialization = await PostInitializeAsync(client, CreateRequest(), "sql-integration-conflict-2");
            var differentBodySameKey = await PostInitializeAsync(
                client,
                CreateRequest() with
                {
                    Company = CreateRequest().Company with { LegalName = "Otra Empresa SL" }
                },
                "sql-integration-conflict-1");

            Assert.Equal(HttpStatusCode.Created, created.StatusCode);
            Assert.Equal(HttpStatusCode.Conflict, secondInitialization.StatusCode);
            Assert.Equal(HttpStatusCode.Conflict, differentBodySameKey.StatusCode);
            Assert.Equal(1, await dbContext.Installations.CountAsync());
            Assert.Equal(2, await dbContext.HttpIdempotencyRecords.CountAsync());
        }
        finally
        {
            await dbContext.Database.EnsureDeletedAsync();
        }
    }

    [Fact]
    public async Task LoginEndpointCreatesInitialSessionInSqlServer()
    {
        var databaseName = $"CriGes_Integration_{Guid.NewGuid():N}";
        var connectionString = $"Server=(localdb)\\MSSQLLocalDB;Database={databaseName};Trusted_Connection=True;TrustServerCertificate=True";

        await using var dbContext = CreateDbContext(connectionString);
        if (!await CanOpenSqlServerAsync(connectionString))
        {
            return;
        }

        await dbContext.Database.MigrateAsync();

        try
        {
            await using var factory = CreateFactory(connectionString);
            using var client = factory.CreateClient();

            var initialized = await PostInitializeAsync(client, CreateRequest(), "sql-integration-login-init");
            var login = await client.PostAsJsonAsync(
                "/api/v1/platform/auth/login",
                new LoginRequest("admin", "StrongPassword1!", "WIN-CLIENT-01", "1.0.0"));
            var session = await login.Content.ReadFromJsonAsync<SessionResponse>();
            var secondLogin = await client.PostAsJsonAsync(
                "/api/v1/platform/auth/login",
                new LoginRequest("admin", "StrongPassword1!", "WIN-CLIENT-02", "1.0.0"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", session!.AccessToken);
            var me = await client.GetAsync("/api/v1/platform/auth/me");
            client.DefaultRequestHeaders.Authorization = null;
            var refresh = await client.PostAsJsonAsync(
                "/api/v1/platform/auth/refresh",
                new RefreshSessionRequest(session.Session.Id, session.RefreshToken));
            var refreshedSession = await refresh.Content.ReadFromJsonAsync<SessionResponse>();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", refreshedSession!.AccessToken);
            var roles = await client.GetFromJsonAsync<IReadOnlyList<RoleSummaryResponse>>("/api/v1/platform/roles");
            Assert.NotNull(roles);
            var administratorRoleId = roles.Single(role => role.Name == "Administrador").Id;
            var billingRoleId = roles.Single(role => role.Name == "Facturacion").Id;
            var createRole = await client.PostAsJsonAsync(
                "/api/v1/platform/roles",
                new CreateRoleRequest("Soporte"));
            var createdRole = await createRole.Content.ReadFromJsonAsync<RoleSummaryResponse>();
            var updateBillingPermissions = await client.PutAsJsonAsync(
                $"/api/v1/platform/roles/{billingRoleId:D}/permissions",
                new UpdateRolePermissionsRequest(
                [
                    PlatformPermissionNames.UseAttachments,
                    PlatformPermissionNames.ViewAudit
                ]));
            var billingPermissions = await client.GetFromJsonAsync<RolePermissionsResponse>(
                $"/api/v1/platform/roles/{billingRoleId:D}/permissions");
            var createUser = await client.PostAsJsonAsync(
                "/api/v1/platform/users",
                new CreateUserRequest("Usuario SQL", "usuario-sql", "+34919999999", administratorRoleId, "StrongPassword1!"));
            var createdUser = await createUser.Content.ReadFromJsonAsync<UserSummaryResponse>();
            var users = await client.GetFromJsonAsync<IReadOnlyList<UserSummaryResponse>>("/api/v1/platform/users");
            var auditEvents = await client.GetFromJsonAsync<IReadOnlyList<AuditEventResponse>>("/api/v1/platform/audit-events?take=10");
            var logout = await client.PostAsync("/api/v1/platform/auth/logout", null);

            Assert.Equal(HttpStatusCode.Created, initialized.StatusCode);
            Assert.Equal(HttpStatusCode.OK, login.StatusCode);
            Assert.NotNull(session);
            Assert.NotEqual(Guid.Empty, session.Session.Id);
            Assert.False(string.IsNullOrWhiteSpace(session.AccessToken));
            Assert.False(string.IsNullOrWhiteSpace(session.RefreshToken));
            Assert.Contains(PlatformPermissionNames.ManageUsers, session.User.Permissions);
            Assert.Equal(HttpStatusCode.Conflict, secondLogin.StatusCode);
            Assert.Equal(HttpStatusCode.OK, me.StatusCode);
            Assert.Equal(HttpStatusCode.OK, refresh.StatusCode);
            Assert.Equal(session.Session.Id, refreshedSession.Session.Id);
            Assert.NotEqual(session.RefreshToken, refreshedSession.RefreshToken);
            Assert.Equal(HttpStatusCode.Created, createRole.StatusCode);
            Assert.NotNull(createdRole);
            Assert.Equal("Soporte", createdRole.Name);
            Assert.Equal("custom", createdRole.Type);
            Assert.Equal(HttpStatusCode.OK, updateBillingPermissions.StatusCode);
            Assert.NotNull(billingPermissions);
            Assert.Contains(PlatformPermissionNames.ViewAudit, billingPermissions.Permissions);
            Assert.Equal(HttpStatusCode.Created, createUser.StatusCode);
            Assert.NotNull(createdUser);
            Assert.Equal("usuario-sql", createdUser.UserName);
            Assert.NotNull(users);
            Assert.Contains(users, user => user.UserName == "admin");
            Assert.Contains(users, user => user.UserName == "Sistema");
            Assert.Contains(users, user => user.UserName == "usuario-sql" && user.Role.Name == "Administrador");
            Assert.NotNull(auditEvents);
            Assert.Contains(auditEvents, audit => audit.Action == "UserCreated" && audit.EntityId == createdUser.Id.ToString("D"));
            Assert.Contains(auditEvents, audit => audit.Action == "RolePermissionsUpdated" && audit.EntityId == billingRoleId.ToString("D"));
            Assert.Contains(auditEvents, audit => audit.Action == "RoleCreated" && audit.EntityId == createdRole.Id.ToString("D"));
            Assert.Equal(HttpStatusCode.NoContent, logout.StatusCode);
            Assert.Equal(1, await dbContext.UserSessions.CountAsync());
            Assert.Equal(1, await dbContext.UserSessions.CountAsync(value => value.Status == 2 && value.ClosedAtUtc != null));
            Assert.Equal(1, await dbContext.Users.CountAsync(user => user.NormalizedUserName == "USUARIO-SQL" && user.Phone == "+34919999999"));
            Assert.Equal(1, await dbContext.ReservedUserNames.CountAsync(userName => userName.NormalizedUserName == "USUARIO-SQL"));
            Assert.Equal(1, await dbContext.Roles.CountAsync(role =>
                role.NormalizedName == "SOPORTE" &&
                role.RoleType == 2 &&
                role.Status == 1 &&
                !role.IsProtected));
            Assert.True(await dbContext.RolePermissions.AnyAsync(permission =>
                permission.RoleId == billingRoleId &&
                permission.Permission == PlatformPermissionNames.ViewAudit));
            Assert.True(await dbContext.AuditEvents.AnyAsync(audit =>
                audit.Action == "UserCreated" &&
                audit.ActorUserId == session.User.Id &&
                audit.EntityId == createdUser.Id.ToString("D") &&
                audit.NewValuesJson != null &&
                audit.NewValuesJson.Contains("usuario-sql")));
            Assert.True(await dbContext.AuditEvents.AnyAsync(audit =>
                audit.Action == "RolePermissionsUpdated" &&
                audit.ActorUserId == session.User.Id &&
                audit.EntityId == billingRoleId.ToString("D") &&
                audit.PreviousValuesJson != null &&
                audit.NewValuesJson != null &&
                audit.NewValuesJson.Contains(PlatformPermissionNames.ViewAudit)));
            var administrationAuditTexts = await dbContext.AuditEvents
                .Where(audit => audit.Action == "UserCreated" || audit.Action == "RolePermissionsUpdated")
                .Select(audit => string.Concat(audit.Description, audit.NewValuesJson, audit.PreviousValuesJson))
                .ToListAsync();
            Assert.DoesNotContain(administrationAuditTexts, value => value.Contains("StrongPassword1!", StringComparison.Ordinal));
            var created = await dbContext.Users.SingleAsync(user => user.NormalizedUserName == "USUARIO-SQL");
            Assert.NotEqual("StrongPassword1!", created.PasswordHash);
            Assert.StartsWith("PBKDF2-SHA256.", created.PasswordHash, StringComparison.Ordinal);
            Assert.True(await dbContext.Users.AnyAsync(user =>
                user.NormalizedUserName == "ADMIN" &&
                user.LastSuccessfulLoginUtc != null &&
                user.FailedLoginCount == 0));
        }
        finally
        {
            await dbContext.Database.EnsureDeletedAsync();
        }
    }

    private static PlatformDbContext CreateDbContext(string connectionString)
    {
        var options = new DbContextOptionsBuilder<PlatformDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        return new PlatformDbContext(options);
    }

    private static async Task<bool> CanOpenSqlServerAsync(string connectionString)
    {
        try
        {
            var builder = new SqlConnectionStringBuilder(connectionString)
            {
                InitialCatalog = "master"
            };
            await using var connection = new SqlConnection(builder.ConnectionString);
            await connection.OpenAsync();
            return true;
        }
        catch (SqlException)
        {
            return false;
        }
    }

    private static WebApplicationFactory<Program> CreateFactory(string connectionString)
    {
        return new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.ConfigureLogging(logging => logging.ClearProviders());
            builder.ConfigureAppConfiguration((_, configurationBuilder) =>
            {
                configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:CriGes"] = connectionString
                });
            });
        });
    }

    private static InitializePlatformRequest CreateRequest()
    {
        return new InitializePlatformRequest(
            new CompanyRequest(
                "Empresa Ejemplo SL",
                "Empresa Ejemplo",
                "B12345678",
                new AddressRequest("Calle Mayor 1", "28001", "Madrid", "Madrid", "ES"),
                "+34910000000",
                "administracion@example.com"),
            new AdministratorRequest("Administrador", "admin", "StrongPassword1!"));
    }

    private static Task<HttpResponseMessage> PostInitializeAsync(
        HttpClient client,
        InitializePlatformRequest request,
        string idempotencyKey)
    {
        var message = JsonContent.Create(request);
        message.Headers.Add("Idempotency-Key", idempotencyKey);
        return client.PostAsync("/api/v1/platform/installation/initialize", message);
    }
}
