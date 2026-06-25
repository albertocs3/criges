using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using CriGes.Modules.Platform.Application.Initialization;
using CriGes.Modules.Platform.Contracts.Administration;
using CriGes.Modules.Platform.Contracts.Auth;
using CriGes.Modules.Platform.Contracts.Customers;
using CriGes.Modules.Platform.Contracts.Installation;
using CriGes.Modules.Platform.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace CriGes.Modules.Platform.ContractTests.Administration;

public sealed class PlatformAdministrationEndpointTests
{
    [Fact]
    public async Task ListRolesRequiresManageRolesPermission()
    {
        await using var factory = CreateFactory();
        using var client = factory.CreateClient();

        var anonymous = await client.GetAsync("/api/v1/platform/roles");
        var anonymousProblem = await anonymous.Content.ReadFromJsonAsync<JsonElement>();

        await InitializeAndLoginAsAdminAsync(client);
        var authorized = await client.GetFromJsonAsync<IReadOnlyList<RoleSummaryResponse>>("/api/v1/platform/roles");

        Assert.Equal(HttpStatusCode.Unauthorized, anonymous.StatusCode);
        Assert.Equal("AUTH.INVALID_TOKEN", anonymousProblem.GetProperty("code").GetString());
        Assert.NotNull(authorized);
        Assert.Contains(authorized, role => role.Name == "Administrador");
        Assert.Contains(authorized.Single(role => role.Name == "Administrador").Permissions, value => value == PlatformPermissionNames.ManageUsers);
    }

    [Fact]
    public async Task CreateRoleRequiresManageRolesPermissionAndRejectsDuplicateName()
    {
        await using var factory = CreateFactory();
        using var client = factory.CreateClient();

        var anonymous = await client.PostAsJsonAsync("/api/v1/platform/roles", new CreateRoleRequest("Soporte"));
        var anonymousProblem = await anonymous.Content.ReadFromJsonAsync<JsonElement>();

        await InitializeAndLoginAsAdminAsync(client);

        var created = await client.PostAsJsonAsync("/api/v1/platform/roles", new CreateRoleRequest("Soporte"));
        var body = await created.Content.ReadFromJsonAsync<RoleSummaryResponse>();
        var roles = await client.GetFromJsonAsync<IReadOnlyList<RoleSummaryResponse>>("/api/v1/platform/roles");
        var duplicate = await client.PostAsJsonAsync("/api/v1/platform/roles", new CreateRoleRequest(" soporte "));
        var duplicateProblem = await duplicate.Content.ReadFromJsonAsync<JsonElement>();
        var auditEvents = await client.GetFromJsonAsync<IReadOnlyList<AuditEventResponse>>("/api/v1/platform/audit-events?take=10");

        Assert.Equal(HttpStatusCode.Unauthorized, anonymous.StatusCode);
        Assert.Equal("AUTH.INVALID_TOKEN", anonymousProblem.GetProperty("code").GetString());
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        Assert.NotNull(body);
        Assert.Equal("Soporte", body.Name);
        Assert.Equal("custom", body.Type);
        Assert.Equal("active", body.Status);
        Assert.Empty(body.Permissions);
        Assert.NotNull(roles);
        Assert.Contains(roles, role => role.Name == "Soporte");
        Assert.Equal(HttpStatusCode.Conflict, duplicate.StatusCode);
        Assert.Equal("SECURITY.ROLENAME_ALREADY_RESERVED", duplicateProblem.GetProperty("code").GetString());
        Assert.NotNull(auditEvents);
        Assert.Contains(auditEvents, audit => audit.Action == "RoleCreated" && audit.EntityId == body.Id.ToString("D"));
    }

    [Fact]
    public async Task CreateUserRequiresManageUsersPermissionAndRejectsDuplicateUserName()
    {
        await using var factory = CreateFactory();
        using var client = factory.CreateClient();
        await InitializeAndLoginAsAdminAsync(client);

        var roles = await client.GetFromJsonAsync<IReadOnlyList<RoleSummaryResponse>>("/api/v1/platform/roles");
        var administratorRoleId = roles!.Single(role => role.Name == "Administrador").Id;
        var request = new CreateUserRequest(
            "Usuario Ejemplo",
            "usuario",
            "+34910000000",
            administratorRoleId,
            "StrongPassword1!");

        var created = await client.PostAsJsonAsync("/api/v1/platform/users", request);
        var body = await created.Content.ReadFromJsonAsync<UserSummaryResponse>();
        var duplicate = await client.PostAsJsonAsync("/api/v1/platform/users", request);
        var duplicateProblem = await duplicate.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        Assert.NotNull(body);
        Assert.Equal("usuario", body.UserName);
        Assert.Equal("+34910000000", body.Phone);
        Assert.Equal("active", body.Status);
        Assert.Equal(HttpStatusCode.Conflict, duplicate.StatusCode);
        Assert.Equal("SECURITY.USERNAME_ALREADY_RESERVED", duplicateProblem.GetProperty("code").GetString());
    }

    [Fact]
    public async Task ListUsersRequiresManageUsersPermissionAndReturnsCreatedUsers()
    {
        await using var factory = CreateFactory();
        using var client = factory.CreateClient();

        var anonymous = await client.GetAsync("/api/v1/platform/users");
        var anonymousProblem = await anonymous.Content.ReadFromJsonAsync<JsonElement>();

        await InitializeAndLoginAsAdminAsync(client);

        var roles = await client.GetFromJsonAsync<IReadOnlyList<RoleSummaryResponse>>("/api/v1/platform/roles");
        var administratorRoleId = roles!.Single(role => role.Name == "Administrador").Id;
        var created = await client.PostAsJsonAsync(
            "/api/v1/platform/users",
            new CreateUserRequest("Usuario Listado", "usuario-listado", null, administratorRoleId, "StrongPassword1!"));
        var users = await client.GetFromJsonAsync<IReadOnlyList<UserSummaryResponse>>("/api/v1/platform/users");

        Assert.Equal(HttpStatusCode.Unauthorized, anonymous.StatusCode);
        Assert.Equal("AUTH.INVALID_TOKEN", anonymousProblem.GetProperty("code").GetString());
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        Assert.NotNull(users);
        Assert.Contains(users, user => user.UserName == "admin");
        Assert.Contains(users, user => user.UserName == "Sistema");
        Assert.Contains(users, user => user.UserName == "usuario-listado" && user.Role.Name == "Administrador");
    }

    [Fact]
    public async Task RolePermissionsCanBeReadAndReplacedByManageRolesUser()
    {
        await using var factory = CreateFactory();
        using var client = factory.CreateClient();

        var roleId = Guid.NewGuid();
        var anonymous = await client.GetAsync($"/api/v1/platform/roles/{roleId:D}/permissions");
        var anonymousProblem = await anonymous.Content.ReadFromJsonAsync<JsonElement>();

        await InitializeAndLoginAsAdminAsync(client);

        var roles = await client.GetFromJsonAsync<IReadOnlyList<RoleSummaryResponse>>("/api/v1/platform/roles");
        roleId = roles!.Single(role => role.Name == "Facturacion").Id;
        var current = await client.GetFromJsonAsync<RolePermissionsResponse>($"/api/v1/platform/roles/{roleId:D}/permissions");
        var updated = await client.PutAsJsonAsync(
            $"/api/v1/platform/roles/{roleId:D}/permissions",
            new UpdateRolePermissionsRequest(
            [
                PlatformPermissionNames.UseAttachments,
                PlatformPermissionNames.ViewAudit
            ]));
        var updatedBody = await updated.Content.ReadFromJsonAsync<RolePermissionsResponse>();
        var invalid = await client.PutAsJsonAsync(
            $"/api/v1/platform/roles/{roleId:D}/permissions",
            new UpdateRolePermissionsRequest(["Platform.DoesNotExist"]));
        var invalidProblem = await invalid.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.Unauthorized, anonymous.StatusCode);
        Assert.Equal("AUTH.INVALID_TOKEN", anonymousProblem.GetProperty("code").GetString());
        Assert.NotNull(current);
        Assert.Contains(PlatformPermissionNames.UseAttachments, current.Permissions);
        Assert.Equal(HttpStatusCode.OK, updated.StatusCode);
        Assert.NotNull(updatedBody);
        Assert.Contains(PlatformPermissionNames.ViewAudit, updatedBody.Permissions);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, invalid.StatusCode);
        Assert.Equal("SECURITY.UNKNOWN_PERMISSION", invalidProblem.GetProperty("code").GetString());
    }

    [Fact]
    public async Task ListAuditEventsRequiresViewAuditPermissionAndReturnsAdministrativeEvents()
    {
        await using var factory = CreateFactory();
        using var client = factory.CreateClient();

        var anonymous = await client.GetAsync("/api/v1/platform/audit-events");
        var anonymousProblem = await anonymous.Content.ReadFromJsonAsync<JsonElement>();

        await InitializeAndLoginAsAdminAsync(client);

        var roles = await client.GetFromJsonAsync<IReadOnlyList<RoleSummaryResponse>>("/api/v1/platform/roles");
        var administratorRoleId = roles!.Single(role => role.Name == "Administrador").Id;
        var created = await client.PostAsJsonAsync(
            "/api/v1/platform/users",
            new CreateUserRequest("Usuario Audit", "usuario-audit", null, administratorRoleId, "StrongPassword1!"));
        var auditEvents = await client.GetFromJsonAsync<IReadOnlyList<AuditEventResponse>>("/api/v1/platform/audit-events?take=10");

        Assert.Equal(HttpStatusCode.Unauthorized, anonymous.StatusCode);
        Assert.Equal("AUTH.INVALID_TOKEN", anonymousProblem.GetProperty("code").GetString());
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        Assert.NotNull(auditEvents);
        Assert.Contains(auditEvents, audit => audit.Action == "UserCreated" && audit.EntityType == "User");
    }

    [Fact]
    public async Task CustomersRequirePermissionAndCanBeCreatedAndListed()
    {
        await using var factory = CreateFactory();
        using var client = factory.CreateClient();

        var anonymous = await client.GetAsync("/api/v1/customers/");
        var anonymousProblem = await anonymous.Content.ReadFromJsonAsync<JsonElement>();

        await InitializeAndLoginAsAdminAsync(client);

        var created = await client.PostAsJsonAsync(
            "/api/v1/customers/",
            new CreateCustomerRequest("Cliente Contrato", "B11111111", "cliente@example.com", "+34910000000"));
        var body = await created.Content.ReadFromJsonAsync<CustomerSummaryResponse>();
        var duplicate = await client.PostAsJsonAsync(
            "/api/v1/customers/",
            new CreateCustomerRequest("Cliente Duplicado", "b11111111", null, null));
        var duplicateProblem = await duplicate.Content.ReadFromJsonAsync<JsonElement>();
        var customers = await client.GetFromJsonAsync<IReadOnlyList<CustomerSummaryResponse>>("/api/v1/customers/");

        Assert.Equal(HttpStatusCode.Unauthorized, anonymous.StatusCode);
        Assert.Equal("AUTH.INVALID_TOKEN", anonymousProblem.GetProperty("code").GetString());
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        Assert.NotNull(body);
        Assert.Equal("Cliente Contrato", body.Name);
        Assert.Equal("active", body.Status);
        Assert.Equal(HttpStatusCode.Conflict, duplicate.StatusCode);
        Assert.Equal("CUSTOMERS.TAXID_ALREADY_EXISTS", duplicateProblem.GetProperty("code").GetString());
        Assert.NotNull(customers);
        Assert.Contains(customers, customer => customer.Name == "Cliente Contrato");
    }

    private static async Task InitializeAndLoginAsAdminAsync(HttpClient client)
    {
        await InitializeAsync(client);
        var response = await client.PostAsJsonAsync(
            "/api/v1/platform/auth/login",
            new LoginRequest("admin", "StrongPassword1!", "WIN-CLIENT-01", "1.0.0"));
        var session = await response.Content.ReadFromJsonAsync<SessionResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(session);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", session.AccessToken);
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

    private static WebApplicationFactory<Program> CreateFactory()
    {
        return new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.ConfigureLogging(logging => logging.ClearProviders());
            builder.ConfigureAppConfiguration((_, configurationBuilder) =>
            {
                configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Database:ApplyMigrationsOnStartup"] = "false"
                });
            });
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IPlatformInitializationStore>();
                services.AddPlatformInMemoryInfrastructureForTests();
            });
        });
    }
}
